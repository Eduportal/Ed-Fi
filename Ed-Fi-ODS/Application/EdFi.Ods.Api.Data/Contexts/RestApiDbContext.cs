using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using EdFi.Ods.Api.Data;
using EdFi.Ods.Api.Data.Model;

namespace EdFi.Ods.Api.Data.Contexts
{
    public class BulkOperationDbContext : DbContext, IBulkOperationDbContext, IEventLogDbContext
    {
        public BulkOperationDbContext() : base("BulkOperationDbContext")
        {            
        }

        public virtual IDbSet<BulkOperation> BulkOperations { get; set; }
        public virtual IDbSet<UploadFile> UploadFiles { get; set; }
        public virtual IDbSet<BulkOperationException> BulkOperationExceptions { get; set; }
        public virtual IDbSet<ApiEventLogEntry> EventLogEntries { get; set; }
        public string ConnectionString
        {
            get { return this.Database.Connection.ConnectionString; }
        }

        public void DeleteUploadFileChunks(string uploadFileId)
        {
            try
            {
                this.Database.CommandTimeout = 60 * 5;//Wait up to 5 minutes.
                this.Database.ExecuteSqlCommand(TransactionalBehavior.EnsureTransaction,
                    "DELETE FROM UploadFileChunks WHERE UploadFile_Id = @uploadFileId",
                    new SqlParameter("@uploadFileId", uploadFileId));
            }
            finally
            {
                this.Database.CommandTimeout = null;
            }
        }

        public List<UploadFileChunkInfo> GetFileChunkInfos(string uploadFileId)
        {
            return this.Database.SqlQuery<UploadFileChunkInfo>(
                "SELECT [Id] ,[Offset] ,[Size], DataLength([Chunk]) as ChunkDataLength, [UploadFile_Id] FROM [dbo].[UploadFileChunks] where UploadFile_Id = @uploadFileId", new SqlParameter("uploadFileId", uploadFileId))
                .ToList();
        }
    }
}
