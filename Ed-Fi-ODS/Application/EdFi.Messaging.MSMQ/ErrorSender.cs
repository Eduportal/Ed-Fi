using System;
using System.Messaging;
using EdFi.Common.Messaging;
using log4net;
using Newtonsoft.Json;

namespace EdFi.Messaging.MSMQ
{
    public class ErrorSender<TCommand> : IErrorSender<TCommand> where TCommand : ICommand
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(ErrorSender<TCommand>));

        public void Send(MessageQueueTransaction transaction, IEnvelope<TCommand> envelope, ErrorDetails errorDetails)
        {
            const string errorQueuePath = @".\PRIVATE$\Errors";
            var errorMsg = new ErrorMessage(envelope, errorDetails) {OriginalMessageType = typeof (TCommand).FullName};
            var message = new Message(JsonConvert.SerializeObject(errorMsg));

            if (!MessageQueue.Exists(errorQueuePath)) MessageQueue.Create(errorQueuePath, true);
            using (var queue = new MessageQueue(errorQueuePath))
            {
                queue.Formatter = new JSonMessageFormatter();
                try
                {
                    queue.Send(message, transaction);
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    _logger.Error(String.Format("An exception was thrown while attempting to send a {0} message to the error queue: {1}", envelope.Message.GetType().Name, ex.Message));
                    transaction.Abort();
                }
            }
        }
    }
}