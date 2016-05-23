using System;
using System.Linq;

namespace EdFi.Ods.Common.Specifications
{
    public class PrimaryRelationshipEntitySpecification
    {
        private static readonly string[] _validTypes = new[]
        {
            "StaffEducationOrganizationAssignmentAssociation", 
            "StaffEducationOrganizationEmploymentAssociation", 
            "StudentSchoolAssociation",
            "StudentParentAssociation"
        };

        public static string[] ValidTypes
        {
            get { return _validTypes; }
        }

        public static bool IsPrimaryRelationshipEntity(Type type)
        {
            return IsPrimaryRelationshipEntity(type.Name);
        }

        public static bool IsPrimaryRelationshipEntity(string typeName)
        {
            return _validTypes.Contains(typeName, StringComparer.CurrentCultureIgnoreCase);
        } 
    }
}