using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdFi.Ods.Api.Data.Model
{
    public class UploadInfo
    {
        public UploadInfo()
        {
            this.UploadFileChunkInfos = new List<UploadFileChunkInfo>();
        }

        public UploadFile UploadFile { get; set; }
        public List<UploadFileChunkInfo> UploadFileChunkInfos { get; set; }
    }
}
