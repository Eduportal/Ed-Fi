using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using EdFi.Ods.Api.Data.Model;
using EdFi.Ods.BulkLoad.Core;
using EdFi.Ods.Common;
using EdFi.Ods.IntegrationTests.Common;
using EdFi.Ods.Api.Data.Contexts;
using EdFi.Ods.Api.Data;
using EdFi.Ods.Api.Models.Resources.Enums;
using EdFi.Ods.Tests.TestObjects;
using FluentValidation;
using FluentValidation.Results;
using NUnit.Framework;
using Rhino.Mocks;
using Should;

namespace EdFi.Ods.IntegrationTests.EdFi.Ods.BulkLoad
{
    using global::EdFi.Ods.Tests._Bases;

    [TestFixture]
    public class When_assembling_an_upload_file : TestBase
    {
        private IUploadFileSourcingResults _result;

        private string _uploadId;
        private ICreateFilePathForUploadFile _createFilePathForUploadFileStrategy;
        private CreateFilePathForUploadFileResult _createFilePathStrategyResult;
        private string _operationId;
        private IQueryable<UploadFile> _uploadFiles;
        private DbExecutor<IBulkOperationDbContext> _executor;
        private IBulkOperationDbContext _mockCtx;

        [TestFixtureSetUp]
        public void Setup()
        {
            _mockCtx = new TestBulkOperationsContext();
            _executor = new DbExecutor<IBulkOperationDbContext>(() => _mockCtx);
            _uploadId = Guid.NewGuid().ToString();
            _createFilePathForUploadFileStrategy = new CreateFilePathForUploadFileLocally();
            _operationId = Guid.NewGuid().ToString();
            _createFilePathStrategyResult = _createFilePathForUploadFileStrategy.Create(_operationId, _uploadId);

            _uploadFiles = new List<UploadFile>
            {
                new UploadFile
                {
                    Id = _uploadId,
                    Size = 5628,
                    Status = UploadFileStatus.Ready,
                    UploadFileChunks =
                        new[]
                        {
                            new UploadFileChunk {Chunk = new byte[628], Offset = 5000, Size = 628},
                            new UploadFileChunk {Chunk = new byte[1000], Offset = 0, Size = 1000},
                            new UploadFileChunk {Chunk = new byte[1000], Offset = 1000, Size = 1000},
                            new UploadFileChunk {Chunk = new byte[1000], Offset = 2000, Size = 1000},
                            new UploadFileChunk {Chunk = new byte[1000], Offset = 3000, Size = 1000},
                            new UploadFileChunk {Chunk = new byte[1000], Offset = 4000, Size = 1000},
                        }
                }
            }.AsQueryable();

            var random = new Random();
            _uploadFiles.ForEach(x => x.UploadFileChunks.ForEach(c => random.NextBytes(c.Chunk)));
            _uploadFiles.ForEach(x => _mockCtx.UploadFiles.Add(x));

            var happyValidator = Stub<IValidator<UploadInfo>>();
            var streamFiles = new TestStreamFileChunksToWriter(_mockCtx);

            happyValidator.Expect(v => v.Validate(new UploadInfo())).IgnoreArguments().Return(new ValidationResult());
            var sut = new ValidateAndSourceFiles(_executor, new PersistUploadFilesLocally(streamFiles, _createFilePathForUploadFileStrategy), happyValidator);

            _result = sut.ValidateMakeLocalAndFindPath(_operationId, _uploadId);
        }

        [TestFixtureTearDown]
        public void Teardown()
        {
            Directory.Delete(_createFilePathStrategyResult.DirectoryPath, true);
        }

        [Test]
        public void Should_return_path()
        {
            _result.FilePathIfValid.ShouldNotBeEmpty();
        }

        [Test]
        public void Should_indicate_success()
        {
            _result.IsFailure.ShouldBeFalse();
        }

        [Test]
        public void Should_write_assembled_file_to_disk_in_proper_order()
        {
            File.Exists(_createFilePathStrategyResult.FilePath).ShouldBeTrue();
            var bytes = File.ReadAllBytes(_createFilePathStrategyResult.FilePath);
            var array = _uploadFiles.ToArray()[0].UploadFileChunks.OrderBy(x => x.Offset).ToArray();
            bytes.Length.ShouldEqual(array.Sum(x => x.Chunk.Length));

            ByteArrayComparer.Compare(bytes.ToArray(), array.SelectMany(x => x.Chunk).ToArray()).ShouldBeTrue();
        }
    }


    [TestFixture]
    public class When_building_an_invalid_upload_file : TestBase
    {
        private IUploadFileSourcingResults _result;
        private string _folder;
        private string _uploadId;
        private string _fileName;
        private string _operationId;
        private IQueryable<UploadFile> _uploadFiles;
        private DbExecutor<IBulkOperationDbContext> _executor;
        private IBulkOperationDbContext _mockCtx;

        [TestFixtureSetUp]
        public void Setup()
        {
            _mockCtx = MockRepository.GenerateMock<IBulkOperationDbContext>();
            _executor = new DbExecutor<IBulkOperationDbContext>(() => _mockCtx);
            var mockDbSet = MockRepository.GenerateStub<IDbSet<UploadFile>>();
            _uploadId = Guid.NewGuid().ToString();
            _fileName = _uploadId + ".xml";
            _folder = Path.Combine(Path.GetTempPath(), _uploadId);
            _operationId = Guid.NewGuid().ToString();

            _uploadFiles = new List<UploadFile>
            {
                new UploadFile
                {
                    Id = _uploadId,
                    Size = 5,
                    Status = UploadFileStatus.Ready,
                    UploadFileChunks =
                        new[]
                        {
                            new UploadFileChunk {Chunk = new byte[] {0x30, 0x15, 0x10}, Offset = 20, Size = 3},
                            new UploadFileChunk {Chunk = new byte[] {0x20, 0x25}, Offset = 0, Size = 2},
                        }
                }
            }.AsQueryable();
            mockDbSet.Stub(m => m.Provider).Return(_uploadFiles.Provider);
            mockDbSet.Stub(m => m.Expression).Return(_uploadFiles.Expression);
            mockDbSet.Stub(m => m.GetEnumerator()).Return(_uploadFiles.GetEnumerator());
            _mockCtx.Stub(c => c.UploadFiles).Return(mockDbSet);

            var grumpyValidator = Stub<IValidator<UploadInfo>>();
            grumpyValidator.Expect(v => v.Validate(new UploadInfo()))
                .IgnoreArguments()
                .Return(new ValidationResult(new[] {new ValidationFailure("Size", "Chunks don't match size.")}));
            var sut = new ValidateAndSourceFiles(_executor, Stub<IPersistUploadFiles>(), grumpyValidator);

            _result = sut.ValidateMakeLocalAndFindPath(_operationId, _uploadId);
        }

        [Test]
        public void Should_indicate_file_is_invalid()
        {
            _result.IsFailure.ShouldBeTrue();
        }

        [Test]
        public void Should_not_write_to_disk()
        {
            File.Exists(Path.Combine(_folder, _fileName)).ShouldBeFalse();
        }

        [Test]
        public void Should_Return_Errors()
        {
            _result.ValidationErrorMessages.Any().ShouldBeTrue();
        }
    }

    [TestFixture]
    public class When_Asked_To_Build_A_File_That_Is_Not_Ready : TestBase
    {
        private IUploadFileSourcingResults _result;
        private string _folder;
        private string _uploadId;
        private string _fileName;
        private string _operationId;
        private IQueryable<UploadFile> _uploadFiles;
        private DbExecutor<IBulkOperationDbContext> _executor;
        private IBulkOperationDbContext _mockCtx;

        [TestFixtureSetUp]
        public void Setup()
        {
            _mockCtx = MockRepository.GenerateMock<IBulkOperationDbContext>();
            _executor = new DbExecutor<IBulkOperationDbContext>(() => _mockCtx);
            var mockDbSet = MockRepository.GenerateStub<IDbSet<UploadFile>>();
            _uploadId = Guid.NewGuid().ToString();
            _fileName = _uploadId + ".xml";
            _folder = Path.Combine(Path.GetTempPath(), _uploadId);
            _operationId = Guid.NewGuid().ToString();

            _uploadFiles = new List<UploadFile>
            {
                new UploadFile
                {
                    Id = _uploadId,
                    Size = 5,
                    Status = UploadFileStatus.Incomplete,
                    UploadFileChunks = new List<UploadFileChunk>(),
                }
            }.AsQueryable();
            mockDbSet.Stub(m => m.Provider).Return(_uploadFiles.Provider);
            mockDbSet.Stub(m => m.Expression).Return(_uploadFiles.Expression);
            mockDbSet.Stub(m => m.GetEnumerator()).Return(_uploadFiles.GetEnumerator());
            _mockCtx.Stub(c => c.UploadFiles).Return(mockDbSet);

            var sut = new ValidateAndSourceFiles(_executor, Stub<IPersistUploadFiles>(), Stub<IValidator<UploadInfo>>());

            _result = sut.ValidateMakeLocalAndFindPath(_operationId, _uploadId);
        }

        [Test]
        public void Should_indicate_file_is_invalid()
        {
            _result.IsFailure.ShouldBeTrue();
        }

        [Test]
        public void Should_not_write_to_disk()
        {
            File.Exists(Path.Combine(_folder, _fileName)).ShouldBeFalse();
        }
    }
}