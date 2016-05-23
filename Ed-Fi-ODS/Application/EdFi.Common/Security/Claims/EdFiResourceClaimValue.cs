using System.Collections.Generic;

namespace EdFi.Common.Security.Claims
{
    /// <summary>
    /// Provides a concrete model for creating and accessing the serialized JSON of the Ed-Fi resource claim values.
    /// </summary>
    public class EdFiResourceClaimValue
    {
        /// <summary>
        /// Gets or sets the actions that can be performed by the claim on the resource.
        /// </summary>
        public string[] Actions { get; set; }

        /// <summary>
        /// Gets or sets the Education Organization Ids to which the claim applies.
        /// </summary>
        public List<int> EducationOrganizationIds { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EdFiResourceClaimValue"/> class.
        /// </summary>
        public EdFiResourceClaimValue()
        {
            EducationOrganizationIds = new List<int>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EdFiResourceClaimValue"/> class using the specified 
        /// action.
        /// </summary>
        /// <param name="action">The action the claim is authorized to perform on the resource.</param>
        public EdFiResourceClaimValue(string action)
        {
            Actions = new[] { action };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EdFiResourceClaimValue"/> class using the specified
        /// action, schema and Education Organization type and ids.
        /// </summary>
        /// <param name="action">The action the claim is authorized to perform on the resource.</param>
        /// <param name="educationOrganizationIds">The Local Education Agency Ids to which the claim applies.</param>
        public EdFiResourceClaimValue(string action, List<int> educationOrganizationIds)
        {
            Actions = new[] { action };
            EducationOrganizationIds = educationOrganizationIds ?? new List<int>();
        }
    }
}
