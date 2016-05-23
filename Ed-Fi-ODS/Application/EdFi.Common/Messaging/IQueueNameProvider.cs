using System;

namespace EdFi.Common.Messaging
{
    /// <summary>
    ///     Defines a method for obtaining the queue name for a specified command message type.
    /// </summary>
    public interface IQueueNameProvider
    {
        /// <summary>
        ///     Gets the queue name for the specified message type.
        /// </summary>
        /// <typeparam name="TMessage">
        ///     The <see cref="Type" /> of the command message.
        /// </typeparam>
        /// <returns>The name of the queue to which the command message should be sent.</returns>
        string GetQueueNameFor<TMessage>()
            where TMessage : ICommand;

        /// <summary>
        ///     Gets the queue name for the specified message type.
        /// </summary>
        /// <param name="t">
        ///     The <see cref="System.Type" /> of the command message.
        /// </param>
        /// <exception cref="System.ArgumentException">
        ///     If t is not an implementation of <see cref="ICommand" />
        /// </exception>
        /// <returns>The name of the queue to which the command message should be sent.</returns>
        string GetQueueNameFor(Type t);
    }
}