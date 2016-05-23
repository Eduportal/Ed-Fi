using System;
using EdFi.Common.Messaging;

namespace EdFi.Ods.Messaging.BulkLoadCommands
{
    public class CommitUploadCommand : ICommand
    {
        public string UploadId { get; set; }
        public DateTime CommitedOn { get; set; }
    }
}