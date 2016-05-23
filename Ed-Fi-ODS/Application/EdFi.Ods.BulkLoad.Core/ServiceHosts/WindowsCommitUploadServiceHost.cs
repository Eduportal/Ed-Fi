using EdFi.Common.Messaging;
using EdFi.Common.Services;
using EdFi.Ods.Messaging.BulkLoadCommands;
using log4net;

namespace EdFi.Ods.BulkLoad.Core.ServiceHosts
{
    public class WindowsCommitUploadServiceHost : IHostedService
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(WindowsCommitUploadServiceHost));
        private readonly ICommandListener<CommitUploadCommand> commitUploadListener;

        public WindowsCommitUploadServiceHost(ICommandListener<CommitUploadCommand> commitUploadListener)
        {
            this.commitUploadListener = commitUploadListener;
        }

        public void Start()
        {
            logger.Debug("Starting commitUploadListener");
            commitUploadListener.StartListening();
        }

        public void Stop()
        {
            logger.Debug("Stopping commitUploadListener");
            commitUploadListener.StopListening();
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