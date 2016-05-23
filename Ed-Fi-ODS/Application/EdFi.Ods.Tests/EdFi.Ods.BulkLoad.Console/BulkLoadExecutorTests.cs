using System.Threading.Tasks;
using EdFi.Ods.Api.Data;
using EdFi.Ods.Api.Models.Resources.Enums;
using EdFi.Ods.BulkLoad.Console;
using EdFi.Ods.BulkLoad.Core.Controllers;
using EdFi.Ods.BulkLoad.Core.Data;
using EdFi.Ods.Common.Utils;
using EdFi.Ods.Messaging.BulkLoadCommands;
using EdFi.Ods.Tests.EdFi.Ods.CommonUtils._Stubs;
using EdFi.Ods.Tests._Bases;
// ReSharper disable CheckNamespace



namespace EdFi.Ods.Tests.EdFi.Ods.BulkLoad.Console
// ReSharper restore CheckNamespace
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NUnit.Framework;
    using Rhino.Mocks;
    using Should;

    public class BulkLoadExecutorTests
    {
        public class TestBulkLoadMaster : IControlBulkLoading
        {
            private List<StartOperationCommand> _handledCommands = new List<StartOperationCommand>();

            public Task Handle(StartOperationCommand command)
            {
                return Task.Run(() => _handledCommands.Add(command));
            }

            public StartOperationCommand[] HandledCommands
            {
                get { return _handledCommands.ToArray(); }
            }
        }

        [TestFixture]
        public class When_operation_does_not_complete : TestBase
        {
            private string _operationId;
            private TestBulkLoadMaster _bulkLoadMaster;
            private bool _result;
            private string[] _output;

            [TestFixtureSetUp]
            public void Setup()
            {
                _operationId = Guid.NewGuid().ToString();
                _bulkLoadMaster = new TestBulkLoadMaster();

                var findBulkOperations = Stub<IFindBulkOperations>();
                var fileId1 = Guid.NewGuid().ToString();
                var fileId2 = Guid.NewGuid().ToString();
                findBulkOperations.Stub(x => x.FindWithFiles(_operationId))
                                  .Return(new BulkOperation
                                              {
                                                  Status = BulkOperationStatus.Error,
                                                  UploadFiles =
                                                      new[]
                                                          {
                                                              new UploadFile {Id = fileId1},
                                                              new UploadFile {Id = fileId2},
                                                          }
                                              });

                var findExceptions = Stub<IFindBulkOperationExceptions>();
                findExceptions.Stub(x => x.FindByUploadFile(fileId1))
                              .Return(new[] {new BulkOperationException {Message = "M - 1", Element = "E - 1"},});
                findExceptions.Stub(x => x.FindByUploadFile(fileId2))
                              .Return(new[]
                                          {
                                              new BulkOperationException {Message = "M - 2.1", Element = "E - 2.1"},
                                              new BulkOperationException {Message = "M - 2.2", Element = "E - 2.2"},
                                          });

                var testOutput = new TestOutput();

                var executor = new BulkLoadExecutor(_bulkLoadMaster, findBulkOperations, findExceptions,
                                                    testOutput);
                _result = executor.Execute(_operationId);
                _output = testOutput.AllOutput;
            }

            [Test]
            public void Should_write_errors()
            {
                _output.Length.ShouldEqual(9);
                _output[0].ShouldEqual("Error: M - 1");
                _output[1].ShouldEqual("Element: E - 1");
                _output[2].ShouldEqual("");
                _output[3].ShouldEqual("Error: M - 2.1");
                _output[4].ShouldEqual("Element: E - 2.1");
                _output[5].ShouldEqual("");
                _output[6].ShouldEqual("Error: M - 2.2");
                _output[7].ShouldEqual("Element: E - 2.2");
                _output[8].ShouldEqual("");
            }

            [Test]
            public void Should_indicate_failure()
            {
                _result.ShouldBeFalse();
            }
        }

        [TestFixture]
        public class When_operation_does_complete : TestBase
        {
            private string _operationId;
            private TestBulkLoadMaster _bulkLoadMaster;
            private bool _result;

            [TestFixtureSetUp]
            public void Setup()
            {
                _operationId = Guid.NewGuid().ToString();
                _bulkLoadMaster = new TestBulkLoadMaster();

                var findBulkOperations = Stub<IFindBulkOperations>();
                findBulkOperations.Stub(x => x.FindWithFiles(_operationId))
                                  .Return(new BulkOperation {Status = BulkOperationStatus.Completed});

                IFindBulkOperationExceptions doNotLookForExceptions = null;
                IOutput doNotUseOutput = null;

                var executor = new BulkLoadExecutor(_bulkLoadMaster, findBulkOperations, doNotLookForExceptions,
                                                    doNotUseOutput);
                _result = executor.Execute(_operationId);
            }

            [Test]
            public void Should_start_processing_for_the_correct_operation()
            {
                _bulkLoadMaster.HandledCommands.Length.ShouldEqual(1);
                _bulkLoadMaster.HandledCommands.Count(x => x.OperationId == new Guid(_operationId)).ShouldEqual(1);
            }

            [Test]
            public void Should_not_write_errors()
            {
                //Output was null so it couldn't have been used
            }

            [Test]
            public void Should_indicate_success()
            {
                _result.ShouldBeTrue();
            }
        }
    }
}