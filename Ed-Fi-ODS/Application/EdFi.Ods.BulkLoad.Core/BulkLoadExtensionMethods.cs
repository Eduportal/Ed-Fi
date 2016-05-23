using System;
using System.Collections.Generic;
using System.Linq;

namespace EdFi.Ods.BulkLoad.Core
{
    public static class BulkLoadExtensionMethods
    {
        public static IDictionary<Type, ILoadAggregates> ToAggregateDictionary(this IEnumerable<ILoadAggregates> loaders)
        {
            var loaderDictionary = new Dictionary<Type, ILoadAggregates>();
            foreach (var loader in loaders.Where(loader => !loaderDictionary.ContainsKey(loader.GetAggregateType())))
            {
                loaderDictionary.Add(loader.GetAggregateType(), loader);
            }
            return loaderDictionary;
        }

        public static IEnumerable<string> GetAggregateNames(this IDictionary<Type, ILoadAggregates> loaderDictionary)
        {
            return loaderDictionary.Select(loader => loader.Key.Name).ToList();
        }

        public static ILoadAggregates GetAndRemoveLoaderFor(this IDictionary<Type, ILoadAggregates> loaderDictionary, Type loaderType)
        {
            ILoadAggregates loader;
            if (!loaderDictionary.TryGetValue(loaderType, out loader))
                throw new KeyNotFoundException(string.Format("Loader dictionary does not contain an entry for the type '{0}'. Please verify that an aggregate loader (ILoadAggregates) exists for this type.", loaderType.Name));

            loaderDictionary.Remove(loaderType);
            return loader;
        }
    }
}
