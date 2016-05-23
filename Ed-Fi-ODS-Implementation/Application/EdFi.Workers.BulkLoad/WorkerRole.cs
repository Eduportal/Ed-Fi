using System.Configuration;
using System.Security.Claims;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using EdFi.Common.Messaging;
using EdFi.Common._Installers;
using EdFi.Ods.Api.Data.Repositories.BulkOperations;
using EdFi.Ods.BulkLoad.Core;
using EdFi.Ods.BulkLoad.Core.Controllers;
using EdFi.Ods.BulkLoad.Core._Installers;
using EdFi.Ods.BulkLoad.Services._Installers;
using EdFi.Ods.Common.ExceptionHandling._Installers;
using EdFi.Ods.Common._Installers;
using EdFi.Ods.Entities.Common._Installers;
using EdFi.Ods.Entities.Repositories.NHibernate._Installers;
using EdFi.Ods.Messaging.BulkLoadCommands;
using EdFi.Ods.Messaging._Installers;
using EdFi.Ods.Pipelines._Installers;
using EdFi.Ods.Security.Claims;
using EdFi.Ods.Security._Installers;
using EdFi.Ods.Api.Data._Installers;
using EdFi.Ods.CodeGen.XmlShredding._Installers;
using EdFi.Ods.WindowsAzure.Services;
using EdFi.Ods.WindowsAzure._Installers;
using EdFi.Ods.XmlShredding._Installers;
using EdFi.Common.Security.Claims;
using EdFi.Ods.Api.Pipelines._Installers;
using SandboxConfigurationSpecificInstaller = EdFi.Workers.BulkLoad._Installers.Sandbox.ConfigurationSpecificInstaller;
using SharedInstanceConfigurationSpecificInstaller = EdFi.Workers.BulkLoad._Installers.SharedInstance.ConfigurationSpecificInstaller;

namespace EdFi.Workers.BulkLoad
{
    public class WorkerRole : WorkerRoleBase<BulkLoadServiceHost>
    {
        //private IHostedService _hostedService;

        protected override void ConfigureIoC(IWindsorContainer container)
        {
            container.Install(new SecurityComponentsInstaller());

            // Register components for establishing claims from context
            container.Register(Component.For<IClaimsIdentityProvider>().ImplementedBy<ClaimsIdentityProvider>());
            ClaimsPrincipal.ClaimsPrincipalSelector = () => EdFiClaimsPrincipalSelector.GetClaimsPrincipal(container.Resolve<IClaimsIdentityProvider>());
            
            // ==================================
            //   BEGIN Temporary Implementation
            // ----------------------------------
            // TODO: Fully integrate ConfigurationSpecificInstaller here
            if (ConfigurationManager.AppSettings["IoC.Configuration"] == "Sandbox")
                container.Install(new SandboxConfigurationSpecificInstaller());
            else
                container.Install(new SharedInstanceConfigurationSpecificInstaller());
            // ----------------------------------
            //   END Temporary Implementation
            // ==================================

            container.Register(Component.For<IMessageHandler<StartOperationCommand>>()
                                        .ImplementedBy<BulkLoadMaster>()
                                        .LifestyleSingleton());

            container.Register(Component.For<IPersistUploadFiles>().ImplementedBy<PersistUploadFilesLocally>());
            container.Register(Component.For<ICreateFilePathForUploadFile>().ImplementedBy<CreateFilePathForUploadFileAzureOnly>());
            container.Register(Component.For<IStreamFileChunksToWriter>().ImplementedBy<SqlStreamFileChunksToWriter>());
            

            container.Install(new AzureMessageReceivingInstaller());
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
        }
    }
}