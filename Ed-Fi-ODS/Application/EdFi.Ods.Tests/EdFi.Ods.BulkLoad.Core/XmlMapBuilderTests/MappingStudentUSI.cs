namespace EdFi.Ods.Tests.EdFi.Ods.BulkLoad.Core.XmlMapBuilderTests
{
    using System.Linq;

    using global::EdFi.Ods.CodeGen.XmlShredding;
    using global::EdFi.Ods.Tests._Bases;

    using NUnit.Framework;

    using Should;

    [TestFixture]
    public class MappingStudentUSI : SchemaMetadataTestBase
    {
        [Test]
        public void
            Given_StudentParentAssociation_When_Ref_Id_Found_Should_Produce_Map_To_StudentUniqueId_In_Student()
        {
            var xsdObject = this.GetParsed(x => x.XmlSchemaObjectName == "StudentParentAssociation");
            var sut = new XPathMapBuilder();
            var tuples = sut.BuildStartingXsdElementSerializedMapTuplesFor(xsdObject);
            var map = tuples.Where(t => t.Item2.Contains("StudentUniqueId")).Select(t => t.Item2).First();
            var step = sut.DeserializeMap(map) as IReferenceStep;
            step.ReferencePointedToAggregateType("Student",string.Empty);
            step.GetXPath().ShouldNotContain("StudentIdentity");
        }
    }
}