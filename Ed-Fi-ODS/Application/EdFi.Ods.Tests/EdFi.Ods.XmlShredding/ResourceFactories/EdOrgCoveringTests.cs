using EdFi.Ods.Tests.TestObjects.TestXml;
using NUnit.Framework;
using Should;

namespace EdFi.Ods.Tests.EdFi.Ods.XmlShredding.ResourceFactories
{
    using global::EdFi.Ods.Tests._Bases;

    [TestFixture]
    [Ignore("Test data is location specific")]
    public class EdOrgCoveringTests : ResourceFactoryTestBase
    {
        [Test]
        [Ignore("Test data is location specific")]
        public void Should_Produce_Resource_For_All_Valid_Xml()
        {
            SetSource(GrandBendTestXml.EdOrg, GrandBendTestXml.EdOrgAggregateRootNames);
            var expected = GetResourceCountInSource();
            var valid = GenerateResourcesAndReturnNumberValidFromSource();
            valid.ShouldEqual(expected);
        }

        [Test]
        [Ignore("Test data is location specific")]
        public void Given_Skyward_TN_EdOrg_Xml_Should_Produce_Valid_Resources_For_All_Valid_Elements()
        {
            SetSource(SkywardTestXml.EducationOrganizationXml, SkywardTestXml.EdOrgAggregateRootNames);
            var expected = GetResourceCountInSource();
            var generated = GenerateResourcesAndReturnNumberValidFromSource();
            generated.ShouldEqual(expected);
        }

        [Test]
        [Ignore("Test data is location specific")]
        public void Given_STI_TN_EdOrg_Xml_Should_Produce_Valid_Resources_For_All_Valid_Elements()
        {
            SetSource(STITestXml.EducationOrganizationXml, STITestXml.EdOrgAggregateRootNames);
            var expected = GetResourceCountInSource();
            var generated = GenerateResourcesAndReturnNumberValidFromSource();
            generated.ShouldEqual(expected);
        }
    }
}