namespace EdFi.Common.Messaging
{
    public interface IInboundEnvelopeDataProcessor
    {
        void ProcessEnvelope<TCommand>(IEnvelope<TCommand> envelopedMessage) where TCommand : ICommand;
    }
}