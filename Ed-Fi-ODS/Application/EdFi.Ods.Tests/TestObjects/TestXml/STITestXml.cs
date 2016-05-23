using System.Collections.Generic;
using System.IO;
using EdFi.Ods.Common.Utils.Resources;

namespace EdFi.Ods.Tests.TestObjects.TestXml
{
    public static class STITestXml
    {
        public static Stream EducationOrganizationXml { get
        {
            return EmbeddedResourceReader.GetResourceStream<IMarkWhereTestXmlLives>("STI.EdFiCoreClasses.InterchangeEducationOrganization.xml");
        }}

        public static IEnumerable<string> EdOrgAggregateRootNames = new List<string> {
            "StateEducationAgency",
            "EducationServiceCenter",
                "FeederSchoolAssociation",
                "LocalEducationAgency",
                "School",
                "Location",
                "ClassPeriod",
                "Course",
                "Program",
                "AccountabilityRating",
                "EducationOrganizationPeerAssociation",
                "EducationOrganizationNetwork",
                "EducationOrganizationNetworkAssociation"
        };

        public static Stream EducationOrganizationCalendarXml { get
        {
            return EmbeddedResourceReader.GetResourceStream<IMarkWhereTestXmlLives>("STI.EdFiCoreClasses.InterchangeEducationOrgCalendar.xml");
        }}

        public static IEnumerable<string> EdOrgCalendarAggregateRootNames = new List<string>
        {
            "Session",
            "GradingPeriod",
            "CalendarDate",
            "AcademicWeek",
        };
    }
}