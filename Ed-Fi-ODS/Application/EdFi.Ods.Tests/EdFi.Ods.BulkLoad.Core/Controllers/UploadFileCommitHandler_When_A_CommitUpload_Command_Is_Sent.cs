using EdFi.Ods.Api.Data;
using EdFi.Ods.Api.Models.Resources.Enums;
using EdFi.Ods.BulkLoad.Core;
using EdFi.Ods.BulkLoad.Core.Controllers;
using EdFi.Ods.Messaging.BulkLoadCommands;
using EdFi.Ods.Tests.TestObjects;

namespace EdFi.Ods.Tests.EdFi.Ods.BulkLoad.Core.Controllers
{
    using System;
    using System.Collections.ObjectModel;
    using NUnit.Framework;
    using Should;

    [TestFixture]
    public class When_A_CommitUpload_Command_Is_Sent_But_The_UploadFile_Resource_Does_Not_Exist
    {
        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Should_Throw_Exception()
        {
            var db = new TestBulkOperationsContext();
            var sut = new UploadFileCommitHandler(new TestDbExecutor(db), new UploadInfoValidator(), new StubBus());
            sut.Handle(new CommitUploadCommand {UploadId = "123"});
        }
    }

    [TestFixture]
    public class When_A_CommitUpload_Command_Is_Sent_And_File_Is_Invalid
    {
        private string _operationId = null;
        private string _uploadFileId;
        private TestBulkOperationsContext _db;

        [TestFixtureSetUp]
        public void CreateContext()
        {
            _db = new TestBulkOperationsContext();
            _uploadFileId = Guid.NewGuid().ToString();
            var uploadFile = new UploadFile
            {
                Id = _uploadFileId, Status = UploadFileStatus.Incomplete, 
                Size = 1000, 
                UploadFileChunks = new Collection<UploadFileChunk>
                {
                    new UploadFileChunk{Chunk = new byte[500], Id = "1234", Offset = 0, Size = 500}
                }
            };
            _db.UploadFiles.Add(uploadFile);
            _db.BulkOperations.Add(new BulkOperation
            {
                Id = _operationId,
                Status = BulkOperationStatus.Incomplete,
                UploadFiles = new Collection<UploadFile>{uploadFile},
            });
            var sut = new UploadFileCommitHandler(new TestDbExecutor(_db), new UploadInfoValidator(), new StubBus());
            sut.Handle(new CommitUploadCommand{UploadId = _uploadFileId, CommitedOn = DateTime.UtcNow});
        }

        [Test]
        public void Should_Set_UploadFileStatus_To_Error()
        {
            var file = _db.UploadFiles.Find(_uploadFileId);
            file.Status.ShouldEqual(UploadFileStatus.Error);
        }

        [Test]
        public void Should_Set_Operation_Status_To_Error()
        {
            var op = _db.BulkOperations.Find(_operationId);
            op.Status.ShouldEqual(BulkOperationStatus.Error);
        }

        //TODO: add behavior and test once abstraction is ready
        [Test]
        public void Should_Write_Exception_To_Exception_Resource()
        {
        }
    }

    [TestFixture]
    public class When_A_CommitUpload_Command_Is_Sent_And_File_Is_valid_And_At_Least_One_File_Has_Not_Completed
    {
        private string _operationId=null;
        private string _uploadFileId;
        private TestBulkOperationsContext _db;

        [TestFixtureSetUp]
        public void CreateContext()
        {
            _db = new TestBulkOperationsContext();
            _uploadFileId = Guid.NewGuid().ToString();
            var uploadFile = new UploadFile
            {
                Id = _uploadFileId,
                Status = UploadFileStatus.Incomplete,
                Size = 1000,
                UploadFileChunks = new Collection<UploadFileChunk>
                {
                    new UploadFileChunk {Chunk = new byte[1000], Id = "1234", Offset = 0, Size = 1000}
                }
            };
            _db.UploadFiles.Add(uploadFile);
            var f1 = new UploadFile{Id = "957393", Status = UploadFileStatus.Incomplete};
            _db.UploadFiles.Add(f1);
            var f2 = new UploadFile{Id = "23544235", Status = UploadFileStatus.Initialized};
            _db.UploadFiles.Add(f2);
            _db.BulkOperations.Add(new BulkOperation
            {
                Id = _operationId,
                Status = BulkOperationStatus.Incomplete,
                UploadFiles = new Collection<UploadFile> {uploadFile, f1, f2},
            });
            var sut = new UploadFileCommitHandler(new TestDbExecutor(_db), new UploadInfoValidator(), new StubBus());
            sut.Handle(new CommitUploadCommand {UploadId = _uploadFileId, CommitedOn = DateTime.UtcNow});
        }

        [Test]
        public void Should_Set_UploadFileStatus_To_Ready()
        {
            var file = _db.UploadFiles.Find(_uploadFileId);
            file.Status.ShouldEqual(UploadFileStatus.Ready);
        }
    }

    [TestFixture]
    public class When_A_CommitUpload_Command_Is_Sent_And_File_Is_valid_And_All_Other_Files_Ready
    {
        private string _operationId;
        private string _uploadFileId;
        private TestBulkOperationsContext _db;
        private StubBus _bus;

        [TestFixtureSetUp]
        public void CreateContext()
        {
            _bus = new StubBus();
            _db = new TestBulkOperationsContext();
            _uploadFileId = Guid.NewGuid().ToString();
            _operationId = Guid.NewGuid().ToString();
            var uploadFile = new UploadFile
            {
                Id = _uploadFileId,
                Status = UploadFileStatus.Incomplete,
                Size = 1000,
                UploadFileChunks = new Collection<UploadFileChunk>
                {
                    new UploadFileChunk {Chunk = new byte[1000], Id = "1234", Offset = 0, Size = 1000}
                }
            };
            _db.UploadFiles.Add(uploadFile);
            var f1 = new UploadFile {Id = "957393", Status = UploadFileStatus.Ready};
            _db.UploadFiles.Add(f1);
            var f2 = new UploadFile {Id = "23544235", Status = UploadFileStatus.Ready};
            _db.UploadFiles.Add(f2);
            _db.BulkOperations.Add(new BulkOperation
            {
                Id = _operationId,
                Status = BulkOperationStatus.Incomplete,
                UploadFiles = new Collection<UploadFile> {uploadFile, f1, f2},
            });
            var sut = new UploadFileCommitHandler(new TestDbExecutor(_db), new UploadInfoValidator(), _bus);
            sut.Handle(new CommitUploadCommand {UploadId = _uploadFileId, CommitedOn = DateTime.UtcNow});
        }

        [Test]
        public void Should_Set_UploadFileStatus_To_Ready()
        {
            var file = _db.UploadFiles.Find(_uploadFileId);
            file.Status.ShouldEqual(UploadFileStatus.Ready);
        }

        [Test]
        public void Should_Set_Operation_Status_To_Ready()
        {
            var op = _db.BulkOperations.Find(_operationId);
            op.Status.ShouldEqual(BulkOperationStatus.Ready);
        }

        [Test]
        public void Should_Send_StartOperationCommand()
        {
            var sentCmd = _bus.GetLastSentCommand();
            sentCmd.ShouldNotBeNull();
            var cmd = sentCmd as StartOperationCommand;
            var idAsGuid = Guid.Parse(_operationId);
            cmd.OperationId.ShouldEqual(idAsGuid);
        }
    }
}