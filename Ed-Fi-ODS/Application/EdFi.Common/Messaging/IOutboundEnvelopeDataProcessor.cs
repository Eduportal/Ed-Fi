namespace EdFi.Common.Messaging
{
    public interface IOutboundEnvelopeDataProcessor
    {
        void ProcessEnvelope<TCommand>(IEnvelope<TCommand> envelopedMessage) where TCommand : ICommand;
    }
}