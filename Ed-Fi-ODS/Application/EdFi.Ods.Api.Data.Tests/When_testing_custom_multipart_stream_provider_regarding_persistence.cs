using System.IO;
using System.Linq;
using EdFi.Common.Database;
using EdFi.Ods.Api.Data.Contexts;
using EdFi.Ods.Api.Data.Repositories.BulkOperations;
using EdFi.Ods.Api.Models.Resources.Enums;
using EdFi.Ods.Api.Services.Providers;
using EdFi.Ods.Common;
using NUnit.Framework;
using Should;
using EdFi.Ods.Tests._Bases;

namespace EdFi.Ods.Api.Data.Tests
{
     [TestFixture]
    public class When_testing_custom_multipart_stream_provider_regarding_persistence : TestFixtureBase
    {
        private BulkOperation insertedBulkOperation;
        private string uploadId;
        private Stream _stream;
        private BulkOperationsFileChunkWriter _bulkOperationsFileChunkWriter;
        private string _chunkId;

        // Arrange
        protected override void Arrange()
        {
            insertedBulkOperation = BulkOperationEfHelper.InsertSampleBulkOperation();
            _bulkOperationsFileChunkWriter =
                new BulkOperationsFileChunkWriter(new NamedDatabaseConnectionStringProvider("BulkOperationDbContext"));

            uploadId = insertedBulkOperation.UploadFiles.Single(x => x.InterchangeType == InterchangeType.Descriptors.Name).Id;
            _chunkId = _bulkOperationsFileChunkWriter.CreateChunk(uploadId, 100, 164);
            var mpsp = new CustomMultipartStreamProvider(_chunkId, _bulkOperationsFileChunkWriter);
            _stream = mpsp.GetStream(null, null);
        }

        // Act
        protected override void Act()
        {
            _stream.Write(new byte[164], 0, 164);
        }

        #region Assert

        [Test]
        public void Should_verify_chunk_size()
        {
            _bulkOperationsFileChunkWriter.VerifyChunkSize(_chunkId, 164).ShouldBeTrue();
        }

        [Test]
        public void Should_insert_a_new_chunk_when_add_a_content()
        {
            UploadFileChunk chunk;
            using (var context = new BulkOperationDbContext())
            {
                var actual = context.UploadFiles.Single(x => x.Id == uploadId);
                chunk = actual.UploadFileChunks.Single(x => x.Offset == 100);
            }
            chunk.Id.ShouldNotBeNull();
            chunk.Offset.ShouldEqual(100);
            chunk.Size.ShouldEqual(164);
            chunk.Chunk.Length.ShouldEqual(164);
        }

        [Test]
        public void Should_update_file_Status_to_incomplete_when_add_a_content()
        {
            UploadFile file;
            using (var context = new BulkOperationDbContext())
            {
                file = context.UploadFiles.Single(x => x.Id == uploadId);
            }
            file.Status.ShouldEqual(UploadFileStatus.Incomplete);
        }

        #endregion
    }
}