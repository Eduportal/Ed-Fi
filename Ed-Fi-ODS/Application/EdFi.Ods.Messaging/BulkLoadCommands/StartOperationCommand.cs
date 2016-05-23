using System;
using EdFi.Common.Messaging;

namespace EdFi.Ods.Messaging.BulkLoadCommands
{
    public class StartOperationCommand : ICommand
    {
        public Guid OperationId { get; set; }
    }
}