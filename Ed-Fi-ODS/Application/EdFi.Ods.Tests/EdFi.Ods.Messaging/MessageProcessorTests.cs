using Castle.Windsor;
using EdFi.Common.InversionOfControl;
using EdFi.Common.Messaging;
using EdFi.Ods.Tests._Bases;
using EdFi.Ods.Tests._Extensions;
using Rhino.Mocks;
using UnitTests.EdFi.Ods.Messaging;

namespace EdFi.Ods.Tests.EdFi.Ods.Messaging
{
    using System;
    using NUnit.Framework;
    using Should;

    [TestFixture]
    public class When_processing_a_message_for_which_no_matching_handler_can_be_found : TestFixtureBase
    {
        private IServiceLocator _container;

        private AnotherCommandHandler anotherCommandHandler = new AnotherCommandHandler();

        private TestCommand suppliedMessage;
        private bool actualResult;

        protected override void EstablishContext()
        {
            _container = MockRepository.GenerateStub<IServiceLocator>();
            _container.Stub(c => c.ResolveAll(Arg<Type>.Is.Anything))
                .Return(null)
                .WhenCalled(x =>
                {
                    var type = (Type)x.Arguments[0];
                    if (type.IsAssignableFrom(typeof(AnotherCommandHandler)))
                    {
                        x.ReturnValue = (Array)new object[] { anotherCommandHandler };
                    }
                    else
                    {
                        x.ReturnValue = new AnotherCommandHandler[0];
                    }
                });
        }

        protected override void ExecuteBehavior()
        {
            suppliedMessage = new TestCommand();

            var messageProcessor = new MessageProcessor(_container);

            actualResult = messageProcessor.Process(suppliedMessage);
        }

        [Test]
        public virtual void Should_indicate_message_was_not_handled()
        {
            actualResult.ShouldBeFalse();
        }
    }

    [TestFixture]
    public class When_processing_a_message_with_a_multiple_matching_handlers : TestFixtureBase
    {
        private IServiceLocator _container;

        private TestCommandHandler testCommandHandler = new TestCommandHandler();
        private AnotherTestCommandHandler anotherTestCommandHandler = new AnotherTestCommandHandler();
        private AnotherCommandHandler anotherCommandHandler = new AnotherCommandHandler();

        private TestCommand suppliedMessage;
        private bool actualResult;

        protected override void EstablishContext()
        {
            _container = MockRepository.GenerateStub<IServiceLocator>();
            _container.Stub(c => c.ResolveAll(Arg<Type>.Is.Anything))
                .Return(null)
                .WhenCalled(x =>
                {
                    var type = (Type)x.Arguments[0];
                    if (type.IsAssignableFrom(typeof(AnotherCommandHandler)))
                    {
                        x.ReturnValue = (Array)new object[] { anotherCommandHandler };
                    }
                    else if (type.IsAssignableFrom(typeof(TestCommandHandler)))
                    {
                        x.ReturnValue = (Array)new object[] { testCommandHandler, anotherTestCommandHandler };
                    }
                    else
                    {
                        x.ReturnValue = null;
                    }
                });
        }

        protected override void ExecuteBehavior()
        {
            suppliedMessage = new TestCommand();

            var messageProcessor = new MessageProcessor(_container);
            actualResult = messageProcessor.Process(suppliedMessage);
        }

        [Test]
        public virtual void Should_handle_the_message()
        {
            suppliedMessage.Handled.ShouldBeTrue();
        }

        [Test]
        public virtual void Should_indicate_that_message_was_handled()
        {
            actualResult.ShouldBeTrue();
        }

        [Test]
        public virtual void Should_handle_the_message_with_each_matching_handler()
        {
            suppliedMessage.AllHandlers.ShouldContain(typeof(TestCommandHandler).Name);
            suppliedMessage.AllHandlers.ShouldContain(typeof(AnotherTestCommandHandler).Name);
        }

        [Test]
        public virtual void Should_not_be_processed_by_non_matching_handler()
        {
            suppliedMessage.AllHandlers.ShouldNotContain(typeof(AnotherCommandHandler).Name);
        }
    }

    [TestFixture]
    public class When_processing_a_message_with_multiple_matching_handlers_where_an_exception_occurs : TestFixtureBase
    {
        private IServiceLocator _container;

        private TestCommandHandler testCommandHandler = new TestCommandHandler();
        private AnotherTestCommandHandler anotherTestCommandHandler = new AnotherTestCommandHandler();
        private TestCommandHandlerThatThrowsAnException testCommandHandlerThatThrowsAnException = new TestCommandHandlerThatThrowsAnException();

        private TestCommand suppliedMessage;
        private bool actualResult;
        private Exception actualException;

        protected override void EstablishContext()
        {
            _container = MockRepository.GenerateStub<IServiceLocator>();
            _container.Stub(c => c.ResolveAll(Arg<Type>.Is.Anything))
                .Return(null)
                .WhenCalled(x =>
                {
                    var type = (Type)x.Arguments[0];
                    if (type.IsAssignableFrom(typeof(TestCommandHandler)))
                    {
                        x.ReturnValue = (Array)new object[] { testCommandHandler, testCommandHandlerThatThrowsAnException, anotherTestCommandHandler };
                    }
                    else
                    {
                        x.ReturnValue = null;
                    }
                });
        }

        protected override void ExecuteBehavior()
        {
            suppliedMessage = new TestCommand();

            var messageProcessor = new MessageProcessor(_container);
            try
            {
                actualResult = messageProcessor.Process(suppliedMessage);
            }
            catch (Exception ex)
            {
                actualException = ex;
            }
        }

        [Test]
        public virtual void Should_handle_the_message()
        {
            suppliedMessage.Handled.ShouldBeTrue();
        }

        [Test]
        public virtual void Should_throw_an_aggregate_exception()
        {
            actualException.ShouldBeExceptionType<AggregateException>();
        }

        [Test]
        public virtual void Should_handle_the_message_with_all_matching_handler()
        {
            suppliedMessage.AllHandlers.ShouldContain(typeof(TestCommandHandler).Name);
            suppliedMessage.AllHandlers.ShouldContain(typeof(TestCommandHandlerThatThrowsAnException).Name);
            suppliedMessage.AllHandlers.ShouldContain(typeof(AnotherTestCommandHandler).Name);
        }
    }
}
