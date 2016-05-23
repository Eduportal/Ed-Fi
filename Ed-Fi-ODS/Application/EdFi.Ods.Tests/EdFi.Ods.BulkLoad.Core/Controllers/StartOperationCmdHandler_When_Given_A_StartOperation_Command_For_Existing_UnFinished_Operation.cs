using EdFi.Ods.Api.Data;
using EdFi.Ods.Api.Models.Resources.Enums;
using EdFi.Ods.BulkLoad.Core;
using EdFi.Ods.BulkLoad.Core.Controllers;
using EdFi.Ods.BulkLoad.Core.Data;
using EdFi.Ods.Common;
using EdFi.Ods.Messaging.BulkLoadCommands;
using EdFi.Ods.Tests.EdFi.Ods.Common._Stubs;
using EdFi.Ods.Tests.TestObjects;
using EdFi.Ods.Tests._Bases;

namespace EdFi.Ods.Tests.EdFi.Ods.BulkLoad.Core.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using NHibernate.Linq;
    using NUnit.Framework;
    using Rhino.Mocks;
    using Should;

    [TestFixture]
    public class StartOperationCmdHandler_When_Given_A_StartOperation_Command_For_Existing_UnFinished_Operation : TestBase
    {
        // TODO: GKM - Remove?
        //public class SetBulkLoadDatabaseContextStub : ISetBulkLoadDatabaseContext
        //{
        //    public void Set(BulkOperation operation)
        //    {
        //        Operation = operation;
        //    }

        //    public BulkOperation Operation { get; private set; }
        //}

        [Test]
        public void And_Has_File_That_Loads_All_Results_Should_Set_File_Status_To_Completed_And_Load_To_Correct_Database()
        {
            var sourceFileDude = Stub<IValidateAndSourceFiles>();
            sourceFileDude.Expect(s => s.ValidateMakeLocalAndFindPath(Arg<string>.Is.Anything, Arg<string>.Is.Anything))
                .IgnoreArguments()
                .Return(StubUploadFileSourcingResults.WithSuccessPath(@"c:\stubby\notreal.xml"));
            var command = new StartOperationCommand
            {
                OperationId = Guid.NewGuid(),
            };
            var operation = new BulkOperation
            {
                Id = command.OperationId.ToString(),
                UploadFiles =
                    new[]
                    {
                        new UploadFile
                        {
                            Id = Guid.NewGuid().ToString(),
                            InterchangeType = InterchangeType.Descriptors.Name,
                            Status = UploadFileStatus.Ready,
                        }
                    }
            };
            operation.UploadFiles.First().UploadFileChunks = new Collection<UploadFileChunk> { new UploadFileChunk() };
            var testContext = new TestBulkOperationsContext();
            testContext.BulkOperations.Add(operation);
            operation.UploadFiles.ForEach(f => testContext.UploadFiles.Add(f));
            var loader = new TestLoader(InterchangeType.Descriptors.Name);
            loader.SetLoadFromBehavior(() => new LoadResult {LoadedResourceCount = 1} );
            
            var sut =
                new BulkLoadMaster(
                    new Dictionary<InterchangeType, IInterchangeController> {{InterchangeType.Descriptors, loader}},
                    sourceFileDude, new FindBulkOperations(() => testContext),
                    new SetBulkOperationStatus(() => testContext),
                    new PersistBulkOperationExceptions(() => testContext, GetErrorProvider()),
                    new SetUploadFileStatus(() => testContext), new DeleteUploadFileChunks(() => testContext));
            sut.Handle(command).Wait();
            var endState = testContext.UploadFiles.Find(operation.UploadFiles.First().Id);
            endState.ShouldNotBeNull();
            endState.Status.ShouldEqual(UploadFileStatus.Completed);
            endState.UploadFileChunks.Count.ShouldEqual(0);
            // TODO: GKM - Not sure what to test here.
            // databaseContext.Operation.ShouldEqual(operation);
        }

        [Test]
        public void And_Has_File_That_Does_Not_Load_All_Results_Should_Set_File_Status_To_Error()
        {
            var sourceFileDude = Stub<IValidateAndSourceFiles>();
            sourceFileDude.Expect(s => s.ValidateMakeLocalAndFindPath(Arg<string>.Is.Anything, Arg<string>.Is.Anything))
                .IgnoreArguments()
                .Return(StubUploadFileSourcingResults.WithSuccessPath(@"c:\stubby\notreal.xml"));
            var command = new StartOperationCommand
            {
                OperationId = Guid.NewGuid(),
            };
            var operation = new BulkOperation
            {
                Id = command.OperationId.ToString(),
                UploadFiles = new [] {new UploadFile{Id = Guid.NewGuid().ToString(), InterchangeType = InterchangeType.Descriptors.Name, Status = UploadFileStatus.Ready}}
            };
            operation.UploadFiles.First().UploadFileChunks = new Collection<UploadFileChunk> { new UploadFileChunk() };
            var testContext = new TestBulkOperationsContext();
            testContext.BulkOperations.Add(operation);
            operation.UploadFiles.ForEach(f => testContext.UploadFiles.Add(f));
            var loader = new TestLoader(InterchangeType.Descriptors.Name);

            loader.SetLoadFromBehavior(() =>
                new LoadResult
                {
                    LoadExceptions =
                        new List<LoadException> {new LoadException {Exception = new Exception()}}
                });
            var sut =
                new BulkLoadMaster(
                    new Dictionary<InterchangeType, IInterchangeController> {{InterchangeType.Descriptors, loader}},
                    sourceFileDude, new FindBulkOperations(() => testContext),
                    new SetBulkOperationStatus(() => testContext),
                    new PersistBulkOperationExceptions(() => testContext, GetErrorProvider()),
                    new SetUploadFileStatus(() => testContext), new DeleteUploadFileChunks(() => testContext));
            sut.Handle(command).Wait();
            var endState = testContext.UploadFiles.Find(operation.UploadFiles.First().Id);
            endState.ShouldNotBeNull();
            endState.Status.ShouldEqual(UploadFileStatus.Error);
            endState.UploadFileChunks.Count.ShouldEqual(0);
        }


        [Test]
        public void And_Operation_Has_Only_Descriptors_Should_Run_Descriptors_Load_And_Cleanup_Working_File()
        {
            var sourceFileDude = Stub<IValidateAndSourceFiles>();
            var workingFile = StubUploadFileSourcingResults.WithSuccessPath(@"c:\stubby\notreal.xml");
            sourceFileDude.Expect(s => s.ValidateMakeLocalAndFindPath(Arg<string>.Is.Anything, Arg<string>.Is.Anything))
                .IgnoreArguments()
                .Return(workingFile);
            var command = new StartOperationCommand
            {
                OperationId = Guid.NewGuid(),
            };
            var operation = new BulkOperation
            {
                Id = command.OperationId.ToString(),
                UploadFiles = new [] {new UploadFile{Id = Guid.NewGuid().ToString(), InterchangeType = InterchangeType.Descriptors.Name, Status = UploadFileStatus.Ready}}
            };
            var testContext = new TestBulkOperationsContext();
            testContext.BulkOperations.Add(operation);
            operation.UploadFiles.ForEach(f => testContext.UploadFiles.Add(f));
            var loader = new TestLoader(InterchangeType.Descriptors.Name);
            loader.SetLoadFromBehavior(() => new LoadResult {LoadedResourceCount = 1} );

            var sut =
                new BulkLoadMaster(
                    new Dictionary<InterchangeType, IInterchangeController> {{InterchangeType.Descriptors, loader}},
                    sourceFileDude, new FindBulkOperations(() => testContext),
                    new SetBulkOperationStatus(() => testContext),
                    new PersistBulkOperationExceptions(() => testContext, GetErrorProvider()),
                    new SetUploadFileStatus(() => testContext), new DeleteUploadFileChunks(() => testContext));
            sut.Handle(command).Wait();
            var operationEndState = testContext.BulkOperations.FirstOrDefault();
            operationEndState.ShouldNotBeNull();
            operationEndState.Status.ShouldEqual(BulkOperationStatus.Completed);
            workingFile.IsDisposed.ShouldBeTrue();
        }

        [Test]
        public void And_A_File_Fails_To_Load_Should_Not_Execute_Loader_And_Update_Status_To_Error()
        {
            InitSystemClock(new DateTime(2003, 3, 3));
            var sourceFileDude = Stub<IValidateAndSourceFiles>();
            sourceFileDude.Expect(s => s.ValidateMakeLocalAndFindPath(Arg<string>.Is.Anything, Arg<string>.Is.Anything))
                .IgnoreArguments()
                .Return(UploadFileSourcingResults.WithValidationErrorMessage("No Worky"));
            var command = new StartOperationCommand
            {
                OperationId = Guid.NewGuid(),
            };
            var operation = new BulkOperation
            {
                Id = command.OperationId.ToString(),
                UploadFiles = new []
                {
                    new UploadFile{Id = Guid.NewGuid().ToString(), InterchangeType = InterchangeType.Descriptors.Name, Status = UploadFileStatus.Ready},
                }
            };
            operation.UploadFiles.First().UploadFileChunks = new Collection<UploadFileChunk> { new UploadFileChunk() };
            var testContext = new TestBulkOperationsContext();
            operation.UploadFiles.ForEach(f => testContext.UploadFiles.Add(f));
            testContext.BulkOperations.Add(operation);
            var loader = new TestLoader(InterchangeType.Descriptors.Name);
            loader.SetLoadFromBehavior(() => new LoadResult());

            var sut =
                new BulkLoadMaster(
                    new Dictionary<InterchangeType, IInterchangeController> {{InterchangeType.Descriptors, loader}},
                    sourceFileDude, new FindBulkOperations(() => testContext),
                    new SetBulkOperationStatus(() => testContext),
                    new PersistBulkOperationExceptions(() => testContext, GetErrorProvider()),
                    new SetUploadFileStatus(() => testContext), new DeleteUploadFileChunks(() => testContext));
            sut.Handle(command).Wait();
            var endState = testContext.UploadFiles.FirstOrDefault();
            endState.ShouldNotBeNull();
            endState.Status.ShouldEqual(UploadFileStatus.Error);
            endState.UploadFileChunks.Count.ShouldEqual(0);
        }

        [Test]
        public void And_At_Least_One_File_Is_Not_Ready_Should_Exit_With_Error()
        {
            InitSystemClock(new DateTime(2001, 5, 4));
            const string expectedErrorMsg = "UploadFileStatus is not set to Ready";
            var sourceFileDude = Stub<IValidateAndSourceFiles>();
            sourceFileDude.Expect(s => s.ValidateMakeLocalAndFindPath("", ""))
                .IgnoreArguments()
                .Return(UploadFileSourcingResults.WithValidationErrorMessage(expectedErrorMsg));
            var command = new StartOperationCommand
            {
                OperationId = Guid.NewGuid(),
            };
            var operation = new BulkOperation
            {
                Id = command.OperationId.ToString(),
                UploadFiles = new []
                {
                    new UploadFile{Id = Guid.NewGuid().ToString(), InterchangeType = InterchangeType.Student.Name, Status = UploadFileStatus.Ready},
                    new UploadFile{Id = Guid.NewGuid().ToString(), InterchangeType = InterchangeType.MasterSchedule.Name, Status = UploadFileStatus.Incomplete}
                }
            };
            var testContext = new TestBulkOperationsContext();
            testContext.BulkOperations.Add(operation);

            var sut = new BulkLoadMaster(new Dictionary<InterchangeType, IInterchangeController>(), sourceFileDude,
                new FindBulkOperations(() => testContext), new SetBulkOperationStatus(() => testContext),
                new PersistBulkOperationExceptions(() => testContext, GetErrorProvider()),
                new SetUploadFileStatus(() => testContext), new DeleteUploadFileChunks(() => testContext));
            sut.Handle(command).Wait();
            var endState = testContext.BulkOperations.FirstOrDefault();
            endState.ShouldNotBeNull();
            endState.Status.ShouldEqual(BulkOperationStatus.Error);
        }
    }

    public class StubLocalXMLFileManager : IValidateAndSourceFiles
    {
        public IUploadFileSourcingResults ValidateMakeLocalAndFindPath(string operationId, string uploadFileId)
        {
            return StubUploadFileSourcingResults.WithSuccessPath(
                string.Format(@"c:\stubby\{0}\{1}.xml", operationId, uploadFileId));
        }
    }
}