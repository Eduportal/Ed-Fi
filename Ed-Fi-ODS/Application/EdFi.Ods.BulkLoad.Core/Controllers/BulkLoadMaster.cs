using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using EdFi.Common.Messaging;
using EdFi.Ods.BulkLoad.Core.Data;
using EdFi.Ods.Common;
using EdFi.Ods.Api.Data;
using EdFi.Ods.Api.Models.Resources.Enums;
using EdFi.Ods.Messaging.BulkLoadCommands;

namespace EdFi.Ods.BulkLoad.Core.Controllers
{
    public class BulkLoadMaster : IControlBulkLoading, IMessageHandler<StartOperationCommand>
    {
        private readonly IDictionary<InterchangeType, IInterchangeController> _sequencedControllerscontrollers;
        private readonly IValidateAndSourceFiles _fileValidateAndSourceMaster;
        private readonly IFindBulkOperations _findBulkOperations;
        private readonly ISetBulkOperationStatus _setBulkOperationStatus;
        private readonly IPersistBulkOperationExceptions _persistBulkOperationExceptions;
        private readonly ISetUploadFileStatus _setUploadFileStatus;
        private readonly IDeleteUploadFileChunks _deleteUploadFileChunks;

        public BulkLoadMaster(
            IDictionary<InterchangeType, IInterchangeController> sequencedControllerscontrollers,
            IValidateAndSourceFiles fileValidateAndSourceMaster,
            IFindBulkOperations findBulkOperations,
            ISetBulkOperationStatus setBulkOperationStatus, 
            IPersistBulkOperationExceptions persistBulkOperationExceptions,
            ISetUploadFileStatus setUploadFileStatus,
            IDeleteUploadFileChunks deleteUploadFileChunks)
        {
            _sequencedControllerscontrollers = sequencedControllerscontrollers;
            _fileValidateAndSourceMaster = fileValidateAndSourceMaster;
            _findBulkOperations = findBulkOperations;
            _setBulkOperationStatus = setBulkOperationStatus;
            _persistBulkOperationExceptions = persistBulkOperationExceptions;
            _setUploadFileStatus = setUploadFileStatus;
            _deleteUploadFileChunks = deleteUploadFileChunks;
        }

        // TODO: GKM - Replaced with Castle logger boilerplate above. Remove this section once logging approach is finalized.
        //protected ILog Logger
        //{
        //    get { return LogManager.GetLogger(GetType()); }
        //}

        void IMessageHandler<StartOperationCommand>.Handle(StartOperationCommand command)
        {
            Handle(command).Wait();
        }

        public async Task Handle(StartOperationCommand command)
        {
            var operationId = command.OperationId.ToString();
            Trace.TraceInformation("{0} handling operation {1}", GetType().FullName, operationId);

            var operation = _findBulkOperations.FindAndStart(operationId);
  
            foreach (var pair in _sequencedControllerscontrollers)
            {
                var interchangeType = pair.Key;
                var controller = pair.Value;
                var files = operation.UploadFiles.Where(f => interchangeType.Equals(InterchangeType.GetByName(f.InterchangeType))).ToArray();
                foreach (var uploadFile in files)
                {
                    var uploadFileId = uploadFile.Id;
                    using(var workingFile = _fileValidateAndSourceMaster.ValidateMakeLocalAndFindPath(operation.Id, uploadFileId))
                    {
                        _deleteUploadFileChunks.DeleteByUploadFileId(uploadFileId);
                        if (workingFile.IsFailure)
                        {
                            _setUploadFileStatus.SetStatus(uploadFileId, UploadFileStatus.Error);
                            _persistBulkOperationExceptions.HandleFileValidationExceptions(uploadFileId, 400, workingFile.ValidationErrorMessages);
                            continue;
                        }

                        _setUploadFileStatus.SetStatus(uploadFileId, UploadFileStatus.Started);

                        var result = await controller.LoadAsync(workingFile.FilePathIfValid);
                        SetStatusAndHandleExceptions(uploadFile, result);
                    }
                }

            }
            var reloadedOperation = _findBulkOperations.FindWithFiles(operationId);
            var uploadFiles = reloadedOperation.UploadFiles.ToArray();
            var endingStatus = uploadFiles.All(u => u.Status == UploadFileStatus.Completed) ? BulkOperationStatus.Completed : BulkOperationStatus.Error;
            _setBulkOperationStatus.SetStatus(operationId, endingStatus);
        }

        private UploadFileStatus GetUploadFileStatus(LoadResult result)
        {
            var loadedWithoutExceptions = !result.LoadExceptions.Any();
            var someResourcesWereLoaded = result.LoadedResourceCount > 0;
            var fileWasSuccessful = loadedWithoutExceptions && someResourcesWereLoaded;
            return fileWasSuccessful ? UploadFileStatus.Completed : UploadFileStatus.Error;
        }

        private void SetStatusAndHandleExceptions(UploadFile uploadFile, LoadResult result)
        {
            var loadExceptions = result.LoadExceptions;
            var uploadFileStatus = GetUploadFileStatus(result);
            _setUploadFileStatus.SetStatus(uploadFile.Id, uploadFileStatus);

            if (result.SourceElementCount == 0)
                loadExceptions.Add(LoadException.ForEmptyInterchange(uploadFile.InterchangeType, uploadFile.Id));


            Trace.TraceInformation("Upload file '{0}' has '{1}' exceptions", uploadFile.Id, loadExceptions.Count);
            if (loadExceptions == null) return;
            if (!loadExceptions.Any() && loadExceptions.All(x => x.Exception == null)) return;

            var loadExceptionsWithException = loadExceptions.Where(x => x.Exception != null).ToArray();
            _persistBulkOperationExceptions.HandleFileLoadingExceptions(uploadFile.Id, loadExceptionsWithException);
        }
    }
}