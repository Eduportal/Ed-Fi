// ****************************************************************************
//  Copyright (C) 2015 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using EdFi.Ods.Common;
using EdFi.Ods.Common.Repositories;

namespace EdFi.Ods.Security.Authorization.Repositories
{
    /// <summary>
    /// Authorizes calls to the "Upsert" repository method.
    /// </summary>
    /// <typeparam name="T">The Type of entity being upserted.</typeparam>
    public class UpsertEntityAuthorizationDecorator<T>
        : IUpsertEntity<T>
        where T : IHasIdentifier, IDateVersionedEntity
    {
        private readonly IUpsertEntity<T> _next;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpsertEntityAuthorizationDecorator{T}"/> class.
        /// </summary>
        /// <param name="next">The decorated instance for which authorization is being performed.</param>
        public UpsertEntityAuthorizationDecorator(IUpsertEntity<T> next)
        {
            _next = next;
        }

        /// <summary>
        /// Authorizes a call to update an entity.
        /// </summary>
        /// <returns>The specified entity if found; otherwise null.</returns>
        public T Upsert(T entity, bool enforceOptimisticLock, out bool isModified, out bool isCreated)
        {
            // We do not need to perform authorization because the UpsertEntity will call other 
            // methods (Create or Update) which will trigger the authorization.
            return _next.Upsert(entity, enforceOptimisticLock, out isModified, out isCreated);
        }
    }
}