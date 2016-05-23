namespace EdFi.Common.Messaging
{
    public interface IBusSender
    {
        void Send<TMessage>(IEnvelope<TMessage> envelope) where TMessage : ICommand;
    }
}