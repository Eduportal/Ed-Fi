using System.Collections.Generic;
using EdFi.TestObjects;

namespace EdFi.Ods.Utilities.LoadGeneration.ReferenceValueBuilders
{
    public class StudentGradebookEntry_GradebookEntryReference_ValueBuilder : ReferenceValueBuilderBase, IContextSpecificReferenceValueBuilder
    {
        protected override bool IsHandled(BuildContext buildContext)
        {
            dynamic instance = buildContext.GetContainingInstance();

            return buildContext.Matches("StudentGradebookEntry", "GradebookEntryReference") 
                    && instance.studentSectionAssociationReference != null;
        }

        protected override IDictionary<string, object> GetPropertyValueConstraints(BuildContext buildContext)
        {
            dynamic instance = buildContext.GetContainingInstance();

            return new Dictionary<string, object>
            {
                {"schoolId",                    instance.studentSectionAssociationReference.schoolId},
                {"classPeriodName",             instance.studentSectionAssociationReference.classPeriodName},
                {"classroomIdentificationCode", instance.studentSectionAssociationReference.classroomIdentificationCode},
                {"localCourseCode",             instance.studentSectionAssociationReference.localCourseCode},
                {"termType",                    instance.studentSectionAssociationReference.termType},
                {"schoolYear",                  instance.studentSectionAssociationReference.schoolYear},
            };
        }
    }
}