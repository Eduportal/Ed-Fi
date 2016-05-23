using System.Threading;
using EdFi.Common.Messaging;
using EdFi.Common.Services;
using EdFi.Ods.Messaging.BulkLoadCommands;

namespace EdFi.Workers.BulkLoad
{
    public class BulkLoadServiceHost : IHostedService
    {
        private readonly ICommandListener<StartOperationCommand> startOperationCommandListener;
        private ManualResetEvent StopEvent = new ManualResetEvent(false);

        public BulkLoadServiceHost(ICommandListener<StartOperationCommand> startOperationCommandListener)
        {
            this.startOperationCommandListener = startOperationCommandListener;
        }

        public void Start()
        {
            startOperationCommandListener.StartListening();
            StopEvent.WaitOne();
        }

        public void Stop()
        {
            startOperationCommandListener.StopListening();
            StopEvent.Set();
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