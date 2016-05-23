using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EdFi.Ods.Utilities.LoadGeneration
{
    public static class ReflectionExtensions
    {
        public static bool HasProperty(this object resource, string propertyName)
        {
            var sourceProperty = GetProperty(resource, propertyName);

            // If we dont have a property with that name then we just return false.
            return (sourceProperty != null);
        }

        public static bool HasPropertyThatMatchesValue(this object resource, string propertyName, object propertyValue)
        {
            var sourceProperty = GetProperty(resource, propertyName);

            // If we dont have a property with that name then we just return false.
            if (sourceProperty == null)
                return false;

            var sourcePropertyValue = sourceProperty.GetValue(resource, null);

            return sourcePropertyValue.Equals(propertyValue);
        }

        private static PropertyInfo GetProperty(object resource, string propertyName)
        {
            var sourceProperty = resource.GetType().GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            return sourceProperty;
        }

        public static IEnumerable<object> WhereObjectsMatchPropertyValueConstraints(this IEnumerable<object> resources, IDictionary<string, object> propertyValueConstraints)
        {
            return 
                (from resource in resources 
                    where propertyValueConstraints.All(constraint => 
                        !resource.HasProperty(constraint.Key) 
                        || resource.HasPropertyThatMatchesValue(constraint.Key, constraint.Value))
                    select resource)
                    .ToList();
        }
    }
}