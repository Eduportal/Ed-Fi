using System;
using System.Threading;
using Castle.MicroKernel.Registration;
using EdFi.Common.Configuration;
using EdFi.Common.InversionOfControl;
using EdFi.Common.Messaging;
using EdFi.Common._Installers;
using EdFi.Messaging.MSMQ._Installers;
using EdFi.MSMQServiceAcceptanceTests;

namespace EdFi.MSMQServices.SendOnly
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = new WindsorContainerEx();

            container.AddSupportForEmptyCollections();

            container.Install(new EdFiCommonInstaller());
            container.Install(new EdFiMessagingInstaller());
            container.Install(new MSMQSendOnlyInstaller());
            container.Register(Component.For<IConfigValueProvider>().ImplementedBy<AppConfigValueProvider>());

            var sender = container.Resolve<ICommandSender>();

            while (true)
            {
                var message = new StartTest {TestRunNumber = Guid.NewGuid(), TestBeginTime = DateTime.UtcNow};
                Console.WriteLine(@"Sending a message for test run {0} . . . ", message.TestRunNumber);
                sender.Send(message);
                Console.WriteLine(". . . message was successfully sent.  Waiting five seconds before sending another.");
                Thread.Sleep(TimeSpan.FromSeconds(5));
            }
        }
    }
}
