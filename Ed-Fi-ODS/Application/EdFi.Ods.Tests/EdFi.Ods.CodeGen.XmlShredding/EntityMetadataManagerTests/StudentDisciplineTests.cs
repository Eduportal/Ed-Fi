namespace EdFi.Ods.Tests.EdFi.Ods.CodeGen.XmlShredding.EntityMetadataManagerTests
{
    using System.Linq;

    using global::EdFi.Ods.CodeGen.XmlShredding;
    using global::EdFi.Ods.Tests._Bases;

    using NUnit.Framework;

    using Should;

    [TestFixture]
    public class StudentDisciplineTests : SchemaMetadataTestBase
    {
        /// <summary>
        /// This is a terrible test and if you wish to delete it you have my blessing . . .
        /// would be really cool if you replaced it with something else . . . 
        /// </summary>
        [Test]
        public void Given_DisciplineIncident_Xsd_Should()
        {
            var name = "DisciplineIncident";
            var xsdObject = this.GetParsed(x => x.XmlSchemaObjectName == name);
            var builder = new XPathMapBuilder();
            var sut = new EntityMetadataManager(xsdObject, builder);
            sut.GetEntityTypedCollectionProperties().Count().ShouldEqual(2);
            sut.GetForeignKeyProperties().Count().ShouldEqual(3);
            sut.GetInlineEntityCollectionProperties().Count().ShouldEqual(0);
            sut.GetSingleEntityTypedProperties().Count().ShouldEqual(0);
            sut.GetSingleSimpleTypedProperties().Count().ShouldEqual(9);
        }
         
    }
}