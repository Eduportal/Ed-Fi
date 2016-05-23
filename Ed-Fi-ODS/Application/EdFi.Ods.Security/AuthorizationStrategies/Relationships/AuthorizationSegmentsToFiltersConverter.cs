using System;
using System.Collections.Generic;
using System.Linq;
using EdFi.Common.Extensions;
using EdFi.Common.Security.Authorization;
using NHibernate;
using NHibernate.Metadata;

namespace EdFi.Ods.Security.AuthorizationStrategies.Relationships
{
    public interface IAuthorizationSegmentsToFiltersConverter
    {
        void Convert(Type entityType, AuthorizationSegmentCollection authorizationSegments, ParameterizedFilterBuilder filterBuilder);
    }

    public class AuthorizationSegmentsToFiltersConverter : IAuthorizationSegmentsToFiltersConverter
    {
        private readonly ISessionFactory _sessionFactory;

        public AuthorizationSegmentsToFiltersConverter(ISessionFactory sessionFactory)
        {
            _sessionFactory = sessionFactory;
        }

        public void Convert(Type entityType, AuthorizationSegmentCollection authorizationSegments, ParameterizedFilterBuilder filterBuilder)
        {
            if (!authorizationSegments.ClaimsAuthorizationSegments.Any())
                return;

            var classMetadata = _sessionFactory.GetClassMetadata(entityType);

            var valuesByViewName = new Dictionary<string, List<Tuple<string, object>>>(StringComparer.InvariantCultureIgnoreCase);

            // For filter-based authorization, there is no sense/need in incorporating the "existing values" segments, 
            // as they would only serve to further constrain data that is already in the target table.
            foreach (var segment in authorizationSegments.ClaimsAuthorizationSegments)
            {
                var targetEndpointName = segment.TargetEndpoint.Name;

                var claimsEndpointsGroupedByName =
                    (from cep in segment.ClaimsEndpoints
                     group cep by cep.Name
                     into g
                     select g)
                        .ToList();

                // Make sure all the claims are of the same type.  NHibernate filters are combined with AND, 
                // so we only have one representation of the 'OR' nature of the claims values 
                // (via an 'IN' clause in the filter itself)
                if (claimsEndpointsGroupedByName.Count() > 1)
                {
                    throw new NotSupportedException(
                        string.Format(
                            "Filter-based authorization does not support claims containing multiple types of values (e.g. claims associated with multiple types of education organizations).  The claim types found were '{0}'.",
                            string.Join("', '", claimsEndpointsGroupedByName.Select(x => x.Key))));
                }

                var claimEndpoints = claimsEndpointsGroupedByName.Single();
                string claimEndpointName = claimEndpoints.First().Name;

                if (targetEndpointName.EqualsIgnoreCase(claimEndpointName))
                    continue;

                // Get the name of the view to use for this segment
                string viewName = GetViewName(classMetadata, targetEndpointName, claimEndpointName);

                List<Tuple<string, object>> values;

                if (!valuesByViewName.TryGetValue(viewName, out values))
                {
                    values = new List<Tuple<string, object>>();
                    valuesByViewName[viewName] = values;
                }

                values.AddRange(claimEndpoints.Select(x => Tuple.Create(x.Name, x.Value)));
            }

            foreach (var kvp in valuesByViewName)
            {
                var filterName = kvp.Key;

                var valuesGroupedByName =
                    (from x in kvp.Value
                     group x by x.Item1 into g
                     select g);

                var filter = filterBuilder.Filter(filterName);

                foreach (var grouping in valuesGroupedByName)
                    filter.Set(grouping.Key, (object) grouping.Select(p => p.Item2).Distinct().ToArray());
            }
        }

        private string GetViewName(IClassMetadata classMetadata, string item1Name, string item2Name)
        {
            string item1 = ReplaceUniqueIdIfNotInMetadata(classMetadata, item1Name);
            string item2 = ReplaceUniqueIdIfNotInMetadata(classMetadata, item2Name);

            // TODO: Embedded convention, building view name using endpoints sorted alphabetically
            var compareResult = string.Compare(item1, item2, StringComparison.InvariantCultureIgnoreCase);

            var viewPrefix = compareResult < 0 ? item1 : item2;
            var viewSuffix = compareResult < 0 ? item2 : item1;

            string viewName = viewPrefix + "To" + viewSuffix;

            return viewName;
        }

        private string ReplaceUniqueIdIfNotInMetadata(IClassMetadata classMetadata, string propertyName)
        {
            // TODO: Embedded convention
            return propertyName.EndsWithIgnoreCase("UniqueId") &&
                   !classMetadata.PropertyNames.Contains(propertyName)
                ? propertyName.Replace("UniqueId", "USI")
                : propertyName;
        }

    }
}