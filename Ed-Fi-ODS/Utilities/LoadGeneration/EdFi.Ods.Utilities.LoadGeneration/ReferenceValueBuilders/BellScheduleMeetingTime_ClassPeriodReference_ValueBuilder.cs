using System;
using System.Collections.Generic;
using EdFi.TestObjects;

namespace EdFi.Ods.Utilities.LoadGeneration.ReferenceValueBuilders
{
    public class BellScheduleMeetingTime_ClassPeriodReference_ValueBuilder : ReferenceValueBuilderBase, IContextSpecificReferenceValueBuilder
    {
        protected override bool IsHandled(BuildContext buildContext)
        {
            return buildContext.Matches("BellScheduleMeetingTime", "ClassPeriodReference");
        }

        protected override IDictionary<string, object> GetPropertyValueConstraints(BuildContext buildContext)
        {
            dynamic bellSchedule = buildContext.GetParentInstance();

            return new Dictionary<string, object>
            {
                {"schoolId", bellSchedule.schoolReference.schoolId}
            };
        }
    }
}