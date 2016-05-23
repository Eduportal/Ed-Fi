using System;
using EdFi.Common.InversionOfControl;

namespace EdFi.Ods.Security.AuthorizationStrategies.Relationships
{
    /// <summary>
    /// Defines a method for obtaining an <see cref="IRelationshipsAuthorizationContextDataProvider{TContextData}"/>
    /// for the specified <i>concrete</i> model type.
    /// </summary>
    /// <remarks>This interface is intended to be used as a non-generic invocation mechanism from within the
    /// <see cref="IRelationshipsAuthorizationContextDataProvider{TContextData}"/> for invoking the non-generic implementation
    /// after obtaining it from the <see cref="IServiceLocator"/>.  The crux of the issue is that the context
    /// data providers are implemented and registered based on the model abstraction (e.g. IStudent) rather than 
    /// either of the concrete models (e.g. Student (resource) or Student (entity)).</remarks>
    public interface IRelationshipsAuthorizationContextDataProviderFactory<out TContextData>
        where TContextData : RelationshipsAuthorizationContextData, new()
    {
        /// <summary>
        /// Get the <see cref="IRelationshipsAuthorizationContextDataProvider{TContextData}"/> implementation appropriate for the
        /// concrete model type specified (resolving to the IoC registration for the model's interface 
        /// abstraction [e.g. IStudent]).
        /// </summary>
        /// <returns>The non-generic context data provider for the requested entity type.</returns>
        IRelationshipsAuthorizationContextDataProvider<TContextData> GetProvider(Type entityType);
    }
}