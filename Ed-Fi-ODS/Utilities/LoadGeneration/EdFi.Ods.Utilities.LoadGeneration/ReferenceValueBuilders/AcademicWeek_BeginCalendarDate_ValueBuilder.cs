using System;
using System.Collections.Generic;
using EdFi.TestObjects;

namespace EdFi.Ods.Utilities.LoadGeneration.ReferenceValueBuilders
{
    public class AcademicWeek_BeginCalendarDate_ValueBuilder : ReferenceValueBuilderBase, IContextSpecificReferenceValueBuilder
    {
        protected override bool IsHandled(BuildContext buildContext)
        {
            if (!(buildContext.Matches("AcademicWeek", "CalendarDateReference")
                && buildContext.LogicalPropertyPath.LastSegmentEquals("beginCalendarDateReference")))
                return false;

            dynamic academicWeek = buildContext.GetContainingInstance();

            return academicWeek.endCalendarDateReference != null;
        }

        protected override IDictionary<string, object> GetPropertyValueConstraints(BuildContext buildContext)
        {
            dynamic academicWeek = buildContext.GetContainingInstance();

            return new Dictionary<string, object>
            {
                {"educationOrganizationId", academicWeek.endCalendarDateReference.educationOrganizationId}
            };
        }
    }
}