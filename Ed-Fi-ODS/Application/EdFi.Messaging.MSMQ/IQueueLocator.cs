using System.Messaging;
using EdFi.Common.Messaging;

namespace EdFi.Messaging.MSMQ
{
    public interface IQueueLocator
    {
        MessageQueue GetQueue<TCommand>(IEnvelope<TCommand> envelope) where TCommand : ICommand;
        MessageQueue GetQueue<TCommand>() where TCommand : ICommand;
    }
}