using System;
using System.Data.Entity;
using System.Linq;
using EdFi.Ods.Api.Data.Contexts;

namespace EdFi.Ods.BulkLoad.Core.Data
{
    public class DeleteUploadFileChunks : IDeleteUploadFileChunks
    {        
        private readonly Func<IBulkOperationDbContext> _createContext;

        public DeleteUploadFileChunks(Func<IBulkOperationDbContext> createContext)
        {
            _createContext = createContext;
        }

        public void DeleteByUploadFileId(string uploadFileId)
        {
            using (var context = _createContext())
            {
                //Using entity framework causes all chunks to be loaded in memory before they can be freed.  
                //This was causing timeouts and out of memory issues when data sets are large, and database is under heavy load.
                context.DeleteUploadFileChunks(uploadFileId);
            }
        }
    }
}