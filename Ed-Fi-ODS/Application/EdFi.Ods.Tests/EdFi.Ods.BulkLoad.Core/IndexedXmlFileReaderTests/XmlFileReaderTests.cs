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
    public class when_getting_invalid_node_by_entity_on_an_xmlfilereader : XmlFileTestBase
    {
        [Test]
        public void it_should_return_null()
        {
            var filename = FileBuilder.Create();
            var filestreambuilder = new FileStreamBuilder();
            var reader = new IndexedXmlFileReader(filename, filestreambuilder, this.Stub<Func<string, string, IXmlGPS>>(), new InMemoryDbDictionary());

            var node = reader.GetNodesByEntity("unknownNode").ToArray();
            node.Length.ShouldEqual(0);
            FileBuilder.Delete();
        }
    }
}