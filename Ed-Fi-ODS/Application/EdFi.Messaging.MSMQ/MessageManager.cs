using System.Messaging;
using EdFi.Common.Messaging;

namespace EdFi.Messaging.MSMQ
{
    public class MessageManager<TCommand> : IMessageManager<TCommand> where TCommand : ICommand
    {
        private readonly MessageQueueTransaction _transaction;
        private readonly IErrorSender<TCommand> _errorSender;

        public MessageManager(Message message, MessageQueueTransaction transaction, IErrorSender<TCommand> errorSender, IMsmqMessageFactory messageFactory)
        {
            _transaction = transaction;
            _errorSender = errorSender;
            Id = message.Id;
            Envelope = messageFactory.GetEnvelope<TCommand>(message);
        }

        public void Complete()
        {
            _transaction.Commit();
        }

        public string Id { get; private set; }

        public void DeadLetter(string reason, string description)
        {
            _errorSender.Send(_transaction, Envelope, new ErrorDetails { Reason = reason, Description = description});
        }

        public IEnvelope<TCommand> Envelope { get; private set; }
    }
}