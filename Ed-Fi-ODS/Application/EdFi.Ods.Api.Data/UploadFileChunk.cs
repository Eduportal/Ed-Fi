
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EdFi.Ods.Api.Data
{
    public class UploadFileChunk
    {
        [Key, ForeignKey("UploadFile"), Column(Order = 1)]
        public string UploadFile_Id { get; set; }
        [Key, Column(Order = 2)]
        public string Id { get; set; }
        public virtual UploadFile UploadFile { get; set; }
        public long Offset { get; set; }
        public long Size { get; set; }
        public byte[] Chunk { get; set; }
    }
}
