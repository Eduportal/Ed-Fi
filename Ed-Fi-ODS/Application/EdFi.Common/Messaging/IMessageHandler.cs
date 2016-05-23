using System;

namespace EdFi.Common.Messaging
{
    /// <summary>
    /// Defines methods for handling a message.
    /// </summary>
    /// <typeparam name="TMessage">The <see cref="Type"/> of the message to be handled.</typeparam>
    public interface IMessageHandler<in TMessage>
    {
        /// <summary>
        /// Handles a message.
        /// </summary>
        /// <param name="message">The message to be handled.</param>
        void Handle(TMessage message);
    }
}