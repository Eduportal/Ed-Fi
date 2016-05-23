using System.Threading;
using EdFi.Common.Messaging;
using EdFi.Common.Services;
using EdFi.Ods.Messaging.BulkLoadCommands;

namespace EdFi.Workers.UploadCommit
{
    public class CommitUploadServiceHost : IHostedService
    {
        private readonly ICommandListener<CommitUploadCommand> commitUploadListener;
        private ManualResetEvent StopEvent = new ManualResetEvent(false);

        public CommitUploadServiceHost(ICommandListener<CommitUploadCommand> commitUploadListener)
        {
            this.commitUploadListener = commitUploadListener;
        }

        public void Start()
        {
            commitUploadListener.StartListening();
            StopEvent.WaitOne();
        }

        public void Stop()
        {
            commitUploadListener.StopListening();
            StopEvent.Set();
        }

        public string Description
        {
            get { return "Validate the completeness of the uploaded files and determine when to start the associated bulk operation."; }
        }


        public string DisplayName
        {
            get { return "CommitUploadWorker"; }
        }

        public string ServiceName { get { return "EdFi.Ods.CommitUploadService"; } }
    }
}