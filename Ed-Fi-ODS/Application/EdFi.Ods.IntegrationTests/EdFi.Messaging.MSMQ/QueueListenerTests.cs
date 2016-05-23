using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Threading;
using System.Threading.Tasks;
using Castle.Core.Logging;
using EdFi.Common.Messaging;
using EdFi.Messaging.MSMQ;
using EdFi.Ods.Tests._Bases;
using NUnit.Framework;
using Rhino.Mocks;
using Should;

namespace EdFi.Ods.IntegrationTests.EdFi.Messaging.MSMQ.QueueListenerTests
{

        [TestFixture]
        public class When_Stopped_And_No_Messages_In_Flight : TestBase
        {
            protected static string queuePathWithoutMessage = @".\PRIVATE$\";
            private string queuePath = string.Format(@"{0}{1}", queuePathWithoutMessage, typeof (SucessfulMessage).Name);

            [TestFixtureSetUp]
            public void Arrange()
            {
                if (!MessageQueue.Exists(queuePath)) MessageQueue.Create(queuePath);
                using (var queue = new MessageQueue(queuePath))
                {
                    queue.Purge();
                }
            }

            [Test]
            public void Should_Not_Process_Any_New_Messages()
            {
                //Arrange some more things
                var queueLocator = Stub<IQueueLocator>();
                queueLocator.Expect(l => l.GetQueue<SucessfulMessage>())
                    .IgnoreArguments()
                    .Return(new MessageQueue(queuePath));
                var errorSender = Stub<IErrorSender<SucessfulMessage>>();
                var sut = new QueueListener<SucessfulMessage>(queueLocator, new ManagerFactory<SucessfulMessage>(errorSender, new MsmqMessageFactory()));

                //we'll set the 'handle' action to mark the message's Processed property to true
                sut.StartListening(m => m.Envelope.Message.Processed = true );
                sut.StopListening();

                //Act
                using (var queue = new MessageQueue(queuePath))
                {
                    var newMessage = new Message(new SucessfulMessage());
                    queue.Send(newMessage);
                    var messagesInQueueArray = queue.GetAllMessages();

                    //Assert
                    messagesInQueueArray.ShouldNotBeNull();
                    messagesInQueueArray.Count().ShouldEqual(1);
                }
            }

            [TestFixtureTearDown]
            public void Cleanup()
            {
                if(MessageQueue.Exists(queuePath)) MessageQueue.Delete(queuePath);
            }
        }

        public class MessageInQueue : ICommand
        {
            public bool Processed { get; set; }
        }


        public class SucessfulMessage : ICommand
        {
            public bool Processed { get; set; }
        }

        public class FailedMessage : ICommand
        {
            public bool Processed { get; set; }
        }
    }
