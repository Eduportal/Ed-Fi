using System;
using EdFi.Ods.Common;
using EdFi.Ods.Common.Repositories;
using EdFi.Common.Security.Claims;

namespace EdFi.Ods.Security.Authorization.Repositories
{
    /// <summary>
    /// Authorizes calls to the "GetById" repository method.
    /// </summary>
    /// <typeparam name="T">The <see cref="Type"/> of entity being queried.</typeparam>
    public class GetEntityByIdAuthorizationDecorator<T> 
        : RepositoryOperationAuthorizationDecoratorBase<T>, IGetEntityById<T> 
        where T : class, IHasIdentifier, IDateVersionedEntity
    {
        private readonly IGetEntityById<T> _next;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetEntityByIdAuthorizationDecorator{T}"/> class.
        /// </summary>
        /// <param name="next">The decorated instance for which authorization is being performed.</param>
        /// <param name="authorizationContextProvider">Provides access to the authorization context, such as the resource and action.</param>
        /// <param name="authorizationProvider">The component capable of authorizing the request, given necessary context.</param>
        public GetEntityByIdAuthorizationDecorator(IGetEntityById<T> next,
            IAuthorizationContextProvider authorizationContextProvider,
            IEdFiAuthorizationProvider authorizationProvider)
            : base(authorizationContextProvider, authorizationProvider)
        {
            _next = next;
        }

        /// <summary>
        /// Authorizes a call to get a single entity by its unique identifier.
        /// </summary>
        /// <param name="id">The value of the unique identifier.</param>
        /// <returns>The specified entity if found; otherwise null.</returns>
        /// <remarks>The <see cref="DeleteEntityByIdAuthorizationDecorator{T}"/> depends on this 
        /// authorization invocation, and runs with a authorization context of "Delete".</remarks>
        public T GetById(Guid id)
        {
            // Pass the call through to the decorated repository method to get the entity
            // to then determine if it can be returned to the caller.
            var entity = _next.GetById(id);

            // If found, authorize access to the located entity
            if (entity != null)
                AuthorizeSingleItem(entity);

            return entity;
        }
    }
}
