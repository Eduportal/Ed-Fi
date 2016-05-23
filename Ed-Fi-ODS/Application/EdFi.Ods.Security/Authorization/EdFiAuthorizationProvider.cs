using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using EdFi.Common.Extensions;
using EdFi.Common.Security;
using EdFi.Common.Security.Authorization;
using EdFi.Common.Security.Claims;
using EdFi.Ods.Security.Metadata.Repositories;
using log4net;

namespace EdFi.Ods.Security.Authorization
{
    /// <summary>
    /// Performs authorization appropriate to the claims, resource, action and context data supplied in the <see cref="EdFiAuthorizationContextData"/>.
    /// </summary>
    public class EdFiAuthorizationProvider : IEdFiAuthorizationProvider
    {
        private readonly IResourceAuthorizationMetadataProvider _resourceAuthorizationMetadataProvider;
        private readonly ISecurityRepository _securityRepository;

        private readonly Dictionary<string, IEdFiAuthorizationStrategy> _authorizationStrategyByName;
        private readonly Lazy<Dictionary<string, int>> _bitValuesByAction;

        private readonly ILog _logger = LogManager.GetLogger(typeof(EdFiAuthorizationProvider));

        /// <summary>
        /// Initializes a new instance of the <see cref="EdFiAuthorizationProvider"/> class using the specified
        /// authorization strategy provider.
        /// </summary>
        /// <param name="resourceAuthorizationMetadataProvider">The component that will be used to supply the claims/strategies that can be used to authorize the resource.</param>
        /// <param name="authorizationStrategies"></param>
        /// <param name="securityRepository"></param>
        public EdFiAuthorizationProvider(
            IResourceAuthorizationMetadataProvider resourceAuthorizationMetadataProvider, 
            IEdFiAuthorizationStrategy[] authorizationStrategies, 
            ISecurityRepository securityRepository)
        {
            if (resourceAuthorizationMetadataProvider == null)
                throw new ArgumentNullException("resourceAuthorizationMetadataProvider");

            if (securityRepository == null)
                throw new ArgumentNullException("securityRepository");

            _resourceAuthorizationMetadataProvider = resourceAuthorizationMetadataProvider;
            _securityRepository = securityRepository;

            _authorizationStrategyByName = CreateAuthorizationStrategyByNameDictionary(authorizationStrategies);

            // Lazy initialization
            _bitValuesByAction = new Lazy<Dictionary<string, int>>(
                () => new Dictionary<string, int>
                {
                    {_securityRepository.GetActionByName("Create").ActionUri, 0x1},
                    {_securityRepository.GetActionByName("Read").ActionUri,   0x2},
                    {_securityRepository.GetActionByName("Update").ActionUri, 0x4},
                    {_securityRepository.GetActionByName("Delete").ActionUri, 0x8}
                });
        }

        private static Dictionary<string, IEdFiAuthorizationStrategy> CreateAuthorizationStrategyByNameDictionary(
            IEdFiAuthorizationStrategy[] authorizationStrategies)
        {
            var strategyByName = new Dictionary<string, IEdFiAuthorizationStrategy>(
                StringComparer.InvariantCultureIgnoreCase);

            foreach (var strategy in authorizationStrategies)
            {
                string strategyTypeName = GetStrategyTypeName(strategy);

                const string authorizationStrategyNameSuffix = "AuthorizationStrategy";

                // TODO: Embedded convention
                // Enforce naming conventions on authorization strategies
                if (!strategyTypeName.EndsWith(authorizationStrategyNameSuffix))
                {
                    throw new ArgumentException(
                        string.Format(
                            "The authorization strategy '{0}' does not follow proper naming conventions, ending with 'AuthorizationStrategy'.",
                            strategyTypeName));
                }

                string strategyName = strategyTypeName.TrimSuffix(authorizationStrategyNameSuffix);
                strategyByName.Add(strategyName, strategy);
            }

            return strategyByName;
        }

        private static string GetStrategyTypeName(IEdFiAuthorizationStrategy strategy)
        {
            string rawTypeName = strategy.GetType().Name;

            int genericMarkerPos = rawTypeName.IndexOf("`");

            string strategyTypeName = (genericMarkerPos < 0)
                ? rawTypeName
                : rawTypeName.Substring(0, genericMarkerPos);
                
            return strategyTypeName;
        }

        private void ValidateAuthorizationContext(EdFiAuthorizationContext authorizationContext)
        {
            if (authorizationContext == null) 
                throw new ArgumentNullException("authorizationContext");

            if (authorizationContext.Resource == null || authorizationContext.Resource.All(r => string.IsNullOrWhiteSpace(r.Value)))
                throw new AuthorizationContextException("Authorization can only be performed if a resource is specified.");

            if (authorizationContext.Resource.Count > 1)
                throw new AuthorizationContextException("Authorization can only be performed on a single resource.");

            if (authorizationContext.Action == null || authorizationContext.Action.All(a => string.IsNullOrWhiteSpace(a.Value)))
                throw new AuthorizationContextException("Authorization can only be performed if an action is specified.");

            if (authorizationContext.Action.Count > 1)
                throw new AuthorizationContextException("Authorization can only be performed on requests with a single action.");
        }

        /// <summary>
        /// Authorizes a single-item request using the claims, resource, action and entity instance supplied in the <see cref="EdFiAuthorizationContext"/>.
        /// </summary>
        /// <param name="authorizationContext">The authorization context to be used in making the authorization decision.</param>
        public void AuthorizeSingleItem(EdFiAuthorizationContext authorizationContext)
        {
            IEdFiAuthorizationStrategy authorizationStrategy;
            Claim relevantClaim;

            GetAuthorizationDetails(authorizationContext, out authorizationStrategy, out relevantClaim);

            authorizationStrategy.AuthorizeSingleItem(new[] {relevantClaim}, authorizationContext);
        }

        /// <summary>
        /// Authorizes a multiple-item read request using the claims, resource, action and entity instance supplied in the <see cref="EdFiAuthorizationContext"/>.
        /// </summary>
        /// <param name="authorizationContext">The authorization context to be used in making the authorization decision.</param>
        /// <param name="filterBuilder">A builder used to activate filters and assign parameter values.</param>
        /// <returns>The dictionary containing the filter information as appropriate, or <b>null</b> if no filters are required.</returns>
        public void ApplyAuthorizationFilters(EdFiAuthorizationContext authorizationContext, ParameterizedFilterBuilder filterBuilder)
        {
            IEdFiAuthorizationStrategy authorizationStrategy;
            Claim relevantClaim;

            GetAuthorizationDetails(authorizationContext, out authorizationStrategy, out relevantClaim);

            authorizationStrategy.ApplyAuthorizationFilters(new[] {relevantClaim}, authorizationContext, filterBuilder);
        }

        /// <summary>
        /// Performs authorization appropriate to the claims, resource, action and context data supplied in the <see cref="EdFiAuthorizationContextData"/>.
        /// </summary>
        /// <param name="authorizationContext">The authorization context to be used in making the authorization decision.</param>
        private void GetAuthorizationDetails(
            EdFiAuthorizationContext authorizationContext,
            out IEdFiAuthorizationStrategy authorizationStrategy,
            out Claim relevantClaim)
        {
            // Initialize "out" values
            authorizationStrategy = null;
            relevantClaim = null;

            // Validate the context
            ValidateAuthorizationContext(authorizationContext);

            // Extract individual values
            string requestedResource = authorizationContext.Resource.Single().Value;
            var requestedAction = authorizationContext.Action.Single().Value;
            var principal = authorizationContext.Principal;

            // Obtain the resource claim/authorization strategy pairs that could be used for authorizing this particular request
            var resourceClaimAuthorizationStrategies = _resourceAuthorizationMetadataProvider
                .GetResourceClaimAuthorizationStrategies(requestedResource, requestedAction)
                .ToList();

            // Get the names of the resource claims that could be used for authorization
            var authorizingResourceClaimNames = resourceClaimAuthorizationStrategies.Select(x => x.ClaimName).ToList();

            // Intersect the potentially authorizing resource claims against the principal's claims
            var relevantPrincipalClaims = GetRelevantPrincipalClaims(authorizingResourceClaimNames, requestedAction, principal);

            // Only use the caller's first matching going up the hierarchy
            var relevantPrincipalClaim = relevantPrincipalClaims.First();

            // Find the strategy for the sole remaining relevant claim, climbing the lineage until an authorization strategy is set
            string metadataAuthorizationStrategyName =
                resourceClaimAuthorizationStrategies
                    .SkipWhile(s => string.IsNullOrWhiteSpace(s.AuthorizationStrategy))
                    .Select(s => s.AuthorizationStrategy)
                    .FirstOrDefault();

            // TODO: GKM - When claimset-specific override support is added, use this logic
            //string claimSpecificOverrideAuthorizationStrategyName =
            //    resourceClaimAuthorizationStrategies
            //        // Find the resource claim that matches the relevant principal claim
            //        .SkipWhile(s => !s.ClaimName.EqualsIgnoreCase(relevantPrincipalClaim.Type))
            //        .Select(s => s.AuthorizationStrategyOverride)
            //        .FirstOrDefault();

            // Use the claim's override, if present
            //string authorizationStrategyName = 
            //    claimSpecificAuthorizationStrategyName ?? metadataAuthorizationStrategyName;

            string authorizationStrategyName = metadataAuthorizationStrategyName;

            // Make sure an authorization strategy is defined for this request
            if (string.IsNullOrWhiteSpace(authorizationStrategyName))
            {
                throw new Exception(string.Format(
                    "No authorization strategy was defined for the requested action '{0}' against resource '{1}' matched by the caller's claim '{2}'.",
                    requestedAction, requestedResource, relevantPrincipalClaim.Type));
            }

            _logger.InfoFormat("Authorization strategy '{0}' selected for request against resource '{1}'.",
                authorizationStrategyName, authorizationContext.Resource.First().Value);

            // Set outbound authorization details
            authorizationStrategy = GetAuthorizationStrategy(authorizationStrategyName);
            relevantClaim = relevantPrincipalClaim;
        }

        private List<Claim> GetRelevantPrincipalClaims(IList<string> authorizingClaimNames, string requestedAction, ClaimsPrincipal principal)
        {
            // Find matching claims, while preserving the ordering of the incoming claim names
            var principalClaimsToEvaluate =
                (from cn in authorizingClaimNames
                 join pc in principal.Claims on cn equals pc.Type
                 select pc)
                    .ToList();

            // 1) First check: Lets make sure the principal at least has a claim that applies for this resource.
            if (!principalClaimsToEvaluate.Any())
                throw new EdFiSecurityException(string.Format("Access to the resource could not be authorized. Are you missing a claim? This resource can be authorized by: {0}.  ClaimsPrincipal has the following claims: {1}",
                    string.Join(", ", authorizingClaimNames),
                    string.Join(", ", principal.Claims.Select(c => c.Type))));

            // 2) Second check: Of the claims that apply for this resource do we have any that match the action requested or a higher action?
            var edfiClaimValuesToEvaluate = principalClaimsToEvaluate.Select(x => new { Claim = x, EdFiResourceClaimValue = x.ToEdFiResourceClaimValue() });

            var claimsWithMatchingActions = edfiClaimValuesToEvaluate.Where(x => IsRequestedActionSatisfiedByClaimActions(requestedAction, x.EdFiResourceClaimValue.Actions)).ToList();
            if (!claimsWithMatchingActions.Any())
                throw new EdFiSecurityException(string.Format("Access to the resource could not be authorized for the requested action '{0}'.", requestedAction));

            return claimsWithMatchingActions.Select(x => x.Claim).ToList();
        }

        private bool IsRequestedActionSatisfiedByClaimActions(string requestedAction, IEnumerable<string> principalClaimActions)
        {
            int requestedActionValue = GetBitValuesByAction(requestedAction);

            // Determine if any of the claim actions can satisfy the requested action
            return principalClaimActions.Any(x => (requestedActionValue & GetBitValuesByAction(x)) != 0);
        }

        private int GetBitValuesByAction(string action)
        {
            int result;

            if (_bitValuesByAction.Value.TryGetValue(action, out result))
                return result;

            // Should never happen
            throw new NotSupportedException("The requested action is not a supported action.  Authorization cannot be performed.");
        }

        private IEdFiAuthorizationStrategy GetAuthorizationStrategy(string strategyName)
        {
            if (!_authorizationStrategyByName.ContainsKey(strategyName))
            {
                throw new Exception(string.Format(
                    "Could not find authorization implementation for strategy '{0}' based on naming convention of '{{strategyName}}AuthorizationStrategy'.", 
                    strategyName));
            }

            return _authorizationStrategyByName[strategyName];
        }
    }
}