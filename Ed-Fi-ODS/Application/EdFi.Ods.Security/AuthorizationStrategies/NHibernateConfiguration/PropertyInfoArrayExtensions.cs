using System;
using System.Linq;
using System.Reflection;
using EdFi.Common.Extensions;

namespace EdFi.Ods.Security.AuthorizationStrategies.NHibernateConfiguration
{
    /// <summary>
    /// Provides methods for evaluating an array of <see cref="PropertyInfo"/> objects.
    /// </summary>
    public static class PropertyInfoArrayExtensions
    {
        /// <summary>
        /// Indicates whether any of the properties has the specified name (case-insensitive matching).
        /// </summary>
        /// <param name="properties">The array of properties to search.</param>
        /// <param name="caseInsensitivePropertyName">The case-insensitive name of the property for which to search.</param>
        /// <returns><b>true</b> if the property exists; otherwise <b>false</b>.</returns>
        public static bool HasPropertyNamed(
            this PropertyInfo[] properties,
            string caseInsensitivePropertyName)
        {
            return properties.Any(x => x.Name.EqualsIgnoreCase(caseInsensitivePropertyName));
        }
    }
}