using Castle.MicroKernel.Registration;
using EdFi.Common.Configuration;
using EdFi.Common.InversionOfControl;
using EdFi.Common.Messaging;
using EdFi.Common.Services;
using EdFi.Common._Installers;
using EdFi.Messaging.MSMQ._Installers;
using EdFi.MSMQServiceAcceptanceTests;
using EdFi.Services.Windows;

namespace EdFi.MSMQServices.ListenAndSend
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
            container.Register(Component.For<IHostedService>().ImplementedBy<ListenAndSendHostedService>());
            container.Register(Component.For<IMessageHandler<StartTest>>().ImplementedBy<StartTestHandler>());
            container.Register(Component.For<IConfigValueProvider>().ImplementedBy<AppConfigValueProvider>());

            var host = WindowsServiceInitiator.InitiateService(container.Resolve<IHostedService>);
            host.Run();
        }
    }
}
