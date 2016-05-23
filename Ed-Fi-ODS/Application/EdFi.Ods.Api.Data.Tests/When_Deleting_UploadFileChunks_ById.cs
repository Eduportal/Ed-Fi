using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EdFi.Common.Database;
using EdFi.Ods.Api.Data.Contexts;
using EdFi.Ods.Api.Data.Repositories.BulkOperations;
using EdFi.Ods.Api.Models.Resources.Enums;
using EdFi.Ods.Common;
using NUnit.Framework;

namespace EdFi.Ods.Api.Data.Tests
{
    public class When_Deleting_UploadFileChunks_ById
    {
        private BulkOperationsFileChunkWriter _bulkOperationsFileChunkWriter;
        private string _operationId = Guid.NewGuid().ToString();
        private string _fileId1 = Guid.NewGuid().ToString();
        private string _fileId2 = Guid.NewGuid().ToString();
        private int _countOfChunks1, _countOfChunks2;

        [TestFixtureSetUp]
        public void Setup()
        {

            var random = new Random();

            Func<int, byte[]> randomBytes = (count) =>
            {
                var bytes = new byte[count];
                random.NextBytes(bytes);
                return bytes;
            };

            var operation = new BulkOperation
            {
                Status = BulkOperationStatus.Ready,
                Id = _operationId,
                UploadFiles = new[]
                    {
                        new UploadFile {
                            Id = _fileId1, 
                            InterchangeType = InterchangeType.Descriptors.Name
                        },
                        new UploadFile
                        {
                            Id = _fileId2, 
                            InterchangeType = InterchangeType.Descriptors.Name
                        }
                    }
            };

            using (var context = new BulkOperationDbContext())
            {
                context.BulkOperations.Add(operation);
                try
                {
                    context.SaveChanges();
                }
                catch (DbEntityValidationException e)
                {
                    BulkOperationEfHelper.WriteEfValidationErrorsToConsole(e);
                    throw;
                }
            }

            _bulkOperationsFileChunkWriter = new BulkOperationsFileChunkWriter(new NamedDatabaseConnectionStringProvider("BulkOperationDbContext"));

            _bulkOperationsFileChunkWriter.CreateChunk(_fileId1, 0, 3);
            _bulkOperationsFileChunkWriter.CreateChunk(_fileId1, 4, 6);

            _bulkOperationsFileChunkWriter.CreateChunk(_fileId2, 0, 3);

            using (var context = new BulkOperationDbContext())
            {
                _countOfChunks1 = context.UploadFiles.Include(x => x.UploadFileChunks).Where(x => x.Id == _fileId1).Sum(x => x.UploadFileChunks.Count);
                _countOfChunks2 = context.UploadFiles.Include(x => x.UploadFileChunks).Where(x => x.Id == _fileId2).Sum(x => x.UploadFileChunks.Count);
            }
        }

        [Test]
        public void Should_Only_Delete_Chunks_For_SpecificFileId()
        {
            Assert.AreEqual(2, _countOfChunks1);
            Assert.AreEqual(1, _countOfChunks2);

            using (var context = new BulkOperationDbContext())
            {
                context.DeleteUploadFileChunks(_fileId1);
            }

            using (var context = new BulkOperationDbContext())
            {
                _countOfChunks1 = context.UploadFiles.Include(x => x.UploadFileChunks).Where(x => x.Id == _fileId1).Sum(x => x.UploadFileChunks.Count);
                _countOfChunks2 = context.UploadFiles.Include(x => x.UploadFileChunks).Where(x => x.Id == _fileId2).Sum(x => x.UploadFileChunks.Count);
            }

            Assert.AreEqual(0, _countOfChunks1);
            Assert.AreEqual(1, _countOfChunks2);
        }
    }
}