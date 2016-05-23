using System.Linq;

namespace EdFi.Ods.Tests.TestObjects
{
    using System.Data.Entity;

    using global::EdFi.Ods.Api.Data;
    using global::EdFi.Ods.Api.Data.Contexts;
    using global::EdFi.Ods.Api.Data.Model;

    public class TestBulkOperationsContext : IBulkOperationDbContext
    {
        public TestBulkOperationsContext()
        {
            this.BulkOperations = new TestBulkOperationSet();
            this.UploadFiles = new TestUploadFilesSet();
            this.UploadFileChunks = new TestDbSet<UploadFileChunk>();
            this.BulkOperationExceptions = new TestDbSet<BulkOperationException>();
        }

        public void Dispose()
        {
            //I'm a dummy.  Nothing to dispose
        }

        public int SaveChanges()
        {
            this.SaveChangesCount++;
            return 1;
        }

        public int SaveChangesCount { get; private set; }
        public IDbSet<BulkOperation> BulkOperations { get; set; }
        public IDbSet<UploadFile> UploadFiles { get; set; }
        public IDbSet<UploadFileChunk> UploadFileChunks { get; set; }
        public IDbSet<BulkOperationException> BulkOperationExceptions { get; set; }

        public void DeleteUploadFileChunks(string uploadFileId)
        {
            //Never include UploadFileChunks in production
            var file = this.UploadFiles.AsQueryable().Include("UploadFileChunks").Single(x => x.Id == uploadFileId);
            file.UploadFileChunks.Clear();
        }


        public System.Collections.Generic.List<UploadFileChunkInfo> GetFileChunkInfos(string uploadFileId)
        {
            var file = this.UploadFiles.AsQueryable().Include("UploadFileChunks").Single(x => x.Id == uploadFileId);
            return
                file.UploadFileChunks.Select(
                    x =>
                        new UploadFileChunkInfo()
                        {
                            ChunkDataLength = x.Chunk.Length,
                            Id = x.Id,
                            Offset = x.Offset,
                            Size = x.Size,
                            UploadFile_Id = x.UploadFile_Id
                        }).ToList();
        }


        public string ConnectionString
        {
            get { return "TheConnectionString"; }
        }
    }
}