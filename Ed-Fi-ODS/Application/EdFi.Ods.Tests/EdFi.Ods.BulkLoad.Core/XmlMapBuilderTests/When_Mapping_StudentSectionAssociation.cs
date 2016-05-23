namespace EdFi.Ods.Tests.EdFi.Ods.BulkLoad.Core.XmlMapBuilderTests
{
    using global::EdFi.Ods.CodeGen.XmlShredding;
    using global::EdFi.Ods.Tests._Bases;

    using NUnit.Framework;

    using Should;

    [TestFixture]
    public class When_Mapping_StudentSectionAssociation : SchemaMetadataTestBase
    {
        [Test]
        public void Should_Create_Map_To_ClassroomIdentification_Code()
        {
            var sut = new XPathMapBuilder();
            var initialStep =
                sut.DeserializeMap(
                    @"{""IsParentIdentity"":true,""IsReference"":false,""ReferenceTarget"":null,""Parent"":{""IsParentIdentity"":true,""IsReference"":true,""ReferenceTarget"":""Location"",""Parent"":{""IsParentIdentity"":false,""IsReference"":true,""ReferenceTarget"":""Section"",""Parent"":null,""ElementName"":""SectionReference"",""ParentElement"":null,""IsEdOrgRef"":false},""ElementName"":""LocationReference"",""ParentElement"":""SectionIdentity"",""IsEdOrgRef"":false},""ElementName"":""ClassroomIdentificationCode"",""ParentElement"":""LocationIdentity"",""IsEdOrgRef"":false}") as IReferenceStep;
            initialStep.ReferencePointedToAggregateType("SectionReference", string.Empty);
            initialStep.GetNextStep().GetXPath().ShouldContain("LocationReference");
        }
    }
}