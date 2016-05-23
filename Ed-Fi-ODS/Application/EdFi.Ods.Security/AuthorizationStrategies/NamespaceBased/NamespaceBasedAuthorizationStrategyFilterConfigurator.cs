using System.Collections.Generic;
using EdFi.Common.Extensions;
using EdFi.Ods.Security.AuthorizationStrategies.NHibernateConfiguration;

namespace EdFi.Ods.Security.AuthorizationStrategies.NamespaceBased
{
    public class NamespaceBasedAuthorizationStrategyFilterConfigurator
        : INHibernateFilterConfigurator
    {
        /// <summary>
        /// Gets the authorization strategy's NHibernate filter definitions and a functional delegate for determining when to apply them. 
        /// </summary>
        /// <returns>A read-only list of filter application details to be applied to the NHibernate configuration and mappings.</returns>
        public IReadOnlyList<FilterApplicationDetails> GetFilters()
        {
            var filters = (new List<FilterApplicationDetails>
            {
                new FilterApplicationDetails(
                    "Namespace", 
                    @"(Namespace IS NOT NULL AND Namespace LIKE :Namespace) OR (Namespace IS NOT NULL AND Namespace LIKE :DefaultNamespace)",
                    (t, p) => t.Name.EqualsIgnoreCase("Descriptor")),

                new FilterApplicationDetails(
                    "Namespace", 
                    @"(Namespace IS NOT NULL AND Namespace LIKE :Namespace) OR (Namespace IS NOT NULL AND Namespace LIKE :DefaultNamespace)",
                    (t, p) => t.Name.EqualsIgnoreCase("AssessmentFamily")),
            }).AsReadOnly();

            return filters;
        }
    }
}