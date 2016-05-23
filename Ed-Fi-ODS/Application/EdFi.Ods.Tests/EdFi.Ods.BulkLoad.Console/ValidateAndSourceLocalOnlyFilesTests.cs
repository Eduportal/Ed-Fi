// ReSharper disable CheckNamespace

namespace EdFi.Ods.Tests.EdFi.Ods.BulkLoad.Console
// ReSharper restore CheckNamespace
{
    using global::EdFi.Ods.BulkLoad.Console;
    using global::EdFi.Ods.Common;
    using global::EdFi.Ods.Tests.EdFi.Ods.CommonUtils._Stubs;
    using global::EdFi.Ods.Tests._Bases;

    using NUnit.Framework;

    using Rhino.Mocks;

    using Should;

    public class ValidateAndSourceLocalOnlyFilesTests
    {
        [TestFixture]
        public class When_bulk_operation_working_folder_is_not_configured : TestBase
        {
            private IUploadFileSourcingResults _result;

            [TestFixtureSetUp]
            public void Setup()
            {
                var localUploadFiles = new[] {new LocalUploadFile("id1", "path1"), new LocalUploadFile("id2", "path2")};
                var config = this.Stub<IConfigurationAccess>();
                config.Stub(x => x.BulkOperationWorkingFolder).Return(null);

                var testFileSystem = new TestFileSystem().WithTempPath("SomeTempPath");

                var sut = new ValidateAndSourceLocalOnlyFiles(localUploadFiles, config, testFileSystem);
                this._result = sut.ValidateMakeLocalAndFindPath(null, "id2");
            }

            [Test]
            public void Should_use_temp_path()
            {
                this._result.FilePathIfValid.ShouldEqual("SomeTempPath\\id2");
            }
        }

        [TestFixture]
        public class When_working_folder_is_configured_and_we_make_local_files : TestBase
        {
            private IUploadFileSourcingResults _result;
            private TestFileSystem.CopiedFile[] _copiedFiles;

            [TestFixtureSetUp]
            public void Setup()
            {
                var localUploadFiles = new[] {new LocalUploadFile("id1", "path1"), new LocalUploadFile("id2", "path2")};
                var config = this.Stub<IConfigurationAccess>();
                config.Stub(x => x.BulkOperationWorkingFolder).Return("SomeFolder");

                var testFileSystem = new TestFileSystem();

                var sut = new ValidateAndSourceLocalOnlyFiles(localUploadFiles, config, testFileSystem);
                this._result = sut.ValidateMakeLocalAndFindPath(null, "id2");
                this._copiedFiles = testFileSystem.CopiedFiles;
            }

            [Test]
            public void Should_copy_the_input_file_to_the_working_directory()
            {
                this._copiedFiles.Length.ShouldEqual(1);
                this._copiedFiles[0].SourcePath.ShouldEqual("path2");
                this._copiedFiles[0].DestinationPath.ShouldEqual("SomeFolder\\id2");
            }

            [Test]
            public void Should_point_to_the_file_in_the_working_directory()
            {
                this._result.FilePathIfValid.ShouldEqual("SomeFolder\\id2");
            }

            [Test]
            public void Should_indicate_successful_file()
            {
                this._result.IsFailure.ShouldBeFalse();
            }
        }
    }
}