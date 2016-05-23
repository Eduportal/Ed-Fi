using System.Messaging;
using EdFi.Common.Messaging;

namespace EdFi.Messaging.MSMQ
{
    public class MsmqMessageFactory : IMsmqMessageFactory
    {
        public Message Create<TCommand>(IEnvelope<TCommand> envelope) where TCommand : ICommand
        {
            return new Message(envelope){UseDeadLetterQueue = true, Formatter = new JSonMessageFormatter(), Recoverable = true};
        }

        public IEnvelope<TCommand> GetEnvelope<TCommand>(Message message) where TCommand : ICommand
        {
            return (IEnvelope<TCommand>) message.Body;
        } 
    }
}