namespace EdFi.Ods.Api.Models.Resources
{
    public class BulkOperationResource
    {
        public string Id { get; set; }
        public UploadFileResource[] UploadFiles { get; set; }
        public string ResetDistrictData { get; set; }
        public string Status { get; set; }
    }
}
