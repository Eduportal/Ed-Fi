using System;
using EdFi.Ods.Api.Data.Contexts;
using EdFi.Ods.Api.Data;

namespace EdFi.Ods.BulkLoad.Core.Data
{
    public class CreateBulkOperation : ICreateBulkOperation
    {
        private readonly Func<IBulkOperationDbContext> _createContext;

        public CreateBulkOperation(Func<IBulkOperationDbContext> createContext)
        {
            _createContext = createContext;
        }

        public void Create(BulkOperation operation)
        {
            using (var context = _createContext())
            {
                context.BulkOperations.Add(operation);
                context.SaveChanges();
            }
        }
    }
}