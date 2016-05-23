using System;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using EdFi.Common.Identity;
using EdFi.Common.Security.Claims;
using EdFi.Common._Installers;
using EdFi.Identity.Models;
using EdFi.Messaging.MSMQ._Installers;
using EdFi.Ods.Admin.Models;
using EdFi.Ods.Api.Common;
using EdFi.Ods.Api.Common.Authorization;
using EdFi.Ods.Api.Data.EventStore;
using EdFi.Ods.Api.Data.Repositories.BulkOperations;
using EdFi.Ods.Api.Data.Repositories.BulkOperations.Exceptions;
using EdFi.Ods.Api.Data._Installers;
using EdFi.Ods.Api.Pipelines._Installers;
using EdFi.Ods.Api.Services.Authorization;
using EdFi.Ods.Api.Services.Providers;
using EdFi.Ods.Common.ExceptionHandling._Installers;
using EdFi.Ods.Common._Installers;
using EdFi.Ods.Entities.Common._Installers;
using EdFi.Ods.Entities.Repositories.NHibernate._Installers;
using EdFi.Ods.Messaging._Installers;
using EdFi.Ods.Security.Claims;
using EdFi.Ods.Security._Installers;
using EduId.WebApi;
using FluentValidation;

namespace EdFi.Ods.Api._Installers
{
    public class WebApiInstaller : IWindsorInstaller
    {
        protected virtual void RegisterSecurityComponents(IWindsorContainer container)
        {
            // TODO: GKM - Profiles - temporary workaround for lack of composability
            container.Install(new SecurityComponentsInstaller());
        }

        protected virtual void RegisterNHibernateComponents(IWindsorContainer container)
        {
            // TODO: GKM - Profiles - temporary workaround for lack of composability
            container.Install(new EdFiOdsRepositoriesNHibernateInstaller());
        }

        public void Install(IWindsorContainer container, Castle.MicroKernel.SubSystems.Configuration.IConfigurationStore store)
        {
            // TODO: GKM - Profiles - temporary workaround for lack of composability
            RegisterSecurityComponents(container);

            container.Install(new EdFiCommonInstaller());
            container.Install(new EdFiOdsCommonInstaller());
            container.Install(new EdFiOdsCommonExceptionHandlingInstaller());

            // TODO: GKM - Profiles - temporary workaround for lack of composability
            RegisterNHibernateComponents(container);

            container.Install(new EdFiOdsEntitiesCommonInstaller());

            RegisterEduIdDependencies(container);

            // Register specific implementations
            // -----------------------------------

            // Validators
            container.Register(
                Component.For<IOAuthTokenValidator>().ImplementedBy<OAuthTokenValidator>(),
                Component.For<IOAuthTokenValidator>().ImplementedBy<CachingOAuthTokenValidatorDecorator>().IsDefault());

            // Register all pipeline steps
            // -------------------------------
            container.Install(new EdFiOdsPipelinesInstaller());

            container.Register(Component.For<IClientAppRepo, IUpdateUser>()
                .ImplementedBy<ClientAppRepo>()
                .LifestyleTransient());

            // Controllers
            container.Register(
                Classes.FromAssemblyContaining<Marker_EdFi_Ods_Api>()
                       .BasedOn<ApiController>()
                       .LifestyleTransient());

            // Swagger
            container.Register(
                Classes.FromAssemblyContaining<Swagger.SwaggerController>()
                       .BasedOn<ApiController>()
                       .LifestyleTransient());

            container.Register(Component
                               .For<IEventLogRepository>()
                               .ImplementedBy<EventLogRepository>());

            container.Register(Component
                                 .For<ICreateBulkOperationAndGetById>()
                                 .ImplementedBy<CreateBulkOperationAndGetById>());

            container.Register(Component
                     .For<IBulkOperationsFileChunkCreator, IVarbinaryWriter>()
                     .ImplementedBy<BulkOperationsFileChunkWriter>());
            container.Register(Component
                     .For<IBulkOperationsExceptionsGetByUploadFileId>()
                     .ImplementedBy<BulkOperationsExceptionsGetByUploadFileId>());

            //Support for EdFi_Bulk database EF objects
            container.Install(new EdFiOdsWebApiDataEntityFrameworkInstaller());

            container.Register(Component
                                  .For<IValidator<BulkOperationCreateRequest>>()
                                  .ImplementedBy<BulkOperationCreateValidator>());

            container.Register(Component
                                  .For<ICreateBulkOperationCommandFactory>()
                                  .ImplementedBy<CreateBulkOperationCommandFactory>());

            container.Register(Component.For<IVerifyUploads>().ImplementedBy<UploadValidator>());

            container.Register(Component
                .For<Func<string, MultipartStreamProvider>>()
                .Instance(id => new CustomMultipartStreamProvider(id, container.Resolve<IVarbinaryWriter>())));

            container.Install(new EdFiMessagingInstaller());
            container.Install(new EdFiOdsMessagingInstaller());
            container.Install(new AllDeployedHandlersInstaller());

            container.Install(new MSMQSendOnlyInstaller());


            // TODO: GKM - IMPORTANT: Review these registrations from Muhammad's pull request
            container.Register(Component.For<ISandboxProvisioner>().ImplementedBy<SqlSandboxProvisioner>());
            container.Register(Component.For<IClaimsIdentityProvider>().ImplementedBy<ClaimsIdentityProvider>());

            ClaimsPrincipal.ClaimsPrincipalSelector =
                () => EdFiClaimsPrincipalSelector.GetClaimsPrincipal(container.Resolve<IClaimsIdentityProvider>());

            container.Register(Component.For<IProfileResourcesProvider>().ImplementedBy<ProfileResourcesProvider>());
        }

        protected virtual void RegisterEduIdDependencies(IWindsorContainer container)
        {
            // Controllers
            container.Register(
                Classes.FromAssemblyContaining<Marker_EdFi_Identity>()
                       .BasedOn<ApiController>()
                       .LifestyleTransient());

            // Unique Identity System Integration
            container.Register(Component
                .For<IUniqueIdentity>()
                .ImplementedBy<EdFi.Identity.Models.UnimplementedUniqueIdentity>());
            
            // Sample Unique Identity class registration
            //container.Register(Component
            //    .For<IUniqueIdentity>()
            //    .ImplementedBy<EdFi.Identity.Models.SampleUniqueIdentity>());

            container.Register(Component
                .For<IValidator<IdentityResource>>()
                .ImplementedBy<IdentityResourceCreateValidator>());

            container.Register(Component
                .For<IIdentityMapper>()
                .ImplementedBy<IdentityMapper>());
        }
    }
}