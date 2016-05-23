using Castle.MicroKernel.Registration;
using EdFi.Common.Configuration;
using EdFi.Common.InversionOfControl;
using EdFi.Common.Messaging;
using EdFi.Common.Services;
using EdFi.Common._Installers;
using EdFi.Messaging.MSMQ._Installers;
using EdFi.MSMQServiceAcceptanceTests;
using EdFi.Services.Windows;

namespace EdFi.MSMQServiceTests.ListenAndFinish
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = new WindsorContainerEx();
            
            container.AddSupportForEmptyCollections();

            container.Install(new EdFiCommonInstaller());
            container.Install(new EdFiMessagingInstaller());
            container.Install(new MSMQInstaller());

            container.Register(Component.For<IHostedService>().ImplementedBy<ListenAndFinishHostedService>());
            container.Register(Component.For<IMessageHandler<FinishTest>>().ImplementedBy<FinishTestHandler>());
            container.Register(Component.For<IConfigValueProvider>().ImplementedBy<AppConfigValueProvider>());  //MSMQ reads data from the config for queue endpoints and configuration
 
            var host = WindowsServiceInitiator.InitiateService(container.Resolve<IHostedService>);
            host.Run();
        }
    }
}
