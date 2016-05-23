namespace EdFi.Common.Messaging
{
    /// <summary>
    /// Defines methods for starting and stopping the monitoring of messages for a specific queue.
    /// </summary>
    public interface ICommandListener<T>
    {
        /// <summary>
        /// Starts receiving and processing messages from the specified queue.
        /// </summary>
        void StartListening();

        /// <summary>
        /// Stops receiving and processing messages from the specified queue.
        /// </summary>
        void StopListening();
    }
}