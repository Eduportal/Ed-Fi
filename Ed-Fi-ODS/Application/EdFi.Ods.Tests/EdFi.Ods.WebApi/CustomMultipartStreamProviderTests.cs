using EdFi.Ods.Api.Data.Repositories.BulkOperations;

namespace EdFi.Ods.Tests.EdFi.Ods.WebApi
{
    using System;
    using System.Net.Http;

    using global::EdFi.Ods.Api.Data;
    using global::EdFi.Ods.Api.Services.Providers;
    using global::EdFi.Ods.Tests.TestObjects;

    using NUnit.Framework;

    using Should;

    [TestFixture]
    public class CustomMultipartStreamProviderTests
    {
        private TestBulkOperationsContext _db;      
        [Test]
        public void When_size_of_contents_matches_size_sent_in_Should_return_true()
        {
            //Arrange
            this._db = new TestBulkOperationsContext();
            var executor = new TestDbExecutor(this._db);
            var idString = Guid.NewGuid().ToString();
            var uploadFile = new UploadFile{Id = idString};
            this._db.UploadFiles.Add(uploadFile);
            var contentSize = 115655;
            var testBulkOperationsFileChunkWriter = new TestBulkOperationsFileChunkWriter();
            var mpsp = new CustomMultipartStreamProvider(idString, testBulkOperationsFileChunkWriter);
            var stream = mpsp.GetStream(null, null);
            stream.Write(new byte[contentSize], 0, contentSize);
            //Act
            var results = testBulkOperationsFileChunkWriter.VerifyChunkSize(string.Empty, contentSize);
            //Assert
            results.ShouldBeTrue();
        }

        [Test]
        public void When_size_of_contents_does_not_match_size_sent_in_Should_return_false()
        {
            //Arrange
            var idString = Guid.NewGuid().ToString();
            var contentSize = 115655;
            var someOtherContentSize = 10;

            var testBulkOperationsFileChunkWriter = new TestBulkOperationsFileChunkWriter();

            var mpsp = new CustomMultipartStreamProvider(idString, testBulkOperationsFileChunkWriter);
            var stream = mpsp.GetStream(null, null);
            stream.Write(new byte[contentSize], 0, contentSize);
            //Act
            var results = testBulkOperationsFileChunkWriter.VerifyChunkSize(string.Empty, someOtherContentSize);
            //Assert
            results.ShouldBeFalse();
        }

        private class TestBulkOperationsFileChunkWriter : IBulkOperationsFileChunkCreator, IVarbinaryWriter
        {
            private int _length;
            public string CreateChunk(string uploadFileId, long offset, long size)
            {
                throw new NotImplementedException();
            }

            public bool VerifyChunkSize(string id, long size)
            {
                return _length == size;
            }

            public void Write(string id, byte[] bytes, int offset, int count)
            {
                _length += count;
            }
         }
    }
}
