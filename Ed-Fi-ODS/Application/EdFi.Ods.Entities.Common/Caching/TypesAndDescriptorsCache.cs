using System;
using System.Linq;
using System.Threading;
using EdFi.Common.Caching;
using EdFi.Common.Configuration;
using EdFi.Ods.Common.Specifications;
using EdFi.Ods.Common.Utils;
using EdFi.Ods.Entities.Common.Providers;

namespace EdFi.Ods.Entities.Common.Caching
{
    public class TypesAndDescriptorsCache : ITypesAndDescriptorsCache
    {
        // This exists as a place to put a global singleton of the Types and Descriptors Cache. Set during the Composite Root creation.
        public static Func<ITypesAndDescriptorsCache> GetCache = () => null;

        private const int DatabaseSynchronizationExpirationSeconds = 60;
        private const string DefaultDescriptorNamespacePrefixAppKey = "DescriptorNamespacePrefix";
        private const string DescriptorCacheKeyPrefix = "Descriptors";
        private const string TypeCacheKeyPrefix = "Types";
        private const string LastSyncedKey = "TypesAndDescriptorsCache.LastSynced";

        private readonly ReaderWriterLockSlim _cacheLock = new ReaderWriterLockSlim();
        private readonly ITypeLookupProvider _typeLookupProvider;
        private readonly IDescriptorLookupProvider _descriptorLookupProvider;
        private readonly ICacheProvider _cacheProvider;
        private readonly IEdFiOdsInstanceIdentificationProvider _edFiOdsInstanceIdentificationProvider;
        private readonly IConfigValueProvider _configValueProvider;

        public TypesAndDescriptorsCache(ITypeLookupProvider typeLookupProvider,
            IDescriptorLookupProvider descriptorLookupProvider, ICacheProvider cacheProvider,
            IEdFiOdsInstanceIdentificationProvider edFiOdsInstanceIdentificationProvider,
            IConfigValueProvider configValueProvider)
        {
            _typeLookupProvider = typeLookupProvider;
            _descriptorLookupProvider = descriptorLookupProvider;
            _cacheProvider = cacheProvider;
            _edFiOdsInstanceIdentificationProvider = edFiOdsInstanceIdentificationProvider;
            _configValueProvider = configValueProvider;

            TryRefreshCache();
        }

        private bool HasLastDatabaseSynchronizationExpired
        {
            get
            {
                var currentTime = SystemClock.Now();
                DateTime lastSynced;
                if (!_cacheProvider.TryGetCachedObject(LastSyncedKey, out lastSynced))
                {
                    lastSynced = DateTime.MinValue;
                }
                var totalSeconds = (currentTime - lastSynced).TotalSeconds;
                return totalSeconds > DatabaseSynchronizationExpirationSeconds;
            }
        }

        private string GetTypeLookupKeyByShortDescription(string typeName, string shortDescription)
        {
            return String.Format("{0}|{1}.{2}.ByCodeValue.{3}", _edFiOdsInstanceIdentificationProvider.GetInstanceIdentification(), TypeCacheKeyPrefix, typeName, shortDescription);
        }

        private string GetTypeLookupKeyById(string typeName, int id)
        {
            return String.Format("{0}|{1}.{2}.ById.{3}", _edFiOdsInstanceIdentificationProvider.GetInstanceIdentification(), TypeCacheKeyPrefix, typeName, id);
        }

        private string GetDescriptorLookupKeyByCodeValue(string descriptorName, string codeValue)
        {
            return String.Format("{0}|{1}.{2}.ByCodeValue.{3}", _edFiOdsInstanceIdentificationProvider.GetInstanceIdentification(), DescriptorCacheKeyPrefix, descriptorName, codeValue);
        }

        private string GetDescriptorLookupKeyById(string descriptorName, int id)
        {
            return String.Format("{0}|{1}.{2}.ById.{3}", _edFiOdsInstanceIdentificationProvider.GetInstanceIdentification(), DescriptorCacheKeyPrefix, descriptorName, id);
        }

        private static string CreateFullyQualifiedDescriptorValue(string nameSpace, string codeValue)
        {
            var workingNameSpace = string.IsNullOrEmpty(nameSpace) ? string.Empty : nameSpace.Trim();
            return string.Format(string.IsNullOrEmpty(workingNameSpace) || workingNameSpace.EndsWith("/")
                ? "{0}{1}"
                : "{0}/{1}",
                workingNameSpace.Trim(), codeValue);
        }

        private string GetDefaultNamespacePrefix()
        {
            var descriptorNamespacePrefix = _configValueProvider.GetValue(DefaultDescriptorNamespacePrefixAppKey);

            if (string.IsNullOrWhiteSpace(descriptorNamespacePrefix))
                throw new Exception(
                    string.Format(
                        "The 'appSettings' configuration value for '{0}' has not been set.  This value should represent the base prefix of the namespace used in the Descriptor table of the ODS (descriptors with namespaces not matching this prefix will be ignored by the REST API).",
                        DefaultDescriptorNamespacePrefixAppKey));

            return descriptorNamespacePrefix;
        }

        /// <summary>
        /// Attempts to refresh the cache instance using the database connection for the current authorization context
        /// </summary>
        private bool TryRefreshCache()
        {
            if (HasLastDatabaseSynchronizationExpired)
            {
                _cacheLock.EnterUpgradeableReadLock();
                try
                {
                    if (HasLastDatabaseSynchronizationExpired)
                    {
                        _cacheLock.EnterWriteLock();
                        try
                        {
                            // Load Types
                            var typeCacheDataDictionary = _typeLookupProvider.GetAllTypeLookups();
                            foreach (var typeCacheData in typeCacheDataDictionary.SelectMany(tcd => tcd.Value))
                            {
                                UpdateTypeLookupCache(typeCacheData);
                            }

                            // Load Descriptors
                            var descriptorCacheDataDictionary = _descriptorLookupProvider.GetAllDescriptorLookups();
                            foreach (
                                var descriptorCacheData in descriptorCacheDataDictionary.SelectMany(dcd => dcd.Value))
                            {
                                UpdateDescriptorLookupCache(descriptorCacheData);
                            }
                            _cacheProvider.SetCachedObject(LastSyncedKey, SystemClock.Now());
                        }
                        finally
                        {
                            _cacheLock.ExitWriteLock();
                        }
                    }
                    return true;
                }
                finally
                {
                    _cacheLock.ExitUpgradeableReadLock();
                }
            }
            return false;
        }

        private void UpdateTypeLookupCache(TypeLookup typeLookup)
        {
            //Types do not have concept of associated namespace
            var codeValueKey = GetTypeLookupKeyByShortDescription(typeLookup.TypeName,
                typeLookup.ShortDescription);
            _cacheProvider.SetCachedObject(codeValueKey, typeLookup.Id);
            var idKey = GetTypeLookupKeyById(typeLookup.TypeName, typeLookup.Id);
            _cacheProvider.SetCachedObject(idKey, typeLookup.ShortDescription);

        }

        private void UpdateDescriptorLookupCache(DescriptorLookup descriptorLookup)
        {
            var defaultNamespacePrefix = GetDefaultNamespacePrefix();

            //Cache by two keys for descriptors
            var fullyQualifiedValue =
                CreateFullyQualifiedDescriptorValue(descriptorLookup.Namespace,
                    descriptorLookup.CodeValue);
            var isDefaultNamespace = fullyQualifiedValue.StartsWith(defaultNamespacePrefix);

            var namespacedCodeValueKey =
                GetDescriptorLookupKeyByCodeValue(descriptorLookup.DescriptorName,
                    fullyQualifiedValue);
            _cacheProvider.SetCachedObject(namespacedCodeValueKey, descriptorLookup.Id);
            // Add entry without namespace only for default namespace
            if (isDefaultNamespace)
            {
                var codeValueKey =
                    GetDescriptorLookupKeyByCodeValue(descriptorLookup.DescriptorName,
                        descriptorLookup.CodeValue);
                _cacheProvider.SetCachedObject(codeValueKey, descriptorLookup.Id);
            }

            //Use fully qualified code value only for non-default namespace entries
            var idKey = GetDescriptorLookupKeyById(descriptorLookup.DescriptorName,
                descriptorLookup.Id);
            var codeValueToUseInCache = isDefaultNamespace
                ? descriptorLookup.CodeValue
                : fullyQualifiedValue;
            _cacheProvider.SetCachedObject(idKey, codeValueToUseInCache);
        }

        private bool TryRefreshSingleTypeCacheById(string typeName, int id)
        {
            var typeLookup = _typeLookupProvider.GetSingleTypeLookupById(typeName, id);
            if (typeLookup == null)
            {
                return false;
            }
            UpdateTypeLookupCache(typeLookup);
            TryRefreshCache();

            return true;
        }

        private bool TryRefreshSingleTypeCacheByShortDescription(string typeName, string shortDescription)
        {
            var typeLookup = _typeLookupProvider.GetSingleTypeLookupByShortDescription(typeName, shortDescription);
            if (typeLookup == null)
            {
                return false;
            }
            UpdateTypeLookupCache(typeLookup);
            TryRefreshCache();

            return true;
        }

        private bool TryRefreshSingleDescriptorCacheById(string descriptorName, int id)
        {
            var descriptorLookup = _descriptorLookupProvider.GetSingleDescriptorLookupById(descriptorName, id);
            if (descriptorLookup == null)
            {
                return false;
            }
            UpdateDescriptorLookupCache(descriptorLookup);
            TryRefreshCache();

            return true;
        }

        private bool TryRefreshSingleDescriptorCache(string descriptorName)
        {
            var descriptorLookups = _descriptorLookupProvider.GetDescriptorLookupsByDescriptorName(descriptorName);
            if (descriptorLookups == null || !descriptorLookups.Any())
            {
                return false;
            }
            descriptorLookups.ToList().ForEach(UpdateDescriptorLookupCache);
            TryRefreshCache();

            return true;
        }

        private bool TryGetCachedId(string lookupKey, out int id)
        {
            _cacheLock.EnterReadLock();
            try
            {
                return _cacheProvider.TryGetCachedObject(lookupKey, out id);
            }
            finally
            {
                _cacheLock.ExitReadLock();
            }
        }

        private bool TryGetCachedValue(string lookupKey, out string value)
        {
            _cacheLock.EnterReadLock();
            try
            {
                return _cacheProvider.TryGetCachedObject(lookupKey, out value);
            }
            finally
            {
                _cacheLock.ExitReadLock();
            }
        }

        public int GetId(string typeName, string value)
        {
            // If no value is supplied, return default int value
            if (string.IsNullOrEmpty(value))
                return default(int);

            if (EdFiTypeEntitySpecification.IsEdFiTypeEntity(typeName))
            {
                return GetTypeId(typeName, value);
            }
            if (EdFiDescriptorEntitySpecification.IsEdFiDescriptorEntity(typeName))
            {
                return GetDescriptorId(typeName, value);
            }
            return default(int);
        }

        public string GetValue(string typeName, int id)
        {
            // If no id supplied, return the default string value
            if (id == default(int))
                return default(string);

            if (EdFiTypeEntitySpecification.IsEdFiTypeEntity(typeName))
            {
                return GetTypeShortDescription(typeName, id);
            }
            if (EdFiDescriptorEntitySpecification.IsEdFiDescriptorEntity(typeName))
            {
                return GetDescriptorCodeValue(typeName, id);
            }
            return default(string);
        }

        private int GetTypeId(string typeName, string shortDescription)
        {
            int id;
            var lookupKey = GetTypeLookupKeyByShortDescription(typeName, shortDescription);

            if (TryGetCachedId(lookupKey, out id))
                return id;

            if (!TryRefreshSingleTypeCacheByShortDescription(typeName, shortDescription))
            {
                return HandleMissingValue(typeName, shortDescription);
            }

            if (TryGetCachedId(lookupKey, out id))
                return id;
            return HandleMissingValue(typeName, shortDescription);
        }

        private int GetDescriptorId(string descriptorName, string codeValue)
        {
            int id;
            var lookupKey = GetDescriptorLookupKeyByCodeValue(descriptorName, codeValue);

            if (TryGetCachedId(lookupKey, out id))
                return id;

            if (!TryRefreshSingleDescriptorCache(descriptorName))
            {
                return HandleMissingValue(descriptorName, codeValue);
            }

            if (TryGetCachedId(lookupKey, out id))
                return id;
            return HandleMissingValue(descriptorName, codeValue);
        }

        private string GetTypeShortDescription(string typeName, int id)
        {
            string shortDescription;
            var lookupKey = GetTypeLookupKeyById(typeName, id);

            if (TryGetCachedValue(lookupKey, out shortDescription))
                return shortDescription;

            if (!TryRefreshSingleTypeCacheById(typeName, id))
            {
                return HandleMissingId(typeName, id);
            }

            if (TryGetCachedValue(lookupKey, out shortDescription))
                return shortDescription;
            return HandleMissingId(typeName, id);
        }

        private string GetDescriptorCodeValue(string descriptorName, int id)
        {
            string codeValue;
            var lookupKey = GetDescriptorLookupKeyById(descriptorName, id);

            if (TryGetCachedValue(lookupKey, out codeValue))
                return codeValue;

            if (!TryRefreshSingleDescriptorCacheById(descriptorName, id))
            {
                return HandleMissingId(descriptorName, id);
            }

            if (TryGetCachedValue(lookupKey, out codeValue))
                return codeValue;
            return HandleMissingId(descriptorName, id);
        }

        // This allows us to deal with the API tests that are simply generating random values for type keys.  Since we have
        // external constraints disabled in the database during these particular tests, we don't want to then enforce these
        // constraints in this cache.
        protected virtual int HandleMissingValue(string typeName, string value)
        {
            throw new ArgumentException(string.Format("Unable to resolve value '{0}' to an existing '{1}' resource.",
                value, typeName));
        }

        // This allows us to deal with the API tests that are simply generating random values for type keys.  Since we have
        // external constraints disabled in the database during these particular tests, we don't want to then enforce these
        // constraints in this cache.
        protected virtual string HandleMissingId(string typeName, int id)
        {
            throw new ArgumentException(string.Format("Unable to resolve id '{0}' to an existing '{1}' resource.", id,
                typeName));
        }
    }
}