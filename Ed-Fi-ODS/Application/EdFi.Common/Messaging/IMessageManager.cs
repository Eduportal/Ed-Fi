namespace EdFi.Common.Messaging
{
    /// <summary>
    /// A wrapper envelope for an ICommand
    /// </summary>
    public interface IMessageManager<TCommand> where TCommand : ICommand
    {
        void Complete();
        string Id { get; }
        void DeadLetter(string reason, string description);
        IEnvelope<TCommand> Envelope { get; }
    }
}