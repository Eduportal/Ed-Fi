// ****************************************************************************
//  Copyright (C) 2014 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System;
using System.Collections.Generic;
using EdFi.TestObjects;

namespace EdFi.Ods.Utilities.LoadGeneration.ReferenceValueBuilders
{
    public class SectionAttendanceTakenEvent_CalendarDateReference_ValueBuilder : ReferenceValueBuilderBase, IContextSpecificReferenceValueBuilder
    {
        protected override bool IsHandled(BuildContext buildContext)
        {
            return buildContext.Matches("SectionAttendanceTakenEvent", "CalendarDateReference");
        }

        protected override IDictionary<string, object> GetPropertyValueConstraints(BuildContext buildContext)
        {
            dynamic sectionAttendanceTakenEvent = buildContext.GetContainingInstance();

            if (sectionAttendanceTakenEvent.sectionReference == null)
            {
                return new Dictionary<string, object>()
                    {
                        {"educationOrganizationId", BuildConcreteEducationOrganizationId("SchoolId") }
                    };
            }

            return new Dictionary<string, object>()
                {
                    {"educationOrganizationId", sectionAttendanceTakenEvent.sectionReference.schoolId }
                };
        }
    }
}