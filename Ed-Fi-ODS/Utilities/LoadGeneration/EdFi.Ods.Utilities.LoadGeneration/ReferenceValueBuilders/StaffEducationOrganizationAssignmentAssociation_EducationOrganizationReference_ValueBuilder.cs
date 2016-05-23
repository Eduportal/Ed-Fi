using System.Collections.Generic;
using EdFi.TestObjects;

namespace EdFi.Ods.Utilities.LoadGeneration.ReferenceValueBuilders
{
    public class StaffEducationOrganizationAssignmentAssociation_EducationOrganizationReference_ValueBuilder : ReferenceValueBuilderBase, IContextSpecificReferenceValueBuilder
    {
        protected override bool IsHandled(BuildContext buildContext)
        {
            dynamic instance = buildContext.GetContainingInstance();

            return buildContext.Matches("StaffEducationOrganizationAssignmentAssociation", "EducationOrganizationReference")
                   && instance.employmentStaffEducationOrganizationEmploymentAssociationReference != null;
        }

        protected override IDictionary<string, object> GetPropertyValueConstraints(BuildContext buildContext)
        {
            dynamic instance = buildContext.GetContainingInstance();

            return new Dictionary<string, object>
            {
                { "educationOrganizationId", instance.employmentStaffEducationOrganizationEmploymentAssociationReference.educationOrganizationId },
            };
        }
    }
}