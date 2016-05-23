using EdFi.Common.Messaging;
using EdFi.Ods.WindowsAzure.Messaging;

namespace EdFi.Ods.Messaging.WindowsAzure.Tests.Extensions
{
    public static class AzureQueueListenerExtensions
    {
        public static void ClearQueue<TCommand>(this AzureQueueListener<TCommand> queueListener) where TCommand : ICommand
        {
            var client = queueListener.Client;
            while (client.Peek() != null)
            {
                var message = client.Receive();
                message.Complete();
            }
        }
    }
}