using log4net;
using System;
using System.Diagnostics;

namespace EdFi.Common.Messaging
{
    public class CommandListener<TCommand> : ICommandListener<TCommand> where TCommand : ICommand
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(CommandListener<TCommand>));
        private readonly IQueueListener<TCommand> listener;
        private readonly IMessageProcessor messageProcessor;
        private readonly IInboundEnvelopeProcessingMgr _envelopeProcessingMgr;

        public CommandListener(IQueueListener<TCommand> listener, IMessageProcessor messageProcessor, IInboundEnvelopeProcessingMgr _envelopeProcessingMgr)
        {
            this.listener = listener;
            this.messageProcessor = messageProcessor;
            this._envelopeProcessingMgr = _envelopeProcessingMgr;
        }

        public Exception LastExceptionStoreForTesting { get; private set; }

        public void StartListening()
        {
            logger.Debug("Starting queueListener");
            listener.StartListening(Handle);
        }

        public void StopListening()
        {
            logger.Debug("Stopping queueListener");
            listener.StopListening();
        }

        private void Handle(IMessageManager<TCommand> _messageManager)
        {
            logger.Debug("Handle Message");
            try
            {
                TraceMessage(_messageManager);

                var envelope = _messageManager.Envelope;
                _envelopeProcessingMgr.Process(envelope);
                var command = envelope.Message;
                messageProcessor.Process(command);
                _messageManager.Complete();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Trace.TraceWarning("CommandListener has exception at {0}", DateTime.Now);
                LastExceptionStoreForTesting = ex;

                // Handle any message processing specific exceptions here
                // TODO: Log exception

                // Don't reprocess the message
                try
                {
                    Trace.TraceError("{0} has seen this exception: {1}", GetType().Name, ex);
                    Trace.TraceWarning("Moving message to dead letter queue. '{0}'", ex.Message);
                    _messageManager.DeadLetter("An exception occurred during processing.", ex.ToString());
                }
                catch (Exception ex2)
                {
                    logger.Error(ex);
                    Trace.WriteLine("Exception occurred while trying to move message to the dead letter queue: " + ex2);
                }

                // TODO: Add some sort of support for handling errors
            }
        }

        private static void TraceMessage(IMessageManager<TCommand> brokeredMessage)
        {
            var message = string.Format("CommandListener<{0}> handling message '{1}'", typeof (TCommand).Name,
                                        brokeredMessage.Id);
            Trace.WriteLine(message);
            logger.Debug(message);
        }
    }
}