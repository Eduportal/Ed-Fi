using System;
using System.Linq;
using System.Data.Entity;
using EdFi.Common.Messaging;
using EdFi.Ods.Api.Data.Contexts;
using EdFi.Ods.Api.Data;
using EdFi.Ods.Api.Models.Resources.Enums;
using EdFi.Ods.Messaging.BulkLoadCommands;
using FluentValidation;
using EdFi.Ods.Api.Data.Model;

namespace EdFi.Ods.BulkLoad.Core.Controllers
{
    public class UploadFileCommitHandler : IMessageHandler<CommitUploadCommand>
    {
        private readonly IDbExecutor<IBulkOperationDbContext> _dbExecutor;
        private readonly IValidator<UploadInfo> _validator;
        private readonly ICommandSender _commandSender;

        public UploadFileCommitHandler(IDbExecutor<IBulkOperationDbContext> dbExecutor, IValidator<UploadInfo> validator, ICommandSender commandSender)
        {
            _dbExecutor = dbExecutor;
            _validator = validator;
            _commandSender = commandSender;
        }

        public void Handle(CommitUploadCommand command)
        {
            var uploadFile = _dbExecutor.Get(c => c.UploadFiles.AsQueryable().Single(x => x.Id == command.UploadId));
            var uploadFileChunkInfos = _dbExecutor.Get(x => x.GetFileChunkInfos(command.UploadId));
            var uploadInfo = new UploadInfo
            {
                UploadFile = uploadFile,
                UploadFileChunkInfos = uploadFileChunkInfos
            };

            var operation =
                _dbExecutor.Get(c => c.BulkOperations.AsQueryable().Include("UploadFiles").FirstOrDefault(o => o.UploadFiles.Any(u => u.Id == command.UploadId)));

            if (uploadFile == null) throw new ArgumentException(string.Format("The upload file id ({0}) supplied cannot be found in the database.", command.UploadId));
            var validationResults = _validator.Validate(uploadInfo);
            if (!validationResults.IsValid)
            {
                _dbExecutor.ApplyChanges(c =>
                {
                    c.UploadFiles.Attach(uploadFile);
                    uploadFile.Status = UploadFileStatus.Error;
                    var operationForUpdate = c.BulkOperations.Find(operation.Id);
                    operationForUpdate.Status = BulkOperationStatus.Error;
                    foreach (var validationError in validationResults.Errors)
                    {
                        c.BulkOperationExceptions.Add(new BulkOperationException()
                        {
                            ParentUploadFileId = uploadFile.Id,
                            DateTime = DateTime.Now,
                            Message = validationError.ErrorMessage
                        });
                    }
                });
                return;
            }
            if (operation.UploadFiles.Count(u => u.Status != UploadFileStatus.Ready) > 1)
            {
                _dbExecutor.ApplyChanges(c =>
                {
                    c.UploadFiles.Attach(uploadFile);
                    uploadFile.Status = UploadFileStatus.Ready;
                });
                return;
            }
            _dbExecutor.ApplyChanges(c =>
            {
                c.UploadFiles.Attach(uploadFile);
                uploadFile.Status = UploadFileStatus.Ready;
                var operationForUpdate = c.BulkOperations.Find(operation.Id);
                operationForUpdate.Status = BulkOperationStatus.Ready;
            });
            var cmd = new StartOperationCommand { OperationId = Guid.Parse(operation.Id) };
            _commandSender.Send(cmd);
        }

    }
}