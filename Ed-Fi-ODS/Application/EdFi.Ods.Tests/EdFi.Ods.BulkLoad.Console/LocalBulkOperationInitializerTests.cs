// ReSharper disable CheckNamespace

namespace EdFi.Ods.Tests.EdFi.Ods.BulkLoad.Console
// ReSharper restore CheckNamespace
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using global::EdFi.Common.IO;
    using global::EdFi.Ods.Api.Data;
    using global::EdFi.Ods.Api.Models.Resources.Enums;
    using global::EdFi.Ods.BulkLoad.Console;
    using global::EdFi.Ods.BulkLoad.Core;
    using global::EdFi.Ods.BulkLoad.Core.Data;
    using global::EdFi.Ods.Common;
    using global::EdFi.Ods.Tests._Bases;

    using NUnit.Framework;

    using Rhino.Mocks;

    using Should;

    public class LocalBulkOperationInitializerTests
    {
        public class TestCreateBulkOperation : ICreateBulkOperation
        {
            private List<BulkOperation> _operations = new List<BulkOperation>();

            public void Create(BulkOperation operation)
            {
                this._operations.Add(operation);
            }

            public BulkOperation[] CreatedOperations
            {
                get { return this._operations.ToArray(); }
            }
        }

        [TestFixture]
        public class When_no_source_files_exist : TestBase
        {
            private LocalBulkOperation _result;

            [TestFixtureSetUp]
            public void Setup()
            {
                IInterchangeFileTypeTranslator doNotCalculateInterchangeType = null;
                ICreateBulkOperation doNotCreateAnOperation = null;
                var config = new BulkLoaderConfiguration {SourceFolder = "SomeCrazyFolder"};

                var fileSystem = this.Stub<IFileSystem>();
                fileSystem.Stub(x => x.DirectoryExists("SomeCrazyFolder")).Return(true);
                fileSystem.Stub(x => x.GetFilesInDirectory("SomeCrazyFolder")).Return(new string[0]);

                var sut = new LocalBulkOperationInitializer(doNotCalculateInterchangeType, doNotCreateAnOperation,
                                                            config,
                                                            fileSystem);
                this._result = sut.CreateOperationAndGetLocalFiles();
            }

            [Test]
            public void Should_return_an_empty_result()
            {
                this._result.IsEmpty.ShouldBeTrue();
            }
        }

        [TestFixture]
        public class When_input_directory_does_not_exist : TestBase
        {
            private ArgumentException _exception;

            [TestFixtureSetUp]
            public void Setup()
            {
                IInterchangeFileTypeTranslator doNotCalculateInterchangeType = null;
                ICreateBulkOperation doNotCreateAnOperation = null;
                var config = new BulkLoaderConfiguration {SourceFolder = "SomeCrazyFolder"};

                var fileSystem = this.Stub<IFileSystem>();
                fileSystem.Stub(x => x.DirectoryExists("SomeCrazyFolder")).Return(false);

                var sut = new LocalBulkOperationInitializer(doNotCalculateInterchangeType, doNotCreateAnOperation,
                                                            config,
                                                            fileSystem);
                this._exception = this.TestForException<ArgumentException>(() => sut.CreateOperationAndGetLocalFiles());
            }

            [Test]
            public void Should_throw_argument_exception()
            {
                this._exception.Message.ShouldEqual("Source path 'SomeCrazyFolder' not found");
            }
        }

        [TestFixture]
        public class When_input_directory_exists_with_files : TestBase
        {
            private BulkOperation[] _createdOperations;
            private LocalBulkOperation _result;

            [TestFixtureSetUp]
            public void Setup()
            {
                var testCreateBulkOperation = new TestCreateBulkOperation();
                var config = new BulkLoaderConfiguration {DatabaseNameOverride = "MyDatabase", SourceFolder = "SomeCrazyFolder"};

                var fileSystem = this.Stub<IFileSystem>();
                fileSystem.Stub(x => x.DirectoryExists("SomeCrazyFolder")).Return(true);
                fileSystem.Stub(x => x.GetFilesInDirectory("SomeCrazyFolder"))
                          .Return(new[] {"SomeCrazyFolder\\One", "SomeCrazyFolder\\Two"});
                fileSystem.Stub(x => x.GetFilenameFromPath("SomeCrazyFolder\\One")).Return("One");
                fileSystem.Stub(x => x.GetFilenameFromPath("SomeCrazyFolder\\Two")).Return("Two");

                var fileTypeTranslator = this.Stub<IInterchangeFileTypeTranslator>();
                fileTypeTranslator.Stub(x => x.GetInterchangeType("One")).Return(InterchangeType.Descriptors.Name);
                fileTypeTranslator.Stub(x => x.GetInterchangeType("Two")).Return(InterchangeType.EducationOrganization.Name);

                var sut = new LocalBulkOperationInitializer(fileTypeTranslator, testCreateBulkOperation, config,
                                                            fileSystem);
                this._result = sut.CreateOperationAndGetLocalFiles();
                this._createdOperations = testCreateBulkOperation.CreatedOperations;
            }

            [Test]
            public void
                Should_provide_a_list_of_files_with_IDs_that_correspond_to_the_upload_files_saved_in_the_operational_database
                ()
            {
                this._result.LocalUploadFiles.Length.ShouldEqual(2);
                var localFile1 = this._result.LocalUploadFiles.Single(x => x.FilePath.Equals("SomeCrazyFolder\\One"));
                var localFile2 = this._result.LocalUploadFiles.Single(x => x.FilePath.Equals("SomeCrazyFolder\\Two"));

                var uploadFile1 = this._createdOperations[0].UploadFiles.Single(x => x.Id == localFile1.Id);
                uploadFile1.InterchangeType.ShouldEqual(InterchangeType.Descriptors.Name);
                uploadFile1.Format.ShouldEqual("text/xml");
                uploadFile1.Status.ShouldEqual(UploadFileStatus.Ready);
                
                var uploadFile2 = this._createdOperations[0].UploadFiles.Single(x => x.Id == localFile2.Id);
                uploadFile2.InterchangeType.ShouldEqual(InterchangeType.EducationOrganization.Name);
                uploadFile2.Format.ShouldEqual("text/xml");
                uploadFile2.Status.ShouldEqual(UploadFileStatus.Ready);
            }

            [Test]
            public void Should_create_an_operation_with_the_correct_database_name()
            {
                this._createdOperations.Length.ShouldEqual(1);
                this._createdOperations[0].DatabaseName.ShouldEqual("MyDatabase");
            }

            [Test]
            public void Should_save_the_operation_in_the_operational_database()
            {
                this._createdOperations.Length.ShouldEqual(1);
                this._createdOperations[0].Id.ShouldEqual(this._result.OperationId);
            }
        }
    }
}