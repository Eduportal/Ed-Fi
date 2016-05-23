using System;
using EdFi.Ods.Common;

namespace EdFi.Ods.Security.Authorization
{
    /// <summary>
    /// Provides extension methods for building a resource's URI for authorization purposes.
    /// </summary>
    public static class IHasIdentifierExtensions
    {
        private const string EdFiOdsResourceBaseUri = "http://ed-fi.org/ods/resources/";

        /// <summary>
        /// Gets the resource URI for the resource, based on convention of the Type's name, camel-cased with an ed-fi.org URI base value.
        /// </summary>
        /// <param name="resource">The resource model for which to build the resource URI.</param>
        /// <returns>The resource URI.</returns>
        public static string GetResourceName(this IHasIdentifier resource)
        {
            string resourceName = resource.GetType().Name;

            return ConvertToCamelCase(resourceName);
        }

        /// <summary>
        /// Gets the resource URI for the resource Type, based on convention of the Type's name, camel-cased with an ed-fi.org URI base value.
        /// </summary>
        /// <param name="resourceType">The resource Type for which to build the resource URI.</param>
        /// <returns>The resource URI.</returns>
        public static string GetResourceName(this Type resourceType)
        {
            string resourceName = resourceType.Name;

            return ConvertToCamelCase(resourceName);
        }

        private static string ConvertToCamelCase(string resourceName)
        {
            // Special handling for single character type names (which really shouldn't happen)
            if (resourceName.Length == 1)
            {
                return EdFiOdsResourceBaseUri
                       + resourceName.ToLower();
            }

            return EdFiOdsResourceBaseUri
                   + resourceName.Substring(0, 1).ToLower()
                   + resourceName.Substring(1);
        }
    }
}