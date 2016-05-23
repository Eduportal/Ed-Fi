namespace EdFi.Ods.Tests.EdFi.Ods.BulkLoad.Core.XmlMapBuilderTests
{
    using System.Linq;

    using global::EdFi.Ods.CodeGen.XmlShredding;
    using global::EdFi.Ods.Tests._Bases;

    using NUnit.Framework;

    using Should;

    [TestFixture]
    public class When_Building_A_Map_Starting_At_An_Identity : SchemaMetadataTestBase
    {
        [Test]
        public void Should_Serialize_Map_With_Identity_Step_As_Beginning()
        {
            var xsdObject = this.GetParsed(x => x.XmlSchemaObjectName.Equals("LearningObjectiveReference"));
            var sut = new XPathMapBuilder();
            var tuples = sut.BuildStartingXsdElementSerializedMapTuplesFor(xsdObject);
            var map = tuples.First().Item2;
            map.ShouldContain("\"ParentElement\":\"LearningObjectiveIdentity\"");
            map.ShouldNotContain("\"Element\":\"LearningObjectiveIdentity\"");
        }

        [Test]
        [Ignore("These tests should be re-done to focus on the underlying pattern - not the specific element - which can change")]
        public void Should_Return_All_Keys_In_Map_Collection()
        {
            var xsdObject = this.GetParsed(x => x.XmlSchemaObjectName.Equals("LearningObjectiveReference"));
            var sut = new XPathMapBuilder();
            var maps = sut.BuildStartingXsdElementSerializedMapTuplesFor(xsdObject);
            maps.Count().ShouldEqual(3);
        }

        [Test]
        [Ignore("These tests should be re-done to focus on the underlying pattern - not the specific element - which can change")]
        public void Should_Create_Extended_Descriptor_References_With_Parent_Element_of_Reference()
        {
            var xsdObject = this.GetParsed(x => x.XmlSchemaObjectName.Equals("LearningObjectiveReference"));
            var sut = new XPathMapBuilder();
            var tuples = sut.BuildStartingXsdElementSerializedMapTuplesFor(xsdObject);
            var map = tuples.First(t => t.Item2.Contains("\"ElementName\":\"CodeValue\"") && t.Item2.Contains("AcademicSubject")).Item2;
            map.ShouldContain("\"ParentElement\":\"AcademicSubject");
        }
    }
}