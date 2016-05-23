using System;

namespace EdFi.Common.Caching
{
    public interface ICacheProvider
    {
        void RemoveCachedObjects(string keyContains);
        void RemoveCachedObject(string keyName);
        bool TryGetCachedObject(string key, out object value);
        void SetCachedObject(string keyName, object obj);
        void Insert(string key, object value, System.Web.Caching.CacheDependency dependencies, DateTime absoluteExpiration, TimeSpan slidingExpiration);
        void Insert(string key, object value, DateTime absoluteExpiration, TimeSpan slidingExpiration);
    }
}