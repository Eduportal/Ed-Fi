using System;
using EdFi.Common.Messaging;
using EdFi.Common.Services;
using EdFi.MSMQServiceAcceptanceTests;

namespace EdFi.MSMQServiceTests.ListenAndFinish
{
    public class ListenAndFinishHostedService : IHostedService
    {
        private readonly ICommandListener<FinishTest> _commandListener;

        public ListenAndFinishHostedService(ICommandListener<FinishTest> commandListener)
        {
            _commandListener = commandListener;
            Description = "Listens for the FinishTest Messages and Finalizes Test data.";
            DisplayName = "ListenAndFinish Test Service";
            ServiceName = "ListenAndFinishService";
        }

        public void Start()
        {
            Console.WriteLine("Telling command listener to start . . . ");
            _commandListener.StartListening();
            Console.WriteLine(". . . command listener should be listening now.");
        }

        public void Stop()
        {
            Console.WriteLine("Telling command listener to stop . . . ");
            _commandListener.StopListening();
            Console.WriteLine(". . . command listener should be stopped.");
        }

        public string Description { get; private set; }
        public string DisplayName { get; private set; }
        public string ServiceName { get; private set; }
    }
}