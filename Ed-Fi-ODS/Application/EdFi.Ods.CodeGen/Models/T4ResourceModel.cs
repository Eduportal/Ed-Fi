using System;
using EdFi.Common.Extensions;

namespace EdFi.Ods.CodeGen.Models
{
    using System.Collections.Generic;
    using System.Linq;

    public class T4ResourceModel
    {
        public T4ResourceModel()
        {
            this.Classes = new List<CodeGenClassModel>();
            this.ExcludedTables = new List<string>(); 
            
            _classModelsByName = new Lazy<IDictionary<string, CodeGenClassModel>>(
                    () => Classes.ToDictionary(x => x.Name, x => x, StringComparer.InvariantCultureIgnoreCase));
        }

        public string DatabaseName { get; set; }
        public List<CodeGenClassModel> Classes { get; set; }
        public List<CodeGenClassModel> NonAbstractClasses { get { return this.Classes.Where(t => !t.IsAbstract).ToList(); } }
        public List<string> ExcludedTables { get; set; }
        public List<string> PropertiesWithCollidingJsonNames { get; set; }

        private readonly Lazy<IDictionary<string, CodeGenClassModel>> _classModelsByName;

        public virtual CodeGenClassModel GetClassModel(string entityName)
        {
            CodeGenClassModel entity;
            
            if (!_classModelsByName.Value.TryGetValue(entityName, out entity))
                throw new Exception(string.Format("Unable to find entity '{0}'.", entityName));

            return entity;
        }

        public bool IsExcluded(string tableName)
        {
            return this.ExcludedTables.Contains(tableName);
        }

        public IEnumerable<KeyValuePair<string, List<CodeGenClassModel>>> ClassesByAggregate
        {
            get
            {
                return
                    this.Classes.GroupBy(t => t.AggregateRootName)
                        .Select(t => new KeyValuePair<string, List<CodeGenClassModel>>(t.Key, t.Select(x => x).ToList()));
            }
        }
        public IEnumerable<KeyValuePair<string, List<CodeGenClassModel>>> NonAbstractClassesByAggregate
        {
            get
            {
                return
                    this.NonAbstractClasses.GroupBy(t => t.AggregateRootName)
                        .Select(t => new KeyValuePair<string, List<CodeGenClassModel>>(t.Key, t.Select(x => x).ToList()));
            }
        }
    }
}
