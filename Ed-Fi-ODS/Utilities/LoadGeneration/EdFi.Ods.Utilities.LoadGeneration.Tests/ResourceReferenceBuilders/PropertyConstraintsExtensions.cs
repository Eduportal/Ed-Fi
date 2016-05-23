using System.Collections.Generic;
using Should.Core.Exceptions;

namespace EdFi.Ods.Utilities.LoadGeneration.Tests.ResourceReferenceBuilders
{
    internal static class PropertyConstraintsExtensions
    {
        public static bool ShouldContain(this IDictionary<string, object> propertyConstraints, string key, object expectedValue)
        {
            object actualValue;

            bool contains =
                propertyConstraints.TryGetValue(key, out actualValue) 
                && actualValue.Equals(expectedValue);

            if (!contains)
                throw new AssertException(string.Format(
                    "Property constraints did not contain an entry with a key of '{0}' and a value of '{1}'.", 
                    key, expectedValue));
            
            return true;
        }
    }
}