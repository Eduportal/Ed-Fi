// ****************************************************************************
//  Copyright (C) 2015 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using Castle.MicroKernel.Registration;
using Castle.Windsor;
using EdFi.Common.InversionOfControl;
using EdFi.Ods.Api.Pipelines.Factories;
using EdFi.Ods.Common;
using EdFi.Ods.Common._Installers;
using EdFi.Ods.Common._Installers.ComponentNaming;
using EdFi.Ods.Entities.Common.Caching;
using EdFi.Ods.Entities.Common.IdentityValueMappers;
using EdFi.Ods.Entities.Common.Validation;
using EdFi.Ods.Pipelines;
using EdFi.Ods.Pipelines._Installers;
using EdFi.Ods.Security.Authorization.Pipeline;

namespace EdFi.Ods.Api._Installers
{
    public class UniqueIdIntegrationInstaller<TUniqueIdValueMapper> : RegistrationMethodsInstallerBase
        where TUniqueIdValueMapper : IUniqueIdToIdValueMapper
    {
        protected virtual void RegisterUniqueIdValueMappers(IWindsorContainer container)
        {
            // Does the supplied identity value mapper type support the Usi interface as well?
            // (i.e. is it the optimized single server instance implementation of both mappers?)
            if (typeof(IUniqueIdToUsiValueMapper).IsAssignableFrom(typeof(TUniqueIdValueMapper)))
            {
                // Register the type as both the Id and Usi value mapper, overriding any other registrations.
                container.Register(
                    Component
                        .For<IUniqueIdToIdValueMapper, IUniqueIdToUsiValueMapper>()
                        .ImplementedBy<TUniqueIdValueMapper>()
                        .IsDefault()); // This will cause it to override the out-of-the-box USI value mapper
            }
            else
            {
                // Only register the type as the Id value mapper
                container.Register(
                    Component
                        .For<IUniqueIdToIdValueMapper>()
                        .ImplementedBy(typeof(TUniqueIdValueMapper)));
            }
        }

        protected virtual void RegisterIPersonUniqueIdToIdCache(IWindsorContainer container)
        {
            container.Register(Component
                .For<IPersonUniqueIdToIdCache>()
                .ImplementedBy<PersonUniqueIdToIdCache>());
        }

        protected virtual void RegisterPipelineStepsProviderDecorators(IWindsorContainer container)
        {
            // Register pipeline step provider decorators
            container.Register(
                Component
                    .For<IPutPipelineStepsProvider>()
                    .ImplementedBy<UniqueIdIntegrationPutPipelineStepsProviderDecorator>()
                    .IsDefault());
        }

        protected virtual void RegisterIPipelineSteps(IWindsorContainer container)
        {
            container.RegisterPipelineStepsForAssembliesMarkedBy(
                typeof(Marker_EdFi_Ods_Pipelines), 
                "UniqueIdIntegration");
        }

        protected virtual void RegisterIObjectValidators(IWindsorContainer container)
        {
            // Register a validator that prevents UniqueIds from being changed
            container.Register(
                Component
                    .For<IObjectValidator>()
                    .ImplementedBy<UniqueIdNotChangedObjectValidator>());

            // Register a validator that ensures that 
            container.Register(
                Component
                    .For<IObjectValidator>()
                    .ImplementedBy<EnsureUniqueIdAlreadyExistsObjectValidator>());
        }

        protected virtual void RegisterUniqueIdIntegratioDatabaseConnectionStringProvider(IWindsorContainer container)
        {
            //DatabaseConnectionStringProviderContainerRegistrationHelper
            CommonDatabaseConnectionStringProvidersInstallerBase
                .RegisterNamedConnectionStringProvider(
                    container, 
                    UniqueIdDatabases.UniqueIdIntegration,
                    UniqueIdDatabases.UniqueIdIntegration.GetConnectionStringName());
        }
    }
}