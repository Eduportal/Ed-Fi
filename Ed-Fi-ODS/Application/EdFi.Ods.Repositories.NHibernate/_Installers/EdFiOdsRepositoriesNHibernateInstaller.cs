using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using EdFi.Ods.Common.Repositories;
using EdFi.Ods.Entities.Common.Caching;
using EdFi.Ods.Entities.Repositories.NHibernate.Architecture;
using EdFi.Ods.Security.Authorization.Repositories;

namespace EdFi.Ods.Entities.Repositories.NHibernate._Installers
{
    public class EdFiOdsRepositoriesNHibernateInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container
                .Register(Component
                .For(typeof (IRepository<>))
                .ImplementedBy(typeof(NHibernateRepository<>)));

            container.Register(Component
                .For(typeof(ICreateEntity<>))
                .ImplementedBy(typeof(CreateEntity<>)));

            container.Register(Component
                .For(typeof(IDeleteEntityById<>))
                .ImplementedBy(typeof(DeleteEntityById<>)));

            container.Register(Component
                .For(typeof(IDeleteEntityByKey<>))
                .ImplementedBy(typeof(DeleteEntityByKey<>)));

            container.Register(Component
                .For(typeof(IGetEntitiesByIds<>))
                .ImplementedBy(typeof(GetEntitiesByIds<>)));

            container.Register(Component
                .For(typeof(IGetEntitiesBySpecification<>))
                .ImplementedBy(typeof(GetEntitiesBySpecification<>)));

            container.Register(Component
                .For(typeof(IGetEntityById<>))
                .ImplementedBy(typeof(GetEntityById<>)));

            container.Register(Component
                .For(typeof(IGetEntityByKey<>))
                .ImplementedBy(typeof(GetEntityByKey<>)));

            container.Register(Component
                .For(typeof(IUpdateEntity<>))
                .ImplementedBy(typeof(UpdateEntity<>)));

            container.Register(Component
                .For(typeof(IUpsertEntity<>))
                .ImplementedBy(typeof(UpsertEntity<>)));

            container.Register(Component
                .For<ITypeLookupProvider>()
                .ImplementedBy<TypeLookupProvider>()
                .LifestyleSingleton());

            container.Register(Component
                .For<IDescriptorLookupProvider>()
                .ImplementedBy<DescriptorLookupProvider>()
                .LifestyleSingleton());


            container.Register(Component
                .For<INHibernateFilterApplicator>()   
                .ImplementedBy<NHibernateFilterApplicator>());
        }
    }
}