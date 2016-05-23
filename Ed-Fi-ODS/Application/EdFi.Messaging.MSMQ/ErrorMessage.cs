using System;
using EdFi.Common.Messaging;

namespace EdFi.Messaging.MSMQ
{
    public class ErrorMessage : ICommand
    {
        public Object Envelope { get; set; }
        public ErrorDetails ErrorDetails { get; set; }

        public ErrorMessage(Object envelope, ErrorDetails errorDetails)
        {
            Envelope = envelope;
            ErrorDetails = errorDetails;
        }

        public string OriginalMessageType { get; set; }
    }
}