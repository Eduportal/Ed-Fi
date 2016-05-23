
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace EdFi.Ods.Api.Data
{
    public class BulkOperationException
    {
        public int Id { get; set; }
        public string ParentUploadFileId { get; set; }
        [ForeignKey("ParentUploadFileId")]
        public virtual UploadFile ParentUploadFile { get; set; }
        public int Code { get; set; }
        public string Type { get; set; }
        public string Element { get; set; }
        public string Message { get; set; }
        public DateTime DateTime { get; set; }
        public string StackTrace { get; set; }
    }
}
