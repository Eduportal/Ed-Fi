using EdFi.Ods.Api.Models.Resources;

namespace EdFi.Ods.Api.Data.Repositories.BulkOperations
{
    public interface ICreateBulkOperationAndGetById
    {
        BulkOperationResource GetByIdAndYear(string id, int year);
        void Execute(CreateBulkOperationCommand command);

    }
}