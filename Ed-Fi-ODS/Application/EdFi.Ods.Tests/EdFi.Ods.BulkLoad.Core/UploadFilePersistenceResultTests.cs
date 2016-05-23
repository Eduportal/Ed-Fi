namespace EdFi.Ods.Tests.EdFi.Ods.BulkLoad.Core
{
    using global::EdFi.Ods.BulkLoad.Core;
    using global::EdFi.Ods.Tests._Bases;

    using NUnit.Framework;

    using Should;

    public class UploadFilePersistenceResultTests
    {
        [TestFixture]
        public class When_creating_result_with_success
        {
            private UploadFilePersistenceResult _result;

            [TestFixtureSetUp]
            public void Setup()
            {
                this._result = UploadFilePersistenceResult.WithSuccessfulFilePath(@"c:\some\path");
            }
            [Test]
            public void Should_indicate_succes()
            {
                this._result.IsSuccessful.ShouldBeTrue();
            }

            [Test]
            public void Should_provide_file_path()
            {
                this._result.FilePath.ShouldEqual(@"c:\some\path");
            }
        }

        [TestFixture]
        public class When_creating_result_for_failure : TestBase
        {
            private UploadFilePersistenceResult _result;

            [TestFixtureSetUp]
            public void Setup()
            {
                this._result = UploadFilePersistenceResult.WithFailureMessage("some message");
            }
            [Test]
            public void Should_indicate_failure()
            {
                this._result.IsSuccessful.ShouldBeFalse();
            }

            [Test]
            public void Should_provide_failure_message()
            {
                this._result.FailureMessage.ShouldEqual("some message");
            }
        }
    }
}