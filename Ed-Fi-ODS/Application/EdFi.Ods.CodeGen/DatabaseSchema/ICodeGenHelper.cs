using System;
using System.Collections.Generic;
using System.Xml.Linq;
using EdFi.Ods.CodeGen.Models;

namespace EdFi.Ods.CodeGen.DatabaseSchema
{
    public interface ICodeGenHelper
    {
        string DatabaseName { get; }
        string Namespace { get; }
        string DefaultSchema { get; }
        List<string> ExcludedTables { get; }
        Table GetTable(string tableName);
        Table GetTable(IDictionary<string, Table> tablesByName, string tableName);
        bool IsExcluded(string tableName);
        List<string> GetCollidingPropertyNames(IDictionary<string, Table> tablesByName, IEnumerable<XElement> aggregatesElts);
        bool IsInAggregate(List<string> aggregateTableNames, string tableName);
        bool IsLookupTable(Table t);
        bool IsDirectLookupReference(Dictionary<string, Table> tablesByName, FKTable fkt);
        bool IsLookupTypeColumn(IDictionary<string, Table> tablesByName, Table table, string columnName);
        bool IsLookupTypeColumn(IDictionary<string, Table> tablesByName, Table table, string columnName, HashSet<Tuple<string, string>> visitedColumns, out string lookupTableName);
        void AppendPropertiesWithCollidingJsonNames(List<string> collidingPropertyNames, Table tbl, Table baseTbl, IEnumerable<FKTable> navigableChildFKs, IEnumerable<FKTable> baseNavigableChildFKs, IEnumerable<FKTable> navigableOneToOneChildFKs, IEnumerable<FKTable> baseNavigableOneToOneChildFKs);
        string ApplyPropertyNameConventions(string columnName, string tableName, List<string> collidingProperties);
        string ApplyPropertyNameConventions(string columnName, string tableName, List<string> collidingProperties, bool skipCamelCasing);
        bool IsColumnValueAutoAssigned(Table tbl, Column pkCol, Table baseTbl);
        string CamelCase(string propertyName);
        List<FKTable> GetNavigableParentRelationships(Table tbl, List<string> aggregateTableNames);
        List<FKTable> GetNonNavigableParentRelationships(Table tbl, List<string> aggregateTableNames);
        string GetRoleName(FKTable fkt);
        bool IsFKPartOfPK(Table table, FKTable fk);
        string CleanForSwaggerAttribute(string text);
        string GetPropertyType(Column column);
        Column GetOtherColumn(IDictionary<string, Table> tablesByName, FKTable fkt, string thisColumn);
        string GetLookupTableName(IDictionary<string, Table> tablesByName, Table table, string columnName);
        string GetFKOtherColumnName(FKTable fkt, string thisColumnName);
        IDictionary<string, Table> TablesByName { get; }
        T4ResourceModel DomainModel { get; }
        bool IsUniqueId(Table table, Column col);

        List<string> GetGenerationContexts(string aggregateRootName);
        bool ShouldGenerateAggregate(string aggregateRootName, string context);
        bool ShouldGenerateEntity(string aggregateRootName, string entityName, string context);
        bool ShouldGenerateMember(string aggregateRootName, string entityName, string memberName, string context);
        bool ShouldGenerateReference(string aggregateRootName, string entityName, FKTable fkt, string context);
    }
}