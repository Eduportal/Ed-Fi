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
    public class DescriptorLookupProvider : IDescriptorLookupProvider
    {
        private readonly ISessionFactory _sessionFactory;
        private readonly ILog _log = LogManager.GetLogger(typeof(TypeLookupProvider));
        private readonly Lazy<List<string>> _descriptorList = new Lazy<List<string>>(LoadDescriptorList);

        public DescriptorLookupProvider(ISessionFactory sessionFactory)
        {
            _sessionFactory = sessionFactory;
        }

        private static List<string> LoadDescriptorList()
        {
            return typeof (Marker_EdFi_Ods_Entities_NHibernate)
                    .Assembly.GetTypes()
                    .Where(t => t.IsClass && EdFiDescriptorEntitySpecification.IsEdFiDescriptorEntity(t) && t.Name != "Descriptor")
                    .Select(t => t.Name)
                    .OrderBy(x => x)
                    .ToList();
        }

        private IList<DescriptorLookup> GetDescriptorCacheData(string descriptorName, int? id = null)
        {
            if (!EdFiDescriptorEntitySpecification.IsEdFiDescriptorEntity(descriptorName))
            {
                throw new ArgumentException(descriptorName + " is not a descriptor, but is being passed as one", "descriptorName");
            }
            try
            {
                using (var session = _sessionFactory.OpenSession())
                {
                    var descriptorIdColumnName = descriptorName + "Id";
                    var criteria = session.CreateCriteria(descriptorName).SetProjection(
                        Projections.ProjectionList()
                            .Add(Projections.Property(descriptorIdColumnName), "Id")
                            .Add(Projections.Property("Namespace"), "Namespace")
                            .Add(Projections.Property("CodeValue"), "CodeValue")
                            .Add(Projections.Constant(descriptorName), "DescriptorName"));
                    if (id.HasValue)
                    {
                        criteria.Add(Restrictions.Eq(descriptorIdColumnName, id.Value));
                    }
                    return criteria.SetResultTransformer(Transformers.AliasToBean<DescriptorLookup>()).List<DescriptorLookup>();
                }
            }
            catch (Exception ex)
            {
                if (_log != null) _log.Error(string.Format("Error creating hql query for {0}.", descriptorName), ex);
                throw;
            }
        }

        public DescriptorLookup GetSingleDescriptorLookupById(string descriptorName, int id)
        {
            return GetDescriptorCacheData(descriptorName, id).SingleOrDefault();
        }

        public IList<DescriptorLookup> GetDescriptorLookupsByDescriptorName(string descriptorName)
        {
            return GetDescriptorCacheData(descriptorName);
        }

        public IDictionary<string, IList<DescriptorLookup>> GetAllDescriptorLookups()
        {
            return
                _descriptorList.Value.Select(descriptorName => new { Key = descriptorName, Value = GetDescriptorCacheData(descriptorName)})
                    .ToDictionary(d => d.Key, d => d.Value);
        }
    }
}