using System.Collections.Generic;
using NHibernate;

namespace EdFi.Ods.Security.Authorization.Repositories
{
    /// <summary>
    /// Defines a method for applying filters for an entity on the current NHibernate session.
    /// </summary>
    public class NHibernateFilterApplicator : INHibernateFilterApplicator
    {
        /// <summary>
        /// Apply filters for the entity on the session.
        /// </summary>
        /// <param name="session">The session to which filters should be applied.</param>
        /// <param name="filters">A dictionary keyed by filter name, whose values are dictionaries keyed by parameter name.</param>
        public void ApplyFilters(ISession session, IDictionary<string, IDictionary<string, object>> filters)
        {
            if (filters != null)
            {
                foreach (var filterDetails in filters)
                {
                    var filter = session.EnableFilter(filterDetails.Key);

                    foreach (var parameterNameAndValue in filterDetails.Value)
                    {
                        // TODO: Need unit test for setting null values, object[] values, and single object values
                        if (parameterNameAndValue.Value == null)
                        {
                            filter.SetParameter(parameterNameAndValue.Key, null);
                        }
                        else
                        {
                            var arrayOfValues = parameterNameAndValue.Value as object[];

                            if (arrayOfValues != null)
                                filter.SetParameterList(parameterNameAndValue.Key, arrayOfValues);
                            else
                                filter.SetParameter(parameterNameAndValue.Key, parameterNameAndValue.Value);
                        }
                    }
                }
            }
        }
    }
}
