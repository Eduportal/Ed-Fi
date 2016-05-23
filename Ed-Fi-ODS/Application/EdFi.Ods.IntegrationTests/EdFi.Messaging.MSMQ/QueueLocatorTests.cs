using System;
using System.Messaging;
using EdFi.Common.Configuration;
using EdFi.Common.Messaging;
using EdFi.Messaging.MSMQ;
using EdFi.Ods.Tests._Bases;
using NUnit.Framework;
using Rhino.Mocks;
using Should;

namespace EdFi.Ods.IntegrationTests.EdFi.Messaging.MSMQ.QueueLocatorTests
{
    [TestFixture]

        public class When_Queue_Does_Not_Exist_And_Create_Authorized : TestBase
        {
            private const string queuePath = ".\\PRIVATE$\\CreateMeQueue";

            [TestFixtureSetUp]
            public void Setup()
            {
                //remove queue if it exists
                if(MessageQueue.Exists(queuePath)) MessageQueue.Delete(queuePath);
            }

            [Test]
            public void And_Endpoint_Is_Local_System_Should_Create_Queue_Based_On_Given_Message_Type()
            {
                var nameProvider = Stub<IQueueNameProvider>();
                nameProvider.Expect(n => n.GetQueueNameFor<CreateMeQueue>()).Return("CreateMeQueue");
                var config = Stub<IConfigValueProvider>();
                config.Expect(n => n.GetValue("QueueAutoCreate")).Return("1");
                config.Expect(n => n.GetValue("CreateMeQueueMessageEndpoint")).Return(".");
                var sut = new QueueLocator(nameProvider, config);
                using (var result = sut.GetQueue(new Envelope<CreateMeQueue>()))
                {
                    result.QueueName.ShouldContain("CreateMeQueue");
                }
            }

            [Test]
            [ExpectedException(ExpectedException = typeof(SystemException), UserMessage =  "Expected to throw a SystemException because the endpoint was remote (cannot create remote queues).")]
            public void And_Endpoint_Is_Remote_System_Should_Throw_SystemException()
            {
                var nameProvider = Stub<IQueueNameProvider>();
                nameProvider.Expect(n => n.GetQueueNameFor<CreateMeQueue>()).Return("CreateMeQueue");
                var config = Stub<IConfigValueProvider>();
                config.Expect(n => n.GetValue("QueueAutoCreate")).Return("1");
                config.Expect(n => n.GetValue("CreateMeQueueMessageEndpoint")).Return("AnotherServer.edfi.org");
                var sut = new QueueLocator(nameProvider, config);
                using (var result = sut.GetQueue(new Envelope<CreateMeQueue>()))
                {
                    Assert.Fail("The queue was created (don't know how) or something else went wrong.");
                }
            }

            [TestFixtureTearDown]
            public void Cleanup()
            {
                if(MessageQueue.Exists(queuePath)) MessageQueue.Delete(queuePath);
            }
        }

        [TestFixture]
        public class When_Queue_Does_Not_Exist_And_Create_Not_Authorized : TestBase
        {
            private const string queuePath = ".\\Private$\\NonExistentQueue";

            [TestFixtureSetUp]
            public void Setup()
            {
                if(MessageQueue.Exists(queuePath)) MessageQueue.Delete(queuePath);
            }

            [Test]
            [ExpectedException(ExpectedException = typeof(SystemException), UserMessage = "Expected to throw a SystemException because the queue did not exist")]
            public void And_Endpoint_Is_Local_System_Should_Throw_Exception()
            {
                var nameProvider = Stub<IQueueNameProvider>();
                nameProvider.Expect(n => n.GetQueueNameFor<NonExistentQueue>()).Return("NonExistentQueue");
                var config = Stub<IConfigValueProvider>();
                config.Expect(n => n.GetValue("QueueAutoCreate")).Return("0");
                config.Expect(n => n.GetValue("NonExistentQueueMessageEndpoint")).Return("Localhost");
                var sut = new QueueLocator(nameProvider, config);
                using (var result = sut.GetQueue(new Envelope<NonExistentQueue>()))
                {
                    Assert.Fail("The queue was created or something else went wrong.");
                }
            }

            [TestFixtureTearDown]
            public void Cleanup()
            {
                if(MessageQueue.Exists(queuePath)) MessageQueue.Delete(queuePath);
            }
        }

    public class CreateMeQueue : ICommand
        {
        }

    public class NonExistentQueue : ICommand
        {
    }
}