using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdFi.Ods.Api.Data.Model
{
    /// <summary>
    /// Use transact SQL DataLength function to compute the length of Chunk column without returning the entire binary array.  
    /// This is far more desirable in production scenarios when working with very large binary data sets.  
    /// </summary>
    public class UploadFileChunkInfo
    {
        public string UploadFile_Id { get; set; }
        public string Id { get; set; }
        public long Offset { get; set; }
        public long Size { get; set; }
        public long ChunkDataLength { get; set; }
    }
}
