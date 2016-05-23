EdFi.Messaging.Windows
==

The windows implementation of EdFi.Common.Messaging uses MSMQ to deliver the expected messaging behaviors.  This document contains details about that implementation and is intended for an audience familiar with the EdFi.Common.Messaging source code.  It will assume knowledge of C#.

##EdFi.Common.Messaging.IBusSender Implementation##

`EdFi.Common.Messaging.IBusSender` is used by the `EdFi.Common.Messaging.CommandSender` to send Command messages.  `EdFi.Messaging.MSMQ.BusSender` implements this interface and is responsible for sending messages via MSMQ.  It depends on `EdFi.Messaging.MSMQ.IQueueLocator` to get access to the queue, `EdFi.Messaging.MSMQ.IMsmqMessageFactory` to put the message into an MSMQ message object, and `Castle.Core.ILogger` to log errors.  

##EdFi.Messaging.MSMQ.IQueueLocator Implementation##

`EdFi.Messaging.MSMQ.QueueLocator` verifies the existence of queues based on the message type.  It has a `GetQueue(IEnvelope<ICommand>)` method and a `GetQueue(ICommand)` method.  Both return an `System.Messaging.MessageQueue` object.  It uses the `EdFi.Common.Messaging.IQueueNameProvider` to determine the name of the queue based on the passed in ICommand.  It uses the `EdFi.Common.Messaging.IConfigValueProvider` to read the configuration and find the endpoint for the given Command.  It will set the queue to use the `EdFi.Messaging.MSMQ.JSonMessageFormatter` to format the messages.

If the queue doesn't exist it will also check for the QueueAutoCreate setting to determine if it should try to create the queue.  If QueueAutoCreate is set to "1" and the endpoint is not local it will throw a SystemException.  The creation of remote queues is not supported at this time.  If the endpoint is the local environment it will create the queue and make it transactional.

##EdFi.Messaging.MSMQ.IMsmqMessageFactory Implementation##

`EdFi.Messaging.MSMQ.MsmqMessageFacory` can create `System.Messaging.Message` objects out of an `EdFi.Common.Messaging.IEnvelope<ICommand>`.  It will make certain the messages are recoverable, are able to be placed in the DeadLetterQueue, and use the `EdFi.Messaging.MSMQ.JSonMessageFormatter` to format the body.  It can also return an `EdFi.Messaging.MSMQ.IEnvelope<ICommand>` from an MSMQ message object.

##EdFi.Messaging.MSMQ.JSonMessageFormatter##

MSMQ queues must serialize the `System.Messaging.Message` objects using a `System.Messaging.IMessageFormatter` as part of writing messages into the queue.  `EdFi.Messaging.MSMQ.JSonMessageFormatter` implements this interface and allows the use of the more modern C# data constructs than would not be allowable using the default MSMQ formatters.  For example, this allows for deeper object graphs and generics.

##EdFi.Common.Messaging.IQueueListener Implementation##

`EdFi.Messaging.MSMQ.QueueListener` implements the `IQueueListener` interface used by the common `CommandListener` to start and stop listening to a queue.  It depends on an `IQueueLocator` to get access to a queue and a `EdFi.Messaging.MSMQ.IManagerFactory` to gain an instance of the `EdFi.Common.Messaging.IMessageManager` based on the top most (oldest) message in the queue.  The `IMessageManager` is what must be passed to the callback `Action<IMessageManager<ICommand>>` supplied by the `CommandListener` in the Common Messaging library.  The callback will ultimately result in the message being passed to a `IMessageHandler.Handle(ICommand)` method.  The listener uses an asynchronous peek with a five second timeout to wait for incoming messages.  This allows the listener to be stopped within five seconds of the StopListening() method being called.

The listener uses a transactional receive to ensure the message is returned to the queue if it is not handled without exceptions.  This ensures the 'at least once' delivery of messages.  This is the reason MSMQ queues must be created as transactional queues and messages must be sent with a transaction.

##EdFi.Common.Messaging.IMessageManager Implementation##

`EdFi.Messaging.MSMQ.MessageManager` implements `IMessageManager` and uses a `System.Messaging.Message` along with a `System.Messaging.MessageQueueTransaction` and an `EdFi.Messaging.MSMQ.IErrorSender<ICommand>`.  If the `Complete()` method is called for a successfully handled message the manager will commit the transaction.  If the `DeadLetter(string reason, string description)` method is called for a failed message it will use the `IErrorSender<ICommand>` to send the message to the error queue.

##EdFi.Messaging.MSMQ.IErrorSender<ICommand> Implementation##

`EdFi.Messaging.MSMQ.ErrorSender` is responsible for putting failed messages into the error queue.  If it cannot move the message into the error queue it will abort the transaction and return the message to the original queue.

##EdFi.Messaging.MSMQ._Installers.MSMQInstaller##

The `EdFi.Messaging.MSMQ._Installers.MSMQInstaller` implements `IWindsorInstaller` and registers all of the MSMQ implementations of the Common Messaging interfaces.  It will not register the `Castle.Core.ILogger` implementation - that is deferred to the solution using the messaging infrastructure.  `ILogger` is used by several of the MSMQ EdFi Messaging classes to log exception data and make troubleshooting issues with the messages more palatable.