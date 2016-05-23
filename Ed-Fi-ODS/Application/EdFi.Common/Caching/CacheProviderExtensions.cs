using System.Collections.Generic;

namespace EdFi.Common.Caching
{
    public static class CacheProviderExtensions
    {
        public static bool TryGetCachedObject<T>(this ICacheProvider cacheProvider, string key, out T value)
        {
            object objectValue;
            if (cacheProvider.TryGetCachedObject(key, out objectValue) && objectValue is T)
            {
                value = (T)objectValue;
            }
            else
            {
                value = default(T);
            }
            return !EqualityComparer<T>.Default.Equals(value, default(T));
        }
    }
}
