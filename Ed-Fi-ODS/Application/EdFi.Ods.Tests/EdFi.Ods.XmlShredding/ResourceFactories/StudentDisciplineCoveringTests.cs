using EdFi.Ods.Tests.TestObjects.TestXml;
using NUnit.Framework;
using Should;

namespace EdFi.Ods.Tests.EdFi.Ods.XmlShredding.ResourceFactories
{
    using global::EdFi.Ods.Tests._Bases;

    [TestFixture]
    public class StudentDisciplineCoveringTests : ResourceFactoryTestBase
    {
        [Test]
        [Ignore("Need to move Core and Extension into seperate assemblies . . . ")]
        public void Given_GrandBend_Sample_Xml_Should_Generate_All_Resources()
        {
            SetSource(GrandBendTestXml.StudentDiscipline, GrandBendTestXml.StudentDisciplineRootNames);
            var expected = GetResourceCountInSource();
            var validGenerated = GenerateResourcesAndReturnNumberValidFromSource();
            validGenerated.ShouldEqual(expected);
        }
    }
}