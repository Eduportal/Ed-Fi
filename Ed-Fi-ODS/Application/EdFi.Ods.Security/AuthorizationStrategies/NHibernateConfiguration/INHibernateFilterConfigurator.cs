using System.Collections.Generic;

namespace EdFi.Ods.Security.AuthorizationStrategies.NHibernateConfiguration
{
    /// <summary>
    /// Provides methods for defining and applying NHibernate parameterized filters to entity mappings.
    /// </summary>
    public interface INHibernateFilterConfigurator
    {
        /// <summary>
        /// Gets the NHibernate filter definitions and a functional delegate for determining when to apply them. 
        /// </summary>
        /// <returns>A read-only list of filter application details to be applied to the NHibernate configuration and entity mappings.</returns>
        IReadOnlyList<FilterApplicationDetails> GetFilters();
    }
}