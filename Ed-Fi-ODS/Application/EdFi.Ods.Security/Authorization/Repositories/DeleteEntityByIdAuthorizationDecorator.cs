using System;
using EdFi.Ods.Common;
using EdFi.Ods.Common.Repositories;

namespace EdFi.Ods.Security.Authorization.Repositories
{
    /// <summary>
    /// Authorizes calls to the "DeleteById" repository method.
    /// </summary>
    /// <typeparam name="T">The Type of entity being queried.</typeparam>
    public class DeleteEntityByIdAuthorizationDecorator<T> : IDeleteEntityById<T>
        where T : IHasIdentifier, IDateVersionedEntity
    {
        private readonly IDeleteEntityById<T> _next;

        /// <summary>
        /// Initializes a new instance of <see cref="DeleteEntityByIdAuthorizationDecorator{T}"/>.
        /// </summary>
        /// <param name="next">The decorated instance for which authorization is being performed.</param>
        public DeleteEntityByIdAuthorizationDecorator(IDeleteEntityById<T> next)
        {
            _next = next;
        }

        /// <summary>
        /// Authorizes a call to delete an entity by its unique identifier.
        /// </summary>
        /// <returns>The specified entity if found; otherwise null.</returns>
        public void DeleteById(Guid id, string etag)
        {
            // We do not need to perform authorization here because the context is already set to "Delete"
            // and DeleteEntityById will call the GetById method which triggers the authorization.
            _next.DeleteById(id, etag);
        }
    }
}