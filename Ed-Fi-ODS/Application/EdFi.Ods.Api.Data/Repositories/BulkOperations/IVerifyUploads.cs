
namespace EdFi.Ods.Api.Data.Repositories.BulkOperations
{
    public interface IVerifyUploads
    {
        UploadValidationErrors IsValid(string uploadId);
        UploadValidationErrors IsValid(string uploadId, long offset, long size);
    }

    public class UploadValidationErrors
    {
        public bool Any { get; private set; }
        public string ErrorMessage { get; private set; }

        public UploadValidationErrors(string errorMessage)
        {
            Any = !string.IsNullOrWhiteSpace(errorMessage);
            ErrorMessage = errorMessage;
        }
    }
}