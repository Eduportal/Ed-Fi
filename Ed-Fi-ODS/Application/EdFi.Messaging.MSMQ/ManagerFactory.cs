using System.Messaging;
using EdFi.Common.Messaging;

namespace EdFi.Messaging.MSMQ
{
    public class ManagerFactory<TCommand> : IManagerFactory<TCommand> where TCommand : ICommand
    {
        private readonly IErrorSender<TCommand> _errorSender;
        private readonly IMsmqMessageFactory _messageFactory;

        public ManagerFactory(IErrorSender<TCommand> errorSender, IMsmqMessageFactory messageFactory)
        {
            _errorSender = errorSender;
            _messageFactory = messageFactory;
        }

        public IMessageManager<TCommand> Create(Message message, MessageQueueTransaction transaction)
        {
            return new MessageManager<TCommand>(message, transaction, _errorSender, _messageFactory);
        }
    }
}