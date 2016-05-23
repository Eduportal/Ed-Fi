using System;
using Castle.MicroKernel.Registration;
using EdFi.Common.Configuration;
using EdFi.Common.InversionOfControl;
using EdFi.Common.Messaging;
using EdFi.Common._Installers;
using EdFi.Messaging.MSMQ._Installers;
using NUnit.Framework;

namespace EdFi.MSMQServiceAcceptanceTests
{
    [TestFixture]
    public class When_Sending_Messages_To_MultiMessage_Process
    {
        [Test]
        public void Should_Not_Take_Longer_Than_thirty_Seconds()
        {
            //register IoC
            var container = new WindsorContainerEx();

            container.AddSupportForEmptyCollections();

            container.Install(new EdFiCommonInstaller());
            container.Install(new EdFiMessagingInstaller());
            container.Install(new MSMQInstaller());

            container.Register(Component.For<IConfigValueProvider>().ImplementedBy<AppConfigValueProvider>());
            
            //get ICommandSender
            var sender = container.Resolve<ICommandSender>();

            //Create and send start message
            var startMsg = new StartTest {TestRunNumber = Guid.NewGuid(), TestBeginTime = DateTime.UtcNow};

            sender.Send(startMsg);
        }
    }
}