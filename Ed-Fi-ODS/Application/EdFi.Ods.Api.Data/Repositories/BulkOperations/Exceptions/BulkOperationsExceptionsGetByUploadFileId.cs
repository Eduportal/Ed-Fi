using System.Linq;
using EdFi.Ods.Api.Common;
using EdFi.Ods.Api.Data.Contexts;
using EdFi.Ods.Api.Data.Mappings;
using EdFi.Ods.Api.Models.Resources;

namespace EdFi.Ods.Api.Data.Repositories.BulkOperations.Exceptions
{
    public class BulkOperationsExceptionsGetByUploadFileId : IBulkOperationsExceptionsGetByUploadFileId
    {
        private readonly IDbExecutor<IBulkOperationDbContext> _executor;

        public BulkOperationsExceptionsGetByUploadFileId(IDbExecutor<IBulkOperationDbContext> executor)
        {
            _executor = executor;
        }

        public Models.Resources.BulkOperationException[] GetByUploadFileId(string id, QueryParameters parameters)
        {
            var entities =
                _executor.Get(
                    ctx =>
                        ctx.BulkOperationExceptions.Where(ex => ex.ParentUploadFileId == id).OrderBy(x => x.DateTime)
                            .Skip(parameters.Offset ?? 0).Take(parameters.Limit ?? 25).ToArray());

            return entities.Select(x => x.ToResource()).ToArray();
        }
    }
}
