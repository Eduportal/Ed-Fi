using System.Messaging;
using EdFi.Common.Messaging;

namespace EdFi.Messaging.MSMQ
{
    public interface IManagerFactory<TCommand> where TCommand : ICommand
    {
        IMessageManager<TCommand> Create(Message message, MessageQueueTransaction transaction);
    }
}