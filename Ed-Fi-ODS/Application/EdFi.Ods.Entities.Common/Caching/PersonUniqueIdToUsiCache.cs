using System;
using System.Runtime.Caching;
using EdFi.Common.Caching;
using EdFi.Ods.Entities.Common.IdentityValueMappers;
using EdFi.Ods.Entities.Common.Providers;

namespace EdFi.Ods.Entities.Common.Caching
{
    public class PersonUniqueIdToUsiCache : IPersonUniqueIdToUsiCache
    {
        private readonly ICacheProvider _cacheProvider;
        private readonly IEdFiOdsInstanceIdentificationProvider _edFiOdsInstanceIdentificationProvider;
        private readonly IUniqueIdToUsiValueMapper _uniqueIdToUsiValueMapper;

        private readonly CacheItemPolicy _cacheItemPolicy;
        private const string CacheKeyPrefix = "PersonIdentifiers.";

        public PersonUniqueIdToUsiCache(
            ICacheProvider cacheProvider,
            IEdFiOdsInstanceIdentificationProvider edFiOdsInstanceIdentificationProvider,
            IUniqueIdToUsiValueMapper uniqueIdToUsiValueMapper,
            int cacheExpirationMinutes = 10)
        {
            _cacheProvider = cacheProvider;
            _edFiOdsInstanceIdentificationProvider = edFiOdsInstanceIdentificationProvider;
            _uniqueIdToUsiValueMapper = uniqueIdToUsiValueMapper;
            _cacheItemPolicy = new CacheItemPolicy {SlidingExpiration = TimeSpan.FromMinutes(cacheExpirationMinutes)};
        }

        /// <summary>
        /// Gets or sets a static delegate to obtain the cache.
        /// </summary>
        /// <remarks>This method exists to serve the cache to the NHibernate-generated entities in a way
        /// that does not require IoC component resolution, for performance reasons.</remarks>
        public static Func<IPersonUniqueIdToUsiCache> GetCache = () => null;

        /// <summary>
        /// Gets the externally defined UniqueId for the specified type of person and the ODS-specific surrogate identifier.
        /// </summary>
        /// <param name="personTypeName">The type of the person (e.g. Staff, Student, Parent).</param>
        /// <param name="usi">The integer-based identifier for the specified representation of the person, 
        /// specific to a particular ODS database instance.</param>
        /// <returns>The UniqueId value assigned to the person if found; otherwise <b>null</b>.</returns>
        public string GetUniqueId(string personTypeName, int usi)
        {
            if (usi == default(int)) return default(string);
            var key = string.Format("{0}{1}_uniqueid_by_usi_{2}_{3}", CacheKeyPrefix, personTypeName, GetUsiKeyTokenContext(), usi);
            object obj;
            if (_cacheProvider.TryGetCachedObject(key, out obj))
                return ((string)obj);

            var valueMap = _uniqueIdToUsiValueMapper.GetUniqueId(personTypeName, usi);

            // Save the primary value
            if (valueMap.UniqueId != null)
                _cacheProvider.Insert(key, valueMap.UniqueId, DateTime.MaxValue, _cacheItemPolicy.SlidingExpiration);

            return valueMap.UniqueId;
        }

        /// <summary>
        /// Gets the ODS-specific integer identifier for the specified type of person and their UniqueId value.
        /// </summary>
        /// <param name="personTypeName">The type of the person (e.g. Staff, Student, Parent).</param>
        /// <param name="uniqueId">The UniqueId value associated with the person.</param>
        /// <returns>The ODS-specific integer identifier for the specified type of representation of 
        /// the person if found; otherwise 0.</returns>
        public int GetUsi(string personTypeName, string uniqueId)
        {
            var usi = GetUsi(personTypeName, uniqueId, false);
            return usi.GetValueOrDefault();
        }

        /// <summary>
        /// Gets the ODS-specific integer identifier for the specified type of person and their UniqueId value.
        /// </summary>
        /// <param name="personTypeName">The type of the person (e.g. Staff, Student, Parent).</param>
        /// <param name="uniqueId">The UniqueId value associated with the person.</param>
        /// <returns>The ODS-specific integer identifier for the specified type of representation of 
        /// the person if found; otherwise <b>null</b>.</returns>
        public int? GetUsiNullable(string personTypeName, string uniqueId)
        {
            var usi = GetUsi(personTypeName, uniqueId, true);

            return usi.HasValue && usi.Value == default(int) ? null : usi;
        }

        private int? GetUsi(string personTypeName, string uniqueId, bool isNullable)
        {
            if (string.IsNullOrWhiteSpace(uniqueId)) return isNullable ? default(int?) : default(int);

            var key = string.Format("{0}{1}_usi_{2}_by_uniqueid_{3}", CacheKeyPrefix, personTypeName, GetUsiKeyTokenContext(), uniqueId);
            object obj;
            if (_cacheProvider.TryGetCachedObject(key, out obj))
            {
                var value = Convert.ToInt32(obj);
                if (value != default(int)) return value;
            }

            var valueMap = _uniqueIdToUsiValueMapper.GetUsi(personTypeName, uniqueId);

            // Save the primary value
            if (valueMap.Usi != default(int))
                _cacheProvider.Insert(key, valueMap.Usi, DateTime.MaxValue, _cacheItemPolicy.SlidingExpiration);

            // Handle opportunistic cache value assignment of alternate value
            if (valueMap.Id != default(Guid))
            {
                var extraEntyKey = string.Format("{0}{1}_id_by_uniqueid_{2}", CacheKeyPrefix, personTypeName, uniqueId);

                if (!_cacheProvider.TryGetCachedObject(extraEntyKey, out obj))
                    _cacheProvider.Insert(extraEntyKey, valueMap.Id, DateTime.MaxValue, _cacheItemPolicy.SlidingExpiration);
            }

            return valueMap.Usi;
        }

        private string GetUsiKeyTokenContext()
        {
            return string.Format("from_{0}", _edFiOdsInstanceIdentificationProvider.GetInstanceIdentification());
        }
    }
}