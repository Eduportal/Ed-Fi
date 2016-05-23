using System.Collections.Generic;
using EdFi.TestObjects;

namespace EdFi.Ods.Utilities.LoadGeneration.ReferenceValueBuilders
{
    public class StudentAcademicRecordReportCard_ReportCardReference_ValueBuilder : ReferenceValueBuilderBase, IContextSpecificReferenceValueBuilder
    {
        protected override bool IsHandled(BuildContext buildContext)
        {
            return buildContext.Matches("StudentAcademicRecordReportCard", "ReportCardReference");
        }

        protected override IDictionary<string, object> GetPropertyValueConstraints(BuildContext buildContext)
        {
            dynamic parent = buildContext.GetParentInstance();

            return new Dictionary<string, object>
            {
                {"studentUniqueId", parent.studentReference.studentUniqueId},
                {"educationOrganizationId", parent.educationOrganizationReference.educationOrganizationId},
            };
        }
    }
}