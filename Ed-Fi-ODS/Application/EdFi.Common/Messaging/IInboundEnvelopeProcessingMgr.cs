namespace EdFi.Common.Messaging
{
    public interface IInboundEnvelopeProcessingMgr
    {
        IEnvelope<TCommand> Process<TCommand>(IEnvelope<TCommand> envelopedMessage) where TCommand : ICommand;
    }
}