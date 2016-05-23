using System;
using System.Collections.Generic;
using EdFi.TestObjects;

namespace EdFi.Ods.Utilities.LoadGeneration.ReferenceValueBuilders
{
    public class GradingPeriod_EndCalendarDate_ValueBuilder : ReferenceValueBuilderBase, IContextSpecificReferenceValueBuilder
    {
        private dynamic _gradingPeriod;

        protected override bool IsHandled(BuildContext buildContext)
        {
            if (!(buildContext.Matches("GradingPeriod", "CalendarDateReference") &&
                buildContext.LogicalPropertyPath.LastSegmentEquals("endCalendarDateReference")))
                return false;

            _gradingPeriod = buildContext.GetContainingInstance();

            return _gradingPeriod.beginCalendarDateReference != null;
        }

        protected override IDictionary<string, object> GetPropertyValueConstraints(BuildContext buildContext)
        {
            return new Dictionary<string, object>
            {
                {"educationOrganizationId", _gradingPeriod.beginCalendarDateReference.educationOrganizationId}
            };
        }
    }
}