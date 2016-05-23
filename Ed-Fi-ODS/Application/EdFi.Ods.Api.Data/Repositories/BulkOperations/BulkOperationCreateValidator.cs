using FluentValidation;

namespace EdFi.Ods.Api.Data.Repositories.BulkOperations
{
    public class BulkOperationCreateValidator : AbstractValidator<BulkOperationCreateRequest>
    {
        public BulkOperationCreateValidator()
        {
            RuleFor(x => x.UploadFiles)
                .NotEmpty()
                .SetCollectionValidator(new UploadFileRequestValidator());

        }
    }
}