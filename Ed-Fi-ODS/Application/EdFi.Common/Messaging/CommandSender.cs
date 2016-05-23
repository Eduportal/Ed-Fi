namespace EdFi.Common.Messaging
{
    public class CommandSender : ICommandSender
    {
        private readonly IOutboundEnvelopeProcessingMgr _envelopeProcessingMgr;
        private readonly IBusSender _sender;

        public CommandSender(IOutboundEnvelopeProcessingMgr envelopeProcessingMgr, IBusSender sender)
        {
            _envelopeProcessingMgr = envelopeProcessingMgr;
            _sender = sender;
        }

        public void Send<TMessage>(TMessage message) where TMessage : ICommand
        {
            var envelope = new Envelope<TMessage>(message);
            _sender.Send(_envelopeProcessingMgr.Process(envelope));
        }
    }
}