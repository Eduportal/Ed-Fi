namespace EdFi.Ods.Tests.EdFi.Ods.CodeGen.XmlShredding
{
    using System.Linq;

    using global::EdFi.Ods.CodeGen.XmlShredding;
    using global::EdFi.Ods.CodeGen.XsdToWebApi.Parse;
    using global::EdFi.Ods.CodeGen.XsdToWebApi.Process;
    using global::EdFi.Ods.Tests._Bases;

    using NUnit.Framework;

    using Should;

    [TestFixture]
    public class ParsedSchemaObjectExtensionsTests : SchemaMetadataTestBase
    {
        [Test]
        public void When_Asked_If_An_Object_With_A_Skipped_Result_Is_An_Extended_Reference_Should_Return_False()
        {
            var skipExtendedRef = new ParsedSchemaElement();
            skipExtendedRef.ProcessResult = new ProcessResult{Expected = new Skip()};
            skipExtendedRef.XmlSchemaTypeGroup = "Extended Reference";
            skipExtendedRef.IsExtendedReference().ShouldBeFalse();
        }

        [Test]
        public void
            When_Asked_If_An_Object_With_An_Extended_Reference_Collection_Is_An_Extended_Reference_Should_Return_False()
        {
            var extRefCollection = new ParsedSchemaElement();
            extRefCollection.XmlSchemaTypeGroup = "Extended Reference";
            extRefCollection.IsCollection = true;
            extRefCollection.IsExtendedReference().ShouldBeFalse();
        }

        [Test]
        public void
            When_Asked_If_An_Object_With_An_Extended_Reference_Collection_Is_An_Entity_Collection_Should_Return_True()
        {
            var extRefCollection = new ParsedSchemaElement();
            extRefCollection.XmlSchemaTypeGroup = "Extended Reference";
            extRefCollection.ProcessResult = new ProcessResult{Expected = new Skip()};
            extRefCollection.IsCollection = true;
            extRefCollection.IsEntityCollection().ShouldBeTrue();
        }

        [Test]
        public void
            When_Asked_If_An_Object_That_is_A_Collection_Has_An_Expected_Rest_Property_And_Is_Not_A_Descriptors_Enumeration_Is_An_Entity_Collection
            ()
        {
            var entityCollection = new ParsedSchemaElement();
            entityCollection.IsCollection = true;
            entityCollection.ProcessResult = new ProcessResult {Expected = new ExpectedRestProperty(), ProcessingRuleName = "Not Descriptor Enumeration"};
            entityCollection.IsEntityCollection().ShouldBeTrue();
        }

        [Test]
        public void
            When_Asked_If_An_Object_That_is_A_Descriptor_Enumeration_Has_An_Expected_Rest_Property_And_Is_Not_A_Descriptors_Enumeration_Is_An_Entity_Collection
            ()
        {
            var entityCollection = new ParsedSchemaElement();
            entityCollection.IsCollection = true;
            entityCollection.ProcessResult = new ProcessResult {Expected = new ExpectedRestProperty(), ProcessingRuleName = "Descriptor Enumeration"};
            entityCollection.IsEntityCollection().ShouldBeFalse();
        }

        [Test]
        public void
            When_Asked_If_An_Object_That_Is_A_Rest_Property_And_Not_A_Collection_Is_An_Single_Entity_Should_Return_True()
        {
            var singleEntity = new ParsedSchemaElement();
            singleEntity.IsCollection = false;
            singleEntity.ProcessResult = new ProcessResult {Expected = new ExpectedRestProperty(), ProcessingRuleName = "Not Descriptor Enumeration"};
            singleEntity.IsSingleEntity().ShouldBeTrue();
        }

        [Test]
        public void When_Asked_If_An_Object_That_Is_An_Identity_Schema_Type_Group_Is_A_Foreign_Key_Should_Return_True()
        {
            var fk = new ParsedSchemaElement();
            fk.XmlSchemaTypeGroup = "Identity";
            fk.ContainsForeignKey().ShouldBeTrue();
        }

        [Test]
        public void When_Asked_If_An_Object_That_is_An_Extended_Descriptors_Reference_Is_One_Should_Return_True()
        {
            var def = new ParsedSchemaElement();
            def.XmlSchemaTypeGroup = "Extended Descriptor Reference";
            def.IsDescriptorsExtRef().ShouldBeTrue();
        }

        [Test]
        public void Should_Recognized_Extended_Descriptor_References_As_Foreign_Keys()
        {
            var def = new ParsedSchemaElement();
            def.XmlSchemaTypeGroup = "Extended Descriptor Reference";
            def.ContainsForeignKey().ShouldBeTrue();
        }

        [Test]
        public void Should_Recognized_Extended_Descriptor_Collections_As_Entity_Collections()
        {
            var def = new ParsedSchemaElement();
            def.XmlSchemaTypeGroup = "Extended Descriptor Reference";
            def.ProcessResult = new ProcessResult {Expected = new Skip()};
            def.IsCollection = true;
            def.IsEntityCollection().ShouldBeTrue();
        }

        [Test]
        public void Should_Recognize_Top_Level_Reference_Type_Results_As_Should_Be_Skipped_Objects()
        {
            var studentGradeBook = this.GetParsed(x => x.XmlSchemaObjectName == "InterchangeStudentGradebook");
            var sectionXsd = studentGradeBook.ChildElements.Single(x => x.XmlSchemaObjectName == "SectionReference");
            sectionXsd.ShouldBeSkipped().ShouldBeTrue();
        }

        [Test]
        public void Should_Recognize_Skip_Process_Results_As_Should_Be_Skipped_Objects()
        {
            var parsedSchemaObject = new ParsedSchemaElement
                                         {
                                             ProcessResult = new ProcessResult
                                                                 {
                                                                         Expected = new Skip()
                                                                 }
                                         };
            parsedSchemaObject.ShouldBeSkipped().ShouldBeTrue();
        }

        [Test]
        public void
            When_Given_Interchange_Element_With_Top_Level_Skippable_Elements_Should_Not_Include_Them_In_Collection()
        {
            var studentGradeBook = this.GetParsed(x => x.XmlSchemaObjectName == "InterchangeStudentGradebook");
            studentGradeBook.GetChildElementsToBeParsed().Any(x => x.XmlSchemaObjectName == "SectionReference").ShouldBeFalse();
        }
    }
}