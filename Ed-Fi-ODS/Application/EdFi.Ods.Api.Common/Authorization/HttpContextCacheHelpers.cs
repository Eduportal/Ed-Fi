using System.Collections.Generic;
using System.Web;

namespace EdFi.Ods.Api.Common.Authorization
{
    public static class HttpContextCacheHelpers
    {
        public static void CacheInRequest<T>(this HttpContext context, string key, T value)
        {
            context.Items.Remove(key);
            context.Items.Add(key, value);
        }

        // TODO: GKM - remove
        //public static bool RequestCacheContains<T>(this HttpContext context, string key, T value)
        //{
        //    if (context.Items.Contains(key))
        //    {
        //        return Equals(RetrieveFromRequestCache<T>(context, key), value);
        //    }
        //    return false;
        //}

        public static T RetrieveFromRequestCache<T>(this HttpContext context, string key)
        {
            if (context.Items.Contains(key))
            {
                return (T) context.Items[key];
            }
            throw new KeyNotFoundException(string.Format("{0} was not a key in the request cache", key));
        }

        public static bool RequestCacheContainsValueForKey(this HttpContext context, string key)
        {
            if (!context.Items.Contains(key)) return false;
            return context.Items[key] != null;
        }
    }
}