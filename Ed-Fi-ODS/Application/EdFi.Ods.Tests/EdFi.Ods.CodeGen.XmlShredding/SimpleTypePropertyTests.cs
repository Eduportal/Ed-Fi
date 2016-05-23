namespace EdFi.Ods.Tests.EdFi.Ods.CodeGen.XmlShredding
{
    using System.Linq;

    using global::EdFi.Ods.CodeGen.XmlShredding;
    using global::EdFi.Ods.Tests._Bases;

    using NUnit.Framework;

    using Should;

    [TestFixture]
    public class SimpleTypePropertyTests : SchemaMetadataTestBase
    {
        [Test]
        public void When_Passed_A_Parsed_Object_That_A_Descriptor_Enumeration_Should_Use_RestProperty_Values()
        {
            var name = "AcademicSubjectMap";
            var parsedObject = this.GetParsed(x => x.XmlSchemaObjectName.Equals(name));
            var sut = new SimpleTypeProperty(parsedObject);
            var expectedType = typeof (string);
            sut.PropertyType.ShouldEqual(expectedType);
            sut.PropertyName.ShouldEqual("AcademicSubjectType");
        }

        [Test]
        public void SHould()
        {
            var ename = "LevelDescriptor";
            var entity = this.GetParsed(o => o.XmlSchemaObjectName == ename);
            var parentEMM = new EntityMetadataManager(entity, this.Stub<IXPathMapBuilder>());
            var propertyEMM = parentEMM.GetEntityTypedCollectionProperties().Single().GetMetaDataMgr();
            var sut = propertyEMM.GetSingleSimpleTypedProperties().Single();
            sut.PropertyName.ShouldEqual("GradeLevelDescriptor");
        }
    }
}