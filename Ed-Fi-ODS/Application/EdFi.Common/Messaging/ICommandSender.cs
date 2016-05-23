namespace EdFi.Common.Messaging
{
    /// <summary>
    /// Defines methods for sending messages.
    /// </summary>
    public interface ICommandSender
    {
        /// <summary>
        /// Sends a message to a specific queue for processing.
        /// </summary>
        /// <typeparam name="TMessage">The <see cref="Type"/> of the message to be sent (must implement the <see cref="IHasProperties"/> interface).</typeparam>
        /// <param name="message">The message to be sent to the queue.</param>
        void Send<TMessage>(TMessage message)
            where TMessage : ICommand;
    }
}
