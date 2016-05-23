using System;
using EdFi.Common.Messaging;

namespace EdFi.Ods.Messaging.WindowsAzure.Tests
{
    public class WindowsAzureTestingQueueNameProvider : IQueueNameProvider
    {
        public string TestQueueName { get; private set; }

        public WindowsAzureTestingQueueNameProvider(int queueId)
        {
            TestQueueName = "TestQueue_" + Environment.MachineName + "_" + queueId;
        }

        public string GetQueueNameFor<TMessage>() 
            where TMessage : ICommand
        {
            return GetQueueNameFor(typeof(TMessage));
        }

        public string GetQueueNameFor(Type t)
        {
            return TestQueueName;
        }
    }

    public class WindowsAzureTestingTopicNameProvider : ITopicNameProvider
    {
        public string TestTopicName { get; private set; }

        public WindowsAzureTestingTopicNameProvider(int topicId)
        {
            TestTopicName = "TestTopic_" + Environment.MachineName + "_" + topicId;
        }

        public string GetTopicNameFor<TMessage>() 
            where TMessage : IEvent
        {
            return TestTopicName;
        }

        public string GetTopicNameFor(Type t)
        {
            return TestTopicName;
        }
    }
}