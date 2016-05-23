using EdFi.Ods.Api.Data.Model;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdFi.Ods.BulkLoad.Core
{
    public class UploadInfoValidator : AbstractValidator<UploadInfo>
    {
        public UploadInfoValidator()
        {
            RuleForEach(model => model.UploadFileChunkInfos).SetValidator(new UploadFileChunkInfoValidator()).WithMessage("Offset is not greater than zero, or chunk data length and size are not equal.");
            RuleFor(x => x.UploadFile.Size).Equal(x => x.UploadFileChunkInfos.Sum(c => c.Size)).WithMessage("File size and sum of file chunks are not equal.");
            RuleFor(x => x.UploadFileChunkInfos).Must(x => x.Any(c => c.Offset == 0)).WithMessage("File chunk offsets must begin with zero");
            RuleFor(x => x.UploadFileChunkInfos).Must(ValidateChunkOffsets).WithMessage("Chunk offsets do not match chunk sizes");
        }

        private static bool ValidateChunkOffsets(IEnumerable<UploadFileChunkInfo> chunks)
        {
            long offset = -1;
            long previousChunkLenghts = 0;
            var sorted = chunks.OrderBy(c => c.Offset);
            foreach (var uploadFileChunk in sorted)
            {
                if (offset > -1)
                {
                    if (previousChunkLenghts != uploadFileChunk.Offset) return false;
                }
                offset = uploadFileChunk.Offset;
                previousChunkLenghts += uploadFileChunk.ChunkDataLength;
            }
            return true;
        }
    }

    public class UploadFileChunkInfoValidator : AbstractValidator<UploadFileChunkInfo>
    {
        public UploadFileChunkInfoValidator()
        {
            RuleFor(x => x.Offset).Must(x => x > -1);
            RuleFor(x => x.Size).Equal(x => x.ChunkDataLength);
        }
    }
}
