namespace EdFi.Ods.Tests.EdFi.Ods.CodeGen.XmlShredding.EntityMetadataManagerTests
{
    using System.Linq;

    using global::EdFi.Ods.CodeGen.XmlShredding;
    using global::EdFi.Ods.CodeGen.XsdToWebApi.Parse;
    using global::EdFi.Ods.Tests._Bases;

    using NUnit.Framework;

    using Should;

    [TestFixture]
    public class StudentTests : SchemaMetadataTestBase
    {
        private ParsedSchemaObject xsdObject;

        [TestFixtureSetUp]
        public void Setup()
        {
            this.xsdObject = this.GetParsed(x => x.XmlSchemaObjectName == "Student");
        }

        [Test]
        public void When_Parsing_Student_Should_Identify_HispanicLatinoEthnicity_As_Bool()
        {
            var sut = new EntityMetadataManager(this.xsdObject, this.Stub<IXPathMapBuilder>());
            var target = sut.GetSingleSimpleTypedProperties().Single(p => p.PropertyName == "HispanicLatinoEthnicity");
            var expected = typeof (bool);
            target.PropertyType.ShouldEqual(expected);
        }

        [Test]
        public void Given_Student_SHould_IDentify_Birthdate_As_Nested_Value()
        {
            var sut = new EntityMetadataManager(this.xsdObject, this.Stub<IXPathMapBuilder>());
            var birthDate = sut.GetSingleSimpleTypedProperties().Single(p => p.ElementName == "BirthDate");
            birthDate.IsNestedValue().ShouldBeTrue();
        }

        [Test]
        public void Given_Xsd_Object_Should_Identify_Nested_Values_When_Asked_For_Inline_Collection()
        {
            var sut = new EntityMetadataManager(this.xsdObject, this.Stub<IXPathMapBuilder>());
            var inlines = sut.GetInlineEntityCollectionProperties();
            inlines.Count().ShouldEqual(2);
        }

        [Test]
        public void Given_Xsd_Object_With_Nested_Inline_Collection_Should_Return_Nested_Properties()
        {
            var sut = new EntityMetadataManager(this.xsdObject, this.Stub<IXPathMapBuilder>());
            var nestedInline = sut.GetInlineEntityCollectionProperties().Single(p => p.IsNested);
            nestedInline.ElementNames.ShouldContain("Visa");
        }

        [Test]
        public void Given_A_Multi_ELement_Entityt_COllection_When_asked_for_Them_Should_Return_It()
        {
            var sut = new EntityMetadataManager(this.xsdObject, this.Stub<IXPathMapBuilder>());
            var mutliElementCollections = sut.GetMultiElementEntityCollectionProperties();
            mutliElementCollections.Count().ShouldEqual(1);
        }
    }
}