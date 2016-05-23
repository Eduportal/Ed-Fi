using System.Web.Http;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using EdFi.Common.Configuration;
using EdFi.Ods.Admin.Models;
using EdFi.Ods.Admin.Services;
using System.Web.Mvc;
using EdFi.Ods.Security.Metadata.Contexts;
using EdFi.Ods.Security.Metadata.Repositories;

namespace EdFi.Ods.Admin.Web.App_Start
{
    public class AdminInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            //NOTE: This should really be switch to instance per web request, but that requires some additional Castle modules to be registered in the web.config.
            container.Register(Component.For<UsersContext>()
                .ImplementedBy<UsersContext>()
                .LifestylePerWebRequest());

            container.Register(Component.For<IClientAppRepo, IUpdateUser>()
                .ImplementedBy<ClientAppRepo>()
                .LifestyleTransient());

            container.Register(Component.For<SecurityContext>()
                .ImplementedBy<SecurityContext>()
                .LifestylePerWebRequest());

            container.Register(Component.For<ISecurityRepository>()
                .ImplementedBy<SecurityRepository>()
                .LifestyleTransient());

            container.Register(Component.For<IClientCreator>()
                                        .ImplementedBy<ClientCreator>()
                                        .LifestyleTransient());

            container.Register(Component.For<IDefaultApplicationCreator>()
                                        .ImplementedBy<DefaultApplicationCreator>()
                                        .LifestyleTransient());

            container.Register(Component.For<IDatabaseTemplateLeaQuery>()
                                        .ImplementedBy<DatabaseTemplateLeaQuery>()
                                        .LifestyleTransient());

            container.Register(Component.For<IRouteService>()
                .ImplementedBy<RouteService>());

            // TODO: GKM - Convert to using IoC container factory and remove this, or don't (and remove this comment)
            container.Register(Component.For<IConfigValueProvider>()
                .ImplementedBy<AppConfigValueProvider>());
            
            container.Register(Component.For<IEmailService>()
                .ImplementedBy<EmailService>());

            container.Register(Component.For<IPasswordService>()
                .ImplementedBy<PasswordService>()
                .LifestyleTransient());

            container.Register(Component.For<IUserAccountManager>()
                .ImplementedBy<UserAccountManager>()
                .LifestyleTransient());

            container.Register(Component.For<ISecurityService>()
                .ImplementedBy<SecurityService>()
                .LifestyleTransient());

            container.Register(Component.For<ISandboxProvisioner>()
                .ImplementedBy(SandboxProvisionerTypeCalculator.GetSandboxProvisionerTypeForNewSandboxes())
                .LifestyleTransient());

            // register controllers in this assembly
            container.Register(Classes.FromAssemblyNamed("EdFi.Ods.Admin")
                .BasedOn<IController>()
                .LifestyleTransient());

            // register ApiControllers in this assembly
            container.Register(Classes.FromAssemblyNamed("EdFi.Ods.Admin")
                .BasedOn<ApiController>()
                .LifestyleTransient());
        }
    }
}