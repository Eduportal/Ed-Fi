using System;
using EdFi.Common.Configuration;

namespace EdFi.Common.Messaging
{
    /// <summary>
    /// Provides an <see cref="IQueueNameProvider"/> implementation that derives the queue name from the message's Type name.
    /// </summary>
    public class ConventionBasedQueueNameProvider : IQueueNameProvider
    {
        private readonly IConfigValueProvider _config;

        public ConventionBasedQueueNameProvider(IConfigValueProvider config)
        {
            _config = config;
        }

        /// <summary>
        /// Returns the queue name for the message based on the message's Type name.
        /// </summary>
        /// <typeparam name="TMessage">The <see cref="Type"/> of the message.</typeparam>
        /// <returns>The queue name.</returns>
        public string GetQueueNameFor<TMessage>() 
            where TMessage : ICommand
        {
            return GetQueueNameFor(typeof(TMessage));
        }

        /// <summary>
        /// Returns the queue name for the message based on the message's Type name.
        /// </summary>
        /// <typeparam name="TMessage">The <see cref="Type"/> of the message.</typeparam>
        /// <returns>The queue name.</returns>
        public string GetQueueNameFor(Type t)
        {
            var iocConfig = _config.GetValue("QueuePrefix");
            if (!typeof(ICommand).IsAssignableFrom(t))
                throw new ArgumentException(string.Format("Type must implement {0}", typeof(ICommand).Name));
            return iocConfig + t.Name;
        }
    }
}