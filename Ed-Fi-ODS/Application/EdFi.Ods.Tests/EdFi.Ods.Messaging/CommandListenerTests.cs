using EdFi.Common.Messaging;
using UnitTests.EdFi.Ods.Messaging;

namespace EdFi.Ods.Tests.EdFi.Ods.Messaging
{
    using System;
    using NUnit.Framework;
    using Rhino.Mocks;

    public class CommandLisenerTests
    {
        static readonly InboundEnvelopeProcessingMgr EnvelopeProcessingMgr = new InboundEnvelopeProcessingMgr(new IInboundEnvelopeDataProcessor[0]);

        [TestFixture]
        public class When_command_listener_starts_listening
        {
            [Test]
            public void Should_delegate_register_callback_with_queue_listener()
            {
                var queueListener = MockRepository.GenerateMock<IQueueListener<TestCommand>>();

                var sut = new CommandListener<TestCommand>(queueListener, null, EnvelopeProcessingMgr);

                sut.StartListening();

                queueListener.AssertWasCalled(
                    x => x.StartListening(Arg<Action<IMessageManager<TestCommand>>>.Is.Anything));
            }
        }

        [TestFixture]
        public class When_command_listener_stops_listening
        {
            [Test]
            public void Should_ask_queue_listener_to_stop()
            {
                var queueListener = MockRepository.GenerateMock<IQueueListener<TestCommand>>();

                var sut = new CommandListener<TestCommand>(queueListener, null, EnvelopeProcessingMgr);

                sut.StopListening();

                queueListener.AssertWasCalled(x => x.StopListening());
            }
        }

        [TestFixture]
        public class When_command_listener_handles_message_successfully
        {
            private IMessageManager<TestCommand> _messageManager;
            private IMessageProcessor messageProcessor;
            private readonly TestCommand command = new TestCommand();

            [SetUp]
            public void SetUp()
            {
                var queueListener = MockRepository.GenerateMock<IQueueListener<TestCommand>>();
                messageProcessor = MockRepository.GenerateMock<IMessageProcessor>();

                var commandListener = new CommandListener<TestCommand>(queueListener, messageProcessor, EnvelopeProcessingMgr);

                commandListener.StartListening();

                var argsMade = queueListener.GetArgumentsForCallsMadeOn(x => x.StartListening(null),
                    opt => opt.IgnoreArguments());
                var handleAction = (Action<IMessageManager<TestCommand>>) (argsMade[0][0]);

                _messageManager = MockRepository.GenerateMock<IMessageManager<TestCommand>>();
                _messageManager.Expect(x => x.Envelope)
                    .Return(new Envelope<TestCommand>(command));

                handleAction(_messageManager);
            }

            [Test]
            public void Should_process_command()
            {
                messageProcessor.AssertWasCalled(x => x.Process(command));
            }

            [Test]
            public void Should_complete_command()
            {
                _messageManager.AssertWasCalled(x => x.Complete());
            }
        }

        [TestFixture]
        public class When_command_listener_failes_to_handle_message
        {
            private IMessageManager<TestCommand> _messageManager;

            [SetUp]
            public void SetUp()
            {
                var queueListener = MockRepository.GenerateMock<IQueueListener<TestCommand>>();
                var messageProcessor = MockRepository.GenerateMock<IMessageProcessor>();

                var commandListener = new CommandListener<TestCommand>(queueListener, messageProcessor, EnvelopeProcessingMgr);

                commandListener.StartListening();

                var argsMade = queueListener.GetArgumentsForCallsMadeOn(x => x.StartListening(null),
                    opt => opt.IgnoreArguments());
                var handleAction = (Action<IMessageManager<TestCommand>>) (argsMade[0][0]);

                _messageManager = MockRepository.GenerateMock<IMessageManager<TestCommand>>();
                _messageManager.Expect(x => x.Envelope).Throw(new Exception("please fail!"));

                handleAction(_messageManager);
            }

            [Test]
            public void Should_move_message_to_dead_queue()
            {
                _messageManager.AssertWasCalled(x => x.DeadLetter("doesn't matter", "doesn't matter"),
                    options => options.IgnoreArguments());
            }
        }
    }
}