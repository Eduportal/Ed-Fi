using System.Collections.Generic;
using EdFi.TestObjects;

namespace EdFi.Ods.Utilities.LoadGeneration.ReferenceValueBuilders
{
    public class StudentAssessmentStudentObjectiveAssessment_ObjectiveAssessmentReference_ValueBuilder : ReferenceValueBuilderBase, IContextSpecificReferenceValueBuilder
    {
        protected override bool IsHandled(BuildContext buildContext)
        {
            return buildContext.Matches("StudentAssessmentStudentObjectiveAssessment", "ObjectiveAssessmentReference");
        }
        protected override IDictionary<string, object> GetPropertyValueConstraints(BuildContext buildContext)
        {
            dynamic parent = buildContext.GetParentInstance();
            return new Dictionary<string, object>
            {
                {"assessmentTitle", parent.assessmentReference.title},
                {"academicSubjectDescriptor", parent.assessmentReference.academicSubjectDescriptor},
                {"assessedGradeLevelDescriptor", parent.assessmentReference.assessedGradeLevelDescriptor},
                {"version", parent.assessmentReference.version},
            };
        }
    }
}