namespace EdFi.Ods.Tests.EdFi.Ods.Common
{
    using global::EdFi.Ods.Common;

    using NUnit.Framework;

    using Should;

    public class UploadFileSourcingResultsTests
    {
        [TestFixture]
        public class When_result_is_created_without_failure_messages
        {
            private UploadFileSourcingResults _result;

            [TestFixtureSetUp]
            public void Setup()
            {
                this._result = UploadFileSourcingResults.WithSuccessPath(@"some\crazy\path");
            }

            [Test]
            public void Should_indicate_result_is_not_a_failure()
            {
                this._result.IsFailure.ShouldBeFalse();
            }

            [Test]
            public void Should_provide_file_path()
            {
                this._result.FilePathIfValid.ShouldEqual(@"some\crazy\path");
            }
        }

        [TestFixture]
        public class When_result_is_created_with_multiple_failure_messages
        {
            private UploadFileSourcingResults _result;

            [TestFixtureSetUp]
            public void Setup()
            {
                this._result = UploadFileSourcingResults.WithValidationErrorMessages(new[] {"foo", "bar"});
            }

            [Test]
            public void Should_indicate_result_is_a_failure()
            {
                this._result.IsFailure.ShouldBeTrue();
            }

            [Test]
            public void Should_provide_failure_message()
            {
                this._result.ValidationErrorMessages.Length.ShouldEqual(2);
                this._result.ValidationErrorMessages.ShouldContain("foo");
                this._result.ValidationErrorMessages.ShouldContain("bar");
            }
        }

        [TestFixture]
        public class When_result_is_created_with_a_single_failure_message
        {
            private UploadFileSourcingResults _result;

            [TestFixtureSetUp]
            public void Setup()
            {
                this._result = UploadFileSourcingResults.WithValidationErrorMessage("foo");
            }

            [Test]
            public void Should_indicate_result_is_a_failure()
            {
                this._result.IsFailure.ShouldBeTrue();
            }

            [Test]
            public void Should_provide_failure_message()
            {
                this._result.ValidationErrorMessages.Length.ShouldEqual(1);
                this._result.ValidationErrorMessages.ShouldContain("foo");
            }
        }
    }
}