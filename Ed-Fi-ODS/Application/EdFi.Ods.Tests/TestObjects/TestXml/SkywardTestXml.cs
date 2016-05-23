using System.Collections.Generic;
using System.IO;
using EdFi.Ods.Common;
using EdFi.Ods.Common.Utils.Resources;

namespace EdFi.Ods.Tests.TestObjects.TestXml
{
    public static class SkywardTestXml
    {
        public static Stream EducationOrganizationXml { get
        {
            return EmbeddedResourceReader.GetResourceStream<IMarkWhereTestXmlLives>("SkywardEducationOrganization.xml");
        } }

        public static KeyValuePair<string, Stream> EdOrgXmlTypePair = new KeyValuePair<string, Stream>(InterchangeType.EducationOrganization.Name, EducationOrganizationXml); 

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
            return EmbeddedResourceReader.GetResourceStream<IMarkWhereTestXmlLives>("SkywardEducationOrgCalendar.xml");
        } }

        public static KeyValuePair<string, Stream> EdOrgCalendarTypePair = new KeyValuePair<string, Stream>(InterchangeType.EducationOrgCalendar.Name, EducationOrganizationCalendarXml); 

        public static IEnumerable<string> EdOrgCalendarAggregateRootNames = new List<string>
        {
            "Session",
            "GradingPeriod",
            "CalendarDate",
            "AcademicWeek",
            "DistrictCalendar",
            "SchoolCalendar",
        };

        public static Stream MasterScheduleXml { get
        {
            return EmbeddedResourceReader.GetResourceStream<IMarkWhereTestXmlLives>("SkywardMasterSchedule.xml");
        } }

        public static KeyValuePair<string, Stream> MasterScheduleTypePair = new KeyValuePair<string, Stream>(InterchangeType.MasterSchedule.Name, MasterScheduleXml); 

        public static IEnumerable<string> MasterScheduleRootNames = new List<string> { "CourseOffering", "Section", "BellSchedule" }; 
    }
}