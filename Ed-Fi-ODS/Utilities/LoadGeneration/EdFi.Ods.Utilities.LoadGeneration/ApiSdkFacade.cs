using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using EdFi.Ods.Utilities.LoadGeneration.Security;
using log4net;
using Newtonsoft.Json;
using RestSharp;

namespace EdFi.Ods.Utilities.LoadGeneration
{
    public class FilePersistedApiSdkFacadeDecorator : IApiSdkFacade
    {
        private ILog _logger = LogManager.GetLogger(typeof(FilePersistedApiSdkFacadeDecorator));

        private readonly IApiSdkFacade _next;
        private readonly IApiSecurityContextProvider _apiSecurityContextProvider;

        public FilePersistedApiSdkFacadeDecorator(IApiSdkFacade next, IApiSecurityContextProvider apiSecurityContextProvider)
        {
            _next = next;
            _apiSecurityContextProvider = apiSecurityContextProvider;
        }

        public IEnumerable GetAll(Type modelType)
        {
            var apiContext = _apiSecurityContextProvider.GetSecurityContext();

            // Check to see if we already have this data cached
            string jsonFilePath;

            if (!apiContext.RefreshCache && File.Exists(jsonFilePath = GetJsonFilePath(modelType)))
            {
                _logger.DebugFormat("Serving GetAll for '{0}' from file '{1}'.", modelType.Name, jsonFilePath);
                return LoadResults(modelType);
            }

            if (apiContext.RefreshCache)
                _logger.Debug("RefreshCache option was set.");

            // Call through to the API
            var results = _next.GetAll(modelType);

            var resultList = results as IList;

            // Cache results
            SaveResults(modelType, resultList);

            return results;
        }

        private void SaveResults(Type modelType, IList results)
        {
            string path = GetJsonFilePath(modelType);
            string json = JsonConvert.SerializeObject(results, Formatting.Indented);
            _logger.DebugFormat("Saving results for GetAll on '{0}' to '{1}'.", modelType.Name, path);
            File.WriteAllText(path, json);
        }

        private IList LoadResults(Type modelType)
        {
            string path = GetJsonFilePath(modelType);
            string json = File.ReadAllText(path);

            var listOfModelType = typeof(IList<>).MakeGenericType(modelType);

            return (IList) JsonConvert.DeserializeObject(json, listOfModelType);
        }

        private string GetJsonFilePath(Type modelType)
        {
            string appName;

            string apiUrl = _apiSecurityContextProvider.GetSecurityContext().ApiUrl;

            if (Assembly.GetEntryAssembly() != null)
            {
                appName = Assembly.GetEntryAssembly().GetName().Name;
            }
            else
            {
                appName = Path.GetFileNameWithoutExtension(AppDomain.CurrentDomain.FriendlyName);
            }

            string apiSpecificFolder = "ApiUrl" + apiUrl.GetHashCode();

            string folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                appName, 
                apiSpecificFolder);

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            return Path.Combine(folder, modelType.Name + ".json");
        }

        public IRestResponse Post(object body)
        {
            // Pass call through
            return _next.Post(body);
        }
    }

    public class ApiSdkFacade : IApiSdkFacade
    {
        private ILog _logger = LogManager.GetLogger(typeof(ApiSdkFacade));

        private readonly IApiSdkReflectionProvider _apiSdkReflectionProvider;
        private readonly IRestClientPool _restClientPool;

        public ApiSdkFacade(IApiSdkReflectionProvider apiSdkReflectionProvider, IRestClientPool restClientPool)
        {
            this._apiSdkReflectionProvider = apiSdkReflectionProvider;
            this._restClientPool = restClientPool;
        }

        private const int PageSize = 100;

        public IEnumerable GetAll(Type modelType)
        {
            var withSchoolYear = ShouldIncludeSchoolYear(modelType);

            var constructorInfo = _apiSdkReflectionProvider.GetApiConstructorForModelType(modelType);
            var restClient = _restClientPool.GetRestClient(withSchoolYear);

            try
            {
                var api = constructorInfo.Invoke(new object[] { restClient });

                _logger.InfoFormat("Retrieving all instances of '{0}'.", modelType.Name);

                // This stores the final result being built.
                var finalResult = new List<object>();

                bool done = false;

                int nextOffset = 0;
                int pageNumber = 1;

                while (!done)
                {
                    _logger.InfoFormat("Retrieving page {0} of model type '{1}'.", pageNumber++, modelType.Name);

                    var partialResult = GetPage(modelType, api, nextOffset, PageSize);
                    nextOffset += PageSize;
                    done = partialResult.Count != PageSize;

                    finalResult.AddRange(partialResult.Cast<object>());
                }

                _logger.InfoFormat("Done retrieving {0} instances of '{1}'.", finalResult.Count, modelType.Name);
            
                return finalResult;
            }
            finally
            {
                _restClientPool.ReleaseRestClient(restClient);
            }
        }

        private static bool ShouldIncludeSchoolYear(Type modelType)
        {
            // TODO: This needs some better design applied, but due to time constraints it will have to do for now
            bool withSchoolYear = !modelType.Name.Equals("Identity"); // Don't use school year with identity system
            
            return withSchoolYear;
        }

        public IRestResponse Post(object body)
        {
            Type modelType = body.GetType();
            var withSchoolYear = ShouldIncludeSchoolYear(modelType);

            var constructorInfo = _apiSdkReflectionProvider.GetApiConstructorForModelType(modelType);
            var restClient = _restClientPool.GetRestClient(withSchoolYear);

            try
            {
                var api = constructorInfo.Invoke(new[] { restClient });

                _logger.InfoFormat("Posting instance of '{0}'.", modelType.Name);
            
                var postMethodInfo = _apiSdkReflectionProvider.LocatePostMethodFrom(api);

                var sw = new Stopwatch();

                sw.Start();
                var response = (IRestResponse) postMethodInfo.Invoke(api, new[] { body });
                sw.Stop();

                if (response.StatusCode != HttpStatusCode.Created
                    && response.StatusCode != HttpStatusCode.OK
                    && response.StatusCode != HttpStatusCode.NoContent)
                {
                    _logger.WarnFormat(
                        "Persistence of '{0}' FAILED (POST returned '{1}' with status '{2}'):\r\nContent:\r\n{3}\r\nPOST body (re-serialized with Json.NET):\r\n{4}",
                        modelType.Name, response.StatusCode, response.StatusDescription, response.Content,
                        JsonConvert.SerializeObject(body, Formatting.Indented));
                }
                else
                {
                    _logger.InfoFormat("POST of '{0}' succeeded, returning status code '{1}' ({2}) in {3} milliseconds.", modelType.Name, response.StatusCode, response.StatusDescription, sw.ElapsedMilliseconds);
                    _logger.DebugFormat("POST message body for '{0}':\r\n{1}", modelType.Name, JsonConvert.SerializeObject(body, Formatting.Indented));
                }

                return response;
            }
            finally
            {
                _restClientPool.ReleaseRestClient(restClient);
            }
        }

        private IList GetPage(Type modelType, object api, int? offset, int? limit)
        {
            var getAllMethod = _apiSdkReflectionProvider.LocateGetAllMethodFrom(api, modelType);

            dynamic restResponse = getAllMethod.Invoke(api, new object[] {offset, limit});
            
            if (restResponse.StatusCode != HttpStatusCode.OK)
                throw new Exception(string.Format("The API returned a status of '{3}' while trying to get a page of data for '{0}' (offset = {1}, limit = {2}):\r\nContent: {4}\r\nError Message: {5}\r\nException:\r\n{6} ", 
                    modelType.Name, offset, limit, restResponse.StatusCode, restResponse.Content, restResponse.ErrorMessage, restResponse.ErrorException));

            var results = restResponse.Data as IList;

            return results;
        }
    }
}