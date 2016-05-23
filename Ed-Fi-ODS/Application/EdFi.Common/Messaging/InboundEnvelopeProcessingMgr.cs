namespace EdFi.Common.Messaging
{
    public class InboundEnvelopeProcessingMgr : IInboundEnvelopeProcessingMgr
    {
        private readonly IInboundEnvelopeDataProcessor[] _dataProcessors;

        public InboundEnvelopeProcessingMgr(IInboundEnvelopeDataProcessor[] dataProcessors)
        {
            _dataProcessors = dataProcessors;
        }

        public IEnvelope<TCommand> Process<TCommand>(IEnvelope<TCommand> envelopedMessage) where TCommand : ICommand
        {
            foreach (var inboundEnvelopeVisitor in _dataProcessors)
            {
                inboundEnvelopeVisitor.ProcessEnvelope(envelopedMessage);
            }
            return envelopedMessage;
        }
    }
}