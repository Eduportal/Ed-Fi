using System.Diagnostics;
using EdFi.Common.Messaging;
using Microsoft.ServiceBus.Messaging;

namespace EdFi.Ods.WindowsAzure.Messaging
{
    public class AzureServiceBusSender : IBusSender
    {
        private readonly IQueueNameProvider queueNameProvider;

        private readonly WindowsAzureMessagingFactory messagingFactory = new WindowsAzureMessagingFactory();

        public AzureServiceBusSender(IQueueNameProvider queueNameProvider)
        {
            this.queueNameProvider = queueNameProvider;
        }

        public void Send<TMessage>(IEnvelope<TMessage> envelope) where TMessage : ICommand
        {
            string queueName = queueNameProvider.GetQueueNameFor<TMessage>();

            Trace.WriteLine("Sending message to queue: " + queueName);

            var client = messagingFactory.GetClientForQueue(queueName);

            var brokeredMessage = new BrokeredMessage(envelope);

            brokeredMessage.Properties["TypeName"] = typeof (TMessage).AssemblyQualifiedName;

            client.Send(brokeredMessage);
        }
    }
}