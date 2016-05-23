using System.Collections.Generic;
using EdFi.TestObjects;

namespace EdFi.Ods.Utilities.LoadGeneration.ReferenceValueBuilders
{
    public class ObjectiveAssessmentLearningObjective_LearningObjectiveReference_ValueBuilder : ReferenceValueBuilderBase, IContextSpecificReferenceValueBuilder
    {
        protected override bool IsHandled(BuildContext buildContext)
        {
            return buildContext.Matches("ObjectiveAssessmentLearningObjective", "LearningObjectiveReference");
        }

        protected override IDictionary<string, object> GetPropertyValueConstraints(BuildContext buildContext)
        {
            dynamic parent = buildContext.GetParentInstance();

            return new Dictionary<string, object>
            {
                {"academicSubjectDescriptor", parent.assessmentReference.academicSubjectDescriptor},
                {"assessedGradeLevelDescriptor", parent.assessmentReference.assessedGradeLevelDescriptor},
            };
        }
    }
}