namespace EdFi.Ods.Tests.EdFi.Ods.BulkLoad.Core.XmlMapBuilderTests
{
    using System.Linq;

    using global::EdFi.Ods.CodeGen.XmlShredding;
    using global::EdFi.Ods.Tests._Bases;

    using NUnit.Framework;

    using Should;

    [TestFixture]
    public class When_Building_Map_For_School : SchemaMetadataTestBase
    {
        [Test]
        public void Should_Find_Two_Mappable_Keys()
        {
            var xsdObject = this.GetParsed(x => x.XmlSchemaObjectName.Equals("School"));
            var sut = new XPathMapBuilder();
            var tuples = sut.BuildStartingXsdElementSerializedMapTuplesFor(xsdObject).Select(x=>x.Item1.XmlSchemaObjectName).ToList();
            tuples.ShouldContain("LocalEducationAgencyId");
            tuples.ShouldContain("CodeValue");
        }

        //TODO: this test needs to be moved and cleaned up a little - if kept.  It demonstrated a bug if the originally parsed object contained only descriptor refs
        [Test]
        public void DoNotForgetToInitializeInternalProperties()
        {
            var xsdObject = this.GetParsed(x => x.XmlSchemaObjectName.Equals("CredentialFieldDescriptor"));
            var sut = new XPathMapBuilder();
            var tuples = sut.BuildStartingXsdElementSerializedMapTuplesFor(xsdObject);
            tuples.Count().ShouldEqual(1);
        }
        //TODO: Should be passing the child element back (not the reference)

        [Test]
        public void Should_Deserialize_Single_Element_Map()
        {
            var serializedMap =
                @"{""IsReference"":true,""Parent"":null,""ElementName"":""CodeValue"",""ParentElement"":""AdministrativeFundingControl"",""IsEdOrgRef"":false}";
            var sut = new XPathMapBuilder();
            var map = sut.DeserializeMap(serializedMap);
            map.GetXPath().ShouldNotBeEmpty();
        }
    }
}