using System;
using EdFi.Common.Messaging;
using EdFi.Common.Services;
using EdFi.MSMQServiceAcceptanceTests;

namespace EdFi.MSMQServices.ListenAndSend
{
    public class ListenAndSendHostedService : IHostedService
    {
        private readonly ICommandListener<StartTest> _listener;

        public ListenAndSendHostedService(ICommandListener<StartTest> listener)
        {
            _listener = listener;
            Description = "Test Host for acceptance testing Service Hosting with MSMQ";
            DisplayName = "ListenAndSendService";
            ServiceName = "ListenAndSend";
        }

        public void Start()
        {
            Console.WriteLine("Telling the command listener to start listening . . .");
            _listener.StartListening();
            Console.WriteLine(". . . command listener should be listening for StartTest messages.");
        }

        public void Stop()
        {
            Console.WriteLine("Telling the command listener to stop. . .");
            _listener.StartListening();
            Console.WriteLine(". . . command listener should be stopped.");
        }

        public string Description { get; private set; }
        public string DisplayName { get; private set; }
        public string ServiceName { get; private set; }
    }
}