using System;
using System.Collections.Generic;
using EdFi.TestObjects;

namespace EdFi.Ods.Utilities.LoadGeneration.ReferenceValueBuilders
{
    public class BellSchedule_CalendarDateReference_ValueBuilder : ReferenceValueBuilderBase, IContextSpecificReferenceValueBuilder
    {
        protected override bool IsHandled(BuildContext buildContext)
        {
            return buildContext.Matches("BellSchedule", "CalendarDateReference");
        }

        protected override IDictionary<string, object> GetPropertyValueConstraints(BuildContext buildContext)
        {
            dynamic bellSchedule = buildContext.GetContainingInstance();

            if (bellSchedule.schoolReference == null)
            {
                return new Dictionary<string, object>()
                {
                    {"educationOrganizationId", BuildConcreteEducationOrganizationId("SchoolId") }
                };
            }

            return new Dictionary<string, object>()
            {
                {"educationOrganizationId", bellSchedule.schoolReference.schoolId}
            };
        }
    }
}