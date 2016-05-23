using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EdFi.Ods.Api.Data
{
    using EdFi.Ods.Api.Models.Resources.Enums;

    public class UploadFile
    {
        public UploadFile()
        {
            UploadFileChunks = new HashSet<UploadFileChunk>();
        }

        public string Id { get; set; }
        public string Format { get; set; }
        public string InterchangeType { get; set; }
        public UploadFileStatus Status { get; set; }
        public virtual ICollection<UploadFileChunk> UploadFileChunks { get; set; }
        public long Size { get; set; }
    }
}