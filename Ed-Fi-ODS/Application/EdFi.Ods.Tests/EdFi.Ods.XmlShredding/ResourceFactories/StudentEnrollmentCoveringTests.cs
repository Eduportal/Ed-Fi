using EdFi.Ods.Tests.TestObjects.TestXml;
using NUnit.Framework;
using Should;

namespace EdFi.Ods.Tests.EdFi.Ods.XmlShredding.ResourceFactories
{
    using global::EdFi.Ods.Tests._Bases;

    [TestFixture]
    [Ignore("Test data is location specific")]
    public class StudentEnrollmentCoveringTests : ResourceFactoryTestBase
    {
        [Test]
        [Ignore("Test data is location specific")]
        public void Given_GrandBend_Sample_Xml_Should_Generate_All_Resources()
        {
            SetSource(GrandBendTestXml.StudentEnrollment, GrandBendTestXml.StudentEnrollmentRootNames);
            var expected = GetResourceCountInSource();
            var validGenerated = GenerateResourcesAndReturnNumberValidFromSource();
            validGenerated.ShouldEqual(expected);
        }
    }
}