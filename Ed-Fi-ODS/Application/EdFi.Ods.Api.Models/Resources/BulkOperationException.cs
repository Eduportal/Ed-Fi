namespace EdFi.Ods.Api.Models.Resources
{
    public class BulkOperationException
    {
        public int Id { get; set; }
        public string UploadFileId { get; set; }
        public string Type { get; set; }
        public string Element { get; set; }
        public string Message { get; set; }
    }
}
