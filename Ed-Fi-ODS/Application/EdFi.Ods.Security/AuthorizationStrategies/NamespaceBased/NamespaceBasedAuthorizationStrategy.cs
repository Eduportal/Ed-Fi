// ****************************************************************************
//  Copyright (C) 2015 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using EdFi.Common.Configuration;
using EdFi.Common.Security;
using EdFi.Common.Security.Authorization;
using EdFi.Common.Security.Claims;
using EdFi.Ods.Security.Claims;

namespace EdFi.Ods.Security.AuthorizationStrategies.NamespaceBased
{
    public class NamespaceBasedAuthorizationStrategy : IEdFiAuthorizationStrategy
    {
        private readonly AuthorizationContextDataFactory _authorizationContextDataFactory
            = new AuthorizationContextDataFactory();

        private readonly IConfigValueProvider _configValueProvider;

        public NamespaceBasedAuthorizationStrategy(
            IConfigValueProvider configValueProvider)
        {
            _configValueProvider = configValueProvider;
        }

        public void AuthorizeSingleItem(
            IEnumerable<Claim> relevantClaims,
            EdFiAuthorizationContext authorizationContext)
        {
            var claimNamespacePrefix = GetClaimNamespacePrefix(authorizationContext);

            var contextData = _authorizationContextDataFactory
                .CreateContextData<NamespaceBasedAuthorizationContextData>(
                    authorizationContext.Data);

            if (contextData == null)
                throw new NotSupportedException(
                    "No 'Namespace' property could be found on the resource in order to perform authorization.  Should a different authorization strategy be used?");

            if (string.IsNullOrWhiteSpace(contextData.Namespace))
                throw new EdFiSecurityException(
                    "Access to the resource item could not be authorized because the Namespace of the resource is empty.");

            if (!contextData.Namespace.StartsWith(claimNamespacePrefix))
                throw new EdFiSecurityException(
                    string.Format(
                        "Access to the resource item with namespace '{0}' could not be authorized based on the caller's NamespacePrefix claim of '{1}'.",
                        contextData.Namespace,
                        claimNamespacePrefix));
        }

        /// <summary>
        /// Applies filtering to a multiple-item request.
        /// </summary>
        /// <param name="relevantClaims">The subset of the caller's claims that are relevant for the authorization decision.</param>
        /// <param name="authorizationContext">The authorization context.</param>
        /// <param name="filterBuilder">A builder used to activate filters and assign parameter values.</param>
        /// <returns>The dictionary containing the filter information as appropriate, or <b>null</b> if no filters are required.</returns>
        public void ApplyAuthorizationFilters(
            IEnumerable<Claim> relevantClaims,
            EdFiAuthorizationContext authorizationContext, 
            ParameterizedFilterBuilder filterBuilder)
        {
            var claimNamespacePrefix = GetClaimNamespacePrefix(authorizationContext);

            //get default descriptor prefix
            var descriptorNamespacePrefix = _configValueProvider.GetValue("DescriptorNamespacePrefix");

            filterBuilder
                .Filter("Namespace")
                .Set("Namespace", claimNamespacePrefix + "%")
                .Set("DefaultNamespace", descriptorNamespacePrefix + "%");
        }

        private static string GetClaimNamespacePrefix(EdFiAuthorizationContext authorizationContext)
        {
            var namespaceClaim =
                authorizationContext.Principal.Claims.FirstOrDefault(
                    c => c.Type == EdFiOdsApiClaimTypes.NamespacePrefix);

            if (namespaceClaim == null || string.IsNullOrWhiteSpace(namespaceClaim.Value))
            {
                throw new EdFiSecurityException(
                    string.Format(
                        "Access to the resource could not be authorized because the caller did not have a NamespacePrefix claim ('{0}') or the claim value was empty.",
                        EdFiOdsApiClaimTypes.NamespacePrefix));
            }

            return namespaceClaim.Value;
        }
    }
}