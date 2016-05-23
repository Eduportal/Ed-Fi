namespace EdFi.Ods.Tests.EdFi.Ods.BulkLoad.Core.XmlMapBuilderTests
{
    using global::EdFi.Ods.CodeGen.XmlShredding;
    using global::EdFi.Ods.Tests._Bases;

    using NUnit.Framework;

    using Should;

    [TestFixture]
    public class when_Creating_A_Map_For_StaffProgram_Program_Type_Element : XmlMapBuilderTestBase 
    {
        [Test]
        public void Should_Return_Parsed_Object_Containing_Target_Element()
        {
            var containingXsd = this.GetParsed(x => x.XmlSchemaObjectName == STAFFPROGRAMASSOCIATION);
            var serializedMap = this.SerializedMapFor(STAFFPROGRAMASSOCIATION, PROGRAMTYPE);
            var sut = new XPathMapBuilder();
            var tuples = sut.BuildStartingXsdElementSerializedMapTuplesFor(containingXsd);
            var targetXsd = this.FindReturnedObjectTo(PROGRAMTYPE, tuples);
            targetXsd.XmlSchemaObjectName.ShouldEqual(PROGRAMTYPE);
        }

        [Test]
        public void Deserialized_Map_Should_Begin_With_Program_Reference_Step()
        {
            var serializedMap = this.SerializedMapFor(STAFFPROGRAMASSOCIATION, PROGRAMTYPE);
            var sut = new XPathMapBuilder();
            var map = sut.DeserializeMap(serializedMap) as IReferenceStep;
            map.GetXPath()[0].ShouldEqual("ProgramReference");
        }

        [Test]
        public void Deserialized_Map_Should_Have_Step_To_Program_Type_As_Next_Reference_If_No_Ref_Id_Exists()
        {
            var serializedMap = this.SerializedMapFor(STAFFPROGRAMASSOCIATION, PROGRAMTYPE);
            var sut = new XPathMapBuilder();
            var map = sut.DeserializeMap(serializedMap) as IReferenceStep;
            var stepToProgramType = map.GetNextStep();
            stepToProgramType.GetXPath()[1].ShouldEqual("ProgramType");
        }
    }
}