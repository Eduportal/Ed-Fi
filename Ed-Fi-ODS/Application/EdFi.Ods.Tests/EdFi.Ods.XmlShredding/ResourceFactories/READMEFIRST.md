### How-To create covering tests for the Code-Generated Resource Factories

## Why Would You Want To?
The resource factories should generate automatically and properly handle all known xml patterns.
Of course, the word 'known' is a point-in-time designation and therefore it becomes prudent to 
test the assumption a new Xsd will not introduce new patterns or that the xml will not expose an 
allowable pattern heretofore unseen.  In other words - Just Do It!  :P

## How To
1. Obtain a valid sample Xml file for each Interchange you expect to load.  Save these in under src\UnitTests\TestObjects\TestXml.
Set these to be an embedded resource with a copy if newer attribute.
2. If a static master file already exists for your Schema/Samples skip to Step 3.  Create a static 
class to house the master meta-information needed for the tests.
3. For each Xml file add a static Stream property using the embedded resource tool as follows:
'
        public static Stream EdOrgCalendar 
        {
            get
            {
                return EmbeddedResourceReader.GetResourceStream<IMarkWhereTestXmlLives>("GrandBendEdOrgCalendar.xml");
            }
        }
'
If you've put your xml in the folder designated in Step 1 the IMarkWhereTestXmlLives interface will 
help our tools locate and load the embedded resource.
4. For each Xml file add a static IEnumerable<string> property with the names of the aggregate roots (hint - these should
be the top level elements of the Xsd for that interchange) as follows:
'
        public static IEnumerable<string> EdOrgCalendarAggregateRootNames = new List<string>
        {
            "Session",
            "GradingPeriod",
            "CalendarDate",
            "AcademicWeek"
        };
'
It's worth noting you may have an aggregate that appears in multiple interchanges - that is to be expected, just add them in each.
5. If a class already exists for your Interchange you may skip to step 6.  Now create a new covering tests class in the 
EdFi.Ods.XmlShredding.ResourceFactories directory under UnitTests.  You will want to inherit from the ResourceFactoriesTestBase 
class to take advantage of the helper methods in the class.  Your class should look similar to this:
'
namespace UnitTests.EdFi.Ods.XmlShredding.ResourceFactories
{
    [TestFixture]
    public class EdOrgCoveringTests : ResourceFactoryTestBase
    {
    }
}
'
6. Now it's time for the test.  Create a test method for your xml file.  First you need to establish the context by 
calling the SetSource(Stream source, IEnumerable<string> aggregateNames) method on the base class - passing in the
static properties you created in Step 3 and 4.  Next establish the expected number of resources that should be 
created from the Xml using the GetResourceCountInSource() method on the base class.  Next, execute the behavior 
(pass the xml to the factories and validate the generated resources) using the GenerateResourcesAndReturnNumberValidFromSource()
 method on the base class.  Finally, confirm that the number of resources created match the expected number using
 an assertion.  You method should look similar to this:

'        [Test]
        public void Should_Produce_Resource_For_All_Valid_Xml_in_GrandBend_Sample()
        {
            SetSource(GrandBendTestXml.EdOrg, GrandBendTestXml.EdOrgAggregateRootNames);
            var expected = GetResourceCountInSource();
            var valid = GenerateResourcesAndReturnNumberValidFromSource();
            valid.ShouldEqual(expected);
        }
'

## Oops.  The Test failed.
Well, that's why we have them.  First, be certain your Xml is well formed.  Make certain it is using the appropriate 
namespace version (must match the one used to generate the factories).  The test will output a message to the console 
for each xml element that either fails to produce a resource or produces an invalid resource.  Use this to guide you 
in troubleshooting the issue with the xml (always start there) or the factories.  Remember, you can use the test to 
debug the factories if you must.  May the force be with you.