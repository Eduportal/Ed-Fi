using System;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using EdFi.Common.InversionOfControl;
using EdFi.Ods.Api.Data.Contexts;

namespace EdFi.Ods.Api.Data._Installers
{
    public class EdFiOdsWebApiDataEntityFrameworkInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component
                                   .For<IBulkOperationDbContext, IEventLogDbContext>()
                                   .ImplementedBy<BulkOperationDbContext>()
                                   .LifestyleTransient());
            container.Register(Component
                                   .For<Func<IBulkOperationDbContext>>()
                                   .UsingFactoryMethod<Func<IBulkOperationDbContext>>(() => container.Resolve<IBulkOperationDbContext>));
            container.Register(Component
                                   .For(typeof(IDbExecutor<>))
                                   .ImplementedBy(typeof(DbExecutor<>)));
        }
    }
}