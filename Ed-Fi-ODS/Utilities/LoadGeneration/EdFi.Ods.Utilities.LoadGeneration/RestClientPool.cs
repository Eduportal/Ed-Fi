using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Castle.DynamicProxy;
using EdFi.Ods.Utilities.LoadGeneration.Security;
using log4net;
using RestSharp;

namespace EdFi.Ods.Utilities.LoadGeneration
{
    public class RestClientPool : IRestClientPool
    {
        private ILog _logger = LogManager.GetLogger(typeof(RestClientPool));

        private readonly IApiSdkReflectionProvider _apiSdkReflectionProvider;
        private readonly IApiSecurityContextProvider _apiSecurityContextProvider;

        private readonly ObjectPool<IRestClient> _restClientPool;
        private readonly ObjectPool<IRestClient> _restClientWithSchoolYearPool;

        public RestClientPool(IApiSdkReflectionProvider apiSdkReflectionProvider, 
            IApiSecurityContextProvider apiSecurityContextProvider)
        {
            _apiSdkReflectionProvider = apiSdkReflectionProvider;
            _apiSecurityContextProvider = apiSecurityContextProvider;

            // Perform Lazy initialization
            _authenticator = LazyAuthenticator();
            _tokenRetriever = LazyTokenRetriever();

            // Trust all SSL certs -- needed for now until signed SSL certificates are configured.
            System.Net.ServicePointManager.ServerCertificateValidationCallback =
                ((sender, certificate, chain, sslPolicyErrors) => true);

            _restClientPool = new ObjectPool<IRestClient>(CreateRestClientWithoutSchoolYear);
            _restClientWithSchoolYearPool = new ObjectPool<IRestClient>(CreateRestClientWithSchoolYear);
        }

        private readonly Lazy<IAuthenticator> _authenticator;

        private Lazy<IAuthenticator> LazyAuthenticator()
        {
            return new Lazy<IAuthenticator>(() =>
            {
                // Plug Oauth into RestSharp's authentication scheme.  We're using dynamic so we don't take any hard references on anything.
                //     The reflection does the same as new BearerTokenAuthenticator(tokenRetriever)
                var authenticatorConstructor = _apiSdkReflectionProvider.GetBearerTokenAuthenticatorConstructor();
                var authenticator = (IAuthenticator) authenticatorConstructor.Invoke(new[] {_tokenRetriever.Value});

                return authenticator;
            }, true);
        }

        private readonly Lazy<object> _tokenRetriever;
 
        private Lazy<object> LazyTokenRetriever()
        {
            return new Lazy<object>(() =>
            {
                var securityContext = _apiSecurityContextProvider.GetSecurityContext();

                var oauthUrl = securityContext.OAuthUrl;
                var clientKey = securityContext.ApiKey;
                var clientSecret = securityContext.ApiSecret;

                // TokenRetriever makes the oauth  calls
                //     The reflection does the same as  new TokenRetriever(oauthUrl, clientKey, clientSecret);
                var tokenRetrieverConstructor = _apiSdkReflectionProvider.GetTokenRetrieverConstructor();
                var tokenRetriever = tokenRetrieverConstructor.Invoke(new object[] { oauthUrl, clientKey, clientSecret });
                
                return tokenRetriever;
            }, true);
        }

        public IRestClient GetRestClient(bool withSchoolYear)
        {
            if (withSchoolYear)
                return _restClientWithSchoolYearPool.GetObject();

            return _restClientPool.GetObject();
        }

        public bool ReleaseRestClient(IRestClient restClient)
        {
            return _restClientWithSchoolYearPool.ReleaseObject(restClient)
                || _restClientPool.ReleaseObject(restClient);
        }

        private IRestClient CreateRestClientWithSchoolYear()
        {
            return CreateRestClient(true);
        }

        private IRestClient CreateRestClientWithoutSchoolYear()
        {
            return CreateRestClient(false);
        }

        private IRestClient CreateRestClient(bool withSchoolYear)
        {
            _logger.InfoFormat("Creating REST API client{0}...",
                withSchoolYear ? " (including schoolyear in base route)" : string.Empty);

            // OAuth configuration
            var securityContext = _apiSecurityContextProvider.GetSecurityContext();

            var apiUrl = securityContext.ApiUrl;
            int schoolYear = securityContext.SchoolYear;

            // Install RestSharp via NuGet (URL with school year, if appropriate)
            var client = new RestClient(apiUrl + (withSchoolYear ? "/" + schoolYear : string.Empty));
            client.Authenticator = _authenticator.Value;
            //((dynamic) client).Authenticator = _authenticator.Value;

            var generator = new ProxyGenerator();
            var proxiedClient = generator.CreateInterfaceProxyWithTarget<IRestClient>(client, new RestClientLoggingInterceptor());

            return proxiedClient;
        }
    }

    internal class ObjectPool<T>
    {
        private ConcurrentBag<T> _availableObjects;
        private List<T> _unavailableObjects;
        private object _lock = new object();

        private Func<T> _objectGenerator;

        public ObjectPool(Func<T> objectGenerator)
        {
            if (objectGenerator == null) throw new ArgumentNullException("objectGenerator");
            
            _availableObjects = new ConcurrentBag<T>();
            _unavailableObjects = new List<T>();
            
            _objectGenerator = objectGenerator;
        }

        public T GetObject()
        {
            T item;

            if (!_availableObjects.TryTake(out item))
                item = _objectGenerator();
             
            lock (_lock)
                _unavailableObjects.Add(item);

            return item;
        }

        public bool ReleaseObject(T item)
        {
            lock (_lock)
            {
                // Readd the item to the list of available items
                if (_unavailableObjects.Remove(item))
                {
                    _availableObjects.Add(item);
                    return true;
                }
            }

            return false;
        }
    }
}
