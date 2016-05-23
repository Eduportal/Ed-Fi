namespace EdFi.Common.Messaging
{
    public interface IOutboundEnvelopeProcessingMgr
    {
        IEnvelope<TCommand> Process<TCommand>(IEnvelope<TCommand> envelopedMessage) where TCommand : ICommand;
    }
}