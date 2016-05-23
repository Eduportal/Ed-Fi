using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using EdFi.Common.InversionOfControl;
using EdFi.Common.Messaging;
using EdFi.Ods.Messaging.WindowsAzure.Tests.Extensions;
using EdFi.Ods.WindowsAzure.Messaging;
using Microsoft.ServiceBus.Messaging;
using NUnit.Framework;
using Should;
using UnitTests.EdFi.Ods.Messaging;

namespace EdFi.Ods.Messaging.WindowsAzure.Tests
{
    using EdFi.Ods.Tests.EdFi.Ods.Messaging;

    public class WindowsAzureCommandSendingIntegrationTests
    {
        static readonly InboundEnvelopeProcessingMgr InboundEnvelopeProcessingMgr = new InboundEnvelopeProcessingMgr(new IInboundEnvelopeDataProcessor[0]);
        static readonly OutboundEnvelopeProcessingMgr OutboundEnvelopeProcessingMgr = new OutboundEnvelopeProcessingMgr(new IOutboundEnvelopeDataProcessor[0]);

        [TestFixture]
        public class When_sending_a_single_message_with_a_queue_listener_attached : WindowsAzureMessagingTestFixtureBase
        {
            private IServiceLocator serviceLocator;
            private ICommandListener<TestCommand> commandListener;
            private TestCommandHandler testCommandHandler;

            private string suppliedMessageText;

            protected override void EstablishContext()
            {
                RemoveAllKnownQueuesAndTopics();
                testCommandHandler = new TestCommandHandler();

                serviceLocator = new FakeServiceLocator(t =>
                                                        (Array) new object[] {testCommandHandler});
            }

            protected override void ExecuteBehavior()
            {
                // Construct the message
                suppliedMessageText = "Message with a listener: " + DateTime.Now.Ticks;
                var message = new TestCommand {Text = suppliedMessageText};

                var producer = new CommandSender(OutboundEnvelopeProcessingMgr, new AzureServiceBusSender(QueueNameProvider));
                var queueListener = new AzureQueueListener<TestCommand>(QueueNameProvider);

                //Clear the queue
                queueListener.ClearQueue();

                // Send the message
                producer.Send(message);

                commandListener = new CommandListener<TestCommand>(queueListener, new MessageProcessor(serviceLocator), InboundEnvelopeProcessingMgr);

                // Start listening for the message
                commandListener.StartListening();

                WaitForMessagesToBeHandled(20000);
            }

            private void WaitForMessagesToBeHandled(int milliseconds)
            {
                var sw = new Stopwatch();
                sw.Start();

                while (sw.ElapsedMilliseconds < milliseconds
                       && (testCommandHandler.HandledMessages.Count == 0))
                {
                    Thread.Sleep(50);
                }
            }

            [Test]
            public virtual void Should_receive_exactly_1_message()
            {
                testCommandHandler.HandledMessages.Count.ShouldEqual(1);
            }

            [Test]
            public virtual void Should_have_correct_message_contents()
            {
                var actualMessage = testCommandHandler.HandledMessages.Single();

                actualMessage.Text.ShouldEqual(suppliedMessageText);
            }

            [Test]
            public virtual void Should_process_the_message_with_the_correct_handler()
            {
                var actualMessage = testCommandHandler.HandledMessages.Single();

                Assert.That(actualMessage.Handled, Is.True);
                Assert.That(actualMessage.HandledBy, Is.EqualTo(typeof (TestCommandHandler).Name));
            }

            public override void RunOnceAfterAll()
            {
                base.RunOnceAfterAll();

                commandListener.StopListening();
            }
        }

        [TestFixture]
        public class When_sending_multiple_messages_of_different_types_with_a_queue_listener_attached :
            WindowsAzureMessagingTestFixtureBase
        {
            private IServiceLocator serviceLocator;
            private ICommandListener<TestCommand> listener1;
            private ICommandListener<AnotherCommand> listener2;

            private string suppliedMessage1Text;
            private string suppliedMessage2Text;
            private string suppliedMessage3Text;

            private TestCommand actualMessage1;
            private AnotherCommand actualMessage2;
            private TestCommand actualMessage3;

            private TestCommandHandler testCommandHandler = new TestCommandHandler();
            private AnotherCommandHandler anotherCommandHandler = new AnotherCommandHandler();

            private WindowsAzureTestingQueueNameProvider nameProvider1;
            private WindowsAzureTestingQueueNameProvider nameProvider2;

            private readonly long ticks = DateTime.Now.Ticks;

            protected override void EstablishContext()
            {
                nameProvider1 = new WindowsAzureTestingQueueNameProvider((GetType().Name + "q1").GetHashCode());
                RegisterQueueNameForCleanup(nameProvider1.TestQueueName);
                nameProvider2 = new WindowsAzureTestingQueueNameProvider((GetType().Name + "q2").GetHashCode());
                RegisterQueueNameForCleanup(nameProvider2.TestQueueName);
                RemoveAllKnownQueuesAndTopics();

                suppliedMessage1Text = "First TestCommand: " + ticks;
                suppliedMessage2Text = "First AnotherCommand: " + ticks;
                suppliedMessage3Text = "Second TestCommand: " + ticks;

                serviceLocator = new FakeServiceLocator(t =>
                                                            {
                                                                if (t.IsAssignableFrom(typeof (AnotherCommandHandler)))
                                                                    return (Array) new object[] {anotherCommandHandler};

                                                                if (t.IsAssignableFrom(typeof (TestCommandHandler)))
                                                                    return (Array) new object[] {testCommandHandler};

                                                                return null;
                                                            });
            }

            protected override void ExecuteBehavior()
            {
                // Construct the message
                var suppliedMessage1 = new TestCommand {Text = suppliedMessage1Text};
                var suppliedMessage2 = new AnotherCommand {Text = suppliedMessage2Text};
                var suppliedMessage3 = new TestCommand {Text = suppliedMessage3Text};

                // Send the message
                var producer1 = new AzureServiceBusSender(nameProvider1);
                producer1.Send(new Envelope<TestCommand>(suppliedMessage1));
                producer1.Send(new Envelope<TestCommand>(suppliedMessage3));

                var producer2 = new AzureServiceBusSender(nameProvider2);
                producer2.Send(new Envelope<AnotherCommand>(suppliedMessage2));

                // Listen for the messages
                listener1 = new CommandListener<TestCommand>(new AzureQueueListener<TestCommand>(nameProvider1),
                                                             new MessageProcessor(serviceLocator),
                                                             InboundEnvelopeProcessingMgr);
                listener1.StartListening();

                listener2 = new CommandListener<AnotherCommand>(new AzureQueueListener<AnotherCommand>(nameProvider2),
                                                                new MessageProcessor(serviceLocator),
                                                                InboundEnvelopeProcessingMgr);
                listener2.StartListening();

                WaitForMessagesToBeHandled(10000);

                // Perform some base-level verification of the messages that were handled
                testCommandHandler.HandledMessages.Count.ShouldEqual(2,
                                                                     "TestCommandHandler did not process 2 messages, as expected.");
                anotherCommandHandler.HandledMessages.Count.ShouldEqual(1,
                                                                        "AnotherCommandHandler did not process 1 message, as expected.");

                // Grab actual messages for assertions below
                actualMessage1 = testCommandHandler.HandledMessages[0];
                actualMessage2 = anotherCommandHandler.HandledMessages[0];
                actualMessage3 = testCommandHandler.HandledMessages[1];
            }

            private void WaitForMessagesToBeHandled(int milliseconds)
            {
                var sw = new Stopwatch();
                sw.Start();

                while (sw.ElapsedMilliseconds < milliseconds
                       && (testCommandHandler.HandledMessages.Count < 2
                           || anotherCommandHandler.HandledMessages.Count < 1))
                {
                    Thread.Sleep(10);
                }
            }

            [Test]
            public virtual void Should_process_all_messages()
            {
                Assert.That(actualMessage1.Handled, Is.True);
                Assert.That(actualMessage2.Handled, Is.True);
                Assert.That(actualMessage3.Handled, Is.True);
            }

            [Test]
            public virtual void Should_find_that_all_messages_have_the_correct_text()
            {
                actualMessage1.Text.ShouldEqual(suppliedMessage1Text);
                actualMessage2.Text.ShouldEqual(suppliedMessage2Text);
                actualMessage3.Text.ShouldEqual(suppliedMessage3Text);
            }

            [Test]
            public virtual void Should_process_all_messages_with_the_correct_handlers()
            {
                actualMessage1.HandledBy.ShouldEqual(typeof (TestCommandHandler).Name);
                actualMessage2.HandledBy.ShouldEqual(typeof (AnotherCommandHandler).Name);
                actualMessage3.HandledBy.ShouldEqual(typeof (TestCommandHandler).Name);
            }

            [Test]
            public virtual void Should_process_all_messages_in_the_order_sent_to_their_respective_handlers()
            {
                actualMessage1.Order.ShouldEqual(1); // First message on first handler
                actualMessage2.Order.ShouldEqual(1); // Different handler, so count starts at 1
                actualMessage3.Order.ShouldEqual(2); // Second message on first handler
            }

            public override void RunOnceAfterAll()
            {
                base.RunOnceAfterAll();

                listener1.StopListening();
                listener2.StopListening();
            }
        }

        [TestFixture]
        public class When_sending_a_message_where_the_handler_throws_an_exception : WindowsAzureMessagingTestFixtureBase
        {
            private IServiceLocator serviceLocator;
            private ICommandListener<TestCommand> listener;

            private TestCommandHandlerThatThrowsAnException testCommandHandler;

            private readonly string suppliedMessageText = "Message that throws an exception.";

            private string testQueueName;
            private List<BrokeredMessage> actualDeadLetterMessages;
            private List<BrokeredMessage> orphanedMessages;

            protected override void EstablishContext()
            {
                RemoveAllKnownQueuesAndTopics();
                testCommandHandler = new TestCommandHandlerThatThrowsAnException();
                testQueueName = QueueNameProvider.GetQueueNameFor<TestCommand>();

                serviceLocator = new FakeServiceLocator(t =>
                                                        (Array) new object[] {testCommandHandler});
            }

            protected override void ExecuteBehavior()
            {
                // Construct the message
                var message = new TestCommand {Text = suppliedMessageText};

                // Send the message
                var producer = new AzureServiceBusSender(QueueNameProvider);
                producer.Send(new Envelope<TestCommand>(message));

                // Start listening for the message
                var queueListener = new AzureQueueListener<TestCommand>(QueueNameProvider);
                listener = new CommandListener<TestCommand>(queueListener, new MessageProcessor(serviceLocator), InboundEnvelopeProcessingMgr);
                listener.StartListening();

                WaitForMessagesToBeHandled(10000);

                // Fetch the messages on the dead letter queue for the
                var messagingFactory =
                    MessagingFactory.CreateFromConnectionString(WindowsAzureMessagingFactory.ConnectionString);
                var deadLetterClient =
                    messagingFactory.CreateQueueClient(QueueClient.FormatDeadLetterPath(testQueueName),
                                                       ReceiveMode.ReceiveAndDelete);
                actualDeadLetterMessages =
                    deadLetterClient.ReceiveBatch(int.MaxValue, TimeSpan.FromSeconds(10)).ToList();
                deadLetterClient.Close();

                // Fetch the messages left in the main queue
                var queueClient = messagingFactory.CreateQueueClient(testQueueName, ReceiveMode.ReceiveAndDelete);
                orphanedMessages = queueClient.ReceiveBatch(int.MaxValue, TimeSpan.FromMilliseconds(100)).ToList();
                queueClient.Close();
            }

            private void WaitForMessagesToBeHandled(int milliseconds)
            {
                var sw = new Stopwatch();
                sw.Start();

                while (sw.ElapsedMilliseconds < milliseconds
                       && (testCommandHandler.HandledMessages.Count == 0))
                {
                    Thread.Sleep(50);
                }
            }

            [Test]
            public virtual void Should_handle_exactly_1_message()
            {
                testCommandHandler.HandledMessages.Count.ShouldEqual(1);
            }

            [Test]
            public virtual void Should_send_failed_message_to_the_dead_letter_queue()
            {
                // The message that failed to be processed should be in the dead letter queue
                actualDeadLetterMessages.Count.ShouldEqual(1);
                actualDeadLetterMessages[0].GetBody<Envelope<TestCommand>>().Message.Text.ShouldEqual(suppliedMessageText);
            }

            [Test]
            public virtual void Should_remove_failed_message_from_the_main_queue()
            {
                // The message that failed to be processed should be in the dead letter queue
                orphanedMessages.Count.ShouldEqual(0);
            }

            public override void RunOnceAfterAll()
            {
                base.RunOnceAfterAll();

                listener.StopListening();
            }
        }


        [TestFixture]
        public class When_processing_a_message_takes_longer_than_the_message_lock_timeout : WindowsAzureMessagingTestFixtureBase
        {
            public class TestCommandHandlerThatWaits : IMessageHandler<TestCommand>
            {
                private readonly TimeSpan _waitTime;
                private readonly Action<TestCommand> _messageCallback;
                private readonly List<TestCommand> _handled = new List<TestCommand>();

                public TestCommand[] HandledMessages { get { return _handled.ToArray(); } }

                public TestCommandHandlerThatWaits(TimeSpan waitTime, Action<TestCommand> messageCallback)
                {
                    _waitTime = waitTime;
                    _messageCallback = messageCallback;
                }

                public void Handle(TestCommand message)
                {
                    Console.WriteLine("Handling message with text '{0}' after a sleep of {1} starting at {2}", message.Text, _waitTime, DateTime.Now);
                    Thread.Sleep(_waitTime);
                    _messageCallback(message);
                    _handled.Add(message);
                    Console.WriteLine("Done at {0}", DateTime.Now);
                }
            }

            private IServiceLocator serviceLocator;
            private CommandListener<TestCommand> listener;

            private TestCommandHandlerThatWaits testCommandHandler;

            private readonly string suppliedMessageText = "I like to sing and dance.";

            private string testQueueName;
            private TestCommand _processedCommand;
            private TimeSpan handlerWaitTime;
            private Exception _lastExceptionInCommandListener;

            protected override void EstablishContext()
            {
                RemoveAllKnownQueuesAndTopics();
                var timeToWait = int.Parse(ConfigurationManager.AppSettings["AzureMessageLockTimeoutSeconds"])*2;
                handlerWaitTime = TimeSpan.FromSeconds(timeToWait);
                testCommandHandler = new TestCommandHandlerThatWaits(handlerWaitTime, x => _processedCommand = x);
                testQueueName = QueueNameProvider.GetQueueNameFor<TestCommand>();

                serviceLocator = new FakeServiceLocator(t =>
                                                        (Array) new object[] {testCommandHandler});
            }

            protected override void ExecuteBehavior()
            {
                // Construct the message
                var message = new TestCommand {Text = suppliedMessageText};

                // Send the message
                var producer = new AzureServiceBusSender(QueueNameProvider);
                producer.Send(new Envelope<TestCommand>(message));

                // Start listening for the message
                var queueListener = new AzureQueueListener<TestCommand>(QueueNameProvider);
                listener = new CommandListener<TestCommand>(queueListener, new MessageProcessor(serviceLocator), InboundEnvelopeProcessingMgr);
                listener.StartListening();

                WaitForMessagesToBeHandled(handlerWaitTime.Add(TimeSpan.FromSeconds(5)));

                _lastExceptionInCommandListener = listener.LastExceptionStoreForTesting;
            }

            private void WaitForMessagesToBeHandled(TimeSpan waitTime)
            {
                var sw = new Stopwatch();
                sw.Start();

                while (sw.Elapsed < waitTime && (testCommandHandler.HandledMessages.Length == 0))
                {
                    Thread.Sleep(50);
                }

                //Allow the message to be cleaned up and finalized since this is the test for that behavior.
                if (sw.Elapsed < waitTime)
                    Thread.Sleep(TimeSpan.FromSeconds(5));
            }

            [Test]
            public void Should_process_the_message()
            {
                _processedCommand.ShouldNotBeNull();
                _processedCommand.Text.ShouldEqual(suppliedMessageText);
            }

            [Test]
            public void Should_not_have_any_exceptions_in_the_command_listener()
            {
                _lastExceptionInCommandListener.ShouldBeNull();
            }

            public override void RunOnceAfterAll()
            {
                base.RunOnceAfterAll();
                listener.StopListening();
            }
        }
    }
}