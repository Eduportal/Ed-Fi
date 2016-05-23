namespace EdFi.Common.Messaging
{
    public class OutboundEnvelopeProcessingMgr : IOutboundEnvelopeProcessingMgr
    {
        private readonly IOutboundEnvelopeDataProcessor[] _dataProcessors;

        public OutboundEnvelopeProcessingMgr(IOutboundEnvelopeDataProcessor[] dataProcessors)
        {
            _dataProcessors = dataProcessors;
        }

        public IEnvelope<TCommand> Process<TCommand>(IEnvelope<TCommand> envelopedMessage) where TCommand : ICommand
        {
            foreach (var outboundEnvelopeVisitor in _dataProcessors)
            {
                outboundEnvelopeVisitor.ProcessEnvelope(envelopedMessage);
            }
            return envelopedMessage;
        }
    }
}