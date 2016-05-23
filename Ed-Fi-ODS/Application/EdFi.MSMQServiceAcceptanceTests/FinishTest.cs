using System;
using EdFi.Common.Messaging;

namespace EdFi.MSMQServiceAcceptanceTests
{
    public class FinishTest : ICommand
    {
        public Guid TestRunNumber { get; set; }

        public DateTime TestRunFinalizedOn { get; set; }
    }
}