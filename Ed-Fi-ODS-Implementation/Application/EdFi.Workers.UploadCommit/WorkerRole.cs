using Castle.MicroKernel.Registration;
using Castle.Windsor;
using EdFi.Common.Caching;
using EdFi.Common.Context;
using EdFi.Common.Messaging;
using EdFi.Common._Installers;
using EdFi.Ods.BulkLoad.Core.Controllers;
using EdFi.Ods.BulkLoad.Core._Installers;
using EdFi.Ods.Common._Installers;
using EdFi.Ods.Messaging.BulkLoadCommands;
using EdFi.Ods.Messaging._Installers;
using EdFi.Ods.Security._Installers;
using EdFi.Ods.Api.Data._Installers;
using EdFi.Ods.WindowsAzure.Services;
using EdFi.Ods.WindowsAzure._Installers;

namespace EdFi.Workers.UploadCommit
{
    public class WorkerRole : WorkerRoleBase<CommitUploadServiceHost>
    {
        protected override void ConfigureIoC(IWindsorContainer container)
        {
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
            container.Install(new AzureMessageSendingInstaller());
            container.Install(new AzureMessageReceivingInstaller());
            container.Install(new EdFiOdsWebApiDataEntityFrameworkInstaller());
            BulkLoadCoreInstaller.RegisterUploadFileValidator(container);
        }
    }
}