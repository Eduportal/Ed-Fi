using System.Collections.Generic;
using EdFi.TestObjects;

namespace EdFi.Ods.Utilities.LoadGeneration.ReferenceValueBuilders
{
    public class StudentCompetencyObjective_StudentProgramAssociationReference_ValueBuilder : ReferenceValueBuilderBase, IContextSpecificReferenceValueBuilder
    {
        protected override bool IsHandled(BuildContext buildContext)
        {
            return buildContext.Matches("StudentCompetencyObjective", "StudentProgramAssociationReference");
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

            // ----------------------------------------------------------------------------------------------------
            // TODO: There's a bug in the REST API that only checks the program side of the relationship for the StudentUniqueId
            // In order to get this to persist correctly, we'll need to ALWAYS generate the program relationship
            // ----------------------------------------------------------------------------------------------------
            var thisProgress = ResourceCountManager.GetProgressForResource("StudentProgramAssociation");
            var otherProgress = ResourceCountManager.GetProgressForResource("StudentSectionAssociation");

            // skip if this progress is ahead or equal of the other mutual relation
            //if (thisProgress >= otherProgress)
            //    return ValueBuildResult.Skip(buildContext.LogicalPropertyPath);
            // ----------------------------------------------------------------------------------------------------

            return base.TryBuild(buildContext);
        }
    }
}