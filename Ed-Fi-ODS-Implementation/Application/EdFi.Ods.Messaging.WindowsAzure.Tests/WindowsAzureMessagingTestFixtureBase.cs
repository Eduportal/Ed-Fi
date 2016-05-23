using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using EdFi.Common.Messaging;
using EdFi.Ods.WindowsAzure.Messaging;
using Microsoft.ServiceBus.Messaging;
using UnitTests.EdFi.Ods.Messaging;

namespace EdFi.Ods.Messaging.WindowsAzure.Tests
{
    using EdFi.Ods.Tests.EdFi.Ods.Messaging;
    using EdFi.Ods.Tests._Bases;

    public abstract class WindowsAzureMessagingTestFixtureBase : TestFixtureBase
    {
        protected IQueueNameProvider QueueNameProvider;
        protected ITopicNameProvider TopicNameProvider;
        private readonly HashSet<string> _queueNames = new HashSet<string>();
        private readonly HashSet<string> _topicNames = new HashSet<string>();

        private readonly WindowsAzureMessagingFactory messagingFactory = new WindowsAzureMessagingFactory();

        protected WindowsAzureMessagingTestFixtureBase()
        {
            // Create a unique queue id based on the name of the test fixture
            int queueId = GetType().Name.GetHashCode();

            var testQueueNameProvider = new WindowsAzureTestingQueueNameProvider(queueId);
            RegisterQueueNameForCleanup(testQueueNameProvider.TestQueueName);
            QueueNameProvider = testQueueNameProvider;

            var testTopicNameProvider = new WindowsAzureTestingTopicNameProvider(queueId);
            TopicNameProvider = testTopicNameProvider;
            RegisterTopicNameForCleanup(testTopicNameProvider.TestTopicName);
        }

        protected IEnumerable<BrokeredMessage> ReceiveAllMessages()
        {
            QueueClient client = null;

            string testQueueName = QueueNameProvider.GetQueueNameFor<TestCommand>();

            try
            {
                client = messagingFactory.GetClientForQueue(testQueueName);

                var messages = client.ReceiveBatch(int.MaxValue, TimeSpan.FromMilliseconds(500)).ToList();

                foreach (var message in messages)
                    message.Complete();

                return messages;
            }
            finally
            {
                if (client != null)
                    messagingFactory.CloseClient(testQueueName);
            }
        }

        public override void RunOnceAfterAll()
        {
            base.RunOnceAfterAll();
            RemoveAllKnownQueuesAndTopics();
        }

        protected void RemoveAllKnownQueuesAndTopics()
        {
            foreach (var queueName in _queueNames)
            {
                Trace.TraceInformation("Cleaning up queue '{0}'", queueName);
                messagingFactory.DeleteQueue(queueName);
            }

            foreach (var topicName in _topicNames)
            {
                messagingFactory.DeleteTopic(topicName);
            }
        }

        protected void RegisterQueueNameForCleanup(string queueName)
        {
            Trace.TraceInformation("Registering queue '{0}' for cleanup", queueName);
            _queueNames.Add(queueName);
        }

        protected void RegisterTopicNameForCleanup(string topicName)
        {
            _topicNames.Add(topicName);
        }
    }
}