using System.Reflection;
using System.Security.Claims;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using EdFi.Common.InversionOfControl;
using EdFi.Common.Messaging;
using EdFi.Common.Security.Claims;
using EdFi.Common.Services;
using EdFi.Common._Installers;
using EdFi.Messaging.MSMQ._Installers;
using EdFi.Ods.Api.Data._Installers;
using EdFi.Ods.BulkLoad.Core;
using EdFi.Ods.BulkLoad.Core.Controllers;
using EdFi.Ods.BulkLoad.Core.ServiceHosts;
using EdFi.Ods.BulkLoad.Core._Installers;
using EdFi.Ods.BulkLoad.Services._Installers;
using EdFi.Ods.CodeGen.XmlShredding._Installers;
using EdFi.Ods.Common;
using EdFi.Ods.Common.ExceptionHandling._Installers;
using EdFi.Ods.Common._Installers;
using EdFi.Ods.Entities.Common._Installers;
using EdFi.Ods.Entities.Repositories.NHibernate._Installers;
using EdFi.Ods.Messaging.BulkLoadCommands;
using EdFi.Ods.Messaging._Installers;
using EdFi.Ods.Pipelines._Installers;
using EdFi.Ods.Security.Claims;
using EdFi.Ods.Security._Installers;
using EdFi.Ods.XmlShredding._Installers;
using EdFi.Services.Windows;
using log4net;
using log4net.Config;
using SandboxConfigurationSpecificInstaller = EdFi.Ods.BulkLoad.Services.Windows.BulkWorker._Installers.Sandbox.ConfigurationSpecificInstaller;
using SharedInstanceConfigurationSpecificInstaller = EdFi.Ods.BulkLoad.Services.Windows.BulkWorker._Installers.SharedInstance.ConfigurationSpecificInstaller;
using EdFi.Ods.Api.Data.Repositories.BulkOperations;
using EdFi.Ods.Api.Pipelines._Installers;

namespace EdFi.Ods.BulkLoad.Services.Windows.BulkWorker
{
    using EdFi.Ods.Security.Metadata.Contexts;

    public class Program
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(Program));

        static void Main(string[] args)
        {
            XmlConfigurator.Configure();
            logger.Debug("Enter Main.");
            logger.Debug("Configure IOC.");
            //Build a container to support the necessary objects for this service
            var container = ConfigureIoC();

            //Register the specific service
            container.Register(Component.For<IHostedService>().ImplementedBy<WindowsBulkWorkerServiceHost>());

            logger.Debug("Initiate Service.");
            //Get the host
            var host = WindowsServiceInitiator.InitiateService(container.Resolve<IHostedService>);

            logger.Debug("Run.");
            //Run it
            host.Run();
            logger.Debug("Exit Main.");
        }

        private static IWindsorContainer ConfigureIoC()
        {
            var containerFactory = new InversionOfControlContainerFactory();

            // Get the container
            var container = containerFactory.CreateContainer(c =>
            {
                c.AddFacility<TypedFactoryFacility>(); 
                c.AddFacility<DatabaseConnectionStringProviderFacility>();
            }, Assembly.GetExecutingAssembly());

            container.AddSupportForEmptyCollections();

            //container.Register(Component.For<SecurityContext>().ImplementedBy<SecurityContext>());
            container.Install(new SecurityComponentsInstaller());

            // Register components for establishing claims from context
            container.Register(Component.For<IClaimsIdentityProvider>().ImplementedBy<ClaimsIdentityProvider>());
            ClaimsPrincipal.ClaimsPrincipalSelector =
                () => EdFiClaimsPrincipalSelector.GetClaimsPrincipal(container.Resolve<IClaimsIdentityProvider>());

            container.Register(Component.For<IMessageHandler<StartOperationCommand>>()
                                        .ImplementedBy<BulkLoadMaster>()
                                        .LifestyleSingleton());

            container.Register(Component.For<IPersistUploadFiles>().ImplementedBy<PersistUploadFilesLocally>());
            container.Register(Component.For<ICreateFilePathForUploadFile>().ImplementedBy<CreateFilePathForUploadFileLocally>());
            container.Register(Component.For<IStreamFileChunksToWriter>().ImplementedBy<SqlStreamFileChunksToWriter>());

            container.Register(Component.For<IValidateAndSourceFiles>().ImplementedBy<ValidateAndSourceFiles>());


            container.Install(new BulkLoadCoreInstaller());
            container.Install(new EdFiOdsCommonInstaller());
            container.Install(new EdFiOdsCommonExceptionHandlingInstaller());
            container.Install(new EdFiOdsEntitiesCommonInstaller());
            container.Install(new EdFiMessagingInstaller());
            container.Install(new EdFiOdsMessagingInstaller());
            container.Install(new EdFiOdsPipelinesInstaller());
            container.Install(new EdFiOdsRepositoriesNHibernateInstaller());
            container.Install(new EdFiOdsXmlShreddingInstaller());
            container.Install(new EdFiOdsWebApiDataEntityFrameworkInstaller());
            container.Install(new XmlShreddingDatabaseInstaller());
            container.Install(new EdFiOdsXmlShreddingCodeGenInstaller());

            container.Install(new MSMQReceiveOnlyInstaller());

            return container;
        }
    }
}
