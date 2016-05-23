using System;
using System.Data.Entity;
using System.Linq;
using EdFi.Ods.Api.Data.Contexts;
using EdFi.Ods.Api.Data;
using EdFi.Ods.Api.Models.Resources.Enums;

namespace EdFi.Ods.BulkLoad.Core.Data
{
    public class FindBulkOperations : IFindBulkOperations
    {
        private readonly Func<IBulkOperationDbContext> _createContext;

        public FindBulkOperations(Func<IBulkOperationDbContext> createContext)
        {
            _createContext = createContext;
        }

        public BulkOperation FindAndStart(string id)
        {
            using (var context = _createContext())
            {
                var operation = FindWithFiles(id, context);
                operation.Status = BulkOperationStatus.Started;
                context.SaveChanges();
                return operation;
            }
        }

        public BulkOperation FindWithFiles(string id)
        {
            using (var context = _createContext())
            {
                return FindWithFiles(id, context);
            }
        }

        public BulkOperation FindWithoutFiles(string id)
        {
            using (var context = _createContext())
            {
                var operation = context.BulkOperations.Single(x => x.Id == id);
                operation.UploadFiles = new UploadFile[0];
                return operation;
            }
        }

        private static BulkOperation FindWithFiles(string id, IBulkOperationDbContext context)
        {
            var operation = context.BulkOperations.AsQueryable().Include(x => x.UploadFiles).Single(x => x.Id == id);
            return operation;
        }
    }
}