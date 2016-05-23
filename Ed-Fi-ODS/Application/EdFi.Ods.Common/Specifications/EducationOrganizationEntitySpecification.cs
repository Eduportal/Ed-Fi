using System;
using System.Linq;
using EdFi.Common.Extensions;

namespace EdFi.Ods.Common.Specifications
{
    // TODO: GKM - This is not an extensible structure.  For extensiblity around EdOrg types, we'll eventually want to introduce an interface for this (CoR?), for defining what the valid EdOrgs are, their relationships, and their identifiers.
    public class EducationOrganizationEntitySpecification
    {
        private static readonly string[] _validEducationOrganizationTypes =
        {
            "EducationOrganization", // Abstract base type
            "StateEducationAgency", 
            "EducationServiceCenter", 
            "EducationOrganizationNetwork",
            "LocalEducationAgency",
            "School"
        };

        public static string[] ValidEducationOrganizationTypes
        {
            get { return _validEducationOrganizationTypes; }
        }

        public static bool IsEducationOrganizationEntity(Type type)
        {
            return IsEducationOrganizationEntity(type.Name);
        }

        public static bool IsEducationOrganizationEntity(string typeName)
        {
            return _validEducationOrganizationTypes.Contains(typeName, StringComparer.CurrentCultureIgnoreCase);
        }

        public static bool IsEducationOrganizationIdentifier(string propertyName)
        {
            string entityName;

            // TODO: Embedded convention (EdOrg identifiers ends with "Id")
            if (propertyName.TryTrimSuffix("Id", out entityName))
                return IsEducationOrganizationEntity(entityName);

            return false;
        }
    }
}