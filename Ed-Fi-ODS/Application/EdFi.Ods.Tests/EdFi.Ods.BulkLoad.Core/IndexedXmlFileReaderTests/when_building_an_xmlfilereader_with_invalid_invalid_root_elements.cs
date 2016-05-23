namespace EdFi.Ods.Tests.EdFi.Ods.BulkLoad.Core.IndexedXmlFileReaderTests
{
    using System;
    using System.Linq;

    using global::EdFi.Ods.BulkLoad.Core;
    using global::EdFi.Ods.BulkLoad.Core.Dictionaries;
    using global::EdFi.Ods.Tests._Bases;

    using NUnit.Framework;

    using Should;

    [TestFixture]
    public class when_building_an_xmlfilereader_with_invalid_invalid_root_elements : XmlFileTestBase
    {
        [Test]
        public void it_should_succeed_and_index_valid_elements()
        {
            var filename = FileBuilder.Create();
            var sb = new FileStreamBuilder();
            var reader = new IndexedXmlFileReader(filename, sb, this.Stub<Func<string, string, IXmlGPS>>(), new InMemoryDbDictionary());
            
            var node = reader.GetNodesByEntity("firstLevelNode").ToArray();
            node.Length.ShouldEqual(1);
            node[0].Name.ShouldEqual("firstLevelNode");
            FileBuilder.Delete();
        }
    }
}