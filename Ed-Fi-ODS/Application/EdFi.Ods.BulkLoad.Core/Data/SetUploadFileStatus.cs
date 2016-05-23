using System;
using EdFi.Ods.Api.Data.Contexts;
using EdFi.Ods.Api.Models.Resources.Enums;

namespace EdFi.Ods.BulkLoad.Core.Data
{
    public class SetUploadFileStatus : ISetUploadFileStatus
    {
        private readonly Func<IBulkOperationDbContext> _createContext;

        public SetUploadFileStatus(Func<IBulkOperationDbContext> createContext)
        {
            _createContext = createContext;
        }

        public void SetStatus(string fileId, UploadFileStatus status)
        {
            using (var context = _createContext())
            {
                var bf = context.UploadFiles.Find(fileId);
                bf.Status = status;
                context.SaveChanges();
            }
        }
    }
}