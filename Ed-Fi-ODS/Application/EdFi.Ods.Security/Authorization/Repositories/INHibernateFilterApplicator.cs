using System.Collections.Generic;
using NHibernate;

namespace EdFi.Ods.Security.Authorization.Repositories
{
    /// <summary>
    /// Defines a method for applying authorization filters for an entity on the current NHibernate session.
    /// </summary>
    public interface INHibernateFilterApplicator
    {
        /// <summary>
        /// Apply authorization filters for the entity on the session.
        /// </summary>
        /// <param name="filters">A dictionary keyed by filter name, whose values are dictionaries keyed by parameter name.</param>
        /// <param name="session">The session to which authorization filters should be applied.</param>
        void ApplyFilters(ISession session, IDictionary<string, IDictionary<string, object>> filters);
    }
}
