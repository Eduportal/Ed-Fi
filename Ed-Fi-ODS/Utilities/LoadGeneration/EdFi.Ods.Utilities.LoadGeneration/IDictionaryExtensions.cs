using System;
using System.Collections.Generic;
using log4net;

namespace EdFi.Ods.Utilities.LoadGeneration
{
    public static class IDictionaryExtensions
    {
        private static ILog _logger = LogManager.GetLogger(typeof(IDictionaryExtensions));

        /// <summary>
        /// Merges the source dictionary into the target, overlaying values in the target that have the same keys.
        /// </summary>
        /// <typeparam name="TKey">The <see cref="Type"/> of the key for the dictionary.</typeparam>
        /// <typeparam name="TValue">The <see cref="Type"/> of the values in the dictionary.</typeparam>
        /// <param name="target">The dictionary into which entries will be merged.</param>
        /// <param name="source">The dictionary from which entries will be merged.</param>
        public static void MergeRange<TKey, TValue>(this IDictionary<TKey, TValue> target, IDictionary<TKey, TValue> source)
        {
            foreach (var kvp in source)
            {
                //target[kvp.Key] = kvp.Value; // TODO: See Ignore with Exploration #1
                target.TrySetValue(kvp.Key, kvp.Value);
            }
        }

        // TODO: The TrySetValue should be removed.  BuildContextConstraints should probably be propogated everywhere.  Lock down key matching semantics on the property bag.

        // TODO: Keep this, or move to BuildContextConstraints class?
        public static bool TrySetValue<TKey, TValue>(this IDictionary<TKey, TValue> target, TKey key, TValue value)
        {
            TValue existingValue;

            if (target.TryGetValue(key, out existingValue))
            {
                if (!value.Equals(existingValue))
                    _logger.WarnFormat("Could not overwrite value for '{0}' from '{1}' to '{2}' (it already exists).",
                        key, existingValue, value);

                return false;
            }

            target.Add(key, value);
            return true;
        }
    }
}
