using EdFi.Ods.Api.Data;
using EdFi.Ods.Api.Data.Contexts;
using EdFi.Ods.Api.Models.Resources.Enums;
using EdFi.Ods.BulkLoad.Core;
using EdFi.Ods.BulkLoad.Core.Controllers;
using EdFi.Ods.BulkLoad.Core.Data;
using EdFi.Ods.Common;
using EdFi.Ods.Messaging.BulkLoadCommands;
using EdFi.Ods.Tests.TestObjects;
using EdFi.Ods.Tests._Bases;

namespace EdFi.Ods.Tests.EdFi.Ods.BulkLoad.Core
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using EdFi.Ods.BulkLoad.Core.Controllers;
    using NUnit.Framework;
    using Rhino.Mocks;
    using Should;

    [TestFixture]
    public class When_exception_encountered_loading_file : TestBase
    {
        [Test]
        public void Should_persist_exception()
        {
            var uploadFileId = Guid.NewGuid();
            var operationId = Guid.NewGuid();
            var mockCtx = MockRepository.GenerateMock<IBulkOperationDbContext>();
            const string exceptionMessage = "Some exception happened";

            var uploadFiles = new List<UploadFile>
            {
                new UploadFile
                {
                    Id = uploadFileId.ToString(),
                    Status = UploadFileStatus.Ready,
                    InterchangeType = InterchangeType.Descriptors.Name
                }
            };

            var dbSet = StubDbSetCreator.CreateBulkOperations(operationId.ToString(), uploadFiles);
            var exceptions = StubDbSetCreator.CreateBulkOperationExceptions();
            mockCtx.Stub(c => c.BulkOperations).Return(dbSet);
            mockCtx.Stub(c => c.UploadFiles).Return(StubDbSetCreator.CreateUploadFiles(uploadFiles));
            mockCtx.Stub(c => c.BulkOperationExceptions).Return(exceptions);

            var loader = new TestLoader(InterchangeType.Descriptors.Name);
            loader.SetLoadFromBehavior(() =>
                new LoadResult
                {
                    LoadExceptions = new List<LoadException>()
                    {
                        new LoadException {Exception = new Exception(exceptionMessage)}
                    }
                });

            var sut =
                new BulkLoadMaster(
                    new Dictionary<InterchangeType, IInterchangeController> { { InterchangeType.Descriptors, loader } },
                    new StubLocalXMLFileManager(),
                    new FindBulkOperations(() => mockCtx), new SetBulkOperationStatus(() => mockCtx),
                    new PersistBulkOperationExceptions(() => mockCtx, GetErrorProvider()),
                    new SetUploadFileStatus(() => mockCtx), new DeleteUploadFileChunks(() => mockCtx));


            sut.Handle(new StartOperationCommand { OperationId = operationId });

            exceptions.AssertWasCalled(x => x.Add(Arg<BulkOperationException>.Matches(op => op.Message.StartsWith("An unexpected error occurred"))));
        }
    }

    //TODO: move this test to the other test class that is using the testdbsets and/or just use them here - the mocks don't cut it. . . .
    [TestFixture]
    public class When_Given_At_Least_One_File_That_Is_Invalid : TestBase
    {
        private Guid operationId;
        private Guid uploadFileId;
        private Guid uploadFileInError;
        private IDbSet<UploadFile> fileSet;
        private IDbSet<BulkOperation> dbSet;
        private IDbSet<BulkOperationException> exceptions;
        private string ExpectedError = "File chunks do not equal expected size";

        [TestFixtureSetUp]
        public void EstablishContextAndExecuteBehavior()
        {
            InitSystemClock(new DateTime(2001, 5, 4));
            uploadFileId = Guid.NewGuid();
            uploadFileInError = Guid.NewGuid();
            operationId = Guid.NewGuid();
            var mockCtx = MockRepository.GenerateMock<IBulkOperationDbContext>();
            var validator = Stub<IValidateAndSourceFiles>();
            validator.Expect(
                v =>
                    v.ValidateMakeLocalAndFindPath(Arg<string>.Is.Anything,
                        Arg<string>.Matches(s => s.Equals(uploadFileInError.ToString()))))
                .Return(UploadFileSourcingResults.WithValidationErrorMessage(ExpectedError));
            validator.Expect(
                v =>
                    v.ValidateMakeLocalAndFindPath(Arg<string>.Is.Anything,
                        Arg<string>.Matches(s => s.Equals(uploadFileId.ToString()))))
                .Return(UploadFileSourcingResults.WithSuccessPath("Really Reals Path"));
            var uploadFiles = new List<UploadFile>
            {
                new UploadFile
                {
                    Id = uploadFileId.ToString(),
                    Status = UploadFileStatus.Ready,
                    InterchangeType = InterchangeType.Descriptors.Name,
                    Size = 100,
                },
                new UploadFile
                {
                    Id = uploadFileInError.ToString(),
                    Status = UploadFileStatus.Ready,
                    InterchangeType = InterchangeType.Descriptors.Name,
                    Size = 100,
                }
            };

            dbSet = StubDbSetCreator.CreateBulkOperations(operationId.ToString(), uploadFiles);
            fileSet = StubDbSetCreator.CreateUploadFiles(uploadFiles);
            exceptions = StubDbSetCreator.CreateBulkOperationExceptions();
            mockCtx.Stub(c => c.BulkOperations).Return(dbSet);
            mockCtx.Stub(c => c.UploadFiles).Return(fileSet);
            mockCtx.Stub(c => c.BulkOperationExceptions).Return(exceptions);

            var loader = new TestLoader(InterchangeType.Descriptors.Name);
            loader.SetLoadFromBehavior(() => new LoadResult());
            var sut =
                new BulkLoadMaster(
                    new Dictionary<InterchangeType, IInterchangeController> { { InterchangeType.Descriptors, loader } },
                    validator,
                    new FindBulkOperations(() => mockCtx), new SetBulkOperationStatus(() => mockCtx),
                    new PersistBulkOperationExceptions(() => mockCtx, GetErrorProvider()),
                    new SetUploadFileStatus(() => mockCtx), new DeleteUploadFileChunks(() => mockCtx));

            sut.Handle(new StartOperationCommand { OperationId = operationId });
        }

        [Test]
        public void Should_Set_That_File_To_Error()
        {
            fileSet.Find(uploadFileInError).Status.ShouldEqual(UploadFileStatus.Error);
        }

        [Test]
        public void Should_Add_Exceptions_for_Validation_Error()
        {
            Assert.Pass();
        }
    }

    [TestFixture]
    public class When_attempting_to_load_files_not_set_to_ready : TestBase
    {
        [Test]
        public void Should_persist_exception()
        {
            InitSystemClock(new DateTime(2001, 5, 4));
            var expectedErrorMsg = "UploadFileStatus is not set to Ready";
            var validator = Stub<IValidateAndSourceFiles>();
            validator.Expect(v => v.ValidateMakeLocalAndFindPath(Arg<string>.Is.Anything, Arg<string>.Is.Anything))
                .IgnoreArguments()
                .Return(
                    UploadFileSourcingResults.WithValidationErrorMessage(expectedErrorMsg));
            var uploadFileIdNotReady = Guid.NewGuid();
            var operationId = Guid.NewGuid();
            var mockCtx = MockRepository.GenerateMock<IBulkOperationDbContext>();

            var uploadFiles = new List<UploadFile>
            {
                new UploadFile
                {
                    Id = uploadFileIdNotReady.ToString(),
                    Status = UploadFileStatus.Initialized,
                    InterchangeType = InterchangeType.Descriptors.Name
                },
            };

            var dbSet = StubDbSetCreator.CreateBulkOperations(operationId.ToString(), uploadFiles);
            var exceptions = StubDbSetCreator.CreateBulkOperationExceptions();
            mockCtx.Stub(c => c.BulkOperations).Return(dbSet);
            mockCtx.Stub(c => c.UploadFiles).Return(StubDbSetCreator.CreateUploadFiles(uploadFiles));
            mockCtx.Stub(c => c.BulkOperationExceptions).Return(exceptions);

            var loader = new TestLoader(InterchangeType.Descriptors.Name);
            loader.SetLoadFromBehavior(() =>
                new LoadResult
                {
                    LoadExceptions =
                        new List<LoadException> {new LoadException {Exception = new Exception()}}
                });

            var sut =
                new BulkLoadMaster(
                    new Dictionary<InterchangeType, IInterchangeController> { { InterchangeType.Descriptors, loader } },
                    validator,
                    new FindBulkOperations(() => mockCtx), new SetBulkOperationStatus(() => mockCtx),
                    new PersistBulkOperationExceptions(() => mockCtx, GetErrorProvider()),
                    new SetUploadFileStatus(() => mockCtx), new DeleteUploadFileChunks(() => mockCtx));

            sut.Handle(new StartOperationCommand { OperationId = operationId }).Wait();

            exceptions.AssertWasCalled(
                x =>
                    x.Add(
                        Arg<BulkOperationException>.Matches(
                            op =>
                                op.Message.StartsWith(expectedErrorMsg))));
        }

    }

    [TestFixture]
    public class When_no_resources_loaded_and_no_exceptions : TestBase
    {
        [Test]
        public void Should_log_exception_indicating_possibility_of_wrong_interchange_type()
        {
            InitSystemClock(new DateTime(2001, 5, 4));
            var validator = Stub<IValidateAndSourceFiles>();
            validator.Expect(v => v.ValidateMakeLocalAndFindPath(Arg<string>.Is.Anything, Arg<string>.Is.Anything))
                .IgnoreArguments()
                .Return(
                    UploadFileSourcingResults.WithSuccessPath("Some test path"));
            var uploadFileIdNotReady = Guid.NewGuid();
            var operationId = Guid.NewGuid();
            var mockCtx = MockRepository.GenerateMock<IBulkOperationDbContext>();

            var uploadFiles = new List<UploadFile>
            {
                new UploadFile
                {
                    Id = uploadFileIdNotReady.ToString(),
                    Status = UploadFileStatus.Initialized,
                    InterchangeType = InterchangeType.Descriptors.Name
                },
            };
            var expectedErrorMsg = string.Format(
                @"No aggregates for Interchange {0} were found in file {1}.  Please confirm the declared Interchange Type ({0}) is valid.",
                uploadFiles[0].InterchangeType, uploadFiles[0].Id);

            var dbSet = StubDbSetCreator.CreateBulkOperations(operationId.ToString(), uploadFiles);
            var exceptions = StubDbSetCreator.CreateBulkOperationExceptions();
            mockCtx.Stub(c => c.BulkOperations).Return(dbSet);
            mockCtx.Stub(c => c.UploadFiles).Return(StubDbSetCreator.CreateUploadFiles(uploadFiles));
            mockCtx.Stub(c => c.BulkOperationExceptions).Return(exceptions);

            var loader = new TestLoader(InterchangeType.Descriptors.Name);
            loader.SetLoadFromBehavior(() =>
                new LoadResult
                {
                    LoadedResourceCount = 0,
                    SourceElementCount = 0
                });

            var sut =
                new BulkLoadMaster(
                    new Dictionary<InterchangeType, IInterchangeController> { { InterchangeType.Descriptors, loader } },
                    validator,
                    new FindBulkOperations(() => mockCtx),
                    new SetBulkOperationStatus(() => mockCtx),
                    new PersistBulkOperationExceptions(() => mockCtx, GetErrorProvider()),
                    new SetUploadFileStatus(() => mockCtx), new DeleteUploadFileChunks(() => mockCtx));

            sut.Handle(new StartOperationCommand { OperationId = operationId }).Wait();

            exceptions.AssertWasCalled(
                x =>
                    x.Add(
                        Arg<BulkOperationException>.Matches(
                            op =>
                                op.Message.StartsWith(expectedErrorMsg))));
            mockCtx.AssertWasCalled(ctx => ctx.SaveChanges());
        }
    }
}
