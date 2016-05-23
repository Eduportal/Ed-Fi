// ****************************************************************************
//  Copyright (C) 2015 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System;
using System.Collections.Generic;
using System.Xml.Linq;
using EdFi.Ods.CodeGen.Models;

namespace EdFi.Ods.CodeGen.DatabaseSchema
{
    /// <summary>
    /// Provides a base class with virtual methods for overriding only what behavior is being
    /// decorated.
    /// </summary>
    public class CodeGenHelperDecoratorBase : ICodeGenHelper
    {
        protected readonly ICodeGenHelper DecoratedHelper;

        public CodeGenHelperDecoratorBase(ICodeGenHelper decoratedHelper)
        {
            DecoratedHelper = decoratedHelper;
        }

        public virtual void AppendPropertiesWithCollidingJsonNames(List<string> collidingPropertyNames, Table tbl, Table baseTbl,
            IEnumerable<FKTable> navigableChildFKs, IEnumerable<FKTable> baseNavigableChildFKs, IEnumerable<FKTable> navigableOneToOneChildFKs,
            IEnumerable<FKTable> baseNavigableOneToOneChildFKs)
        {
            DecoratedHelper.AppendPropertiesWithCollidingJsonNames(collidingPropertyNames, tbl, baseTbl, navigableChildFKs, baseNavigableChildFKs, navigableOneToOneChildFKs, baseNavigableOneToOneChildFKs);
        }

        public virtual string ApplyPropertyNameConventions(string columnName, string tableName, List<string> collidingProperties, bool skipCamelCasing)
        {
            return DecoratedHelper.ApplyPropertyNameConventions(columnName, tableName, collidingProperties, skipCamelCasing);
        }

        public virtual string ApplyPropertyNameConventions(string columnName, string tableName, List<string> collidingProperties)
        {
            return DecoratedHelper.ApplyPropertyNameConventions(columnName, tableName, collidingProperties);
        }

        public virtual string CamelCase(string propertyName)
        {
            return DecoratedHelper.CamelCase(propertyName);
        }

        public virtual string CleanForSwaggerAttribute(string text)
        {
            return DecoratedHelper.CleanForSwaggerAttribute(text);
        }

        public virtual string DatabaseName
        {
            get { return DecoratedHelper.DatabaseName; }
        }

        public virtual string DefaultSchema
        {
            get { return DecoratedHelper.DefaultSchema; }
        }

        public virtual List<string> ExcludedTables
        {
            get { return DecoratedHelper.ExcludedTables; }
        }

        public virtual List<string> GetCollidingPropertyNames(IDictionary<string, Table> tablesByName, IEnumerable<XElement> aggregatesElts)
        {
            return DecoratedHelper.GetCollidingPropertyNames(tablesByName, aggregatesElts);
        }

        public virtual string GetFKOtherColumnName(FKTable fkt, string thisColumnName)
        {
            return DecoratedHelper.GetFKOtherColumnName(fkt, thisColumnName);
        }

        public virtual IDictionary<string, Table> TablesByName
        {
            get { return DecoratedHelper.TablesByName; }
        }

        public virtual T4ResourceModel DomainModel
        {
            get { return DecoratedHelper.DomainModel; }
        }

        public virtual string GetLookupTableName(IDictionary<string, Table> tablesByName, Table table, string columnName)
        {
            return DecoratedHelper.GetLookupTableName(tablesByName, table, columnName);
        }

        public virtual List<FKTable> GetNavigableParentRelationships(Table tbl, List<string> aggregateTableNames)
        {
            return DecoratedHelper.GetNavigableParentRelationships(tbl, aggregateTableNames);
        }

        public virtual List<FKTable> GetNonNavigableParentRelationships(Table tbl, List<string> aggregateTableNames)
        {
            return DecoratedHelper.GetNonNavigableParentRelationships(tbl, aggregateTableNames);
        }

        public virtual Column GetOtherColumn(IDictionary<string, Table> tablesByName, FKTable fkt, string thisColumn)
        {
            return DecoratedHelper.GetOtherColumn(tablesByName, fkt, thisColumn);
        }

        public virtual string GetPropertyType(Column column)
        {
            return DecoratedHelper.GetPropertyType(column);
        }

        public virtual string GetRoleName(FKTable fkt)
        {
            return DecoratedHelper.GetRoleName(fkt);
        }

        public virtual Table GetTable(string tableName)
        {
            return DecoratedHelper.GetTable(tableName);
        }

        public virtual Table GetTable(IDictionary<string, Table> tablesByName, string tableName)
        {
            return DecoratedHelper.GetTable(tablesByName, tableName);
        }

        public virtual bool IsUniqueId(Table table, Column col)
        {
            return DecoratedHelper.IsUniqueId(table, col);
        }

        public virtual bool ShouldGenerateAggregate(string aggregateRootName, string context)
        {
            return DecoratedHelper.ShouldGenerateAggregate(aggregateRootName, context);
        }

        public virtual bool ShouldGenerateEntity(string aggregateRootName, string entityName, string context)
        {
            return DecoratedHelper.ShouldGenerateEntity(aggregateRootName, entityName, context);
        }

        public virtual bool ShouldGenerateMember(string aggregateRootName, string entityName, string memberName, string context)
        {
            return DecoratedHelper.ShouldGenerateMember(aggregateRootName, entityName, memberName, context);
        }

        public virtual bool ShouldGenerateReference(string aggregateRootName, string entityName, FKTable incomingFK, string context)
        {
            return DecoratedHelper.ShouldGenerateReference(aggregateRootName, entityName, incomingFK, context);
        }

        public virtual List<string> GetGenerationContexts(string aggregateRootName)
        {
            return DecoratedHelper.GetGenerationContexts(aggregateRootName);
        }

        public virtual bool IsColumnValueAutoAssigned(Table tbl, Column pkCol, Table baseTbl)
        {
            return DecoratedHelper.IsColumnValueAutoAssigned(tbl, pkCol, baseTbl);
        }

        public virtual bool IsDirectLookupReference(Dictionary<string, Table> tablesByName, FKTable fkt)
        {
            return DecoratedHelper.IsDirectLookupReference(tablesByName, fkt);
        }

        public virtual bool IsExcluded(string tableName)
        {
            return DecoratedHelper.IsExcluded(tableName);
        }

        public virtual bool IsFKPartOfPK(Table table, FKTable fk)
        {
            return DecoratedHelper.IsFKPartOfPK(table, fk);
        }

        public virtual bool IsInAggregate(List<string> aggregateTableNames, string tableName)
        {
            return DecoratedHelper.IsInAggregate(aggregateTableNames, tableName);
        }

        public virtual bool IsLookupTable(Table t)
        {
            return DecoratedHelper.IsLookupTable(t);
        }

        public virtual bool IsLookupTypeColumn(IDictionary<string, Table> tablesByName, Table table, string columnName)
        {
            return DecoratedHelper.IsLookupTypeColumn(tablesByName, table, columnName);
        }

        public virtual bool IsLookupTypeColumn(IDictionary<string, Table> tablesByName, Table table, string columnName, HashSet<Tuple<string, string>> visitedColumns, out string lookupTableName)
        {
            return DecoratedHelper.IsLookupTypeColumn(tablesByName, table, columnName, visitedColumns, out lookupTableName);
        }

        public virtual string Namespace
        {
            get { return DecoratedHelper.Namespace; }
        }
    }
}