// ****************************************************************************
//  Copyright (C) 2014 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System.Collections.Generic;
using EdFi.TestObjects;

namespace EdFi.Ods.Utilities.LoadGeneration.ReferenceValueBuilders
{
    public class SectionAttendanceTakenEvent_SectionReference_ValueBuilder : ReferenceValueBuilderBase, IContextSpecificReferenceValueBuilder
    {
        protected override bool IsHandled(BuildContext buildContext)
        {
            if (!buildContext.Matches("SectionAttendanceTakenEvent", "SectionReference"))
                return false;

            dynamic sectionAttendanceTakenEvent = buildContext.GetContainingInstance();

            // Don't perform special handling of this request if CalendarDateReference has not been set (let the StandardReferenceValueBuilder build it)
            return sectionAttendanceTakenEvent.calendarDateReference != null;
        }

        protected override IDictionary<string, object> GetPropertyValueConstraints(BuildContext buildContext)
        {
            dynamic sectionAttendanceTakenEvent = buildContext.GetContainingInstance();

            return new Dictionary<string, object>()
                {
                    {"schoolId", sectionAttendanceTakenEvent.calendarDateReference.educationOrganizationId }
                };
        }
    }
}