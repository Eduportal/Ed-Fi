using EdFi.Common.Messaging;
using EdFi.Ods.Api.Data.Contexts;
using EdFi.Ods.Api.Data.Repositories.BulkOperations;
using EdFi.Ods.Api.Services.Controllers;
using EdFi.Ods.Api.Services.Providers;
using EdFi.Ods.Tests._Bases;

namespace EdFi.Ods.Tests.EdFi.Ods.WebApi
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http.Results;
    using NUnit.Framework;
    using Should;

    [TestFixture]
    public class UploadsControllerTests : TestBase
    {
        
        public class When_Commiting_An_Upload : UploadsControllerTests
        {
            [Test]
            public void And_the_Chunks_Add_Up_To_The_Expected_Size_And_The_Offsets_Match_Then_Should_Accept_Commit()
            {
                var uploadId = Guid.NewGuid().ToString();
                var sut = new UploadsController(new StubVerifier(uploadId, false),
                        (chunkId) => new CustomMultipartStreamProvider(chunkId, Stub<IVarbinaryWriter>()),
                        Stub<ICommandSender>(), Stub<IBulkOperationsFileChunkCreator>());

                var result = sut.PostCommit(uploadId) as StatusCodeResult;
                result.StatusCode.ShouldEqual(HttpStatusCode.Accepted);
            }

            [Test]
            public void And_The_Chunks_Do_Not_Add_Up_to_The_Expected_Size_Or_the_Offsets_Do_Not_Match_Then_Should_Reject_Commit()
            {
                var uploadId = Guid.NewGuid().ToString();
                var sut = new UploadsController(new StubVerifier(uploadId, true),
                        (chunkId) => new CustomMultipartStreamProvider(chunkId, Stub<IVarbinaryWriter>()),
                        Stub<ICommandSender>(), Stub<IBulkOperationsFileChunkCreator>());

                sut.PostCommit(uploadId).ShouldBeType<BadRequestErrorMessageResult>();
            }

            [Test]
            public void And_Upload_Request_is_Invalid_Then_Should_Log_To_Bulk_Operations_Exceptions()
            {
                //var uploadId = Guid.NewGuid().ToString();
                var sut = new UploadsController(new StubVerifier(null, true),
                        (chunkId) => new CustomMultipartStreamProvider(chunkId, Stub<IVarbinaryWriter>()),
                        Stub<ICommandSender>(), Stub<IBulkOperationsFileChunkCreator>()); 
                sut.PostChunk(null, 0, 10);
                //persistBulkOperationExceptions.AssertWasCalled(x=>x.HandleFileValidationExceptions(Arg<string>.Is.Anything,Arg<int>.Is.Anything,Arg<IEnumerable<string>>.Is.Anything));
            }

            [Test]
            [Ignore("Needs to be reimplemented using Web Api 2.1 approach")]
            public void And_Uploaded_Files_Are_Invalid_Then_Should_Log_To_Bulk_Operations_Exceptions()
            {
                var uploadId = Guid.NewGuid().ToString();
                var sut = new UploadsController(new StubVerifier(uploadId, false),
                        (chunkId) => new CustomMultipartStreamProvider(chunkId, Stub<IVarbinaryWriter>()),
                        Stub<ICommandSender>(), Stub<IBulkOperationsFileChunkCreator>()); 
                sut.Request = new HttpRequestMessage();
                sut.Request.Content = Stub<HttpContent>();
                sut.PostChunk(uploadId, 0, 10);
            }

            [Test]
            public void And_Internal_Server_Error_Occurrs_Then_Should_Log_To_Bulk_Operations_Exceptions()
            {
                var uploadId = Guid.NewGuid().ToString();
                var sut = new UploadsController(new StubVerifier(uploadId, false),                     
                    (chunkId) => null,
                    Stub<ICommandSender>(), Stub<IBulkOperationsFileChunkCreator>());
                sut.PostChunk(uploadId, 0, 10);
            }
        }

        public class StubVerifier : IVerifyUploads
        {
            private readonly string _expectedId;
            private readonly bool _isInvalid;

            public StubVerifier(string expectedId, bool isInvalid)
            {
                _expectedId = expectedId;
                _isInvalid = isInvalid;
            }

            public UploadValidationErrors IsValid(string uploadId)
            {
                var errors = string.Empty;
                if (this._isInvalid || uploadId != this._expectedId) errors = "Not Valid";
                return new UploadValidationErrors(errors);
            }

            public UploadValidationErrors IsValid(string uploadId, long offset, long size)
            {
                var erros = string.Empty;
                if (this._isInvalid || uploadId != this._expectedId) erros = "Not Valid";
                return new UploadValidationErrors(erros);
            }
        }
    }
}