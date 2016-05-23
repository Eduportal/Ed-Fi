using System;
using System.Data.SqlClient;
using EdFi.Common.Database;
using EdFi.Ods.Api.Models.Resources.Enums;

namespace EdFi.Ods.Api.Data.Repositories.BulkOperations
{
    public class BulkOperationsFileChunkWriter : IBulkOperationsFileChunkCreator, IVarbinaryWriter
    {
        private readonly IDatabaseConnectionStringProvider bulkOperationsDatabaseConnectionStringProvider;

        public BulkOperationsFileChunkWriter(IDatabaseConnectionStringProvider bulkOperationsDatabaseConnectionStringProvider)
        {
            this.bulkOperationsDatabaseConnectionStringProvider = bulkOperationsDatabaseConnectionStringProvider;
        }

        public string CreateChunk(string uploadFileId, long offset, long size)
        {
            var id = Guid.NewGuid().ToString();

            using (var connection = new SqlConnection(bulkOperationsDatabaseConnectionStringProvider.GetConnectionString()))
            using (var cmd = new SqlCommand())
            {
                cmd.Connection = connection;
                cmd.CommandText = @"UPDATE [dbo].[UploadFiles] SET [Status] = @status WHERE [Id] = @uploadFileId AND [Status] != @status;
                                    INSERT INTO [dbo].[UploadFileChunks] ([Id]
                                            ,[Offset]
                                            ,[Size]
                                            ,[Chunk]
                                            ,[UploadFile_Id])
                                        VALUES
                                            (@id
                                            ,@offset
                                            ,@size
                                            ,0x0
                                            ,@uploadFileId)";

                cmd.Parameters.Add(new SqlParameter("@status", (int) UploadFileStatus.Incomplete));
                cmd.Parameters.Add(new SqlParameter("@id", id));
                cmd.Parameters.Add(new SqlParameter("@offset", offset));
                cmd.Parameters.Add(new SqlParameter("@size", size));
                cmd.Parameters.Add(new SqlParameter("@uploadFileId", uploadFileId));
                connection.Open();
                cmd.ExecuteNonQuery();
            }
            return id;
        }

        public bool VerifyChunkSize(string id, long size)
        {
            using (var connection = new SqlConnection(bulkOperationsDatabaseConnectionStringProvider.GetConnectionString()))
            using (var cmd = new SqlCommand())
            {
                cmd.Connection = connection;
                cmd.CommandText = @" SELECT CAST(CASE WHEN @size = DATALENGTH([Chunk]) THEN 1 ELSE 0 END AS bit)
                                     FROM [dbo].[UploadFileChunks]
                                     WHERE [Id] = @id";
                cmd.Parameters.Add(new SqlParameter("@id", id));
                cmd.Parameters.Add(new SqlParameter("@size", size));
                connection.Open();
                return (bool)cmd.ExecuteScalar();
            }
        }

        public void Write(string id, byte[] bytes, int offset, int count)
        {
            using (var connection = new SqlConnection(bulkOperationsDatabaseConnectionStringProvider.GetConnectionString()))
            using (var cmd = new SqlCommand())
            {
                cmd.Connection = connection;
                if (offset == 0)
                {
                    // for the first write we just send the bytes to the Column
                    cmd.CommandText = @"UPDATE [dbo].[UploadFileChunks]
                                                    SET [Chunk] = @firstchunk 
                                                WHERE [Id] = @id";
                    cmd.Parameters.Add(new SqlParameter("@firstchunk", bytes));
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                }
                else
                {
                    // for all updates after the first one we use the TSQL command .WRITE() to append the data in the database
                    cmd.CommandText = @"UPDATE [dbo].[UploadFileChunks]
                                                SET [Chunk].WRITE(@chunk, NULL, @length)
                                            WHERE [Id] = @id";
                    cmd.Parameters.Add(new SqlParameter("@chunk", bytes));
                    cmd.Parameters.Add(new SqlParameter("@length", count));
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                }
                connection.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}