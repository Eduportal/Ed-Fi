namespace EdFi.Ods.Tests.EdFi.Ods.BulkLoad.Core.IndexedXmlFileReaderTests
{
    using System;
    using System.IO;

    using global::EdFi.Ods.BulkLoad.Core;
    using global::EdFi.Ods.BulkLoad.Core.Dictionaries;
    using global::EdFi.Ods.Tests._Bases;

    using NUnit.Framework;

    [TestFixture]
    public class when_building_an_xmlfilereader_with_invalid_file : XmlFileTestBase
    {
        [Test]
        [ExpectedException(typeof(DirectoryNotFoundException))]
        public void it_should_throw_a_directory_not_found_exception()
        {
            var filename = Path.Combine(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()), "testing.xml");
            var fileStreamBuilder = new FileStreamBuilder();
            var reader = new IndexedXmlFileReader(filename, fileStreamBuilder, this.Stub<Func<string, string, IXmlGPS>>(), new InMemoryDbDictionary());
        }
    }
}
