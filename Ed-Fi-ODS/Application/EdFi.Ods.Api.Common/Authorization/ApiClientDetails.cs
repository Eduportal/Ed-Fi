using System.Collections.Generic;
using System.Web.Configuration;

namespace EdFi.Ods.Api.Common.Authorization
{
    /// <summary>
    /// Provides details about an API client after OAuth bearer token validation.
    /// </summary>
    public class ApiClientDetails
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApiClientDetails"/> class.
        /// </summary>
        public ApiClientDetails()
        {
            EducationOrganizationIds = new List<int>();
            Profiles = new List<string>();
        }

        /// <summary>
        /// Gets or sets the API key for the client.
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Indicates whether the API bearer token was valid.
        /// </summary>
        public bool IsTokenValid
        {
            get { return !string.IsNullOrEmpty(ApiKey); }
        }

        /// <summary>
        /// Gets or sets the list of Education Organization Ids associated with the API key.
        /// </summary>
        public IList<int> EducationOrganizationIds { get; set; }

        /// <summary>
        /// Gets or sets the ApplicationId for the client.
        /// </summary>
        public int ApplicationId { get; set; }

        /// <summary>
        /// Gets or sets the Claim Set name of the application for the client
        /// </summary>
        public string ClaimSetName { get; set; }

        /// <summary>
        /// Gets or sets the NamespacePrefix name of the vendor of the application for the client
        /// </summary>
        public string NamespacePrefix { get; set; }

        /// <summary>
        /// Gets or sets the Profiles that are assigned to the client
        /// </summary>
        public IList<string> Profiles { get; set; } 
    }
}