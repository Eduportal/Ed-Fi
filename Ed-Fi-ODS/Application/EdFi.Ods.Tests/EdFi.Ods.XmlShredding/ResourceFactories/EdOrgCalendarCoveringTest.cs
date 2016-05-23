using EdFi.Ods.Tests.TestObjects.TestXml;
using NUnit.Framework;
using Should;

namespace EdFi.Ods.Tests.EdFi.Ods.XmlShredding.ResourceFactories
{
    using global::EdFi.Ods.Tests._Bases;

    [TestFixture]
    [Ignore("Test data is location specific")]
    public class EdOrgCalendarCoveringTest : ResourceFactoryTestBase
    {
        [Test]
        [Ignore("Test data is location specific")]
        public void Given_Valid_Xml_Should_Produce_Valid_Resources()
        {
            SetSource(GrandBendTestXml.EdOrgCalendar, GrandBendTestXml.EdOrgCalendarAggregateRootNames);
            var expected = GetResourceCountInSource();
            var valid = GenerateResourcesAndReturnNumberValidFromSource();
            valid.ShouldEqual(expected);
        }

        [Test]
        [Ignore("Test data is location specific")]
        public void Given_STI_Xml_Should_Produce_Valid_Resources_for_All_Valid_Elements()
        {
            SetSource(STITestXml.EducationOrganizationCalendarXml, STITestXml.EdOrgCalendarAggregateRootNames);
            var expected = GetResourceCountInSource();
            var generated = GenerateResourcesAndReturnNumberValidFromSource();
            generated.ShouldEqual(expected);
        }

        [Test]
        [Ignore("Test data is location specific")]
        public void Given_Acceptance_Xml_Should_Produce_Valid_Resources_For_All_Valid_Elements()
        {
            SetSource(BugRegressionTestXml.EdOrgCalendarXml, BugRegressionTestXml.EdOrgCalendarAggregateRootNames);
            var expected = GetResourceCountInSource();
            var generated = GenerateResourcesAndReturnNumberValidFromSource();
            generated.ShouldEqual(expected);
        }
    }
}