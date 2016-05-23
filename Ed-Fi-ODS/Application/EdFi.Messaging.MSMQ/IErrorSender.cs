using System.Messaging;
using EdFi.Common.Messaging;

namespace EdFi.Messaging.MSMQ
{
    public interface IErrorSender<TCommand> where TCommand : ICommand
    {
        void Send(MessageQueueTransaction transaction, IEnvelope<TCommand> envelope, ErrorDetails errorDetails);
    }
}