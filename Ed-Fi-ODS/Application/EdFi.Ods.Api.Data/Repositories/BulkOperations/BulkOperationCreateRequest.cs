namespace EdFi.Ods.Api.Data.Repositories.BulkOperations
{
    public class BulkOperationCreateRequest
    {
        public UploadFileRequest[] UploadFiles { get; set; }
        public string ResetDistrictData { get; set; }
    }

    public class UploadFileRequest
    {
        public string Format { get; set; }
        public string InterchangeType { get; set; }
        public long Size { get; set; }
    }
}