using System.Collections.Generic;
using EdFi.TestObjects;

namespace EdFi.Ods.Utilities.LoadGeneration.ReferenceValueBuilders
{
    public class StaffEducationOrganizationAssignmentAssociation_StaffEducationOrganizationEmploymentAssociationReference_ValueBuilder : ReferenceValueBuilderBase, IContextSpecificReferenceValueBuilder
    {
        protected override bool IsHandled(BuildContext buildContext)
        {
            dynamic instance = buildContext.GetContainingInstance();

            return buildContext.Matches("StaffEducationOrganizationAssignmentAssociation", "StaffEducationOrganizationEmploymentAssociationReference")
                   && (instance.staffReference != null || instance.educationOrganizationReference != null);
        }

        protected override IDictionary<string, object> GetPropertyValueConstraints(BuildContext buildContext)
        {
            dynamic instance = buildContext.GetContainingInstance();

            var constraints = new Dictionary<string, object>();

            
            if (instance.staffReference != null)
                constraints.Add("staffUniqueId", instance.staffReference.staffUniqueId);
           
            if (instance.educationOrganizationReference != null)
                constraints.Add("educationOrganizationId", instance.educationOrganizationReference.educationOrganizationId);

            return constraints;

        }
    }
}