namespace EdFi.Ods.BulkLoad.Core
{
    public class UploadFilePersistenceResult
    {
        private UploadFilePersistenceResult()
        {
        }

        public bool IsSuccessful { get; private set; }
        public string FailureMessage { get; private set; }
        public string FilePath { get; private set; }

        public static UploadFilePersistenceResult WithSuccessfulFilePath(string filePath)
        {
            return new UploadFilePersistenceResult {FilePath = filePath, IsSuccessful = true};
        }

        public static UploadFilePersistenceResult WithFailureMessage(string failureMessage)
        {
            return new UploadFilePersistenceResult {FailureMessage = failureMessage, IsSuccessful = false};
        }
    }
}