using System.Collections.Generic;
using EdFi.TestObjects;

namespace EdFi.Ods.Utilities.LoadGeneration.ReferenceValueBuilders
{
    public class ReportCardStudentLearningObjective_StudentLearningObjectiveReference_ValueBuilder : ReferenceValueBuilderBase, IContextSpecificReferenceValueBuilder
    {
        protected override bool IsHandled(BuildContext buildContext)
        {
            return buildContext.Matches("ReportCardStudentLearningObjective", "StudentLearningObjectiveReference");
        }
        protected override IDictionary<string, object> GetPropertyValueConstraints(BuildContext buildContext)
        {
            dynamic parent = buildContext.GetParentInstance();

            return new Dictionary<string, object>
            {
                {"studentUniqueId", parent.studentReference.studentUniqueId},
                {"gradingPeriodEducationOrganizationId", parent.gradingPeriodReference.educationOrganizationId},
                {"gradingPeriodDescriptor", parent.gradingPeriodReference.descriptor},
                {"gradingPeriodBeginDate", parent.gradingPeriodReference.beginDate},
            };
        }
    }
}