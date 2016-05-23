namespace EdFi.Ods.Tests.EdFi.Ods.CodeGen.XmlShredding
{
    using System;

    using global::EdFi.Ods.CodeGen.XmlShredding;
    using global::EdFi.Ods.CodeGen.XsdToWebApi.Process;
    using global::EdFi.Ods.Tests._Bases;

    using NUnit.Framework;

    using Should;

    [TestFixture, Ignore("No InlineEntityProperties in 2.0 Schema")]
    public class InlineEntityPropertyTests : SchemaMetadataTestBase
    {
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void When_Passed_A_Non_Inline_Entity_Parsed_Object_Should_Throw_Argument_Exception()
        {
            var badXsd = this.GetParsed(b => b.ProcessResult.Expected.GetType() != typeof (ExpectedInlineCollection));
            var sut = new InlineEntityProperty(badXsd);
        }

        [Test]
        public void When_Asked_For_Property_Name_Should_Be_Name_Of_Property_On_Containing_Entity()
        {
            var inlineXsd = this.GetParsed(i => i.XmlSchemaObjectName == "EducationOrganizationCategory");
            var sut = new InlineEntityProperty(inlineXsd);
            sut.PropertyName.ShouldEqual("EducationOrganizationCategories");
        }

        [Test]
        public void When_Asked_For_Interface_Name_Should_return_name()
        {
            var inlineXsd = this.GetParsed(i => i.XmlSchemaObjectName == "EducationOrganizationCategory");
            var sut = new InlineEntityProperty(inlineXsd);
            sut.InterfaceName.ShouldEqual("IEducationOrganizationCategory");
        }

        [Test]
        public void When_Asked_For_InlineProperty_Name_Should_Return_Name_of_Property_of_the_Inline_Entity()
        {
            var inlineXsd = this.GetParsed(i => i.XmlSchemaObjectName == "EducationOrganizationCategory");
            var sut = new InlineEntityProperty(inlineXsd);
            sut.InlinePropertyName.ShouldEqual("EducationOrganizationCategoryType");
        }

        [Test]
        public void When_Asked_For_Inline_Property_Type_Should_Return_Type_of_Simple_Property_of_the_Inline_Entity()
        {
            var inlineXsd = this.GetParsed(i => i.XmlSchemaObjectName == "EducationOrganizationCategory");
            var sut = new InlineEntityProperty(inlineXsd);
            var expectedType = typeof (string);
            sut.InlinePropertyType.ShouldEqual(expectedType);
        }

        [Test]
        public void When_Asked_For_Namespace_Should_Return_Namespace_of_The_Collection_Entity()
        {
            var inlineXsd = this.GetParsed(i => i.XmlSchemaObjectName == "EducationOrganizationCategory");
            var sut = new InlineEntityProperty(inlineXsd);
            sut.Namespace.ShouldEqual("EdFi.Ods.Api.Models.Resources.EducationOrganization");
        }
    }
}