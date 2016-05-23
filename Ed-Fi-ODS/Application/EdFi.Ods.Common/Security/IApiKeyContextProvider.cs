using System;
using System.Collections.Generic;

namespace EdFi.Ods.Common.Security
{
    /// <summary>
    /// Defines methods for getting and settng the contextual details for the caller's API key.
    /// </summary>
    public interface IApiKeyContextProvider
    {
        /// <summary>
        /// Gets the details of the API key currently in context.
        /// </summary>
        /// <returns>An <see cref="ApiKeyContext"/> instance.</returns>
        ApiKeyContext GetApiKeyContext();

        /// <summary>
        /// Sets the details of the API key into context.
        /// </summary>
        /// <param name="apiKeyContext">The <see cref="ApiKeyContext"/> instance to be stored.</param>
        void SetApiKeyContext(ApiKeyContext apiKeyContext);
    }

    /// <summary>
    /// Contains contextual information about the current API caller.
    /// </summary>
    public class ApiKeyContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:ApiKeyContext"/> class.
        /// </summary>
        public ApiKeyContext(string apiKey, string claimSetName, IEnumerable<int> educationOrganizationIds, string namespacePrefix, IEnumerable<string> profiles )
        {
            ApiKey = apiKey;
            ClaimSetName = claimSetName;
            EducationOrganizationIds = educationOrganizationIds ?? new List<int>();
            NamespacePrefix = namespacePrefix;
            Profiles = profiles ?? new List<string>();
        }

        public string ApiKey { get; private set; }
        public string ClaimSetName { get; private set; }
        public IEnumerable<int> EducationOrganizationIds { get; private set; }
        public string NamespacePrefix { get; private set; }
        public IEnumerable<string> Profiles { get; private set; }

        private static readonly ApiKeyContext _empty = new ApiKeyContext(null, null, null, null, null);

		/// <summary>
		/// Returns an empty, uninitialized <see cref="ApiKeyContext"/> instance.
		/// </summary>
        public static ApiKeyContext Empty
        {
            get { return _empty; }
        }
    }
}
