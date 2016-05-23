// ****************************************************************************
//  Copyright (C) 2014 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System;
using System.Collections.Generic;
using EdFi.Ods.Common;
using EdFi.Ods.Common.Repositories;

namespace EdFi.Ods.Security.Authorization.Repositories
{
    // TODO: GKM - Remove this class completely.  It does not perform any authorization.

    /// <summary>
    /// Authorizes calls to the "GetByIds" repository method.
    /// </summary>
    /// <typeparam name="T">The Type of entity being queried.</typeparam>
    public class GetEntitiesByIdsAuthorizationDecorator<T> : IGetEntitiesByIds<T>
        where T : class, IHasIdentifier, IDateVersionedEntity
    {
        private readonly IGetEntitiesByIds<T> _next;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetEntitiesByIdsAuthorizationDecorator{T}"/> class.
        /// </summary>
        /// <param name="next">The decorated instance for which authorization is being performed.</param>
        public GetEntitiesByIdsAuthorizationDecorator(IGetEntitiesByIds<T> next)
        {
            _next = next;
        }

        /// <summary>
        /// Authorizes a call to get a single entity by its unique identifier.
        /// </summary>
        /// <param name="ids">The values of the unique identifiers to be retrieved.</param>
        /// <returns>The specified entity if found; otherwise null.</returns>
        public IList<T> GetByIds(IList<Guid> ids)
        {
            // This method is still used by GetById and GetBySpecification, but should never be exposed 
            // to the callers since no authorization is performed.
            return _next.GetByIds(ids);
        }
    }
}