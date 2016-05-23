// ****************************************************************************
//  Copyright (C) 2014 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EdFi.Ods.Utilities.LoadGeneration._Extensions
{
    public static class TypeExtensions
    {
        // TODO: Needs unit test
        public static bool IsCustomClass(this Type type)
        {
            return type.IsClass && !type.Namespace.StartsWith("System");
        }

        // TODO: Needs unit test
        public static bool IsPersonUniqueIdProperty(this PropertyInfo p)
        {
            return p.Name.Equals("StudentUniqueId", StringComparison.InvariantCultureIgnoreCase)
                   || p.Name.Equals("StaffUniqueId", StringComparison.InvariantCultureIgnoreCase)
                   || p.Name.Equals("ParentUniqueId", StringComparison.InvariantCultureIgnoreCase);
        }

        // TODO: Needs unit test
        public static IEnumerable<PropertyInfo> GetCustomClassProperties(this Type type)
        {
            if (type == null)
                return Enumerable.Empty<PropertyInfo>();

            var properties = type.GetProperties();

            return properties.Where(p => p.PropertyType.IsCustomClass());
        }

        // TODO: Needs unit test
        public static IEnumerable<PropertyInfo> GetEducationOrganizationIdentifierProperties(this Type containingType)
        {
            if (containingType == null)
                return Enumerable.Empty<PropertyInfo>();

            var properties = containingType.GetProperties();

            // TODO: Needs unit test showing prioritization of properties (SchoolId, LocalEducationAgencyId, then EdOrgId)
            var educationOrganizationIdProperties =
                (properties.Where(p => p.Name.Equals("SchoolId", StringComparison.InvariantCultureIgnoreCase)))
                    .Concat(properties.Where(p => p.Name.Equals("LocalEducationAgencyId", StringComparison.InvariantCultureIgnoreCase)))
                    .Concat(properties.Where(p => p.Name.Equals("EducationOrganizationId", StringComparison.InvariantCultureIgnoreCase)));

            return educationOrganizationIdProperties;
        }

        // TODO: Needs unit test
        public static IEnumerable<PropertyInfo> GetPersonUniqueIdProperties(this Type containingType)
        {
            var properties = containingType.GetProperties();

            var personUniqueIdProperties = properties.Where(IsPersonUniqueIdProperty);

            return personUniqueIdProperties;
        }

        /// <summary>
        /// Gets an enumerable collection of Key/Value pairs representing education organizations in the hierarchy 
        /// from bottom to top (i.e. school, local education agency and then general education organization id).
        /// </summary>
        /// <param name="values">The values to process.</param>
        /// <returns>The ordered education organization identifiers.</returns>
        public static IEnumerable<KeyValuePair<string, object>> GetOrderedEducationOrganizationIdentifierValues(
            this IDictionary<string, object> values)
        {
            if (values == null)
                return Enumerable.Empty<KeyValuePair<string, object>>();

            var pairs = values.AsEnumerable();

            var educationOrganizationIdValues =
                (pairs.Where(p => p.Key.Equals("SchoolId", StringComparison.InvariantCultureIgnoreCase)))
                    .Concat(pairs.Where(p => p.Key.Equals("LocalEducationAgencyId", StringComparison.InvariantCultureIgnoreCase)))
                    .Concat(pairs.Where(p => p.Key.Equals("EducationOrganizationId", StringComparison.InvariantCultureIgnoreCase)));

            return educationOrganizationIdValues;
        }
    }
}