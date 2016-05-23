// ****************************************************************************
//  Copyright (C) 2015 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using Castle.MicroKernel.Registration;
using Castle.Windsor;
using EdFi.Common.InversionOfControl;
using EdFi.Ods.Api.Common;
using EdFi.Ods.Common;
using EdFi.Ods.Entities.Common.Caching;
using EdFi.Ods.Entities.Common.IdentityValueMappers;
using EdFi.Ods.Entities.Common.Providers;
using EdFi.Ods.Entities.Common.UniqueIdIntegration;
using EdFi.Ods.Entities.Common.Validation;

namespace EdFi.Ods.Entities.Common._Installers
{
    public class EdFiOdsEntitiesCommonInstaller : RegistrationMethodsInstallerBase
    {
        protected virtual void RegisterIEdFiOdsInstanceIdentificationProvider(IWindsorContainer container)
        {
            container.Register(
                Component
                    .For<IEdFiOdsInstanceIdentificationProvider>()
                    .ImplementedBy<EdFiOdsInstanceIdentificationProvider>());
        }

        // ReSharper disable once InconsistentNaming
        protected virtual void RegisterIETagProvider(IWindsorContainer container)
        {
            container.Register(
                Component
                    .For<IETagProvider>()
                    .ImplementedBy<ETagProvider>());
        }

        protected virtual void RegisterUniqueIdToUsiValueMapper(IWindsorContainer container)
        {
            container.Register(
                Component
                    .For<IUniqueIdToUsiValueMapper>()
                    .ImplementedBy<EdFiOdsBasedUsiValueMapper>());
        }

        protected virtual void RegisterIPersonUniqueIdToUsiCache(IWindsorContainer container)
        {
            container.Register(
                Component
                    .For<IPersonUniqueIdToUsiCache>()
                    .ImplementedBy<PersonUniqueIdToUsiCache>());

            PersonUniqueIdToUsiCache.GetCache = container.Resolve<IPersonUniqueIdToUsiCache>;
        }

        protected virtual void RegisterIUniqueIdPersonMappingFactory(IWindsorContainer container)
        {
            container.Register(
                Component
                    .For<IUniqueIdPersonMappingFactory>()
                    .ImplementedBy<UniqueIdPersonMappingFactory>());
        }

        protected virtual void RegisterITypeAndDescriptorCache(IWindsorContainer container)
        {
            container.Register(Component.For<ITypesAndDescriptorsCache>().ImplementedBy<TypesAndDescriptorsCache>().LifestyleSingleton());
            TypesAndDescriptorsCache.GetCache = container.Resolve<ITypesAndDescriptorsCache>;
        }

        protected virtual void RegisterValidators(IWindsorContainer container)
        {
            container.Register(
                Component
                    .For<IObjectValidator>()
                    .ImplementedBy<DataAnnotationsObjectValidator>());

            container.Register(
                Component
                    .For<IObjectValidator>()
                    .ImplementedBy<FluentValidationPutPostRequestObjectValidator>());

            // TODO: For UniqueId integration - These validators should be moved to a separate installer for UniqueId integration.
            //container.Register(
            //    Component
            //        .For<IObjectValidator>()
            //        .ImplementedBy<UniqueIdNotChangedObjectValidator>());
            //
            //container.Register(
            //    Component
            //        .For<IObjectValidator>()
            //        .ImplementedBy<EnsureUniqueIdAlreadyExistsObjectValidator>());
        }
    }
}