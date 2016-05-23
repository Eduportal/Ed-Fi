using System.Collections.Generic;
using System.IO;
using EdFi.Ods.Common;
using EdFi.Ods.Common.Utils.Resources;

namespace EdFi.Ods.Tests.TestObjects.TestXml
{
    public class BugRegressionTestXml
    {
        public static Stream EdOrgCalendarXml
        {
            get
            {
                return EmbeddedResourceReader.GetResourceStream<IMarkWhereTestXmlLives>("BugRegressionEducationOrgCalendar.xml");
            }
        }

        public static IEnumerable<string> EdOrgCalendarAggregateRootNames = new List<string>
        {
            "Session",
            "GradingPeriod",
            "CalendarDate",
            "AcademicWeek",
        };

        public static KeyValuePair<string, Stream> EdOrgXmlTypePair = new KeyValuePair<string, Stream>(InterchangeType.EducationOrgCalendar.Name, EdOrgCalendarXml);
    }
}