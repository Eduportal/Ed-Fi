using System.Data.Entity;
using System.Linq;
using EdFi.Ods.Api.Data.Contexts;
using EdFi.Ods.Common.Context;

namespace EdFi.Ods.Api.Data.Repositories.BulkOperations
{
    public class UploadValidator : IVerifyUploads
    {
        private readonly IDbExecutor<IBulkOperationDbContext> _executor;
        private readonly ISchoolYearContextProvider _schoolYearContextProvider;

        public UploadValidator(IDbExecutor<IBulkOperationDbContext> executor, ISchoolYearContextProvider schoolYearContextProvider)
        {
            _executor = executor;
            _schoolYearContextProvider = schoolYearContextProvider;
        }

        public UploadValidationErrors IsValid(string uploadId)
        {
            var uploadEntity = _executor.Get(x => x.UploadFiles.Find(uploadId));
            if (uploadEntity == null || uploadEntity.Id == null)
                return
                    new UploadValidationErrors(
                        string.Format(
                            "Could not find an upload file with {0}.  The file has either expired or has completed processing.",
                            uploadId));

            return ValidateSchoolYear(uploadEntity);
        }

        private UploadValidationErrors ValidateSchoolYear(UploadFile uploadFile)
        {
            var operation =
                _executor.Get(
                    ctx =>
                        ctx.BulkOperations.Include("UploadFiles")
                            .SingleOrDefault(x => x.UploadFiles.Any(f => f.Id == uploadFile.Id)));

            var schoolYear = _schoolYearContextProvider.GetSchoolYear();
            if (schoolYear != operation.SchoolYear)
                return
                    new UploadValidationErrors(
                        string.Format(
                            "School year of {0} does not match bulk operation's school year of {1}.",
                            schoolYear, operation.SchoolYear));

            return new UploadValidationErrors(string.Empty);          
        }

        public UploadValidationErrors IsValid(string uploadId, long offset, long size)
        {
            var uploadEntity = _executor.Get(x => x.UploadFiles.Find(uploadId));
            if(uploadEntity == null || uploadEntity.Id == null) return new UploadValidationErrors(string.Format("An upload file with id {0} could not be found and has either expired or has been processed.", uploadId));
            if ((offset + size) > uploadEntity.Size)
                return new UploadValidationErrors(
                    string.Format("The offset + size exceeds the expected total size of the file.  {0} + {1} > {2}",
                        offset, size, uploadEntity.Size));

            return ValidateSchoolYear(uploadEntity);
        }
    }
}