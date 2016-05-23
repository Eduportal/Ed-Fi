using System.Collections.Generic;
using EdFi.TestObjects;

namespace EdFi.Ods.Utilities.LoadGeneration.ReferenceValueBuilders
{
    public class StaffEducationOrganizationAssignmentAssociation_StaffReference_ValueBuilder : ReferenceValueBuilderBase, IContextSpecificReferenceValueBuilder
    {
        protected override bool IsHandled(BuildContext buildContext)
        {
            dynamic instance = buildContext.GetContainingInstance();

            return buildContext.Matches("StaffEducationOrganizationAssignmentAssociation", "StaffReference")
                   && instance.employmentStaffEducationOrganizationEmploymentAssociationReference != null;
        }

        protected override IDictionary<string, object> GetPropertyValueConstraints(BuildContext buildContext)
        {
            dynamic instance = buildContext.GetContainingInstance();
            
            return new Dictionary<string, object>
            {
                {"staffUniqueId", instance.employmentStaffEducationOrganizationEmploymentAssociationReference.staffUniqueId },
            };
        }
    }
}