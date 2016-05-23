using System;
using System.Collections.Generic;
using System.Linq;
using EdFi.Ods.Common.Specifications;
using EdFi.Ods.Entities.Common.Caching;
using EdFi.Ods.Entities.NHibernate;
using log4net;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;

namespace EdFi.Ods.Entities.Repositories.NHibernate.Architecture
{
    public class TypeLookupProvider : ITypeLookupProvider
    {
        private readonly ISessionFactory _sessionFactory;
        private readonly ILog _log = LogManager.GetLogger(typeof(TypeLookupProvider));
        private readonly Lazy<List<string>> _typeList = new Lazy<List<string>>(LoadTypeList); 

        public TypeLookupProvider(ISessionFactory sessionFactory)
        {
            _sessionFactory = sessionFactory;
        }

        private static List<string> LoadTypeList()
        {
            return typeof (Marker_EdFi_Ods_Entities_NHibernate)
                    .Assembly.GetTypes()
                    .Where(t => t.IsClass && EdFiTypeEntitySpecification.IsEdFiTypeEntity(t))
                    .Select(t => t.Name)
                    .OrderBy(x => x)
                    .ToList();
        }

        private IList<TypeLookup> GetTypeCacheData(string typeName, int? id = null, string shortDescription = null)
        {
            if (!EdFiTypeEntitySpecification.IsEdFiTypeEntity(typeName))
            {
                throw new ArgumentException(typeName + " is not a type, but is being passed as one", "typeName");
            }
            try
            {
                using (var session = _sessionFactory.OpenSession())
                {
                    var typeIdColumnName = typeName + "Id";
                    var criteria = session.CreateCriteria(typeName);
                    if (id.HasValue)
                    {
                        criteria.Add(Restrictions.Eq(typeIdColumnName, id.Value));
                    }
                    else if (!string.IsNullOrWhiteSpace(shortDescription))
                    {
                        criteria.Add(Restrictions.Eq("ShortDescription", shortDescription));
                    }
                    return criteria.SetProjection(
                        Projections.ProjectionList()
                            .Add(Projections.Property(typeIdColumnName), "Id")
                            .Add(Projections.Property("ShortDescription"), "ShortDescription")
                            .Add(Projections.Constant(typeName), "TypeName"))
                        .SetResultTransformer(Transformers.AliasToBean<TypeLookup>())
                        .List<TypeLookup>();
                }
            }
            catch (Exception ex)
            {
                if (_log != null) _log.Error(string.Format("Error creating hql query for {0}.", typeName), ex);
                throw;
            }
        }

        public TypeLookup GetSingleTypeLookupById(string typeName, int id)
        {
            return GetTypeCacheData(typeName, id).SingleOrDefault();
        }

        public TypeLookup GetSingleTypeLookupByShortDescription(string typeName, string shortDescription)
        {
            return GetTypeCacheData(typeName, null, shortDescription).SingleOrDefault();
        }

        public IDictionary<string, IList<TypeLookup>> GetAllTypeLookups()
        {
            return
                _typeList.Value.Select(typeName => new {Key = typeName, Value = GetTypeCacheData(typeName)})
                    .ToDictionary(t => t.Key, t => t.Value);
        }
    }
}