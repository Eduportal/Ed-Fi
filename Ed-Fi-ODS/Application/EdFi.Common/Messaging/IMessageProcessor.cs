namespace EdFi.Common.Messaging
{
    /// <summary>
    /// Defines a method for processing messages.
    /// </summary>
    public interface IMessageProcessor
    {
        /// <summary>
        /// Processes a message received from a queue by finding the appropriate handler.
        /// </summary>
        /// <param name="message">The message that was received and processed.</param>
        bool Process(object message);
    }
}
