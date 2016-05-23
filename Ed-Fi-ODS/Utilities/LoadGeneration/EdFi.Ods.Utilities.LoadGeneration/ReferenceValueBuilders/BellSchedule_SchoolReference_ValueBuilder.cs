using System.Collections.Generic;
using EdFi.TestObjects;

namespace EdFi.Ods.Utilities.LoadGeneration.ReferenceValueBuilders
{
    public class BellSchedule_SchoolReference_ValueBuilder : ReferenceValueBuilderBase, IContextSpecificReferenceValueBuilder
    {
        protected override bool IsHandled(BuildContext buildContext)
        {
            if (!buildContext.Matches("BellSchedule", "SchoolReference"))
                return false;

            dynamic bellSchedule = buildContext.GetContainingInstance();

            // Don't perform special handling of this request if CalendarDateReference has not been set (let the StandardReferenceValueBuilder build it)
            return bellSchedule.calendarDateReference != null;
        }

        protected override IDictionary<string, object> GetPropertyValueConstraints(BuildContext buildContext)
        {
            dynamic bellSchedule = buildContext.GetContainingInstance();

            return new Dictionary<string, object>
                {
                    {"schoolId", bellSchedule.calendarDateReference.educationOrganizationId }
                };
        }
    }
}