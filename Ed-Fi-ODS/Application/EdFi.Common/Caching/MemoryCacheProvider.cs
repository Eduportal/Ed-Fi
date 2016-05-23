using System;
using System.Linq;
using System.Runtime.Caching;
using System.Web.Caching;

namespace EdFi.Common.Caching
{
    public class MemoryCacheProvider : ICacheProvider
    {
        private readonly static object NullObject = new object();
        private static readonly CacheItemPolicy NeverExpireCacheItemPolicy = new CacheItemPolicy() { AbsoluteExpiration = DateTimeOffset.MaxValue};

        public void RemoveCachedObjects(string keyContains)
        {
            var keysToRemove = 
                (from kvp in _memoryCache 
                 where kvp.Key.ToLower().Contains(keyContains.ToLower()) 
                 select kvp.Key)
                 .ToList();

            foreach (var keyToRemove in keysToRemove)
            {
                RemoveCachedObject(keyToRemove);
            }
        }

        public void RemoveCachedObject(string keyName)
        {
            _memoryCache.Remove(keyName);
        }

        public bool TryGetCachedObject(string key, out object value)
        {
            value = _memoryCache.Get(key);

            if (value == NullObject)
            {
                value = null;
                return true;
            }

            return value != null;
        }

        public void SetCachedObject(string key, object value)
        {
            _memoryCache.Set(key, value ?? NullObject, NeverExpireCacheItemPolicy);
        }

        public void Insert(string key, object value, CacheDependency dependencies, DateTime absoluteExpiration,
            TimeSpan slidingExpiration)
        {
            throw new NotImplementedException("Implementation of this method would require monitoring of dependencies which is not supported by the 'MemoryCache' class.");
        }

        public void Insert(string key, object value, DateTime absoluteExpiration, TimeSpan slidingExpiration)
        {
            _memoryCache.Set(key, value ?? NullObject, 
                new CacheItemPolicy()
                {
                    AbsoluteExpiration = absoluteExpiration == DateTime.MaxValue 
                        ? DateTimeOffset.MaxValue : absoluteExpiration,
                    
                    SlidingExpiration = slidingExpiration
                });
        }

        private MemoryCache _memoryCache = MemoryCache.Default;

        public MemoryCache MemoryCache
        {
            get { return _memoryCache; }
            set { _memoryCache = value; }
        }
    }
}