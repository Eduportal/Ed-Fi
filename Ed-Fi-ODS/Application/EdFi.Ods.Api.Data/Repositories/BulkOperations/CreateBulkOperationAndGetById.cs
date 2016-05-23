using System;
using System.Data.Entity.Migrations;
using System.Linq;
using EdFi.Ods.Common.Utils.Extensions;
using EdFi.Ods.Api.Data.Contexts;
using EdFi.Ods.Api.Data.Mappings;
using EdFi.Ods.Api.Models.Resources;
using EdFi.Ods.Api.Models.Resources.Enums;
using System.Data.Entity;

namespace EdFi.Ods.Api.Data.Repositories.BulkOperations
{
    //TODO: Once we have more of this pattern, we should find a right home for the classes in this folder
    public class CreateBulkOperationAndGetById : ICreateBulkOperationAndGetById
    {
        private readonly IDbExecutor<IBulkOperationDbContext> _executor;

        public CreateBulkOperationAndGetById(IDbExecutor<IBulkOperationDbContext> executor)
        {
            _executor = executor;
        }

        public BulkOperationResource GetByIdAndYear(string id, int year)
        {
            var entity =
                _executor.Get(
                    ctx =>
                        ctx.BulkOperations.Include("UploadFiles")
                            .SingleOrDefault(x => x.Id == id && x.SchoolYear == year));

            return entity == null ? null : entity.ToResource();
        }

        public void Execute(CreateBulkOperationCommand command)
        {
            var operation = new BulkOperationResource
            {
                Id = Guid.NewGuid().ToString(),
                ResetDistrictData = command.ResetDistrictData,
                Status = BulkOperationStatus.Initialized.GetName(),
                UploadFiles = command.UploadFiles.Select(x => new UploadFileResource
                {
                    Id = Guid.NewGuid().ToString(),
                    Format = x.Format,
                    InterchangeType = x.InterchangeType,
                    Status = UploadFileStatus.Initialized.GetName(),
                    Size = x.Size
                }).ToArray()
            };

            var entity = operation.ToEntity();
            entity.SchoolYear = command.SchoolYear;
            entity.DatabaseName = command.ApiKey;

            _executor.ApplyChanges(ctx => ctx.BulkOperations.AddOrUpdate(entity));
            command.Resource = operation;
        }
    }
}