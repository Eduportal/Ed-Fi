using EdFi.Common.Messaging;
using EdFi.Common.Services;
using EdFi.Ods.Messaging.BulkLoadCommands;

namespace EdFi.Ods.BulkLoad.Core.ServiceHosts
{
    public class WindowsBulkWorkerServiceHost : IHostedService
    {
        private readonly ICommandListener<StartOperationCommand> startOperationCommandListener;

        public WindowsBulkWorkerServiceHost(ICommandListener<StartOperationCommand> startOperationCommandListener)
        {
            this.startOperationCommandListener = startOperationCommandListener;
        }

        public void Start()
        {
            startOperationCommandListener.StartListening();
        }

        public void Stop()
        {
            startOperationCommandListener.StopListening();
        }

        public string Description
        {
            get { return "Processes all files in a given Operation into the appropriate ODS and records any exceptions."; }
        }


        public string DisplayName
        {
            get { return "BulkLoadWorker"; }
        }

        public string ServiceName { get { return "EdFi.Ods.BulkLoadService"; } }
    }
}