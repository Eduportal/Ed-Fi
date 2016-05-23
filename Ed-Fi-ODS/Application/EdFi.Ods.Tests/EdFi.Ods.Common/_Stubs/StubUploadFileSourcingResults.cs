namespace EdFi.Ods.Tests.EdFi.Ods.Common._Stubs
{
    using global::EdFi.Ods.Common;

    public class StubUploadFileSourcingResults : IUploadFileSourcingResults
    {
        public void Dispose()
        {
            this.IsDisposed = true;
        }

        public bool IsDisposed { get; private set; }

        public string FilePathIfValid { get; set; }
        public bool IsFailure { get; set; }
        public string[] ValidationErrorMessages { get; set; }

        public static StubUploadFileSourcingResults WithSuccessPath(string filePath)
        {
            return new StubUploadFileSourcingResults {IsFailure = false, FilePathIfValid = filePath};
        }
    }
}