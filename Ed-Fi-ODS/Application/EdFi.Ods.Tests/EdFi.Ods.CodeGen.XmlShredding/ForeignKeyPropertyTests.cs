namespace EdFi.Ods.Tests.EdFi.Ods.CodeGen.XmlShredding
{
    using global::EdFi.Ods.CodeGen.XmlShredding;
    using global::EdFi.Ods.Tests._Bases;

    using NUnit.Framework;

    using Should;

    [TestFixture]
    public class ForeignKeyPropertyTests : SchemaMetadataTestBase
    {
        [Test]
        public void When_Given_Extended_Reference_Should_Set_Property_Type_To_Expected()
        {
            var refXsd = this.GetParsed(x => x.XmlSchemaObjectName == "ProgramType");
            var key = new ForeignKeyProperty(refXsd, "This would be serialized map");
            var expectedType = typeof (string);
            key.PropertyType.ShouldEqual(expectedType);
        }

    }
}