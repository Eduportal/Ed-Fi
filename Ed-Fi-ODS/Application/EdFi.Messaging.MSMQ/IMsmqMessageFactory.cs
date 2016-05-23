using System.Messaging;
using EdFi.Common.Messaging;

namespace EdFi.Messaging.MSMQ
{
    public interface IMsmqMessageFactory
    {
        Message Create<TCommand>(IEnvelope<TCommand> envelope) where TCommand : ICommand;

        IEnvelope<TCommand> GetEnvelope<TCommand>(Message message) where TCommand : ICommand;
    }
}