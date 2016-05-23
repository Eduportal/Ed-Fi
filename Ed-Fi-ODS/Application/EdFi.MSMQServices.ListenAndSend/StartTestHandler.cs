using System;
using EdFi.Common.Messaging;
using EdFi.MSMQServiceAcceptanceTests;

namespace EdFi.MSMQServices.ListenAndSend
{
    public class StartTestHandler : IMessageHandler<StartTest>
    {
        private readonly ICommandSender _sender;

        public StartTestHandler(ICommandSender sender)
        {
            _sender = sender;
        }

        public void Handle(StartTest message)
        {
            //Really ought to log this to db . . .
            Console.WriteLine(string.Format(@"Received Start Test message for run ({0}) starting on {1}.", message.TestRunNumber, message.TestBeginTime));

            var finishMessage = new FinishTest
            {
                TestRunNumber = message.TestRunNumber,
                TestRunFinalizedOn = DateTime.UtcNow
            };

            Console.WriteLine("Sending Finish message . . .");
            _sender.Send(finishMessage);
            Console.WriteLine("Finish message sent.  Waiting for next test run . . . ");
        }
    }
}