using System;
using System.Collections.Generic;
using EdFi.TestObjects;

namespace EdFi.Ods.Utilities.LoadGeneration.ReferenceValueBuilders
{
    public class AcademicWeek_EndCalendarDate_ValueBuilder : ReferenceValueBuilderBase, IContextSpecificReferenceValueBuilder
    {
        protected override bool IsHandled(BuildContext buildContext)
        {
            if (!(buildContext.Matches("AcademicWeek", "CalendarDateReference")
                && buildContext.LogicalPropertyPath.LastSegmentEquals("endCalendarDateReference")))
                return false;

            dynamic academicWeek = buildContext.GetContainingInstance();

            return academicWeek.beginCalendarDateReference != null;
        }

        protected override IDictionary<string, object> GetPropertyValueConstraints(BuildContext buildContext)
        {
            dynamic academicWeek = buildContext.GetContainingInstance();

            return new Dictionary<string, object>
            {
                {"educationOrganizationId", academicWeek.beginCalendarDateReference.educationOrganizationId}
            };
        }
    }
}