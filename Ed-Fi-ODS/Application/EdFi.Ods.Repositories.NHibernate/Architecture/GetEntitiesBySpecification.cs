// ****************************************************************************
//  Copyright (C) 2015 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using EdFi.Common.Extensions;
using EdFi.Ods.Api.Data.Repositories;
using EdFi.Ods.Common;
using EdFi.Ods.Common.Repositories;
using EdFi.Ods.Common.Specifications;
using EdFi.Ods.Entities.Common.Caching;
using EdFi.Ods.Entities.NHibernate.Architecture;
using NHibernate;
using NHibernate.Context;
using NHibernate.Criterion;
using NHibernate.Persister.Entity;

namespace EdFi.Ods.Entities.Repositories.NHibernate.Architecture
{
    public class GetEntitiesBySpecification<TEntity>
        : NHibernateRepositoryOperationBase, IGetEntitiesBySpecification<TEntity>
        where TEntity : DomainObjectBase, IHasIdentifier, IDateVersionedEntity
    {
        private readonly IGetEntitiesByIds<TEntity> _getEntitiesByIds;
        protected static IList<TEntity> EmptyList = new List<TEntity>();

        // TODO: Embedded convention/specification, person unique Ids
        private static readonly string[] uniqueIdProperties =
        {
            "StudentUniqueId",
            "StaffUniqueId",
            "ParentUniqueId"
        };

        private static readonly string[] propertiesToIgnore =
        {
            "Offset",
            "Limit",
            "Q",
            "SortBy",
            "SortDirection"
        };

        public GetEntitiesBySpecification(ISessionFactory sessionFactory,
            IGetEntitiesByIds<TEntity> getEntitiesByIds)
            : base(sessionFactory)
        {
            _getEntitiesByIds = getEntitiesByIds;
        }

        public IList<TEntity> GetBySpecification(TEntity specification, IQueryParameters queryParameters)
        {
            var shouldReleaseBind = EnsureSessionContextBinding();

            try
            {
                // Get the identifiers for a subsequent IN query
                // Do not attempt to combine this into a single ICriteria query, as it will result in a separate query for each row
                // This approach yields all the data in 2 trips to the database (one for the Ids, and a second for all the aggregates)
                var ids = GetPagedAggregateIds(specification, queryParameters);

                if (ids.Count == 0)
                    return EmptyList;

                // Get the full results
                var result = _getEntitiesByIds.GetByIds(ids);

                // Restore original order of the result rows (GetByIds sorts by Id)
                var resultWithOriginalOrder =
                    (from id in ids
                     join r in result on id equals r.Id
                     select r)
                        .ToList();

                return resultWithOriginalOrder;
            }
            finally
            {
                if (shouldReleaseBind)
                    CurrentSessionContext.Unbind(SessionFactory);
            }
        }

        protected IList<Guid> GetPagedAggregateIds(TEntity specification, IQueryParameters queryParameters)
        {
            var idQueryCriteria = Session.CreateCriteria<TEntity>()
                                         .SetProjection(Projections.Property("Id"))
                                         .SetFirstResult(queryParameters.Offset ?? 0)
                                         .SetMaxResults(queryParameters.Limit ?? 25);

            // Add specification-based criteria
            ProcessSpecification(idQueryCriteria, specification);

            // Add special query fields
            ProcessQueryParameters(idQueryCriteria, queryParameters);

            AddDefaultOrdering(idQueryCriteria);

            // Get final, paged list of Ids based on order
            var ids = idQueryCriteria.List<Guid>();

            return ids;
        }

        private void AddDefaultOrdering(ICriteria queryCriteria)
        {
            var persister = (AbstractEntityPersister) SessionFactory.GetClassMetadata(typeof(TEntity));

            if (persister.IdentifierColumnNames != null && persister.IdentifierColumnNames.Length > 0)
            {
                foreach (var identifierColumnName in persister.IdentifierColumnNames)
                    queryCriteria.AddOrder(Order.Asc(identifierColumnName));
            }
            else
            {
                queryCriteria.AddOrder(Order.Asc("Id"));
            }
        }

        private void ProcessSpecification(ICriteria queryCriteria, TEntity specification)
        {
            if (specification != null)
            {
                var propertyValuePairs =
                    specification.ToDictionary(
                        (descriptor, o) => ShouldIncludeInQueryCriteria(descriptor, o, specification));

                foreach (var key in propertyValuePairs.Keys)
                {
                    LookupColumnDetails columnDetails;
                    IHasLookupColumnPropertyMap map = specification as IHasLookupColumnPropertyMap;

                    if (map.IdPropertyByLookupProperty.TryGetValue(key, out columnDetails))
                    {
                        // Look up the corresponding lookup id value from the cache
                        var lookupId = TypesAndDescriptorsCache
                            .GetCache()
                            .GetId(
                                columnDetails.LookupTypeName,
                                Convert.ToString(propertyValuePairs[key]));

                        // Add criteria for the lookup Id value, to avoid need to incorporate an INNER JOIN into the query
                        queryCriteria.Add(
                            propertyValuePairs[key] != null
                                ? Restrictions.Eq(columnDetails.PropertyName, lookupId)
                                : Restrictions.IsNull(key));
                    }
                    else
                    {
                        // Add the property equality condition to the query criteria
                        queryCriteria.Add(
                            propertyValuePairs[key] != null
                                ? Restrictions.Eq(key, propertyValuePairs[key])
                                : Restrictions.IsNull(key));
                    }
                }
            }
        }

        private static void ProcessQueryParameters(ICriteria queryCriteria, IQueryParameters parameters)
        {
            foreach (IQueryCriteriaBase criteria in parameters.QueryCriteria)
            {
                TextCriteria textCriteria = criteria as TextCriteria;

                if (textCriteria != null)
                {
                    MatchMode mode;

                    switch (textCriteria.MatchMode)
                    {
                        case TextMatchMode.Anywhere:
                            mode = MatchMode.Anywhere;
                            break;

                        case TextMatchMode.Start:
                            mode = MatchMode.Start;
                            break;

                        case TextMatchMode.End:
                            mode = MatchMode.End;
                            break;

                        //case TextMatchMode.Exact:
                        default:
                            mode = MatchMode.Exact;
                            break;
                    }

                    queryCriteria.Add(Restrictions.Like(textCriteria.PropertyName, textCriteria.Value, mode));
                }
            }
        }

        private bool ShouldIncludeInQueryCriteria(PropertyDescriptor property, object value, TEntity entity)
        {
            // Null values and underscore-prefixed properties are ignored for specification purposes
            if (value == null
                || property.Name.StartsWith("_")
                || "|Url|".Contains(property.Name))
            {
                // TODO: Come up with better way to exclude non-data properties
                return false;
            }

            Type valueType = value.GetType();

            // Only use value types (or strings), and non-default values (i.e. ignore 0's)
            var result = ((valueType.IsValueType || valueType == typeof(string))
                          && !value.Equals(valueType.GetDefaultValue()));

            // Don't include properties that are explicitly to be ignored
            result = result && !propertiesToIgnore.Contains(property.Name);
            
            // Don't include UniqueId properties when they appear on a Person entity
            result = result
                     && (!uniqueIdProperties.Contains(property.Name)
                         || PersonEntitySpecification.IsPersonEntity(entity.GetType()));

            return result;
        }
    }
}