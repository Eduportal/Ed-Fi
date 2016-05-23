using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using EdFi.Common.InversionOfControl;

namespace EdFi.Common.Messaging
{
    /// <summary>
    /// Processes a message by locating the appropriate handlers for the message and invoking them.
    /// </summary>
    public class MessageProcessor : IMessageProcessor
    {
        private readonly IServiceLocator _locator;

        /// <summary>
        /// Creates and initializes a new instance of the <see cref="MessageProcessor"/> class.
        /// </summary>
        /// <param name="locator">The service locator, used to resolve handlers from the IoC container based on incoming message types.</param>
        public MessageProcessor(IServiceLocator locator)
        {
            _locator = locator;
        }

        /// <summary>
        /// Processes a message by finding and invoking the appropriate handlers.
        /// </summary>
        /// <param name="message">The message that was received.</param>
        /// <returns><b>true</b> if at least one message handler was found; otherwise <b>false</b>.</returns>
        public bool Process(object message)
        {
            var handlerDetails = GetHandlerDetails(message);

            var handlers = _locator.ResolveAll(handlerDetails.HandlerType);

            if (handlers.Length == 0) // Castle appears to return an empty array if nothing is configured.
                return false;

            var exceptions = new List<Exception>(); 

            foreach (var messageHandler in handlers)
            {
                try
                {
                    handlerDetails.HandlerMethod.Invoke(messageHandler, new[] { message });
                }
                catch (Exception ex)
                {
                    Trace.TraceError("{0} got exception: {1}", GetType().FullName, ex); 
                    exceptions.Add(ex);
                }
            }

            // TODO: Modify behavior to return true if ANY handlers succeeded, and false if ALL failed

            // Throw a new aggregate exception with all the failed handlers
            if (exceptions.Count > 0)
                throw new AggregateException(exceptions);

            return true;
        }

        private readonly ConcurrentDictionary<Type, HandlerDetails> _handlerDetailsByMessageType
            = new ConcurrentDictionary<Type, HandlerDetails>();

        private HandlerDetails GetHandlerDetails(object message)
        {
            Type messageType = message.GetType();

            var details = _handlerDetailsByMessageType.GetOrAdd(messageType, mt =>
                {
                    var handlerType = typeof(IMessageHandler<>).MakeGenericType(messageType);
                    var handlerMethod = handlerType.GetMethod("Handle");

                    return new HandlerDetails {HandlerType = handlerType, HandlerMethod = handlerMethod};
                });

            return details;
        }

        /// <summary>
        /// Provides details necessary for invoking the handler.
        /// </summary>
        private class HandlerDetails
        {
            /// <summary>
            /// Gets or sets the closed generic <see cref="Type"/> for the handler.
            /// </summary>
            public Type HandlerType { get; set; }

            /// <summary>
            /// Gets or sets the <see cref="MethodInfo"/> for the method to be invoked to handle the message.
            /// </summary>
            public MethodInfo HandlerMethod { get; set; }
        }
    }
}