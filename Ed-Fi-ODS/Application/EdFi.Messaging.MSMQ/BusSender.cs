using System;
using System.Messaging;
using EdFi.Common.Messaging;
using log4net;

namespace EdFi.Messaging.MSMQ
{
    public class BusSender : IBusSender
    {
        private readonly IQueueLocator _locator;
        private readonly IMsmqMessageFactory _messageFactory;
        private readonly ILog _logger = LogManager.GetLogger(typeof(BusSender));

        public BusSender(IQueueLocator locator, IMsmqMessageFactory messageFactory)
        {
            _locator = locator;
            _messageFactory = messageFactory;
        }

        public void Send<TMessage>(IEnvelope<TMessage> envelope) where TMessage : ICommand
        {
            using (var queue = _locator.GetQueue(envelope))
            using (var message = _messageFactory.Create(envelope))
            using (var transaction = new MessageQueueTransaction())
            {
                try
                {
                    transaction.Begin();
                    queue.Send(message, transaction);
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Abort();
                    _logger.Error(string.Format(@"The following exception was thrown while attemtpting to send a(n) {0} message to {1} queue: {2}",
                        envelope.Message.GetType().Name, queue.FormatName, ex.Message));
                    throw;
                }
            }
        }
    }
}