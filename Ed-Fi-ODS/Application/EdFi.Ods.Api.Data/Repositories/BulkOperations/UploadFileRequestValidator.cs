using System;
using System.Collections.Generic;
using System.Linq;
using EdFi.Ods.Common;
using FluentValidation;

namespace EdFi.Ods.Api.Data.Repositories.BulkOperations
{
    public class UploadFileRequestValidator : AbstractValidator<UploadFileRequest>
    {
        public static readonly string InvalidFileFileFormat = "'{PropertyValue}' is not a valid value for 'format'.";
        public static readonly string InvalidInterchangeTypeMessage = "'{PropertyValue}' is not a valid value for 'Interchange Type'.";

        public UploadFileRequestValidator()
        {
            RuleFor(x => x.Format).NotEmpty()
                                  .Must(x => InterchangeFileFormat.GetByName(x) != null)
                                  .WithMessage(InvalidFileFileFormat);

            RuleFor(x => x.InterchangeType).NotEmpty();

            RuleFor(x => x.InterchangeType)
                .Must(x => InterchangeType.GetByName(x) != null)
                .When(x => !String.IsNullOrWhiteSpace(x.InterchangeType))
                .WithMessage(InvalidInterchangeTypeMessage);
        }
    }
}