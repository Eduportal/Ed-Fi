// ****************************************************************************
//  Copyright (C) 2015 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System;
using System.Collections.Generic;
using EdFi.Common.Security.Authorization;
using EdFi.Common.Security.Claims;
using EdFi.Ods.Common;
using EdFi.Ods.Common.Repositories;
using NHibernate;
using NHibernate.Context;

namespace EdFi.Ods.Security.Authorization.Repositories
{
    /// <summary>
    /// Authorizes calls to the "GetEntitiesBySpecification" repository method.
    /// </summary>
    /// <typeparam name="TEntity">The <see cref="Type"/> of entity being queried.</typeparam>
    public class GetEntitiesBySpecificationAuthorizationDecorator<TEntity>
        : RepositoryOperationAuthorizationDecoratorBase<TEntity>, IGetEntitiesBySpecification<TEntity>
        where TEntity : class, IHasIdentifier, IDateVersionedEntity
    {
        private readonly IGetEntitiesBySpecification<TEntity> _next;
        private readonly ISessionFactory _sessionFactory;
        private readonly INHibernateFilterApplicator _filterApplicator;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetEntityByKeyAuthorizationDecorator{T}"/> class.
        /// </summary>
        /// <param name="next">The decorated instance for which authorization is being performed.</param>
        /// <param name="sessionFactory">The NHibernate session factory used to manage session (database connection) context.</param>
        /// <param name="filterApplicator">Applies authorization-related filters for the entity on the current NHiberate session.</param>
        /// <param name="authorizationContextProvider">Provides access to the authorization context, such as the resource and action.</param>
        /// <param name="authorizationProvider">The component capable of authorizing the request, given necessary context.</param>
        public GetEntitiesBySpecificationAuthorizationDecorator(
            IGetEntitiesBySpecification<TEntity> next,
            ISessionFactory sessionFactory,
            INHibernateFilterApplicator filterApplicator,
            IAuthorizationContextProvider authorizationContextProvider,
            IEdFiAuthorizationProvider authorizationProvider)
            : base(authorizationContextProvider, authorizationProvider)
        {
            _next = next;
            _sessionFactory = sessionFactory;
            _filterApplicator = filterApplicator;
        }

        /// <summary>
        /// Authorizes a call to get multiple entities using an open query specification.
        /// </summary>
        /// <param name="specification">An entity instance that has all the primary key properties assigned with values.</param>
        /// <param name="queryParameters">The additional query parameter to apply the results (e.g. paging, sorting).</param>
        /// <returns>A list of matching resources, or an empty result.</returns>
        public IList<TEntity> GetBySpecification(TEntity specification, IQueryParameters queryParameters)
        {
            // Use the authorization subsystem to set filtering context
            var filterBuilder = new ParameterizedFilterBuilder();
            ApplyAuthorizationFilters<TEntity>(filterBuilder);
            
            // Ensure we've bound an NHibernate session to the current context
            var shouldReleaseBind = EnsureSessionContextBinding();

            try
            {
                // Apply authorization filtering to the entity for the current session
                _filterApplicator.ApplyFilters(Session, filterBuilder.Value);

                // Pass call through to the repository operation implementation to execute the query
                return _next.GetBySpecification(specification, queryParameters);
            }
            finally
            {
                if (shouldReleaseBind)
                    CurrentSessionContext.Unbind(_sessionFactory);
            }
        }

        // ----------------------------------------------------------------------------
        // These methods were copied from the NHibernateOperationBase class.
        // ----------------------------------------------------------------------------
        private bool EnsureSessionContextBinding()
        {
            if (CurrentSessionContext.HasBind(_sessionFactory))
                return false;

            CurrentSessionContext.Bind(_sessionFactory.OpenSession());
            
            return true;
        }

        private ISession Session
        {
            get { return _sessionFactory.GetCurrentSession(); }
        }
        // ----------------------------------------------------------------------------
    }
}