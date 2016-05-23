using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.Diagnostics;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;

namespace EdFi.Ods.WindowsAzure.Messaging
{
    public class WindowsAzureMessagingFactory
    {
        private readonly TimeSpan _queueLockTimeout;
        private static string _connectionString;

        public WindowsAzureMessagingFactory()
        {
            var configuredQueueLockTimeoutSeconds = ConfigurationManager.AppSettings["AzureMessageLockTimeoutSeconds"];
            if (string.IsNullOrEmpty(configuredQueueLockTimeoutSeconds))
                throw new Exception("The setting AzureMessageLockTimeoutSeconds is required.");
            _queueLockTimeout = TimeSpan.FromSeconds(int.Parse(configuredQueueLockTimeoutSeconds));
        }

        /// <summary>
        /// Gets the configured connection string Azure Service Bus connection string.
        /// </summary>
        public static string ConnectionString
        {
            get
            {
                if (_connectionString == null)
                    _connectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");

                return _connectionString;
            }
        }

        private NamespaceManager _namespaceManager;

        /// <summary>
        /// Gets the NamespaceManager for the currently configured connection string.
        /// </summary>
        private NamespaceManager NamespaceManager
        {
            get
            {
                if (_namespaceManager == null)
                    _namespaceManager = NamespaceManager.CreateFromConnectionString(ConnectionString);

                return _namespaceManager;
            }
        }

        private ConcurrentDictionary<Tuple<string, string>, SubscriptionClient> subscriptionClientsBySubscriptionName
            = new ConcurrentDictionary<Tuple<string, string>, SubscriptionClient>();

        /// <summary>
        /// Gets a <see cref="Microsoft.ServiceBus.Messaging.SubscriptionClient" /> for the specified queue name, creating the queue if necessary.
        /// </summary>
        /// <param name="topicName">The name of the topic for which the subscription exists.</param>
        /// <param name="subscriptionName">The name of the subscription for which to retrieve a <see cref="Microsoft.ServiceBus.Messaging.SubscriptionClient"/>.</param>
        /// <returns>The <see cref="Microsoft.ServiceBus.Messaging.SubscriptionClient"/> for the subscription.</returns>
        [Obsolete("This method and the underlying implementation were not completed or used in Messaging V1.")]
        public SubscriptionClient GetClientForSubscription(string topicName, string subscriptionName)
        {
            var key = GetSubscriptionKey(topicName, subscriptionName);

            return subscriptionClientsBySubscriptionName.GetOrAdd(key, qn =>
            {
                EnsureTopicExists(qn.Item1);
                EnsureSubscriptionExists(qn.Item1, qn.Item2);
                return SubscriptionClient.CreateFromConnectionString(ConnectionString, qn.Item1, qn.Item2, ReceiveMode.PeekLock);
            });
        }

        [Obsolete("This method and the underlying implementation were not completed or used in Messaging V1.")]
        private static Tuple<string, string> GetSubscriptionKey(string topicName, string subscriptionName)
        {
            var key = Tuple.Create(topicName, subscriptionName);
            return key;
        }

        [Obsolete("This method and the underlying implementation were not completed or used in Messaging V1.")]
        private void EnsureSubscriptionExists(string topicName, string subscriptionName)
        {
            if (!NamespaceManager.SubscriptionExists(topicName, subscriptionName))
                NamespaceManager.CreateSubscription(topicName, subscriptionName);
        }

        /// <summary>
        /// Deletes the specified subscription.
        /// </summary>
        /// <param name="topicName">The name of the topic for which the subscription exists.</param>
        /// <param name="subscriptionName">The name of the subscription to delete.</param>
        [Obsolete("This method and the underlying implementation were not completed or used in Messaging V1.")]
        public void DeleteSubscription(string topicName, string subscriptionName)
        {
            NamespaceManager.DeleteSubscription(topicName, subscriptionName);
        }

        /// <summary>
        /// Closes the specified <see cref="Microsoft.ServiceBus.Messaging.SubscriptionClient"/>.
        /// </summary>
        /// <param name="topicName">The name of the topic for which the subscription exists.</param>
        /// <param name="subscriptionName">The name of the subscription whose corresponding <see cref="Microsoft.ServiceBus.Messaging.SubscriptionClient"/> should be closed.</param>
        [Obsolete("This method and the underlying implementation were not completed or used in Messaging V1.")]
        public void CloseSubscriptionClient(string topicName, string subscriptionName)
        {
            SubscriptionClient subscriptionClient;

            var key = GetSubscriptionKey(topicName, subscriptionName);

            subscriptionClientsBySubscriptionName.TryRemove(key, out subscriptionClient);

            if (subscriptionClient == null)
                throw new Exception(string.Format("Client for subscription '{0}' of topic '{1}' has not been initialized.", subscriptionName, topicName));

            subscriptionClient.Close();
        }

        [Obsolete("This method and the underlying implementation were not completed or used in Messaging V1.")]
        private ConcurrentDictionary<string, TopicClient> topicClientsByTopicName
            = new ConcurrentDictionary<string, TopicClient>();

        /// <summary>
        /// Gets a <see cref="Microsoft.ServiceBus.Messaging.TopicClient" /> for the specified queue name, creating the queue if necessary.
        /// </summary>
        /// <param name="topicName">The name of the topic for which to retrieve a <see cref="Microsoft.ServiceBus.Messaging.TopicClient"/>.</param>
        /// <returns>The <see cref="Microsoft.ServiceBus.Messaging.TopicClient"/> for the topic.</returns>
        [Obsolete("This method and the underlying implementation were not completed or used in Messaging V1.")]
        public TopicClient GetClientForTopic(string topicName)
        {
            return topicClientsByTopicName.GetOrAdd(topicName, qn =>
            {
                EnsureTopicExists(topicName);
                return TopicClient.CreateFromConnectionString(ConnectionString, topicName);
            });
        }

        [Obsolete("This method and the underlying implementation were not completed or used in Messaging V1.")]
        private void EnsureTopicExists(string topicName)
        {
            if (!NamespaceManager.TopicExists(topicName))
                NamespaceManager.CreateTopic(topicName);
        }

        /// <summary>
        /// Deletes the specified topic.
        /// </summary>
        /// <param name="topicName">The name of the topic to delete.</param>
        [Obsolete("This method and the underlying implementation were not completed or used in Messaging V1.")]
        public void DeleteTopic(string topicName)
        {
            NamespaceManager.DeleteTopic(topicName);
        }

        /// <summary>
        /// Closes the specified <see cref="Microsoft.ServiceBus.Messaging.TopicClient"/>.
        /// </summary>
        /// <param name="topicName">The name of the topic whose corresponding <see cref="Microsoft.ServiceBus.Messaging.TopicClient"/> should be closed.</param>
        [Obsolete("This method and the underlying implementation were not completed or used in Messaging V1.")]
        public void CloseTopicClient(string topicName)
        {
            TopicClient topicClient;

            topicClientsByTopicName.TryRemove(topicName, out topicClient);

            if (topicClient == null)
                throw new Exception(string.Format("Client for queue {0} has not been initialized.", topicName));

            topicClient.Close();
        }

        private ConcurrentDictionary<string, QueueClient> queueClientsByQueueName
            = new ConcurrentDictionary<string, QueueClient>();

        /// <summary>
        /// Gets a <see cref="Microsoft.ServiceBus.Messaging.QueueClient" /> for the specified queue name, creating the queue if necessary.
        /// </summary>
        /// <param name="queueName">The name of the queue for which to retrieve a <see cref="Microsoft.ServiceBus.Messaging.QueueClient"/>.</param>
        /// <returns>The <see cref="Microsoft.ServiceBus.Messaging.QueueClient"/> for the queue.</returns>
        public QueueClient GetClientForQueue(string queueName)
        {
            return queueClientsByQueueName.GetOrAdd(queueName, qn =>
                {
                    EnsureQueueExists(queueName);
                    return QueueClient.CreateFromConnectionString(ConnectionString, queueName, ReceiveMode.PeekLock);
                });
        }

        /// <summary>
        /// Deletes the specified queue.
        /// </summary>
        /// <param name="queueName">The name of the queue to delete.</param>
        public void DeleteQueue(string queueName)
        {
            if (NamespaceManager.QueueExists(queueName))
            {
                Trace.TraceWarning("Deleting queue '{0}'", queueName);
                NamespaceManager.DeleteQueue(queueName);
            }
        }

        /// <summary>
        /// Gets the existing <see cref="Microsoft.ServiceBus.Messaging.QueueClient"/> for the specified queue name.
        /// </summary>
        /// <param name="queueName">The name of the queue for which to look for an existing <see cref="Microsoft.ServiceBus.Messaging.QueueClient"/>.</param>
        /// <param name="queueClient">On return, contains the existing <see cref="Microsoft.ServiceBus.Messaging.QueueClient"/>; otherwise <b>null</b>.</param>
        /// <returns><b>true</b> if the <see cref="Microsoft.ServiceBus.Messaging.QueueClient"/> already existed; otherwise <b>false</b>.</returns>
        public bool TryGetClientForQueue(string queueName, out QueueClient queueClient)
        {
            if (!queueClientsByQueueName.TryGetValue(queueName, out queueClient))
                return false;

            return true;
        }

        /// <summary>
        /// Closes the specified <see cref="Microsoft.ServiceBus.Messaging.QueueClient"/>.
        /// </summary>
        /// <param name="queueName">The name of the queue whose corresponding <see cref="Microsoft.ServiceBus.Messaging.QueueClient"/> should be closed.</param>
        public void CloseClient(string queueName)
        {
            QueueClient queueClient;

            queueClientsByQueueName.TryRemove(queueName, out queueClient);

            if (queueClient == null)
                throw new Exception(string.Format("Client for queue {0} has not been initialized.", queueName));

            queueClient.Close();
        }

        private void EnsureQueueExists(string queueName)
        {
            if (!NamespaceManager.QueueExists(queueName))
            {
                Trace.TraceInformation("Creating queue '{0}'", queueName);
                var description = new QueueDescription(queueName) {LockDuration = _queueLockTimeout};
                NamespaceManager.CreateQueue(description);
            }
        }
    }
}