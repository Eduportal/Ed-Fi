// ****************************************************************************
//  Copyright (C) 2015 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System;
using System.Collections.ObjectModel;
using System.Security.Claims;

namespace EdFi.Common.Security.Claims
{
    /// <summary>
    /// Provides an Ed-Fi-specific authorization context for making authorization decisions.
    /// </summary>
    public class EdFiAuthorizationContext : AuthorizationContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EdFiAuthorizationContext"/> class using the principal, resource, action and Ed-Fi authorization context data.
        /// </summary>
        /// <param name="principal">The ClaimsPrincipal containing the claims.</param>
        /// <param name="resource">The resource being authorized.</param>
        /// <param name="action">The action being taken on the resource.</param>
        /// <param name="data">An object containing the data available for authorization which implements one of the 
        /// model interfaces (e.g. IStudent).</param>
        public EdFiAuthorizationContext(
            ClaimsPrincipal principal,
            string resource,
            string action,
            object data)
            : base(principal, resource, action)
        {
            Data = data;

            if (data != null)
                Type = data.GetType();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EdFiAuthorizationContext"/> class using the principal, resource claims, action claims and Ed-Fi authorization context data.
        /// </summary>
        /// <param name="principal">The ClaimsPrincipal containing the claims.</param>
        /// <param name="resource">The claim(s) representing the resource being authorized.</param>
        /// <param name="action">The claims(s) representing the action being taken on the resource.</param>
        /// <param name="data">An object containing the data available for authorization which implements one of the 
        /// model interfaces (e.g. IStudent).</param>
        public EdFiAuthorizationContext(
            ClaimsPrincipal principal,
            Collection<Claim> resource,
            Collection<Claim> action,
            object data)
            : base(principal, resource, action)
        {
            Data = data;

            if (data != null)
                Type = data.GetType();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EdFiAuthorizationContext"/> class using the principal, resource, action and Ed-Fi authorization context data.
        /// </summary>
        /// <param name="principal">The ClaimsPrincipal containing the claims.</param>
        /// <param name="resource">The resource being authorized.</param>
        /// <param name="action">The action being taken on the resource.</param>
        /// <param name="type">The entity type which implements one of the model interfaces (e.g. IStudent) which is the subject of a multiple-item request.</param>
        public EdFiAuthorizationContext(
            ClaimsPrincipal principal,
            string resource,
            string action,
            Type type)
            : base(principal, resource, action)
        {
            Type = type;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EdFiAuthorizationContext"/> class using the principal, resource claims, action claims and Ed-Fi authorization context data.
        /// </summary>
        /// <param name="principal">The ClaimsPrincipal containing the claims.</param>
        /// <param name="resource">The claim(s) representing the resource being authorized.</param>
        /// <param name="action">The claims(s) representing the action being taken on the resource.</param>
        /// <param name="type">The entity type which implements one of the model interfaces (e.g. IStudent) which is the subject of a multiple-item request.</param>
        public EdFiAuthorizationContext(
            ClaimsPrincipal principal,
            Collection<Claim> resource,
            Collection<Claim> action,
            Type type)
            : base(principal, resource, action)
        {
            Type = type;
        }

        /// <summary>
        /// Gets an object containing the data available for authorization which implements one of 
        /// the model interfaces (e.g. IStudent).
        /// </summary>
        public object Data { get; private set; }

        /// <summary>
        /// Gets the <see cref="Type"/> of the data associated with the request represented by the authorization context.
        /// </summary>
        public Type Type { get; private set; }
    }
}