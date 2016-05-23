using System.Collections.Generic;
using System.Diagnostics;
using EdFi.Common.Messaging;

namespace UnitTests.EdFi.Ods.Messaging
{
    public class TestCommand : ICommand
    {
        public TestCommand()
        {
            AllHandlers = new List<string>();
        }

        public string Text { get; set; }
        public bool Handled { get; set; }

        private string _handledBy;

        public string HandledBy
        {
            get { return _handledBy; }
            set 
            { 
                _handledBy = value;
                AllHandlers.Add(value);
            }
        }

        public int Order { get; set; }
        public List<string> AllHandlers { get; set; }
    }

    public class AnotherCommand : ICommand
    {
        public string Text { get; set; }
        public bool Handled { get; set; }
        public string HandledBy { get; set; }
        public int Order { get; set; }
    }

    public class TestCommandHandler : IMessageHandler<TestCommand>
    {
        private int messageNumber = 1;

        public TestCommandHandler()
        {
            HandledMessages = new List<TestCommand>();
        }

        public List<TestCommand> HandledMessages { get; set; }
        
        public void Handle(TestCommand message)
        {
            Trace.WriteLine("Handling message with text: " + message.Text);

            message.Handled = true;
            message.HandledBy = this.GetType().Name;
            message.Order = messageNumber++;

            HandledMessages.Add(message);
        }

        public void Reset()
        {
            HandledMessages.Clear();
            messageNumber = 1;
        }
    }

    public class AnotherCommandHandler : IMessageHandler<AnotherCommand>
    {
        private int messageNumber = 1;

        public AnotherCommandHandler()
        {
            HandledMessages = new List<AnotherCommand>();
        }

        public void Handle(AnotherCommand message)
        {
            message.Handled = true;
            message.HandledBy = this.GetType().Name;
            message.Order = messageNumber++;

            HandledMessages.Add(message);
        }

        public List<AnotherCommand> HandledMessages { get; set; } 
    }

    public class AnotherTestCommandHandler : IMessageHandler<TestCommand>
    {
        private int messageNumber = 1;

        public AnotherTestCommandHandler()
        {
            HandledMessages = new List<TestCommand>();
        }

        public void Handle(TestCommand message)
        {
            message.Handled = true;
            message.HandledBy = this.GetType().Name;
            message.Order = messageNumber++;

            HandledMessages.Add(message);
        }

        public List<TestCommand> HandledMessages { get; set; } 
    }

    public class TestCommandHandlerThatThrowsAnException : IMessageHandler<TestCommand>
    {
        private int messageNumber = 1;

        public TestCommandHandlerThatThrowsAnException()
        {
            HandledMessages = new List<TestCommand>();
        }

        public void Handle(TestCommand message)
        {
            Trace.WriteLine("Handling message with text: " + message.Text);

            message.Handled = true;
            message.HandledBy = this.GetType().Name;
            message.Order = messageNumber++;

            HandledMessages.Add(message);

            throw new System.NotSupportedException("This handler throws an exception.");
        }

        public List<TestCommand> HandledMessages { get; set; } 
    }
}