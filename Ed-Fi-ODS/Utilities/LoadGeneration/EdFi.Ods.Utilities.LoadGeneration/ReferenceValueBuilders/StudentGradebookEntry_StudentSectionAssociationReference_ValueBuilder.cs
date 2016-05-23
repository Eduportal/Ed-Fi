using System.Collections.Generic;
using EdFi.TestObjects;

namespace EdFi.Ods.Utilities.LoadGeneration.ReferenceValueBuilders
{
    public class StudentGradebookEntry_StudentSectionAssociationReference_ValueBuilder : ReferenceValueBuilderBase, IContextSpecificReferenceValueBuilder
    {
        protected override bool IsHandled(BuildContext buildContext)
        {
            dynamic instance = buildContext.GetContainingInstance();

            return buildContext.Matches("StudentGradebookEntry", "StudentSectionAssociationReference") 
                    && instance.gradebookEntryReference != null;
        }

        protected override IDictionary<string, object> GetPropertyValueConstraints(BuildContext buildContext)
        {
            dynamic instance = buildContext.GetContainingInstance();

            return new Dictionary<string, object>
            {
                {"schoolId",                    instance.gradebookEntryReference.schoolId},
                {"classPeriodName",             instance.gradebookEntryReference.classPeriodName},
                {"classroomIdentificationCode", instance.gradebookEntryReference.classroomIdentificationCode},
                {"localCourseCode",             instance.gradebookEntryReference.localCourseCode},
                {"termType",                    instance.gradebookEntryReference.termType},
                {"schoolYear",                  instance.gradebookEntryReference.schoolYear},
            };
        }
    }
}