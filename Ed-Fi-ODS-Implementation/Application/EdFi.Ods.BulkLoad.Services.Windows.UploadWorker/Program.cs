using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using EdFi.Common.Caching;
using EdFi.Common.Configuration;
using EdFi.Common.Context;
using EdFi.Common.InversionOfControl;
using EdFi.Common.Messaging;
using EdFi.Common.Services;
using EdFi.Common._Installers;
using EdFi.Messaging.MSMQ._Installers;
using EdFi.Ods.Api.Data._Installers;
using EdFi.Ods.BulkLoad.Core.Controllers;
using EdFi.Ods.BulkLoad.Core.ServiceHosts;
using EdFi.Ods.BulkLoad.Core._Installers;
using EdFi.Ods.Common._Installers;
using EdFi.Ods.Messaging.BulkLoadCommands;
using EdFi.Ods.Messaging._Installers;
using EdFi.Ods.Security._Installers;
using EdFi.Services.Windows;
using log4net;
using log4net.Config;

namespace EdFi.Ods.BulkLoad.Services.Windows.UploadWorker
{
    public class Program
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(Program));

        static void Main(string[] args)
        {
            XmlConfigurator.Configure();
            logger.Debug("Entered Main, setting up windsor.");
            var container = new WindsorContainerEx();

            container.AddSupportForEmptyCollections();
            container.AddFacility<TypedFactoryFacility>();
            container.AddFacility<DatabaseConnectionStringProviderFacility>();

            container.Install(new SecurityComponentsInstaller());

            // ==================================
            //   BEGIN Temporary Implementation
            // ----------------------------------
            // TODO: Fully integrate ConfigurationSpecificInstaller here
            container.Register(Component
                .For<IContextStorage>()
                .ImplementedBy<CallContextStorage>());

            container.Register(Component
                .For<ICacheProvider>()
                .ImplementedBy<MemoryCacheProvider>());
            // ----------------------------------
            //   END Temporary Implementation
            // ==================================

            container.Register(Component.For<IMessageHandler<CommitUploadCommand>>()
                                        .ImplementedBy<UploadFileCommitHandler>()
                                        .LifestyleSingleton());

            container.Install(new EdFiMessagingInstaller());
            container.Install(new EdFiOdsMessagingInstaller());
            container.Install(new EdFiOdsCommonInstaller());
            container.Install(new EdFiOdsWebApiDataEntityFrameworkInstaller());
            BulkLoadCoreInstaller.RegisterUploadFileValidator(container);
            container.Install(new MSMQInstaller());
            container.Install(new LocalFileSystemManagementInstaller());

            container.Register(Component.For<IConfigValueProvider>().ImplementedBy<AppConfigValueProvider>());
            
            //Register service with the container
            container.Register(Component.For<IHostedService>().ImplementedBy<WindowsCommitUploadServiceHost>());

            logger.Debug("Running IHostedService.");
            //Create the host for the service
            var host = WindowsServiceInitiator.InitiateService(container.Resolve<IHostedService>);

            //Run the host
            host.Run();
        }
    }
}
