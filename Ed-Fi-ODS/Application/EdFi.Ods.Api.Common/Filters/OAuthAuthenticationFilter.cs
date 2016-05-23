using EdFi.Common.Security.Claims;
using EdFi.Ods.Api.Common.Authorization;
using EdFi.Ods.Common.Security;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using System.Web.Http.Results;

namespace EdFi.Ods.Api.Common.Filters
{
    public class OAuthAuthenticationFilter : IAuthenticationFilter
    {
        private const string AuthenticationScheme = "Bearer";

        public IOAuthTokenValidator OAuthTokenValidator { get; set; }
        public IApiKeyContextProvider ApiKeyContextProvider { get; set; }
        public IClaimsIdentityProvider ClaimsIdentityProvider { get; set; }

        public OAuthAuthenticationFilter(IOAuthTokenValidator oauthTokenValidator, IApiKeyContextProvider apiKeyContextProvider, IClaimsIdentityProvider claimsIdentityProvider)
        {
            OAuthTokenValidator = oauthTokenValidator;
            ApiKeyContextProvider = apiKeyContextProvider;
            ClaimsIdentityProvider = claimsIdentityProvider;
        }

        public Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            // 1. Look for credentials in the request.
            HttpRequestMessage request = context.Request;
            AuthenticationHeaderValue authorization = request.Headers.Authorization;

            // 2. If there are no credentials, do nothing.
            if (authorization == null)
            {
                return Task.FromResult(0);
            }

            // 3. If there are credentials but the filter does not recognize the 
            //    authentication scheme, do nothing.
            if (authorization.Scheme != AuthenticationScheme)
            {
                return Task.FromResult(0);
            }

            // 4. If there are credentials that the filter understands, try to validate them.
            // 5. If the credentials are bad, set the error result.
            if (String.IsNullOrEmpty(authorization.Parameter))
            {
                context.ErrorResult = new AuthenticationFailureResult("Missing credentials", request);
                return Task.FromResult(0);
            }

            Guid token;
            if (!Guid.TryParse(authorization.Parameter, out token))
            {
                context.ErrorResult = new AuthenticationFailureResult("Invalid credentials", request);
                return Task.FromResult(0);
            }

            // Validate the token and get the corresponding API key details
            var apiClientDetails = OAuthTokenValidator.GetClientDetailsForToken(token);

            if (!apiClientDetails.IsTokenValid)
            {
                context.ErrorResult = new AuthenticationFailureResult("Invalid token", request);
                return Task.FromResult(0);
            }

            // Store API key details into context
            ApiKeyContextProvider.SetApiKeyContext(
                new ApiKeyContext(
                    apiClientDetails.ApiKey, 
                    apiClientDetails.ClaimSetName,
                    apiClientDetails.EducationOrganizationIds,
                    apiClientDetails.NamespacePrefix, 
                    apiClientDetails.Profiles));
            
            var claimsIdentity = ClaimsIdentityProvider.GetClaimsIdentity(apiClientDetails.EducationOrganizationIds, apiClientDetails.ClaimSetName, apiClientDetails.NamespacePrefix);
            context.Principal = new ClaimsPrincipal(claimsIdentity);

            return Task.FromResult(1);
        }

        public async Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            var result = await context.Result.ExecuteAsync(cancellationToken);

            if (result.StatusCode == HttpStatusCode.Unauthorized)
                result.Headers.WwwAuthenticate.Add(new AuthenticationHeaderValue(AuthenticationScheme, Guid.Empty.ToString()));

            context.Result = new ResponseMessageResult(result);
        }

        public bool AllowMultiple { get{ return false; } }
    }
}