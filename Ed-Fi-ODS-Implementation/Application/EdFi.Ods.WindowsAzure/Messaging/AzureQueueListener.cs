using System;
using System.Diagnostics;
using EdFi.Common.Messaging;
using Microsoft.ServiceBus.Messaging;

namespace EdFi.Ods.WindowsAzure.Messaging
{
    public class AzureQueueListener<TCommand> : IQueueListener<TCommand> where TCommand : ICommand
    {
        private readonly IQueueNameProvider queueNameProvider;
        private readonly WindowsAzureMessagingFactory messagingFactory = new WindowsAzureMessagingFactory();
        private string queueName;
        private QueueClient clientInstance;

        public AzureQueueListener(IQueueNameProvider queueNameProvider)
        {
            this.queueNameProvider = queueNameProvider;
        }

        public void StartListening(Action<IMessageManager<TCommand>> callback)
        {
            Trace.TraceInformation("Listening for messages on queue path '{0}'", Client.Path);
            var options = BuildListeningOptions();
            Client.OnMessage(x =>
                                 {
                                     Trace.TraceInformation("Message being handled from queue '{0}'", QueueName);
                                     using (var keepAlive = BrokeredMessageLockRenewer.KeepMessageAlive(x))
                                     using (var broker = new AzureMessageManager<TCommand>(x, keepAlive))
                                     {
                                         callback(broker);
                                     }
                                 }, options);
        }

        private OnMessageOptions BuildListeningOptions()
        {
            return new OnMessageOptions
                       {
                           MaxConcurrentCalls = 1,
                           AutoComplete = false
                       };
        }

        public QueueClient Client
        {
            get
            {
                if (clientInstance == null)
                    clientInstance = messagingFactory.GetClientForQueue(QueueName);
                return clientInstance;
            }
        }

        private string QueueName
        {
            get
            {
                if (queueName == null)
                    queueName = queueNameProvider.GetQueueNameFor<TCommand>();
                return queueName;
            }
        }

        public void StopListening()
        {
            try
            {
                messagingFactory.CloseClient(QueueName);
            }
            catch (Exception ex)
            {
                // TODO: Change this to a log statement
                Trace.WriteLine(
                    string.Format("An exception occurred while attempting to stop listening to queue '{0}':\r\n{1}",
                                  QueueName, ex));
            }
        }
    }
}