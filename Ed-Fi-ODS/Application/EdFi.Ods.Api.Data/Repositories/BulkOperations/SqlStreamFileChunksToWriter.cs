using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EdFi.Common.Database;

namespace EdFi.Ods.Api.Data.Repositories.BulkOperations
{
    public class SqlStreamFileChunksToWriter : IStreamFileChunksToWriter
    {
        private readonly IDatabaseConnectionStringProvider bulkOperationsDatabaseConnectionStringProvider;

        public SqlStreamFileChunksToWriter(IDatabaseConnectionStringProvider bulkOperationsDatabaseConnectionStringProvider)
        {
            this.bulkOperationsDatabaseConnectionStringProvider = bulkOperationsDatabaseConnectionStringProvider;
        }

        public void Write(string uploadFileId, Stream writer)
        {
            using (var connection = new SqlConnection(bulkOperationsDatabaseConnectionStringProvider.GetConnectionString()))
            using (var cmd = new SqlCommand())
            {
                cmd.CommandTimeout = 60 * 5;
                cmd.Connection = connection;
                cmd.CommandText = @"SELECT [Chunk] FROM [dbo].[UploadFileChunks] where UploadFile_Id = @uploadFileId order by Offset asc";
                cmd.Parameters.Add(new SqlParameter("@uploadFileId", uploadFileId));
                connection.Open();

                using (var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess))
                {
                    while (reader.Read())
                    {
                        if (reader.IsDBNull(0))
                            continue;

                        var buffer = new byte[4000000];
                        using (var stream = reader.GetStream(0))
                        {
                            var bytesRead = 0;
                            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                writer.Write(buffer, 0, bytesRead);
                                writer.Flush();
                            }
                        }
                    }
                }//reader, fs
            }//connection, cmd
        }
    }
}
