using EdFi.Ods.Tests.TestObjects.TestXml;
using EdFi.Ods.Tests.TestObjects;

namespace EdFi.Ods.Tests.EdFi.Ods.BulkLoad.Core.IndexedXmlFileReaderTests
{
    using System;
    using System.Linq;

    using global::EdFi.Ods.BulkLoad.Core;
    using global::EdFi.Ods.BulkLoad.Core.Dictionaries;
    using global::EdFi.Ods.CodeGen.XmlShredding;
    using global::EdFi.Ods.Tests._Bases;
    using global::EdFi.Ods.XmlShredding;

    using NUnit.Framework;

    using Should;

    [TestFixture]
    [Ignore("Test data is location specific")]
    public class When_Finding_Section_Foreign_Keys_In_A_Referenced_Section_Reference_Element : XmlFileTestBase
    {
        private string serializedMapForClassroomIdentificationCode = @"{""IsParentIdentity"":true,""IsReference"":false,""ReferenceTarget"":null,""Parent"":{""IsParentIdentity"":true,""IsReference"":true,""ReferenceTarget"":""Location"",""Parent"":{""IsParentIdentity"":false,""IsReference"":true,""ReferenceTarget"":""Section"",""Parent"":null,""ElementName"":""SectionReference"",""ParentElement"":null,""IsEdOrgRef"":false},""ElementName"":""LocationReference"",""ParentElement"":""SectionIdentity"",""IsEdOrgRef"":false},""ElementName"":""ClassroomIdentificationCode"",""ParentElement"":""LocationIdentity"",""IsEdOrgRef"":false}";
        [Test]
        [Ignore("Test data is location specific")]
        public void Should_Navigate_To_And_Find_The_Values_From_The_Reference()
        {
            var builder = new TestStreamBuilder(() => TNTestXml.StudentEnrollmentWithSectionReference);
            var gpsBuilder = new Func<string, string, IXmlGPS>((map, prefix) => new XmlGPS(new XPathMapBuilder().DeserializeMap(map), prefix));
            var sut = new IndexedXmlFileReader("Not Important For This Test", builder, gpsBuilder, new InMemoryDbDictionary());
            var xelement = sut.GetNodesByEntity("StudentSectionAssociation").Single();
            var classroomIdentificationCode = sut.FindForeignKeyElement(xelement,
                this.serializedMapForClassroomIdentificationCode);
            classroomIdentificationCode.ValueOrDefault<string>().ShouldEqual("101");
        }
    }
}
