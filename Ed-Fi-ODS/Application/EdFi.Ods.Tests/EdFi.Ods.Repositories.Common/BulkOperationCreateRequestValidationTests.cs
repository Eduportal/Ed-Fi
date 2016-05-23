namespace EdFi.Ods.Tests.EdFi.Ods.Repositories.Common
{
    using System.Linq;

    using global::EdFi.Ods.Api.Data.Repositories.BulkOperations;
    using global::EdFi.Ods.Common;

    using NUnit.Framework;

    using Should;

    [TestFixture]
    public class When_validating_BulkOperationCreateRequest_object
    {
        private BulkOperationCreateValidator validator;

        [SetUp]
        public void SetUp()
        {
            this.validator = new BulkOperationCreateValidator();
        }

        [Test]
        public void Should_return_no_errors_given_a_valid_object()
        {
            var objectToValidate = new BulkOperationCreateRequest
            {
                UploadFiles = new[]
                    {
                        new UploadFileRequest
                            {
                                Format = InterchangeFileFormat.TextXml.Name,
                                InterchangeType = "eDuCationOrganizatioN"
                            },
                    }
            };

            this.validator.Validate(objectToValidate).IsValid.ShouldBeTrue();
        }

        [Test]
        public void Should_return_error_when_UploadFiles_is_empty()
        {
            var objectToValidate = new BulkOperationCreateRequest
            {
                UploadFiles = new UploadFileRequest[] {}
            };


            var validationResults = this.validator.Validate(objectToValidate);

            validationResults.Errors.Any(x => x.PropertyName == "UploadFiles").ShouldBeTrue();
        }

        [Test]
        public void Should_return_error_when_UploadFiles_is_null()
        {
            var objectToValidate = new BulkOperationCreateRequest();

            var validationResults = this.validator.Validate(objectToValidate);

            validationResults.Errors.Any(x => x.PropertyName == "UploadFiles").ShouldBeTrue();
        }

        [Test]
        public void Should_return_error_when_file_format_is_not_provided()
        {
            var objectToValidate = new BulkOperationCreateRequest
                {
                    UploadFiles = new[]
                        {
                            new UploadFileRequest
                                {
                                    InterchangeType = "SomeInterchange"
                                }
                        }
                };

            var validationResults = this.validator.Validate(objectToValidate);

            validationResults.Errors.Any(x => x.PropertyName == "UploadFiles[0].Format").ShouldBeTrue();
        }

        [Test]
        public void Should_return_error_when_file_format_is_invalid()
        {
            var objectToValidate = new BulkOperationCreateRequest
            {
                UploadFiles = new[]
                        {
                            new UploadFileRequest
                                {
                                    Format = "Some wierd format",
                                    InterchangeType = "SomeInterchange"
                                }
                        }
            };

            var validationResults = this.validator.Validate(objectToValidate);

            validationResults.Errors.Any(x => x.PropertyName == "UploadFiles[0].Format").ShouldBeTrue();
        }

        [Test]
        public void Should_return_error_when_interchange_type_is_not_provided()
        {
            var objectToValidate = new BulkOperationCreateRequest
            {
                UploadFiles = new[]
                        {
                            new UploadFileRequest
                                {
                                    Format = InterchangeFileFormat.ApplicationXml.Name
                                }
                        }
            };

            var validationResults = this.validator.Validate(objectToValidate);

            var error = validationResults.Errors.SingleOrDefault(x => x.PropertyName == "UploadFiles[0].InterchangeType");
            error.ShouldNotBeNull();
            var expectedErrorMessage = FluentValidation.Resources.Messages.notempty_error.Replace("{PropertyName}", "Interchange Type");
            error.ErrorMessage.ShouldEqual(expectedErrorMessage);
        }

        [Test]
        public void Should_return_error_when_interchange_type_is_invalid()
        {
            var objectToValidate = new BulkOperationCreateRequest
            {
                UploadFiles = new[]
                        {
                            new UploadFileRequest
                                {
                                    InterchangeType = "InvalidType"
                                }
                        }
            };

            var validationResults = this.validator.Validate(objectToValidate);

            var error = validationResults.Errors.SingleOrDefault(x => x.PropertyName == "UploadFiles[0].InterchangeType");
            error.ShouldNotBeNull();
            var expectedErrorMessage = UploadFileRequestValidator.InvalidInterchangeTypeMessage.Replace("{PropertyValue}", objectToValidate.UploadFiles.First().InterchangeType);
            error.ErrorMessage.ShouldEqual(expectedErrorMessage);
        }
    }
}
