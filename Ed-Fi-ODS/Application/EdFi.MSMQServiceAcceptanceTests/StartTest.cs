using System;
using EdFi.Common.Messaging;

namespace EdFi.MSMQServiceAcceptanceTests
{
    public class StartTest : ICommand
    {
        public Guid TestRunNumber { get; set; }
        public DateTime TestBeginTime { get; set; }
    }
}