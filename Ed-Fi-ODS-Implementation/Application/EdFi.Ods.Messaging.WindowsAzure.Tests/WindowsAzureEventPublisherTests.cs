using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using EdFi.Common.InversionOfControl;
using EdFi.Common.Messaging;
using EdFi.Ods.Common;
using EdFi.Ods.WindowsAzure.Messaging;
using Microsoft.ServiceBus.Messaging;
using NUnit.Framework;
using Should;
using UnitTests.EdFi.Ods.Messaging;

namespace EdFi.Ods.Messaging.WindowsAzure.Tests
{
    using EdFi.Ods.Tests.EdFi.Ods.Messaging;

    public class When_publishing_a_single_event : WindowsAzureMessagingTestFixtureBase
    {
        private const string subscriptionName = "AllMessages";

        private string suppliedMessageText = "Message to a topic: " + DateTime.Now.Ticks;

        private WindowsAzureMessagingFactory messagingFactory = new WindowsAzureMessagingFactory();

        protected override void BeforeBehaviorExecution()
        {
            // Clear the test subscription
            GetSubscriptionMessages(subscriptionName);
        }

        protected override void ExecuteBehavior()
        {
            var message = new TestCommand() { Text = suppliedMessageText };

            var producer = new WindowsAzureEventPublisher(TopicNameProvider);
            producer.Publish(message);
        }

        [Test]
        public virtual void Should_publish_just_that_message_in_the_topic()
        {
            var actualMessages = GetSubscriptionMessages(subscriptionName);

            //    var actualMessages = ReceiveAllMessages().ToList();

            actualMessages.Count.ShouldEqual(1);

            var actualMessage = actualMessages.Single().GetBody<TestCommand>();

            actualMessage.Text.ShouldEqual(suppliedMessageText);
        }

        private List<BrokeredMessage> GetSubscriptionMessages(string subscriptionName)
        {
            string topicName = TopicNameProvider.GetTopicNameFor<TestCommand>();

            var subscriptionClient = messagingFactory.GetClientForSubscription(topicName, subscriptionName);

            var actualMessages = subscriptionClient.ReceiveBatch(int.MaxValue, TimeSpan.FromSeconds(10)).ToList();

            actualMessages.ForEach(m => m.Complete());

            return actualMessages;
        }
    }

    public class When_publishing_multiple_events_to_a_topic_with_two_subscriptions_each_monitored_by_a_different_listener : WindowsAzureMessagingTestFixtureBase
    {
        private IServiceLocator serviceLocator;
        private TestCommandHandler testCommandHandler;

        private IEventListener eventListener1;
        private IEventListener eventListener2;

        private const string subscriptionName1 = "AllMessages1";
        private const string subscriptionName2 = "AllMessages2";

        private string suppliedMessageText1 = "Message 1 to a topic: " + DateTime.Now.Ticks;
        private string suppliedMessageText2 = "Message 2 to a topic: " + DateTime.Now.Ticks;
        private string suppliedMessageText3 = "Message 3 to a topic: " + DateTime.Now.Ticks;

        private WindowsAzureMessagingFactory messagingFactory = new WindowsAzureMessagingFactory();
        private string topicName;
        private List<TestCommand> actualSubscription1HandledMessages;
        private List<TestCommand> actualSubscription2HandledMessages;

        protected override void EstablishContext()
        {
            topicName = TopicNameProvider.GetTopicNameFor<TestCommand>();

            testCommandHandler = new TestCommandHandler();

            serviceLocator = new FakeServiceLocator(t =>
                (Array)new object[] { testCommandHandler });
        }

        protected override void BeforeBehaviorExecution()
        {
            // Clear the test subscriptions of messages
            GetSubscriptionMessages(subscriptionName1);
            GetSubscriptionMessages(subscriptionName2);
        }

        protected override void ExecuteBehavior()
        {
            var message1 = new TestCommand() { Text = suppliedMessageText1 };
            var message2 = new TestCommand() { Text = suppliedMessageText2 };
            var message3 = new TestCommand() { Text = suppliedMessageText3 };

            var producer = new WindowsAzureEventPublisher(TopicNameProvider);
            producer.Publish(message1);
            producer.Publish(message2);
            producer.Publish(message3);

            // Reset handler
            testCommandHandler.Reset();

            // Start listening to subscription 2
            eventListener1 = new WindowsAzureEventListener(new MessageProcessor(serviceLocator));
            eventListener1.StartListening(topicName, subscriptionName1);

            WaitForMessagesToBeHandled(10000);

            actualSubscription1HandledMessages = new List<TestCommand>(testCommandHandler.HandledMessages);

            // Reset handler
            testCommandHandler.Reset();

            // Start listening to subscription 2
            eventListener2 = new WindowsAzureEventListener(new MessageProcessor(serviceLocator));
            eventListener2.StartListening(topicName, subscriptionName2);

            WaitForMessagesToBeHandled(10000);

            actualSubscription2HandledMessages = new List<TestCommand>(testCommandHandler.HandledMessages);
        }

        [Test]
        public virtual void Should_receive_all_messages_in_order_on_the_first_subscription()
        {
            var messages = actualSubscription1HandledMessages;
            
            messages.Count.ShouldEqual(3);

            messages[0].Text.ShouldEqual(suppliedMessageText1);
            messages[1].Text.ShouldEqual(suppliedMessageText2);
            messages[2].Text.ShouldEqual(suppliedMessageText3);
        }

        [Test]
        public virtual void Should_receive_all_messages_in_order_on_the_second_subscription()
        {
            var messages = actualSubscription2HandledMessages;
                
            messages.Count.ShouldEqual(3);

            messages[0].Text.ShouldEqual(suppliedMessageText1);
            messages[1].Text.ShouldEqual(suppliedMessageText2);
            messages[2].Text.ShouldEqual(suppliedMessageText3);
        }

        private void WaitForMessagesToBeHandled(int milliseconds)
        {
            var sw = new Stopwatch();
            sw.Start();

            while (sw.ElapsedMilliseconds < milliseconds
                   && (testCommandHandler.HandledMessages.Count < 3))
            {
                Thread.Sleep(50);
            }
        }

        public override void RunOnceAfterAll()
        {
            base.RunOnceAfterAll();

            eventListener1.StopListening(topicName, subscriptionName1);
            eventListener2.StopListening(topicName, subscriptionName2);
        }

        private List<BrokeredMessage> GetSubscriptionMessages(string subscriptionName)
        {
            var subscriptionClient = messagingFactory.GetClientForSubscription(topicName, subscriptionName);

            var actualMessages = subscriptionClient.ReceiveBatch(int.MaxValue, TimeSpan.FromSeconds(10)).ToList();

            actualMessages.ForEach(m => m.Complete());

            return actualMessages;
        }
    }

}