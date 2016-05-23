using System;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using NUnit.Framework;

namespace EdFi.Ods.Messaging.WindowsAzure.Tests
{
    [TestFixture]
    public class QueueCleaningHarness
    {
        [Test, Ignore("To be run manually only")]
        public void Clean()
        {
            var connectionString = @"";
            var cleaner = new QueueCleaner(connectionString);
//            cleaner.Clean("commituploadcommand", cleanDeadLetters: true);
//            cleaner.Clean("startoperationcommand", cleanDeadLetters: true);
        }
    }

    public class QueueCleaner
    {
        private readonly string _connectionString;
        private NamespaceManager _namespaceManager;

        public QueueCleaner(string connectionString)
        {
            _connectionString = connectionString;
        }

        private NamespaceManager NamespaceManager
        {
            get { return _namespaceManager ?? (_namespaceManager = NamespaceManager.CreateFromConnectionString(_connectionString)); }
        }

        public void Clean(string queueName, bool cleanDeadLetters = false)
        {
            Console.WriteLine("Cleaning queue '{0}'", queueName);

            if (QueueDoesNotExist(queueName)) 
                return;
            ReceiveAndCompleteAllMessages(queueName);
            if (cleanDeadLetters)
                ReceiveAndCompleteAllMessages(queueName + "/$DeadLetterQueue");
        }

        private bool QueueDoesNotExist(string queueName)
        {
            Console.WriteLine("Making sure queue exists...");
            if (!NamespaceManager.QueueExists(queueName))
            {
                Console.WriteLine("Could not find queue with name '{0}' so it must be empty!", queueName);
                return true;
            }
            return false;
        }

        private void ReceiveAndCompleteAllMessages(string queueName)
        {
            Console.WriteLine("Creating client for queue {0}...", queueName);
            var client = QueueClient.CreateFromConnectionString(_connectionString, queueName, ReceiveMode.PeekLock);
            var counter = 1;

            Console.WriteLine("Checking for messages...");
            while (client.Peek() != null)
            {
                Console.WriteLine("Deleting message {0}...", counter++);
                var message = client.Receive();
                message.Complete();
            }
            Console.WriteLine("Done with messages for queue {0}", queueName);
        }
    }
}