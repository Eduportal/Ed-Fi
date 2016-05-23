using System;
using EdFi.Common.Messaging;
using EdFi.MSMQServiceAcceptanceTests;

namespace EdFi.MSMQServiceTests.ListenAndFinish
{
    public class FinishTestHandler : IMessageHandler<FinishTest>
    {

        public void Handle(FinishTest message)
        {
            //Ought to log to db

            Console.WriteLine(string.Format(@"Received finish message for test run ({0}).  Test officially stopped at {1}.", message.TestRunNumber, message.TestRunFinalizedOn));
        }
    }
}