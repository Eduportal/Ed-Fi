namespace EdFi.Ods.Tests.EdFi.Ods.CodeGen.XmlShredding
{
    using System;

    using global::EdFi.Ods.CodeGen.XmlShredding;
    using global::EdFi.Ods.CodeGen.XsdToWebApi.Parse;
    using global::EdFi.Ods.CodeGen.XsdToWebApi.Process;

    using NUnit.Framework;

    using Should;

    [TestFixture]
    public class MultiElementEntityCollectionPropertyTests
    {
        private ParsedSchemaObject _expected1;
        private ParsedSchemaObject _expected2;
        private ParsedSchemaObject _expected3;

        [TestFixtureSetUp]
        public void Setup()
        {
            var entityRestType = new ExpectedRestType();
            entityRestType.ClassName = "Entity";
            entityRestType.Namespace = "EdFi.Ods.Tests";
            var entityType = new ProcessResult
            {
                Expected =
                    new ExpectedRestProperty {PropertyExpectedRestType = entityRestType}
            };
            this._expected1 = new StubParsedSchemaObject("Larry", entityType);
            this._expected2 = new StubParsedSchemaObject("Moe", entityType);
            this._expected3 = new StubParsedSchemaObject("Curly", entityType);
       }

        [Test]
        public void Given_At_Least_Two_Elements_Should_Produce_String_With_Comma_Delimited_Strings_Of_Their_Names()
        {
            var given = new[] {this._expected1, this._expected2, this._expected3};
            var expected = @"""Larry"",""Moe"",""Curly""";
            var sut = new MultiElementEntityCollectionProperty(given);
            sut.ParticipatingElementsAsCommaDelimentedStringOfStrings.ShouldEqual(expected);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void Given_Entity_Properties_Of_Different_Types_Should_Throw_Exception()
        {
            var oopsType = new ExpectedRestType();
            oopsType.ClassName = "IDoNotPlayFair";
            oopsType.Namespace = "BadApple";
            var oops = new StubParsedSchemaObject("Bad", new ProcessResult{Expected = new ExpectedRestProperty{PropertyExpectedRestType = oopsType}}) as ParsedSchemaObject;
            var given = new ParsedSchemaObject[]{this._expected1, this._expected2, oops};
            var sut = new MultiElementEntityCollectionProperty(given);
        }
    }
}
