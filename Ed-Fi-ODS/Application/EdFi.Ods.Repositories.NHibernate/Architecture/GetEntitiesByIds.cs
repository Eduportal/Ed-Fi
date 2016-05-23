using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EdFi.Common;
using EdFi.Common.Extensions;
using EdFi.Ods.Common;
using EdFi.Ods.Common.Repositories;
using EdFi.Ods.Entities.NHibernate.Architecture;
using NHibernate;
using NHibernate.Context;

namespace EdFi.Ods.Entities.Repositories.NHibernate.Architecture
{
    public class GetEntitiesByIds<TEntity> : NHibernateRepositoryOperationBase, IGetEntitiesByIds<TEntity>
        where TEntity : DomainObjectBase, IHasIdentifier, IDateVersionedEntity
    {
        public GetEntitiesByIds(ISessionFactory sessionFactory)
            : base(sessionFactory)
        {
        }

        // Static members, not shared between concrete generic types
        private static readonly string entityName = typeof(TEntity).Name;
        private static readonly Lazy<List<string>> aggregateHqlStatements = new Lazy<List<string>>(CreateAggregateHqlStatements, true);
        private static readonly string[] queryAliases = new[] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" };

        public IList<TEntity> GetByIds(IList<Guid> ids)
        {
            var shouldReleaseBind = EnsureSessionContextBinding();

            try
            {
                // Get aggregate root level data
                var query = GetAggregateLevelQuery(entityName, ids);

                // Process multiple results to a list of distinct aggregates (with appropriate children)
                IList<TEntity> results = query.ToList();

                return results;
            }
            finally
            {
                if (shouldReleaseBind)
                    CurrentSessionContext.Unbind(SessionFactory);
            }
        }

        private IEnumerable<TEntity> GetAggregateLevelQuery(string entityName, IList<Guid> ids)
        {
            IEnumerable<TEntity> query;
            string whereClause = null, orderByClause = null;

            // Closest to working perf, but still produces duplicate objects at level 2 of the aggregate, despite us of "DISTINCT" keyword in query
            var ancestorCollections = new List<string>();

            if (ids.Count == 1)
            {
                whereClause = " where a.Id = :id";

                string mainHql = string.Format(
                    "from {0} a {1}",
                    entityName, whereClause);

                query = Session.CreateQuery(mainHql)
                               .SetParameter("id", ids[0])
                               .Future<TEntity>();

                foreach (string hql in aggregateHqlStatements.Value)
                {
                    Session.CreateQuery(hql + whereClause + orderByClause)
                            .SetParameter("id", ids[0])
                            .Future<TEntity>();
                }
            }
            else
            {
                whereClause = " where a.Id IN (:ids)";
                orderByClause = " order by a.Id ";

                string mainHql = string.Format(
                    "from {0} a {1} {2}",
                    entityName, whereClause, orderByClause);

                query = Session.CreateQuery(mainHql)
                               .SetParameterList("ids", ids)
                               .Future<TEntity>();


                foreach (string hql in aggregateHqlStatements.Value)
                {
                    Session.CreateQuery(hql + whereClause + orderByClause)
                            .SetParameterList("ids", ids)
                            .Future<TEntity>();
                }
            }

            return query;
        }

        private static List<PropertyInfo> GetChildCollectionProperties(Type entityType)
        {
            var childCollectionProperties =
                (entityType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Concat(
                        from p in entityType.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic)
                        where p.Name.EndsWith("PeristentList")
                        select p
                ))
                .Where(p => p.PropertyType.IsGenericList())
                .ToList();

            return childCollectionProperties;
        }

        private static List<string> CreateAggregateHqlStatements()
        {
            var aggregateHqlStatementsOuter = new List<string>();
            var ancestorCollections = new List<string>();

            AddChildCollectionJoinQueries(aggregateHqlStatementsOuter, entityName, GetChildCollectionProperties(typeof(TEntity)), ancestorCollections);
            return aggregateHqlStatementsOuter;
        }

        private static void AddChildCollectionJoinQueries(List<string> aggregateHqlStatementsInner, string entityName, IEnumerable<PropertyInfo> childCollectionProperties, List<string> ancestorCollections)
        {
            foreach (var childCollectionProperty in childCollectionProperties)
            {
                string childCollectionName = childCollectionProperty.Name;

                // Original 2-level only implementation
                //string hql = string.Format(
                //    "from {0} a left join fetch a.{1} where a.Id IN (:ids)",
                //    entityName, childCollectionName);

                string hql2 = "from " + entityName + " a ";

                int aliasIndex = 0;

                foreach (string collection in ancestorCollections.Concat(new[] { childCollectionName }))
                {
                    hql2 += "left join fetch " + queryAliases[aliasIndex++] + "." + collection + " " + queryAliases[aliasIndex] + " ";
                }

                aggregateHqlStatementsInner.Add(hql2);

                var collectionTypeGenericArguments = childCollectionProperty.PropertyType.GetGenericArguments();

                Type childEntityType;

                if (!collectionTypeGenericArguments.Any())
                    // No generic arguments, so we must be dealing with a 1-to-1 child property reference
                    childEntityType = childCollectionProperty.PropertyType;
                else
                    // Generic arguments present, so treat as an IList<T>, where T is the child entity type
                    childEntityType = collectionTypeGenericArguments[0];

                var grandChildCollectionProperties = GetChildCollectionProperties(childEntityType);

                if (grandChildCollectionProperties.Any())
                {
                    var newAncestorCollection = new List<string>(ancestorCollections);
                    newAncestorCollection.Add(childCollectionName);
                    AddChildCollectionJoinQueries(aggregateHqlStatementsInner, entityName, grandChildCollectionProperties, newAncestorCollection);
                }
            }
        }

    }
}