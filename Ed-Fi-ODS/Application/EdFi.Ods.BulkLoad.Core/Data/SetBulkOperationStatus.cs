using System;
using System.Linq;
using EdFi.Ods.Api.Data.Contexts;
using EdFi.Ods.Api.Models.Resources.Enums;

namespace EdFi.Ods.BulkLoad.Core.Data
{
    public class SetBulkOperationStatus : ISetBulkOperationStatus
    {
        private readonly Func<IBulkOperationDbContext> _createContext;

        public SetBulkOperationStatus(Func<IBulkOperationDbContext> createContext)
        {
            _createContext = createContext;
        }

        public void SetStatus(string id, BulkOperationStatus status)
        {
            using (var context = _createContext())
            {
                var operation = context.BulkOperations.Single(x => x.Id == id);
                operation.Status = status;
                context.SaveChanges();
            }
        }
    }
}