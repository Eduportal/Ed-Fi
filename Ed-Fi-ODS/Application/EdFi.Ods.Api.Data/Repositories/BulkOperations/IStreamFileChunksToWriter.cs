using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdFi.Ods.Api.Data.Repositories.BulkOperations
{
    public interface IStreamFileChunksToWriter
    {
        void Write(string uploadFileId, Stream writer);
    }
}
