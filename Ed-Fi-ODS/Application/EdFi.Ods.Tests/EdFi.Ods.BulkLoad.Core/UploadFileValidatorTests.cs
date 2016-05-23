using EdFi.Ods.Api.Data.Model;

namespace EdFi.Ods.Tests.EdFi.Ods.BulkLoad.Core
{
    using System.Linq;

    using global::EdFi.Ods.Api.Data;
    using global::EdFi.Ods.BulkLoad.Core;

    using NUnit.Framework;

    using Should;

    [TestFixture]
    public class When_upload_file_is_valid
    {
        [Test]
        public void Should_return_isvalid_result()
        {
            var uploadInfo = new UploadInfo()
            {
                UploadFile = new UploadFile
                {
                    Size = 5
                },
                UploadFileChunkInfos =
                {
                    new UploadFileChunkInfo(){ Offset = 2, Size = 3, ChunkDataLength = 3},
                    new UploadFileChunkInfo(){ Offset = 0, Size = 2, ChunkDataLength = 2}
                }
            };


            var validator = new UploadInfoValidator();
            var result = validator.Validate(uploadInfo);
            result.IsValid.ShouldBeTrue();
        }
    }

    [TestFixture]
    public class When_file_chunk_size_does_not_match_file_size
    {
        [Test]
        public void Should_fail_with_size_error_message()
        {
            var uploadInfo = new UploadInfo()
            {
                UploadFile = new UploadFile
                {
                    Size = 10
                },
                UploadFileChunkInfos =
                {
                    new UploadFileChunkInfo(){ Offset = 2, Size = 3, ChunkDataLength = 3},
                    new UploadFileChunkInfo(){ Offset = 0, Size = 2, ChunkDataLength = 2}
                }
            };

            var validator = new UploadInfoValidator();
            var result = validator.Validate(uploadInfo);
            result.IsValid.ShouldBeFalse();
            result.Errors.Count.ShouldEqual(1);
            result.Errors.Any(e => e.PropertyName.Contains("Size")).ShouldBeTrue();
        }
    }


    [TestFixture]
    public class When_file_chunk_offset_is_not_valid
    {
        [Test]
        public void Should_fail_with_offset_error_message()
        {
            var uploadInfo = new UploadInfo()
            {
                UploadFile = new UploadFile
                {
                    Size = 5
                },
                UploadFileChunkInfos =
                {
                    new UploadFileChunkInfo(){ Offset = 2, Size = 3, ChunkDataLength = 3},
                    new UploadFileChunkInfo(){ Offset = 10, Size = 2, ChunkDataLength = 2}
                }
            };

            var validator = new UploadInfoValidator();
            var result = validator.Validate(uploadInfo);
            result.IsValid.ShouldBeFalse();
            result.Errors.Any(e => e.ToString().Contains("File chunk offsets must begin with zero")).ShouldBeTrue();
        }
    }


    [TestFixture]
    public class When_chunk_offset_does_not_match_previous_chunk_size
    {
        [Test]
        public void Should_fail_with_offset_error_message()
        {
            var uploadInfo = new UploadInfo()
            {
                UploadFile = new UploadFile
                {
                    Size = 5
                },
                UploadFileChunkInfos =
                {
                    new UploadFileChunkInfo(){ Offset = 2, Size = 3, ChunkDataLength = 3},
                    new UploadFileChunkInfo(){ Offset = 0, Size = 2, ChunkDataLength = 2},
                    new UploadFileChunkInfo(){ Offset = 8, Size = 3, ChunkDataLength = 3}, //offset should be 7
                    new UploadFileChunkInfo(){ Offset = 5, Size = 2, ChunkDataLength = 2},
                }
            };

            var validator = new UploadInfoValidator();
            var result = validator.Validate(uploadInfo);
            result.IsValid.ShouldBeFalse();
            result.Errors.Any(e => e.ToString().Contains("Chunk offsets do not match chunk sizes")).ShouldBeTrue();
        }
    }

    [TestFixture]
    public class When_chunk_size_does_not_match_chunk_length
    {
        [Test]
        public void Should_fail_with_size_error_message()
        {
            var uploadInfo = new UploadInfo()
            {
                UploadFile = new UploadFile
                {
                    Size = 10
                },
                UploadFileChunkInfos =
                {
                    new UploadFileChunkInfo(){ Offset = 2, Size = 3, ChunkDataLength = 3},
                    new UploadFileChunkInfo(){ Offset = 0, Size = 7, ChunkDataLength = 2}
                }
            };

            var validator = new UploadInfoValidator();
            var result = validator.Validate(uploadInfo);

            result.IsValid.ShouldBeFalse();
            result.Errors.Count.ShouldEqual(1);
            result.Errors.Any(e => e.PropertyName.Contains("Size")).ShouldBeTrue();
        }
    }
}
