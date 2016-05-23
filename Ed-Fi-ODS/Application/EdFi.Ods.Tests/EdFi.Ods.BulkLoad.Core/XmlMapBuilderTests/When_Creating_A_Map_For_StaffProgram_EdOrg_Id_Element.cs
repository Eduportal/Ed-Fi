namespace EdFi.Ods.Tests.EdFi.Ods.BulkLoad.Core.XmlMapBuilderTests
{
    using global::EdFi.Ods.CodeGen.XmlShredding;
    using global::EdFi.Ods.Tests._Bases;

    using NUnit.Framework;

    using Should;

    [TestFixture]
    public class When_Creating_A_Map_For_StaffProgram_EdOrg_Id_Element : XmlMapBuilderTestBase
    {
        [Test]
        public void Deserialized_Map_Should_Begin_With_Program_Reference()
        {
            var serializedMap = this.SerializedMapFor(STAFFPROGRAMASSOCIATION, EDUCATIONORGANIZATIONID);
            var sut = new XPathMapBuilder();
            var map = sut.DeserializeMap(serializedMap) as IReferenceStep;
            map.GetXPath()[0].ShouldEqual("ProgramReference");
        }

        [Test]
        public void Deserialized_Map_Should_Have_EducationOrganizationId_As_Terminal_Step_If_EdOrgRef_Id_Missing()
        {
            var serializedMap = this.SerializedMapFor(STAFFPROGRAMASSOCIATION, EDUCATIONORGANIZATIONID);
            var sut = new XPathMapBuilder();
            var map = sut.DeserializeMap(serializedMap) as IReferenceStep;
            var edOrgRefStep = map.GetNextStep();
            var edOrgIdStep = edOrgRefStep.GetNextStep();
            edOrgIdStep.GetXPath()[1].ShouldEqual(EDUCATIONORGANIZATIONID);
        }

        [Test]
        public void Returned_Xsd_Object_Should_For_Eductaion_Organization_Should_Have_String_Property_Type()
        {
            
        }

        [Test]
        public void Deserialized_Map_Should_Have_School_As_An_Available_Reference_Target()
        {
            var serializedMap = this.SerializedMapFor(STAFFPROGRAMASSOCIATION, EDUCATIONORGANIZATIONID);
            var sut = new XPathMapBuilder();
            var map = sut.DeserializeMap(serializedMap) as IReferenceStep;
            var edOrgRefStep = map.GetNextStep() as IReferenceStep;
            edOrgRefStep.GetTargetTypeNames().ShouldContain("School");
        }

        [Test]
        public void Deserialized_Map_Should_Have_LEA_As_An_Available_Reference_Target()
        {
            var serializedMap = this.SerializedMapFor(STAFFPROGRAMASSOCIATION, EDUCATIONORGANIZATIONID);
            var sut = new XPathMapBuilder();
            var map = sut.DeserializeMap(serializedMap) as IReferenceStep;
            var edOrgRefStep = map.GetNextStep() as IReferenceStep;
            edOrgRefStep.GetTargetTypeNames().ShouldContain("LocalEducationAgency");
        }

        [Test]
        public void Deserialized_Map_Should_Have_ESC_As_An_Available_Reference_Target()
        {
            var serializedMap = this.SerializedMapFor(STAFFPROGRAMASSOCIATION, EDUCATIONORGANIZATIONID);
            var sut = new XPathMapBuilder();
            var map = sut.DeserializeMap(serializedMap) as IReferenceStep;
            var edOrgRefStep = map.GetNextStep() as IReferenceStep;
            edOrgRefStep.GetTargetTypeNames().ShouldContain("EducationServiceCenter");
        }

        [Test]
        public void Deserialized_Map_Should_Have_EdOrgNetwork_As_An_Available_Reference_Target()
        {
            var serializedMap = this.SerializedMapFor(STAFFPROGRAMASSOCIATION, EDUCATIONORGANIZATIONID);
            var sut = new XPathMapBuilder();
            var map = sut.DeserializeMap(serializedMap) as IReferenceStep;
            var edOrgRefStep = map.GetNextStep() as IReferenceStep;
            edOrgRefStep.GetTargetTypeNames().ShouldContain("EducationOrganizationNetwork");
        }

        [Test]
        public void Deserialized_Map_Should_Have_State_Agency_As_An_Available_Reference_Target()
        {
            var serializedMap = this.SerializedMapFor(STAFFPROGRAMASSOCIATION, EDUCATIONORGANIZATIONID);
            var sut = new XPathMapBuilder();
            var map = sut.DeserializeMap(serializedMap) as IReferenceStep;
            var edOrgRefStep = map.GetNextStep() as IReferenceStep;
            edOrgRefStep.GetTargetTypeNames().ShouldContain("StateEducationAgency");
        }
    }
}