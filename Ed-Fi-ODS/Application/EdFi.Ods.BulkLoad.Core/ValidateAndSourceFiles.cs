using System.Data.Entity;
using System.Linq;
using EdFi.Ods.Common;
using EdFi.Ods.Api.Data.Contexts;
using EdFi.Ods.Api.Data;
using EdFi.Ods.Api.Models.Resources.Enums;
using FluentValidation;
using EdFi.Ods.Api.Data.Model;

namespace EdFi.Ods.BulkLoad.Core
{
    public class ValidateAndSourceFiles : IValidateAndSourceFiles
    {
        private readonly IDbExecutor<IBulkOperationDbContext> _executor;
        private readonly IPersistUploadFiles _persistUploadFiles;
        private readonly IValidator<UploadInfo> _validator;

        public ValidateAndSourceFiles(IDbExecutor<IBulkOperationDbContext> executor, IPersistUploadFiles persistUploadFiles, IValidator<UploadInfo> validator)
        {
            _executor = executor;
            _persistUploadFiles = persistUploadFiles;
            _validator = validator;
        }

        public IUploadFileSourcingResults ValidateMakeLocalAndFindPath(string operationId, string uploadFileId)
        {
            var uploadFile = _executor.Get(ctx => ctx.UploadFiles.AsQueryable().First(f => f.Id == uploadFileId));
            var uploadFileChunkInfos = _executor.Get(x => x.GetFileChunkInfos(uploadFileId));
            var uploadInfo = new UploadInfo
            {
                UploadFile = uploadFile,
                UploadFileChunkInfos = uploadFileChunkInfos
            };

            if (uploadFile.Status != UploadFileStatus.Ready)
                return UploadFileSourcingResults.WithValidationErrorMessage(string.Format("UploadFile {0} was not set to ready.", uploadFileId));

            var result = _validator.Validate(uploadInfo);
            if (!result.IsValid)
            {
                return UploadFileSourcingResults.WithValidationErrorMessages(result.Errors.Select(e => e.ErrorMessage).ToArray());
            }

            var persistenceResult = _persistUploadFiles.Persist(operationId, uploadFileId);
            return persistenceResult.IsSuccessful ? UploadFileSourcingResults.WithSuccessPath(persistenceResult.FilePath) : UploadFileSourcingResults.WithValidationErrorMessage(persistenceResult.FailureMessage);
        }
    }
}
