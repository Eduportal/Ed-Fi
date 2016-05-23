using System;
using EdFi.Common.Caching;
using EdFi.Common.Configuration;
using EdFi.Ods.Api.Common.Authorization;

namespace EdFi.Ods.Api.Services.Authorization
{
    /// <summary>
    /// Provides local caching behavior for OAuth token validation.
    /// </summary>
    public class CachingOAuthTokenValidatorDecorator : IOAuthTokenValidator
    {
        // Dependencies
        private readonly IOAuthTokenValidator _next;
        private readonly ICacheProvider _cacheProvider;

        // Lazy initialized fields
        private readonly Lazy<int> _bearerTokenTimeoutMinutes;

        private const string ConfigBearerTokenTimeoutMinutes = "BearerTokenTimeoutMinutes";

        /// <summary>
        /// Initializes a new instance of the <see cref="CachingOAuthTokenValidatorDecorator"/> class.
        /// </summary>
        /// <param name="next">The decorated implementation.</param>
        /// <param name="cacheProvider">The cache provider.</param>
        /// <param name="configValueProvider">The configuration value provider.</param>
        public CachingOAuthTokenValidatorDecorator(
            IOAuthTokenValidator next, 
            ICacheProvider cacheProvider,
            IConfigValueProvider configValueProvider)
        {
            _next = next;
            _cacheProvider = cacheProvider;

            // Lazy initialization
            _bearerTokenTimeoutMinutes = new Lazy<int>(() =>
                Convert.ToInt32(configValueProvider.GetValue(ConfigBearerTokenTimeoutMinutes) ?? "30"));
        }

        private const string CacheKeyFormat = "OAuthTokenValidator.ApiClientDetails.{0}";

        /// <summary>
        /// Checks the cache for an existing value, and if not found, calls through to decorated implementation to retrieve the details (which is then cached).
        /// </summary>
        /// <param name="token">The OAuth security token.</param>
        /// <returns>The <see cref="ApiClientDetails"/> associatd with the token.</returns>
        public ApiClientDetails GetClientDetailsForToken(Guid token)
        {
            string cachKey = string.Format(CacheKeyFormat, token);

            object apiClientDetailsAsObject;

            // Try to load API client details from cache
            if (_cacheProvider.TryGetCachedObject(cachKey, out apiClientDetailsAsObject))
                return (ApiClientDetails) apiClientDetailsAsObject;

            // Pass call through to implementation
            var apiClientDetails = _next.GetClientDetailsForToken(token);

            // If token is valid, insert API client details into the cache for **half** the duration of the externally managed expiration period
            if (apiClientDetails.IsTokenValid)
                _cacheProvider.Insert(cachKey, apiClientDetails, DateTime.Now.AddMinutes(_bearerTokenTimeoutMinutes.Value / 2.0), TimeSpan.Zero);

            return apiClientDetails;
        }
    }
}