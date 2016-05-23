using EdFi.Ods.Tests.TestObjects.TestXml;
using NUnit.Framework;
using Should;

namespace EdFi.Ods.Tests.EdFi.Ods.XmlShredding.ResourceFactories
{
    using global::EdFi.Ods.Tests._Bases;

    /// <summary>
    /// These tests cover generated code (IResourceFactory) and should be considered in
    /// that light.  They help us avoid using Debug - Friends don't let Friends Debug.
    /// </summary>
    [TestFixture]
    public class DescriptorsCoveringTests : ResourceFactoryTestBase
    {
        [Test]
        public void Factories_Should_Produce_For_Each_Aggregate_In_Xml()
        {
            SetSource(TNTestXml.Descriptors, TNTestXml.DescriptorsAggregateRoots);
            var expected = GetResourceCountInSource();
            var valid = GenerateResourcesAndReturnNumberValidFromSource();
            valid.ShouldEqual(expected);
        }
    }
}