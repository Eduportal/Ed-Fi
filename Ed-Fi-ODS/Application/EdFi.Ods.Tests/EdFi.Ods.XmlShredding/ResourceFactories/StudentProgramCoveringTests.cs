using EdFi.Ods.Tests.TestObjects.TestXml;
using NUnit.Framework;
using Should;

namespace EdFi.Ods.Tests.EdFi.Ods.XmlShredding.ResourceFactories
{
    using global::EdFi.Ods.Tests._Bases;

    [TestFixture]
    [Ignore("Test data is location specific")]
    public class StudentProgramCoveringTests : ResourceFactoryTestBase
    {
        [Test]
        [Ignore("Test data is location specific")]
        public void Given_GrandBend_Sample_Should_Generate_Resources_For_All()
        {
            SetSource(GrandBendTestXml.StudentProgram, GrandBendTestXml.StudentProgramRootNames);
            var expected = GetResourceCountInSource();
            var validGenerated = GenerateResourcesAndReturnNumberValidFromSource();
            validGenerated.ShouldEqual(expected);
        }
    }
}