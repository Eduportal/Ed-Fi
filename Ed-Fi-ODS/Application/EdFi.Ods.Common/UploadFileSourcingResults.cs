using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EdFi.Ods.Common
{
    public class UploadFileSourcingResults : IUploadFileSourcingResults
    {
        private UploadFileSourcingResults()
        {
        }

        public string FilePathIfValid { get; private set; }
        public bool IsFailure { get; private set; }
        public string[] ValidationErrorMessages { get; private set; }

        public static UploadFileSourcingResults WithSuccessPath(string filePath)
        {
            return new UploadFileSourcingResults {IsFailure = false, FilePathIfValid = filePath};
        }

        public static UploadFileSourcingResults WithValidationErrorMessages(IEnumerable<string> validationErrorMessages)
        {
            return new UploadFileSourcingResults
                       {
                           IsFailure = true,
                           ValidationErrorMessages = validationErrorMessages.ToArray()
                       };
        }

        public static UploadFileSourcingResults WithValidationErrorMessage(string failureMessage)
        {
            return new UploadFileSourcingResults {IsFailure = true, ValidationErrorMessages = new[] {failureMessage}};
        }

        public void Dispose()
        {
            if (string.IsNullOrWhiteSpace(FilePathIfValid))
                return;
            File.Delete(FilePathIfValid);
        }
    }
}