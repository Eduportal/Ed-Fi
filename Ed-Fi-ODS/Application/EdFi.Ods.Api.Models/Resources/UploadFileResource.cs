namespace EdFi.Ods.Api.Models.Resources
{
    public class UploadFileResource
    {
        public string Id { get; set; }
        public long Size { get; set; }
        public string Format { get; set; }
        public string InterchangeType { get; set; }
        public string Status { get; set; }
    }
}
