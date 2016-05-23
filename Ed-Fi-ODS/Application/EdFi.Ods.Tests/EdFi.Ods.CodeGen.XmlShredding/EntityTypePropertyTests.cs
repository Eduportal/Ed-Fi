namespace EdFi.Ods.Tests.EdFi.Ods.CodeGen.XmlShredding
{
    using System;
    using System.Linq;

    using global::EdFi.Ods.CodeGen.XmlShredding;
    using global::EdFi.Ods.CodeGen.XsdToWebApi.Process;
    using global::EdFi.Ods.Tests._Bases;

    using NUnit.Framework;

    using Should;

    [TestFixture]
    public class EntityTypePropertyTests : SchemaMetadataTestBase
    {
        [Test]
        public void When_Asked_To_Build_EntityManager_Should_Return_Manager_Based_On_Itself()
        {
            var pname = "LocalEducationAgencyAccountability";
            var xsdObject = this.GetParsed(p => p.XmlSchemaObjectName == pname);
            var sut = new EntityTypeProperty(xsdObject);
            var mgr = sut.GetMetaDataMgr();
            mgr.EntityName.ShouldEqual(pname);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void When_Passed_A_non_Entity_Parsed_Object_Should_Throw_Argument_Exception()
        {
            var badXsd = this.GetParsed(b => b.ProcessResult.Expected.GetType() != typeof (ExpectedRestProperty));
            var sut = new EntityTypeProperty(badXsd);
        }

        [Test]
        public void Should_Handle_Extended_Reference_Objects()
        {
            var name = "School";
            var schoolXsd = this.GetParsed(x => x.XmlSchemaObjectName.Equals(name));
            var property = schoolXsd.ChildElements.First(c => c.XmlSchemaObjectName.Equals("GradeLevel"));
            var sut = new EntityTypeProperty(property);
            sut.PropertyName.ShouldEqual("SchoolGradeLevels");
        }
    }
}