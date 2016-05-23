using System;
using System.Linq;
using EdFi.Ods.Api.Data.Contexts;
using EdFi.Ods.Api.Data;

namespace EdFi.Ods.BulkLoad.Core.Data
{
    public class FindBulkOperationExceptions : IFindBulkOperationExceptions
    {
        private readonly Func<IBulkOperationDbContext> _createContext;

        public FindBulkOperationExceptions(Func<IBulkOperationDbContext> createContext)
        {
            _createContext = createContext;
        }

        public BulkOperationException[] FindByUploadFile(string uploadFileId)
        {
            using(var context = _createContext())
            {
                return context.BulkOperationExceptions.Where(x => x.ParentUploadFileId == uploadFileId).ToArray();
            }
        }
    }
}