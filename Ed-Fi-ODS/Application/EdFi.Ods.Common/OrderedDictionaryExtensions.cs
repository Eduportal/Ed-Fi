using System.Collections;
using System.Collections.Specialized;

namespace EdFi.Ods.Common
{
    public static class OrderedDictionaryExtensions
    {
        public static OrderedDictionary Clone(this OrderedDictionary source)
        {
            var newCopy = new OrderedDictionary();

            foreach (DictionaryEntry entry in source)
                newCopy.Add(entry.Key, entry.Value);

            return newCopy;
        }
    }
}