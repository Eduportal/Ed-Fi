using EdFi.Ods.BulkLoad.Core;
using EdFi.Ods.BulkLoad.Core.Dictionaries;
using EdFi.Ods.Tests.TestObjects;
using EdFi.Ods.Tests.TestObjects.TestXml;
using NUnit.Framework;
using Should;
using System.Linq;
using System.Xml.Linq;

namespace EdFi.Ods.Tests.EdFi.Ods.BulkLoad.Core.IndexedXmlFileReaderTests
{
    using _Bases;

    /// <summary>
    /// These tests actually test both the XmlGPS and the IndexedXmlFileReader because it is easier to 
    /// setup a manually produced test map and hand it to the XmlGPS than to mock the XmlGPS recusive
    /// behavior.  There are tests of the XmlGPS independent of the IndexedXmlFileReader.
    /// 
    /// Yes, this comment is an apology so if you know how to isolate the IndexedXmlFileReader please
    /// do so (and yes, you have my permission to smirk and make remarks about my intelligence or lack 
    /// thereof the whole time you're doing it).
    /// </summary>
    [TestFixture]
    public class When_finding_foreign_key_xElements : XmlFileTestBase
    {
        [Test]
        [Ignore("Test data is location specific")]
        public void And_Given_Valid_Starting_Element_With_Identity_In_Element_And_Map_Should_Return_Value_Element()
        {
            var builder = new TestStreamBuilder(() => GrandBendTestXml.EdOrg);
            var sut = new IndexedXmlFileReader("This DOes Not Matter For Test", builder, GetProgramGPS, new InMemoryDbDictionary());
            var elementToTestWith = sut.GetNodesByEntity("Program")
                    .First(e => e.Attribute(XName.Get("id")).Value == GrandBendTestXml.IdForProgramWithEdOrgIdentityElement);
            var foreignKeyValueElement = sut.FindForeignKeyElement(elementToTestWith, "This just can't be an empty string - not used in test");
            foreignKeyValueElement.Value.ShouldEqual(GrandBendTestXml.ReferencedEdOrgIdInProgram);
        }

        [Test]
        [Ignore("Test data is location specific")]
        public void
            And_Given_Valid_Starting_Element_With_Reference_Id_In_Reference_Element_And_Map_Should_Return_Value_Element()
        {
            var builder = new TestStreamBuilder(() => GrandBendTestXml.EdOrg);
            var sut = new IndexedXmlFileReader("This does not matter for test", builder, GetSchoolGPS, new InMemoryDbDictionary());
            var elementToTestWith =
                sut.GetNodesByEntity("School")
                    .First(e => e.Attribute(XName.Get("id")).Value == GrandBendTestXml.IdForSchoolWithRefIdToLEAElement);
            var foreignKeyValueElement = sut.FindForeignKeyElement(elementToTestWith, "This just can't be an empty string - not used in test");
            foreignKeyValueElement.Value.ShouldEqual(GrandBendTestXml.ReferencedLEAIdInSchool);
        }

        [Test]
        public void If_Map_Is_Empty_Should_Return_null()
        {
            var builder = new TestStreamBuilder(() => GrandBendTestXml.EdOrg);
            var sut = new IndexedXmlFileReader("Bob", builder,
                GetSerializedMapBuilder, new InMemoryDbDictionary());
            var elementToTestWith =
                sut.GetNodesByEntity("School")
                    .First(e => e.Attribute(XName.Get("id")).Value == GrandBendTestXml.IdForSchoolWithRefIdToLEAElement);
            sut.FindForeignKeyElement(elementToTestWith, string.Empty).ShouldBeNull();
        }

        [Test]
        [Ignore("Test data is location specific")]
        public void Should_Find_Descriptor_Extended_References_On_Base_Element()
        {
            var builder = new TestStreamBuilder(() => GrandBendTestXml.EdOrg);
            var sut = new IndexedXmlFileReader("pz", builder, GetSerializedMapBuilder, new InMemoryDbDictionary());
            var elementForTest =
                sut.GetNodesByEntity("School")
                    .First(e => e.Attribute(XName.Get("id")).Value == GrandBendTestXml.IdForSchoolWithRefIdToLEAElement);
            const string map = @"{""IsReference"":true,""Parent"":null,""ElementName"":""CodeValue"",""ParentElement"":""AdministrativeFundingControl"",""IsEdOrgRef"":false}";
            var codeValue = (string)sut.FindForeignKeyElement(elementForTest, map);
            codeValue.ShouldEqual("Public School");
        }
    }
}