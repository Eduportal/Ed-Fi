using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Castle.Core.Internal;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using EdFi.Common.Extensions;
using EdFi.Common.InversionOfControl;
using EdFi.Common.Messaging;
using EdFi.Common.Security;
using EdFi.Common.Security.Authorization;
using EdFi.Common.Security.Claims;
using EdFi.Ods.Admin.Models;
using EdFi.Ods.Api.Pipelines.Factories;
using EdFi.Ods.Common.Repositories;
using EdFi.Ods.Security.Authorization;
using EdFi.Ods.Security.Authorization.Pipeline;
using EdFi.Ods.Security.Authorization.Repositories;
using EdFi.Ods.Security.AuthorizationStrategies;
using EdFi.Ods.Security.AuthorizationStrategies.NHibernateConfiguration;
using EdFi.Ods.Security.AuthorizationStrategies.Relationships;
using EdFi.Ods.Security.Messaging;
using EdFi.Ods.Security.Metadata.Repositories;
using EdFi.Ods.Security.Profiles;

namespace EdFi.Ods.Security._Installers
{
    public class SecurityComponentsInstaller : RegistrationMethodsInstallerBase
    {
        private readonly IDictionary<Type, Type> decoratorRegistrations = new Dictionary<Type, Type>
        {
            {typeof (IGetEntityByKey<>), typeof (GetEntityByKeyAuthorizationDecorator<>)},
            {typeof (IGetEntitiesBySpecification<>), typeof (GetEntitiesBySpecificationAuthorizationDecorator<>)},
            {typeof (IGetEntityById<>), typeof (GetEntityByIdAuthorizationDecorator<>)},
            {typeof (IGetEntitiesByIds<>), typeof (GetEntitiesByIdsAuthorizationDecorator<>)},
            {typeof (ICreateEntity<>), typeof (CreateEntityAuthorizationDecorator<>)},
            {typeof (IDeleteEntityById<>), typeof (DeleteEntityByIdAuthorizationDecorator<>)},
            {typeof (IUpdateEntity<>), typeof (UpdateEntityAuthorizationDecorator<>)},
            {typeof (IUpsertEntity<>), typeof (UpsertEntityAuthorizationDecorator<>)}
        };

        protected virtual void RegisterIAuthorizationSegmentsToFiltersConverter(IWindsorContainer container)
        {
            container.Register(Component
                .For<IAuthorizationSegmentsToFiltersConverter>()
                .ImplementedBy<AuthorizationSegmentsToFiltersConverter>());
        }

        protected virtual void RegisterIEducationOrganizationHierarchyProvider(IWindsorContainer container)
        {
            container.Register(Component
                .For<IEducationOrganizationHierarchyProvider>()
                .ImplementedBy<EducationOrganizationHierarchyProvider>());
        }

        protected virtual void RegisterComponentsUsingLegacyMechanism(IWindsorContainer container)
        {
            // Register the decorators
            foreach (var registration in decoratorRegistrations)
            {
                container.Register(Component
                    .For(registration.Key)
                    .ImplementedBy(registration.Value)
                    .IsDefault());
            }

            container.Register(Component
                .For<ISecurityRepository>()
                .ImplementedBy<SecurityRepository>()
                .LifestyleTransient()
                .OnlyNewServices());

            container.Register(Component
                .For<UsersContext>()
                .ImplementedBy<UsersContext>());

            container.Register(Component
                .For<Security.Metadata.Contexts.SecurityContext>()
                .ImplementedBy<Security.Metadata.Contexts.SecurityContext>());

            container.Register(Component
                .For<IAdminProfileNamesPublisher>()
                .ImplementedBy<AdminProfileNamesPublisher>());

            container.Register(Component
                .For<IAuthorizationContextProvider>()
                .ImplementedBy<AuthorizationContextProvider>());

            // Register pipeline step provider decorators
            container.Register(Component
                .For<IGetPipelineStepsProvider>()
                .ImplementedBy<AuthorizationContextGetPipelineStepsProviderDecorator>()
                .IsDefault());

            container.Register(Component
                .For<IGetByKeyPipelineStepsProvider>()
                .ImplementedBy<AuthorizationContextGetByKeyPipelineStepsProviderDecorator>()
                .IsDefault());

            container.Register(Component
                .For<IGetBySpecificationPipelineStepsProvider>()
                .ImplementedBy<AuthorizationContextGetBySpecificationPipelineStepsProviderDecorator>()
                .IsDefault());

            container.Register(Component
                .For<IPutPipelineStepsProvider>()
                .ImplementedBy<AuthorizationContextPutPipelineStepsProviderDecorator>()
                .IsDefault());

            container.Register(Component
                .For<IDeletePipelineStepsProvider>()
                .ImplementedBy<AuthorizationContextDeletePipelineStepsProviderDecorator>()
                .IsDefault());

            // Register authorization context pipeline steps
            container.Register(
                Component.For(typeof(SetAuthorizationContextForGet<,,,>))
                    .ImplementedBy(typeof(SetAuthorizationContextForGet<,,,>)),
                Component.For(typeof(SetAuthorizationContextForPut<,,,>))
                    .ImplementedBy(typeof(SetAuthorizationContextForPut<,,,>)),
                Component.For(typeof(SetAuthorizationContextForDelete<,,,>))
                    .ImplementedBy(typeof(SetAuthorizationContextForDelete<,,,>)),
                Component.For(typeof(SetAuthorizationContextForPost<,,,>))
                    .ImplementedBy(typeof(SetAuthorizationContextForPost<,,,>))
                );

            // Register EdOrg Cache components
            container.Register(Component
                .For<IEducationOrganizationCache>()
                .ImplementedBy<EducationOrganizationCache>());

            container.Register(Component
                .For<IEducationOrganizationCacheDataProvider>()
                .ImplementedBy<EducationOrganizationCacheDataProvider>());

            container.Register(Component
                .For<IAssessmentMetadataNamespaceProvider>()
                .ImplementedBy<AssessmentMetadataNamespaceProvider>());

            // -----------------------------------------------
            //   Register authorization subsystem components
            // -----------------------------------------------
            container.Register(Component
                .For<IEdFiAuthorizationProvider>()
                .ImplementedBy<EdFiAuthorizationProvider>());

            container.Register(Component
                .For<IAuthorizationSegmentsVerifier>()
                .ImplementedBy<EdFiOdsAuthorizationSegmentsVerifier>());

            container.Register(Component
                .For<IResourceAuthorizationMetadataProvider>()
                .ImplementedBy<ResourceAuthorizationMetadataProvider>());

            container.Register(Component.For<IInboundEnvelopeDataProcessor>()
                .ImplementedBy<SecurityInboundEnvelopeDataProcessor>());

            container.Register(Component.For<IOutboundEnvelopeDataProcessor>()
                .ImplementedBy<SecurityOutboundEnvelopeDataProcessor>());
        }

        protected virtual void RegisterIRelationshipsAuthorizationContextDataProviderFactory(IWindsorContainer container)
        {
            // Register authorization context data provider factory
            container.Register(Component
                .For(typeof(IRelationshipsAuthorizationContextDataProviderFactory<>).MakeGenericType(GetRelationshipBasedAuthorizationStrategyContextDataType()))
                .ImplementedBy(typeof(RelationshipsAuthorizationContextDataProviderFactory<>).MakeGenericType(GetRelationshipBasedAuthorizationStrategyContextDataType()))
                .DependsOn(new { serviceLocator = container }));
        }

        protected virtual void RegisterIRelationshipsAuthorizationContextDataProviders(IWindsorContainer container)
        {
            // Register all context data providers
            var relationshipContextDataProviderTypes =
                (from t in typeof(Marker_EdFi_Ods_Security).Assembly.GetTypes()
                 where !t.IsAbstract
                       && typeof(IRelationshipsAuthorizationContextDataProvider<,>).IsAssignableFromGeneric(t)
                 select t)
                .ToList();

            foreach (var providerType in relationshipContextDataProviderTypes)
            {
                var partiallyClosedInterfaceType =
                    providerType.GetInterfaces()
                        .SingleOrDefault(
                            i => i.Name == typeof(IRelationshipsAuthorizationContextDataProvider<,>).Name);

                var modelType = partiallyClosedInterfaceType.GetGenericArguments()[0];

                var closedInterfaceType =
                    typeof(IRelationshipsAuthorizationContextDataProvider<,>).MakeGenericType(
                        modelType,
                        GetRelationshipBasedAuthorizationStrategyContextDataType());

                var closedServiceType =
                    providerType.MakeGenericType(GetRelationshipBasedAuthorizationStrategyContextDataType());

                container.Register(
                    Component
                        .For(closedInterfaceType)
                        .ImplementedBy(closedServiceType));
            }

            #region Commented Out, Non-working - Possible more elegant registration approach

            //container.Register(Classes
            //    .FromAssemblyContaining<Marker_EdFi_Ods_Security>()
            //    .BasedOn(typeof(IRelationshipsAuthorizationContextDataProvider<,>))
            //    .Configure(
            //        r =>
            //        {
            //            var genericStrategy =
            //                new PassthroughGenericStrategy();

            //            r.ExtendedProperties(
            //                Property.ForKey(Constants.GenericImplementationMatchingStrategy)
            //                        .Eq((object)genericStrategy));
            //        })
            //    .WithServiceBase());

            #endregion
        }

        protected virtual void RegisterIConcreteEducationOrganizationIdAuthorizationContextDataTransformer(IWindsorContainer container)
        {
            var relationshipContextDataType = GetRelationshipBasedAuthorizationStrategyContextDataType();

            var transformerInterfaceType =
                typeof(IConcreteEducationOrganizationIdAuthorizationContextDataTransformer<>).MakeGenericType(
                    relationshipContextDataType);

            var transformerServiceType =
                typeof(ConcreteEducationOrganizationIdAuthorizationContextDataTransformer<>).MakeGenericType(
                    relationshipContextDataType);

            container.Register(Component
                .For(transformerInterfaceType)
                .ImplementedBy(transformerServiceType));
        }

        protected virtual void RegisterAuthorizationStrategies(IWindsorContainer container)
        {
            // TODO: GKM - For EdOrg extensibility, an implementation assembly will need to be scanned here as well.
            var assembly = Assembly.GetAssembly(typeof(Marker_EdFi_Ods_Security));
            
            // Register array of IEdFiAuthorizationStrategy
            var strategyTypes = (from t in assembly.GetTypes()
                             where typeof(IEdFiAuthorizationStrategy).IsAssignableFrom(t) &&
                                 !t.IsAbstract
                             select t);

            foreach (var strategyType in strategyTypes)
            {
                // Handle relationship-based authorization strategies explicitly
                if (typeof(RelationshipsAuthorizationStrategyBase<>).IsAssignableFromGeneric(strategyType))
                {
                    var genericStrategy = new RelationshipBasedAuthorizationGenericStrategy(
                        GetRelationshipBasedAuthorizationStrategyContextDataType());

                    container.Register(
                        Component
                            .For<IEdFiAuthorizationStrategy>()
                            .ImplementedBy(strategyType, genericStrategy));
                }
                else
                {
                    container.Register(
                        Component
                            .For<IEdFiAuthorizationStrategy>()
                            .ImplementedBy(strategyType));
                }
            }
        }

        /// <summary>
        /// Gets the context data type for the relationship-based authorizations strategies, providing an extensibility point for additional education organization types.
        /// </summary>
        protected virtual Type GetRelationshipBasedAuthorizationStrategyContextDataType()
        {
            return typeof(RelationshipsAuthorizationContextData);
        }

        protected virtual void RegisterINHibernateFilterConfigurators(IWindsorContainer container)
        {
            // TODO: GKM - For EdOrg extensibility, an implementation assembly will need to be scanned here as well.
            container.Register(Classes
                .FromThisAssembly()
                .BasedOn<INHibernateFilterConfigurator>()
                .WithServiceFromInterface());
        }
    }
}
