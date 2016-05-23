using System.Collections.Generic;
using EdFi.TestObjects;

namespace EdFi.Ods.Utilities.LoadGeneration.ReferenceValueBuilders
{
    public class StudentLearningObjective_StudentProgramAssociationReference_ValueBuilder : ReferenceValueBuilderBase, IContextSpecificReferenceValueBuilder
    {
        protected override bool IsHandled(BuildContext buildContext)
        {
            return buildContext.Matches("StudentLearningObjective", "StudentProgramAssociationReference");
        }

        protected override IDictionary<string, object> GetPropertyValueConstraints(BuildContext buildContext)
        {
            return null;
        }

        public override ValueBuildResult TryBuild(BuildContext buildContext)
        {
            if (!IsHandled(buildContext))
                return ValueBuildResult.NotHandled;

            dynamic instance = buildContext.GetContainingInstance();

            // skip building if the other mutual relation has been built already
            if (instance.studentSectionAssociationReference != null)
                return ValueBuildResult.Skip(buildContext.LogicalPropertyPath);

            var thisProgress = ResourceCountManager.GetProgressForResource("StudentProgramAssociation");
            var otherProgress = ResourceCountManager.GetProgressForResource("StudentSectionAssociation");

            // skip if this progress is ahead or equal of the other mutual relation
            if (thisProgress >= otherProgress)
                return ValueBuildResult.Skip(buildContext.LogicalPropertyPath);

            return base.TryBuild(buildContext);
        }
    }
}