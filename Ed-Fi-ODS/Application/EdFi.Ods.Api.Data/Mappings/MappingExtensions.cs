using System.Linq;
using EdFi.Ods.Common.Utils.Extensions;
using EdFi.Ods.Api.Models.Resources.Enums;
using EdFi.Ods.Api.Models.Resources;

namespace EdFi.Ods.Api.Data.Mappings
{
    public static class MappingExtensions
    {
        public static BulkOperation ToEntity(this BulkOperationResource resource)
        {
            return new BulkOperation
            {
                Id = resource.Id,
                ResetDistrictData = resource.ResetDistrictData,
                Status = resource.Status.ToEnum<BulkOperationStatus>(true),
                UploadFiles =
                    resource.UploadFiles == null ? null : resource.UploadFiles.Select(f => f.ToEntity()).ToArray()
            };
        }

        public static UploadFile ToEntity(this UploadFileResource resource)
        {
            return new UploadFile
            {
                Format = resource.Format,
                InterchangeType = resource.InterchangeType,
                Status = resource.Status.ToEnum<UploadFileStatus>(true),
                Id = resource.Id,
                Size = resource.Size
            };
        }

        public static BulkOperationResource ToResource(this BulkOperation entity)
        {
            return new BulkOperationResource
            {
                Id = entity.Id,
                ResetDistrictData = entity.ResetDistrictData,
                Status = entity.Status.GetName(),
                UploadFiles =
                    entity.UploadFiles == null ? null : entity.UploadFiles.Select(x => x.ToResource()).ToArray()
            };
        }

        public static Models.Resources.BulkOperationException ToResource(this BulkOperationException entity)
        {
            return new Models.Resources.BulkOperationException()
            {
                Id = entity.Id,
                UploadFileId = entity.ParentUploadFileId,
                Type = entity.Type,
                Element = entity.Element,
                Message = entity.Message
            };
        }

        public static UploadFileResource ToResource(this UploadFile entity)
        {
            return new UploadFileResource
            {
                Format = entity.Format,
                InterchangeType = entity.InterchangeType,
                Status = entity.Status.GetName(),
                Id = entity.Id,
                Size = entity.Size,
            };
        }
    }
}
