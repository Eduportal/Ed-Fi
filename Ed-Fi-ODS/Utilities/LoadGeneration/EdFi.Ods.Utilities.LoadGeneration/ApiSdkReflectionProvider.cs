using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using Castle.Core.Logging;
using EdFi.Ods.Utilities.LoadGeneration._Extensions;
using log4net;
using RestSharp;

namespace EdFi.Ods.Utilities.LoadGeneration
{
    public class ApiSdkReflectionProvider : IApiSdkReflectionProvider
    {
        private ILog _logger = LogManager.GetLogger(typeof(ApiSdkReflectionProvider));

        private readonly IApiSdkAssemblyProvider _apiSdkAssemblyProvider;

        public ApiSdkReflectionProvider(IApiSdkAssemblyProvider _apiSdkAssemblyProvider)
        {
            this._apiSdkAssemblyProvider = _apiSdkAssemblyProvider;
        }

        public ConstructorInfo GetApiConstructorForModelType(Type modelType)
        {
            Type apiType;

            if (!TryGetApiTypeForModelType(modelType, out apiType))
                throw new Exception(string.Format("Unable to find api class for model type '{0}'.", modelType.Name));

            var constructorInfo = apiType.GetConstructor(new[] { typeof(IRestClient) });
            if (constructorInfo == null)
                throw new Exception("Failed to find the constructor for type " + modelType.FullName);

            return constructorInfo;
        }

        private ConcurrentDictionary<Type, Type> _apiTypeByModelType;

        private bool TryGetApiTypeForModelType(Type modelType, out Type apiType)
        {
            if (_apiTypeByModelType == null)
            {
                InitializeApiTypeByModelType();
            }

            return _apiTypeByModelType.TryGetValue(modelType, out apiType);
        }

        private void InitializeApiTypeByModelType()
        {
            var apiTypeByModelType = new ConcurrentDictionary<Type, Type>();
            var apiTypes = SdkTypes.Where(t => t.Name.EndsWith("Api")).ToList();
            var apiGetMethods =
                from a in apiTypes
                from m in a.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                where m.Name.StartsWith("Get") && m.ReturnType.IsGenericType
                select new {ApiType = a, Method = m};

            var groupedApiGetMethods =
                from x in apiGetMethods
                group x by x.ApiType
                into g
                select new {ApiType = g.Key, Method = g.Select(y => y.Method).FirstOrDefault()};

            foreach (var item in groupedApiGetMethods)
            {
                Type modelType = item.Method.ReturnType;

                while (modelType.IsGenericType)
                {
                    modelType = modelType.GetGenericArguments()[0];
                }

                apiTypeByModelType[modelType] = item.ApiType;
            }

            _apiTypeByModelType = apiTypeByModelType;
        }

        private bool LooksLikeTheGetAllMethod(MethodInfo mi, Type modelType)
        {
            //find the correct method.  We're looking at the method's signature so that we don't have to find any kind of pluralization/naming issues.
            //The correct method has a signature like public IRestResponse<List<GradeLevelDescriptor>> GetGradeLevelDescriptorsAll(int? offset, int? limit)
            //If the arguments are wrong, then the calls to Invoke() will also break, so it's a reasonable way of locating the correct method.
            //return mi.ReturnType == typeof(IRestResponse<>).MakeGenericType(new[] { typeof(List<>).MakeGenericType(new[] { modelType }) })
            //       && mi.GetParameters().Length == 2 && mi.GetParameters()[0].ParameterType == typeof(int?) && mi.GetParameters()[1].ParameterType == typeof(int?);

            return mi.Name.StartsWith("Get", StringComparison.InvariantCultureIgnoreCase)
                   && mi.Name.EndsWith("All", StringComparison.InvariantCultureIgnoreCase)
                   && mi.ReturnType.IsGenericType
                   && mi.ReturnType.GetGenericArguments()[0].IsGenericType
                   && mi.ReturnType.GetGenericArguments()[0].GetGenericArguments()[0] == modelType;
        }

        public MethodInfo LocateGetAllMethodFrom(object api, Type modelType)
        {
            return api.GetType().GetMethods().Single(mi => LooksLikeTheGetAllMethod(mi, modelType));
        }

        public MethodInfo LocatePostMethodFrom(object api)
        {
            return api.GetType().GetMethods().Single(mi => 
                //mi.ReturnType == typeof(IRestResponse)
                   mi.Name.StartsWith("Post") && mi.GetParameters().Length == 1);
        }

        private readonly HashSet<string> _warnedMissingModelTypes = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        private readonly ReaderWriterLockSlim _warnedMissingModelTypesLock = new ReaderWriterLockSlim();

        public bool TryGetModelType(string modelName, out Type modelType)
        {
            modelType = 
                (from t in SdkTypes
                where t.Namespace != null 
                && (t.Namespace.Contains(".Models") && t.Name.Equals(modelName, StringComparison.InvariantCultureIgnoreCase))
                select t)
                .FirstOrDefault();

            if (modelType == null)
            {
                // Reduce log noise by only warning once for each type.
                if (_warnedMissingModelTypes.TryAdd(modelName, _warnedMissingModelTypesLock))
                    _logger.WarnFormat("Unable to find SDK model type for '{0}'.", modelName);

                return false;
            }

            return true;
        }

        private Type[] _sdkTypes;

        private Type[] SdkTypes
        {
            get
            {
                if (_sdkTypes == null)
                     _sdkTypes = _apiSdkAssemblyProvider.GetAssembly().GetTypes();

                return _sdkTypes;
            }
        }

        public bool TryGetModelType(Type resourceReferenceType, out Type modelType)
        {
            string modelName = Regex.Replace(resourceReferenceType.Name, "^(.*?)Reference$", "$1");

            if (modelName == resourceReferenceType.Name)
                throw new ArgumentException(string.Format("'{0}' is not a reference type.", resourceReferenceType.Name), "resourceReferenceType");

            return TryGetModelType(modelName, out modelType);
        }

        private readonly HashSet<string> _warnedNonExistingReferenceTypes = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        private readonly ReaderWriterLockSlim _warnedNonExistingReferenceTypesLock = new ReaderWriterLockSlim();

        public bool TryGetReferenceType(Type resourceType, out Type resourceReferenceType)
        {
            // Initialize output parameters
            resourceReferenceType = null;

            string searchResourceTypeName = resourceType.Name + "Reference";
            string referenceTypeName = searchResourceTypeName;

            var locatedReferenceType = 
                (from t in SdkTypes
                where t.Name.Equals(referenceTypeName, StringComparison.InvariantCultureIgnoreCase)
                select t)
                .FirstOrDefault();

            if (locatedReferenceType == null)
            {
                // Reduce log noise by only warning once for each type.
                if (_warnedNonExistingReferenceTypes.TryAdd(searchResourceTypeName, _warnedNonExistingReferenceTypesLock))
                    _logger.WarnFormat("Type '{0}' has no reference type.", searchResourceTypeName);

                return false;
            }

            resourceReferenceType = locatedReferenceType;
            return true;
        }

        public ConstructorInfo GetTokenRetrieverConstructor()
        {
            return SdkTypes.Single(type => type.Namespace != null && (type.Name == "TokenRetriever" && type.Namespace.Contains(".Sdk"))).GetConstructors().Single();
        }

        public ConstructorInfo GetBearerTokenAuthenticatorConstructor()
        {
            return SdkTypes.Single(type => type.Namespace != null && (type.Name == "BearerTokenAuthenticator" && type.Namespace.Contains(".Sdk"))).GetConstructors().Single();
        }
    }
}