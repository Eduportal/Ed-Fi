using System;
using System.Linq;
using EdFi.Ods.BulkLoad.Core.Controllers;
using EdFi.Ods.BulkLoad.Core.Data;
using EdFi.Ods.Common.Utils;
using EdFi.Ods.Api.Data;
using EdFi.Ods.Api.Models.Resources.Enums;
using EdFi.Ods.Messaging.BulkLoadCommands;

namespace EdFi.Ods.BulkLoad.Console
{
    public class BulkLoadExecutor : IBulkExecutor
    {
        private readonly IControlBulkLoading _bulkLoadMaster;
        private readonly IFindBulkOperations _findBulkOperations;
        private readonly IFindBulkOperationExceptions _findBulkOperationExceptions;
        private readonly IOutput _output;

        public BulkLoadExecutor(IControlBulkLoading bulkLoadMaster, IFindBulkOperations findBulkOperations,
                                IFindBulkOperationExceptions findBulkOperationExceptions,
                                IOutput output)
        {
            _output = output;
            _bulkLoadMaster = bulkLoadMaster;
            _findBulkOperations = findBulkOperations;
            _findBulkOperationExceptions = findBulkOperationExceptions;
        }

        public bool Execute(string operationId)
        {
            _bulkLoadMaster.Handle(new StartOperationCommand {OperationId = new Guid(operationId)}).Wait();

            var operation = _findBulkOperations.FindWithFiles(operationId);

            if (operation.Status != BulkOperationStatus.Completed)
                WriteErrors(operation);

            return operation.Status == BulkOperationStatus.Completed;
        }

        private void WriteErrors(BulkOperation operation)
        {
            var bulkOperationExceptions =
                operation.UploadFiles.Where(x => x.Status != UploadFileStatus.Completed)
                         .Select(uploadFile => _findBulkOperationExceptions.FindByUploadFile(uploadFile.Id))
                         .SelectMany(exceptions => exceptions);
            foreach (var bulkOperationException in bulkOperationExceptions)
            {
                _output.WriteLine("Error: {0}", bulkOperationException.Message);
                _output.WriteLine("Element: {0}", bulkOperationException.Element);
                _output.WriteLine();
            }
        }
    }
}