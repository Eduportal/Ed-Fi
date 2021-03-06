using System.Collections.Generic;
using EdFi.TestObjects;

namespace EdFi.Ods.Utilities.LoadGeneration.ReferenceValueBuilders
{
    public class StudentSchoolAttendanceEvent_SchoolReference_ValueBuilder : ReferenceValueBuilderBase, IContextSpecificReferenceValueBuilder
    {
        protected override bool IsHandled(BuildContext buildContext)
        {
            dynamic instance = buildContext.GetContainingInstance();

            return buildContext.Matches("StudentSchoolAttendanceEvent", "SchoolReference") &&
                   instance.sessionReference != null;
        }

        protected override IDictionary<string, object> GetPropertyValueConstraints(BuildContext buildContext)
        {
            dynamic instance = buildContext.GetContainingInstance();

            return new Dictionary<string, object>()
            {
                {"schoolId", instance.sessionReference.schoolId}
            };
        }
    }
}