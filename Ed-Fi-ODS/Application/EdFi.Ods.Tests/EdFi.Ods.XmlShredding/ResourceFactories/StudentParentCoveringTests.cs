using EdFi.Ods.Tests.TestObjects.TestXml;
using NUnit.Framework;
using Should;

namespace EdFi.Ods.Tests.EdFi.Ods.XmlShredding.ResourceFactories
{
    using global::EdFi.Ods.Tests._Bases;

    [TestFixture]
    [Ignore("Test data is location specific")]
    public class StudentParentCoveringTests : ResourceFactoryTestBase
    {
        [Test]
        [Ignore("Test data is location specific")]
        public void Given_Valid_Grand_Bend_Xml_Should_Produce_Valid_Resources_For_Each_Element()
        {
            SetSource(GrandBendTestXml.StudentParent, GrandBendTestXml.StudentParentRootNames);
            var expected = GetResourceCountInSource();
            var valid = GenerateResourcesAndReturnNumberValidFromSource();
            valid.ShouldEqual(expected);
        }
    }
}