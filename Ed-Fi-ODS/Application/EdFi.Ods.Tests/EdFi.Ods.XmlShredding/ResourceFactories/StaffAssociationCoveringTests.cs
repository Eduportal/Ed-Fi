using EdFi.Ods.Tests.TestObjects.TestXml;
using NUnit.Framework;
using Should;

namespace EdFi.Ods.Tests.EdFi.Ods.XmlShredding.ResourceFactories
{
    using global::EdFi.Ods.Tests._Bases;

    [TestFixture]
    [Ignore("Test data is location specific")]
    public class StaffAssociationCoveringTests : ResourceFactoryTestBase
    {
        [Test]
        [Ignore("Test data is location specific")]
        public void Should_Generate_Valid_Resources_from_GrandBend_Sample()
        {
            SetSource(GrandBendTestXml.StaffAssociation, GrandBendTestXml.StaffAssociationRootNames);
            var expected = GetResourceCountInSource();
            var actual = GenerateResourcesAndReturnNumberValidFromSource();
            actual.ShouldEqual(expected);
        }
    }
}