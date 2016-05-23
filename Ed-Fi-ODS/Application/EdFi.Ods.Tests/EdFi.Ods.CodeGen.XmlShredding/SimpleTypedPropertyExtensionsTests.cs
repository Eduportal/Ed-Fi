using System.Linq;
using EdFi.Ods.XmlShredding.CodeGen;
using NUnit.Framework;
using Should;
using UnitTests._Bases;

namespace UnitTests.EdFi.Ods.XmlShredding.CodeGen
{
    [TestFixture]
    public class SimpleTypedPropertyExtensionsTests : SchemaMetadataTestBase
    {
        [Test]
        public void Given_A_Simple_Nested_Property_Should_Return_NestedValueOf_Statement()
        {
            var student = GetParsed(x => x.XmlSchemaObjectName == "Student");
            var name = student.ChildElements.Single(c => c.XmlSchemaObjectName == "Name");
            var nested = name.ChildElements.First(c => c.XmlSchemaObjectName == "FirstName");
            var expected = @"element.NestedValueOf(new []{""Name"",""FirstName""})";
            var sut = new SimpleTypeProperty(nested);
            var elementAssignment = sut.GetAssignmentExpressionAsStringFor("element");
            elementAssignment.ShouldEqual(expected);
        }

        [Test]
        public void Given_A_DateTime_Nested_Property_Should_Return_NestedDateTimeValueOf_Statement()
        {
            var student = GetParsed(x => x.XmlSchemaObjectName == "Student");
            var birthDate =
                student.ChildElements.Single(c => c.XmlSchemaObjectName == "BirthData")
                    .ChildElements.Single(nc => nc.XmlSchemaObjectName == "BirthDate");
            var expected = @"element.NestedDateTimeValueOf(new []{""BirthData"",""BirthDate""})";
            var sut = new SimpleTypeProperty(birthDate);
            var result = sut.GetAssignmentExpressionAsStringFor("element");
            result.ShouldEqual(expected);
        }

        //TODO: need unit tests around all expected types (see convertmethods property in the extension class)
    }
}