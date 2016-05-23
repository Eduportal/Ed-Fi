using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using EdFi.Common.Extensions;
using EdFi.Common.Security;
using EdFi.Common.Security.Authorization;
using EdFi.Common.Security.Claims;
using EdFi.Ods.Security.Authorization;
using EdFi.Ods.Security.Claims;

namespace EdFi.Ods.Security.AuthorizationStrategies.AssessmentMetadata 
{
    /// <summary>
    /// For use with the assessment, assessmentItem, objectiveAssessment, and studentAssessment resources
    /// each of those resources gets their namespace from assessmentFamily
    /// </summary>
    public class AssessmentMetadataAuthorizationStrategy : IEdFiAuthorizationStrategy
    {
        private readonly IAssessmentMetadataNamespaceProvider _assessmentMetadataNamespaceProvider;
        
        private readonly AuthorizationContextDataFactory _authorizationContextDataFactory 
            = new AuthorizationContextDataFactory();

        public AssessmentMetadataAuthorizationStrategy(
            IAssessmentMetadataNamespaceProvider assessmentMetadataNamespaceProvider)
        {
            _assessmentMetadataNamespaceProvider = assessmentMetadataNamespaceProvider;
        }

        public void AuthorizeSingleItem(IEnumerable<Claim> relevantClaims, EdFiAuthorizationContext authorizationContext)
        {
            var claimNamespacePrefix = GetClaimNamespacePrefix(authorizationContext);

            var contextData = _authorizationContextDataFactory
                .CreateContextData<AssessmentMetadataAuthorizationContextData>(
                    authorizationContext.Data);

            if (contextData == null)
            {
                throw new NotSupportedException(
                    string.Format(
                        "No properties for context data type '{0}' could be found on the resource in order to perform authorization.  Should a different authorization strategy be used?",
                        typeof(AssessmentMetadataAuthorizationContextData).Name));
            }

            string contextNamespace;

            if (!TryGetNamespace(contextData, out contextNamespace))
                throw new EdFiSecurityException("Access to the resource could not be authorized. The Namespace of the resource is empty.");

            if (!contextNamespace.StartsWith(claimNamespacePrefix))
                throw new EdFiSecurityException(
                    string.Format(
                        "Access to the resource item with namespace '{0}' could not be authorized based on the caller's NamespacePrefix claim of '{1}'.",
                        contextNamespace,
                        claimNamespacePrefix));
            
            //throw new EdFiSecurityException(string.Format("Access to the resource could not be authorized for the requested namespace '{0}'.", contextNamespace));
        }

        /// <summary>
        /// Applies filtering to a multiple-item request.
        /// </summary>
        /// <param name="relevantClaims">The subset of the caller's claims that are relevant for the authorization decision.</param>
        /// <param name="authorizationContext">The authorization context.</param>
        /// <param name="filterBuilder">A builder used to activate filters and assign parameter values.</param>
        /// <returns>The dictionary containing the filter information as appropriate, or <b>null</b> if no filters are required.</returns>
        public void ApplyAuthorizationFilters(IEnumerable<Claim> relevantClaims, EdFiAuthorizationContext authorizationContext, ParameterizedFilterBuilder filterBuilder)
        {
            var claimNamespacePrefix = GetClaimNamespacePrefix(authorizationContext);

            filterBuilder
                .Filter("AssessmentMetadata")
                .Set("Namespace", claimNamespacePrefix + "%");
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

        private bool TryGetNamespace(AssessmentMetadataAuthorizationContextData contextData, out string @namespace)
        {
            @namespace = null;

            if (contextData != null)
            {
                //Assessment may have a Namespace specified
                if (!string.IsNullOrWhiteSpace(contextData.Namespace))
                {
                    @namespace = contextData.Namespace;
                    return true;
                }

                //Assessment's that don't specify a Namespace may have an AssessmentFamilyTitle which may also have a namespace
                if (!string.IsNullOrWhiteSpace(contextData.AssessmentFamilyTitle))
                {
                    @namespace = _assessmentMetadataNamespaceProvider.GetNamespaceByAssessmentFamilyTitle(contextData.AssessmentFamilyTitle);
                    return true;
                }

                //AssessmentItem, ObjectiveAssessment, StudentAssessment are child objects of an Assessment 
                //so use that AssessmentTitle to get the Namespace from the Assessment or that Assessment's AssessmentFamily
                if (!string.IsNullOrWhiteSpace(contextData.AssessmentTitle))
                {
                    @namespace = _assessmentMetadataNamespaceProvider.GetNamespaceByAssessmentTitle(contextData.AssessmentTitle);
                    return true;
                }
            }

            return false;
        }
    }
}

