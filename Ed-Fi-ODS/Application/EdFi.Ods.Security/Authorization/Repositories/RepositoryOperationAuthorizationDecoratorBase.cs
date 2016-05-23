// ****************************************************************************
//  Copyright (C) 2015 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System;
using System.Collections.Generic;
using System.Security.Claims;
using EdFi.Common.Security.Authorization;
using EdFi.Common.Security.Claims;

namespace EdFi.Ods.Security.Authorization.Repositories
{
    /// <summary>
    /// Provides an abstract base class for authorization decorators to use for invoking 
    /// authorization.
    /// </summary>
    /// <typeparam name="T">The <see cref="Type"/> of the entity/request.</typeparam>
    public abstract class RepositoryOperationAuthorizationDecoratorBase<T>
    {
        private readonly IAuthorizationContextProvider _authorizationContextProvider;
        private readonly IEdFiAuthorizationProvider _authorizationProvider;

        protected RepositoryOperationAuthorizationDecoratorBase(
            IAuthorizationContextProvider authorizationContextProvider,
            IEdFiAuthorizationProvider authorizationProvider)
        {
            _authorizationContextProvider = authorizationContextProvider;
            _authorizationProvider = authorizationProvider;
        }

        /// <summary>
        /// Invokes authorization of the request using the resource/action currently in context.
        /// </summary>
        /// <param name="entity">The request/entity being authorized.</param>
        protected void AuthorizeSingleItem(T entity)
        {
            var action = _authorizationContextProvider.GetAction();

            AuthorizeSingleItem(entity, action);
        }

        /// <summary>
        /// Invokes authorization of the request using the resource currently in context but wit 
        /// an overide action (e.g. for converting the "Upsert" action to either "Create" or "Update").
        /// </summary>
        /// <param name="entity">The request/entity being authorized.</param>
        /// <param name="actionUri">The action being performed with the request/entity.</param>
        protected void AuthorizeSingleItem(T entity, string actionUri)
        {
            // Make sure Authorization context is present before proceeding
            _authorizationContextProvider.VerifyAuthorizationContextExists();

            // Build the AuthorizationContext
            EdFiAuthorizationContext authorizationContext = new EdFiAuthorizationContext(
                ClaimsPrincipal.Current,
                _authorizationContextProvider.GetResource(),
                actionUri,
                entity);

            // Authorize the call
            _authorizationProvider.AuthorizeSingleItem(authorizationContext);
        }

        protected void ApplyAuthorizationFilters<TEntity>(ParameterizedFilterBuilder filterBuilder)
        {
            // Make sure Authorization context is present before proceeding
            _authorizationContextProvider.VerifyAuthorizationContextExists();

            // Build the AuthorizationContext
            EdFiAuthorizationContext authorizationContext = new EdFiAuthorizationContext(
                ClaimsPrincipal.Current,
                _authorizationContextProvider.GetResource(),
                _authorizationContextProvider.GetAction(),
                typeof(TEntity));

            // Authorize the call
            _authorizationProvider.ApplyAuthorizationFilters(authorizationContext, filterBuilder);
        }
    }
}