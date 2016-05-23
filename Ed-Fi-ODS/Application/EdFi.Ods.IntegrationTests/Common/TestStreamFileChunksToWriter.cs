using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EdFi.Ods.Api.Data.Contexts;
using EdFi.Ods.Api.Data.Repositories.BulkOperations;

namespace EdFi.Ods.IntegrationTests.Common
{
    public class TestStreamFileChunksToWriter : IStreamFileChunksToWriter
    {
        private IBulkOperationDbContext bulkOperationDbContext;
        public TestStreamFileChunksToWriter(IBulkOperationDbContext bulkOperationDbContext)
        {
            this.bulkOperationDbContext = bulkOperationDbContext;
        }

        public void Write(string uploadFileId, Stream writer)
        {
            //NEVER DO THIS IN PRODUCTION - Large Files will cause performance issues and out of memory exceptions to occur. 
            //This implementation exists in IntegrationTests project on purpose.
            var file = this.bulkOperationDbContext.UploadFiles.Include("UploadFileChunks").Single(x => x.Id == uploadFileId);
            var chunks = file.UploadFileChunks.OrderBy(x => x.Offset).SelectMany(x => x.Chunk).ToArray();

            var buffer = new byte[2048];
            using (var mem = new MemoryStream(chunks))
            {
                var bytesRead = 0;
                while ((bytesRead = mem.Read(buffer, 0, buffer.Length)) > 0)
                {
                    writer.Write(buffer, 0, bytesRead);
                    writer.Flush();
                }
            }
        }
    }
}
