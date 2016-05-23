using EdFi.Ods.Tests.TestObjects.TestXml;
using NUnit.Framework;
using Should;

namespace EdFi.Ods.Tests.EdFi.Ods.XmlShredding.ResourceFactories
{
    using global::EdFi.Ods.Tests._Bases;

    [TestFixture]
    [Ignore("Test data is location specific")]
    public class MasterScheduleCoveringTests : ResourceFactoryTestBase
    {
        [Test]
        [Ignore("Test data is location specific")]
        public void When_Loading_EdFi_GrandBend_Sample_XML_Should_Produce_Valid_Resources()
        {
            SetSource(GrandBendTestXml.MasterSchedule, GrandBendTestXml.MasterScheduleRootNames);
            var expected = GetResourceCountInSource();
            var result = GenerateResourcesAndReturnNumberValidFromSource();
            result.ShouldEqual(expected);
        }

        [Test]
        [Ignore("Test data is location specific")]
        public void Given_Skyward_Xml_Should_Generate_Valid_Resources_For_All_Valid_Elements()
        {
            SetSource(SkywardTestXml.MasterScheduleXml, SkywardTestXml.MasterScheduleRootNames);
            var expected = GetResourceCountInSource();
            var generated = GenerateResourcesAndReturnNumberValidFromSource();
            generated.ShouldEqual(expected);
        }
    }
}