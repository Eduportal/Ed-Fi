using System.Linq;
using EdFi.Ods.Common.Context;
using EdFi.Ods.Common.Security;
using EdFi.Ods.Api.Common.Authorization;
using FluentValidation;

namespace EdFi.Ods.Api.Data.Repositories.BulkOperations
{
    public interface ICreateBulkOperationCommandFactory
    {
        CreateBulkOperationCommand Create(BulkOperationCreateRequest request);
    }

    public class CreateBulkOperationCommandFactory : ICreateBulkOperationCommandFactory
    {
        private readonly IValidator<BulkOperationCreateRequest> _bulkOperationCreateValidator;
        private readonly IApiKeyContextProvider apiKeyContextProvider;
        private readonly ISchoolYearContextProvider schoolYearContextProvider;

        public CreateBulkOperationCommandFactory(IValidator<BulkOperationCreateRequest> bulkOperationCreateValidator, IApiKeyContextProvider apiKeyContextProvider, ISchoolYearContextProvider schoolYearContextProvider)
        {
            _bulkOperationCreateValidator = bulkOperationCreateValidator;
            this.apiKeyContextProvider = apiKeyContextProvider;
            this.schoolYearContextProvider = schoolYearContextProvider;
        }

        public CreateBulkOperationCommand Create(BulkOperationCreateRequest request)
        {
            if(request == null)
                return new CreateBulkOperationCommand
                    {
                        ValidationErrors = new []{"Request is null."}
                    };

            var validationResults = _bulkOperationCreateValidator.Validate(request);

            return new CreateBulkOperationCommand
                {
                    ValidationErrors = validationResults.Errors.Select(x => x.ErrorMessage),
                    ResetDistrictData = request.ResetDistrictData,
                    UploadFiles = request.UploadFiles,
                    ApiKey = apiKeyContextProvider.GetApiKeyContext().ApiKey,
                    SchoolYear = schoolYearContextProvider.GetSchoolYear(),
                };
        }
    }
}