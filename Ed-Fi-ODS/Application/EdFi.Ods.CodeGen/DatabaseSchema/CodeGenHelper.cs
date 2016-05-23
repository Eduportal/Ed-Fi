using EdFi.Common.Inflection;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using EdFi.Ods.CodeGen.Models;

namespace EdFi.Ods.CodeGen.DatabaseSchema
{
    public class CodeGenHelper : ICodeGenHelper
    {
        private readonly IDatabaseSchemaProvider _databaseSchemaProvider;

        public CodeGenHelper(IDatabaseSchemaProvider databaseSchemaProvider, string extensionDomainMetadataPath)
        {
            _databaseSchemaProvider = databaseSchemaProvider;
            _tablesByName = GetTablesByName();
            _domainModel = new Lazy<T4ResourceModel>(() => new DomainModelFactory(this, extensionDomainMetadataPath).GetModel());
        }

        private Lazy<T4ResourceModel> _domainModel;

        public T4ResourceModel DomainModel
        {
            get { return _domainModel.Value; }
        }

        public string Namespace
        {
            get { return "EdFi.Ods.Entities"; }
        }

        public string DefaultSchema
        {
            get { return "edfi"; }
        }

        public bool IsUniqueId(Table table, Column col)
        {
            return (col.Name.EndsWith("UniqueId", StringComparison.InvariantCultureIgnoreCase) &&
                    col.Name.StartsWith(table.Name, StringComparison.InvariantCultureIgnoreCase));
        }

        public List<string> GetGenerationContexts(string aggregateRootName)
        {
            return new List<string>();
        }

        //this is a list of tables you don't want generated
        public List<string> ExcludedTables
        {
            get
            {
                return new List<string>{
                    "sysdiagrams",
                    "BuildVersion", 
                    "LocalEducationAgencyFederalFunds", // TODO: Naming issues with multiple/singular
                    "StateEducationAgencyFederalFunds", // TODO: Naming issues with multiple/singular
                    "EdFiException",
                };
            }
        }

        public string DatabaseName
        {
            get
            {
                return "EdFi_Ods";
            }
        }

        public string GetLookupTableName(IDictionary<string, Table> tablesByName, Table table, string columnName)
        {
            string lookupTableName;

            var visitedColumns = new HashSet<Tuple<string, string>>();

            if (this.IsLookupTypeColumn(tablesByName, table, columnName, visitedColumns, out lookupTableName))
                return lookupTableName;

            return null;
        }

        private IDictionary<string, Table> _tablesByName;

        private IDictionary<string, Table> GetTablesByName()
        {
            var tablesByName = new Dictionary<string, Table>(StringComparer.InvariantCultureIgnoreCase);

            foreach (var t in _databaseSchemaProvider.LoadTables())
                tablesByName[t.Name] = t;

            return tablesByName;
        }

        public IDictionary<string, Table> TablesByName
        {
            get { return _tablesByName; }
        }

        public Table GetTable(string tableName)
        {
            return GetTable(TablesByName, tableName);
        }

        public Table GetTable(IDictionary<string, Table> tablesByName, string tableName)
        {
            Table tbl;

            if (!tablesByName.TryGetValue(tableName, out tbl))
                throw new Exception(string.Format("Unable to find table '{0}'.", tableName));

            return tbl;
        }

        public bool IsExcluded(string tableName)
        {
            return this.ExcludedTables.Contains(tableName);

            /*  For debugging code generation with a small scope, uncomment this section and modify to taste
            return !(new List<string> {
                "Table1",
                ...
                "TableN" })
                .Contains(tableName);
            */
        }

        public List<string> GetCollidingPropertyNames(IDictionary<string, Table> tablesByName, IEnumerable<XElement> aggregatesElts)
        {
            List<string> propertiesWithCollidingJsonNames = new List<string>();

            // First, process all tables and columns for naming collisions when applying naming conventions related 
            // to translating Database-based C# names to JSON property names serialization.  This convention is used
            // to trim table names appearing as prefixes on columns, but collisions can result 
            // (e.g. EducationOrganization's EducationOrganizationCategory and School's SchoolCategory).

            // This enables us to detect, during code generation, when not to apply JSON conventions
            foreach (var aggregateElt in aggregatesElts)
            {
                string aggregateRootName = aggregateElt.Attribute("root").Value;

                List<string> aggregateTableNames =
                    (from e in aggregateElt.Descendants("Entity")
                     select e.Attribute("table").Value)
                        .ToList();

                foreach (string aggregateTableName in aggregateTableNames)
                {
                    Table tbl;

                    if (!tablesByName.TryGetValue(aggregateTableName, out tbl))
                        continue;

                    if (this.IsExcluded(tbl.Name))
                        continue;

                    var navigableChildFKs = tbl.GetNavigableChildTables(tablesByName).Where(ct => this.IsInAggregate(aggregateTableNames, ct.OtherTable)).ToList();
                    var navigableOneToOneChildFKs = tbl.GetNavigableOneToOneChildTables(tablesByName).Where(ct => this.IsInAggregate(aggregateTableNames, ct.OtherTable) && !this.IsExcluded(ct.OtherTable)).ToList();

                    bool isAbstract = aggregateElt.Descendants("Entity")
                                                  .Where(e => e.Attribute("table").Value == tbl.Name
                                                              && e.Attributes("isAbstract").FirstOrDefault(a => a.Value == "true") != null)
                                                  .Any();

                    if (isAbstract)
                        continue;

                    var baseTableNameAttr = aggregateElt.Descendants("Entity")
                                                        .Where(e => e.Attribute("table").Value == tbl.Name)
                                                        .Select(e => e.Attribute("isA"))
                                                        .SingleOrDefault();

                    string baseTableName = baseTableNameAttr == null ? null : baseTableNameAttr.Value;

                    Table baseTbl = null;

                    if (baseTableName != null)
                        tablesByName.TryGetValue(baseTableName, out baseTbl);

                    bool isDerived = baseTableName != null;

                    string baseAggregateRootName =
                        (from a in aggregatesElts
                         from e in a.Descendants("Entity")
                         where e.Attribute("table").Value == baseTableName
                         select a.Attribute("root").Value)
                            .SingleOrDefault();

                    List<string> baseAggregateTableNames =
                        (from a in aggregatesElts
                         where a.Attribute("root").Value == baseAggregateRootName
                         from e in a.Descendants("Entity")
                         select e.Attribute("table").Value)
                            .ToList();

                    var baseNavigableChildFKs = baseTbl == null ?
                        new List<FKTable>()
                        : baseTbl.GetNavigableChildTables(tablesByName).Where(ct => this.IsInAggregate(baseAggregateTableNames, ct.OtherTable)).ToList();

                    var baseNavigableOneToOneChildFKs = baseTbl == null ?
                        new List<FKTable>()
                        : baseTbl.GetNavigableOneToOneChildTables(tablesByName).Where(ct => this.IsInAggregate(baseAggregateTableNames, ct.OtherTable) && !this.IsExcluded(ct.OtherTable)).ToList();

                    this.AppendPropertiesWithCollidingJsonNames(propertiesWithCollidingJsonNames, tbl, baseTbl, navigableChildFKs, baseNavigableChildFKs, navigableOneToOneChildFKs, baseNavigableOneToOneChildFKs);
                }
            }

            return propertiesWithCollidingJsonNames;
        }

        public bool IsInAggregate(List<string> aggregateTableNames, string tableName)
        {
            return aggregateTableNames.Any(tn => tn.Equals(tableName, StringComparison.InvariantCultureIgnoreCase));
        }

        public bool IsLookupTable(Table t)
        {
            return this.IsTypeTable(t) || this.IsDescriptorTable(t);
        }

        public bool IsDirectLookupReference(Dictionary<string, Table> tablesByName, FKTable fkt)
        {
            var tbl = tablesByName[fkt.ThisTable];
            return fkt.ThisColumns.Count() == 1 && this.IsLookupTypeColumn(tablesByName, tbl, fkt.ThisColumns[0]);
        }
        private bool IsTypeTable(Table t)
        {
            if (!t.Name.EndsWith("Type"))
                return false;

            int qualifyingColumns = 0;

            // Type tables have at least 2 out of the 3 columns
            if (t.Columns.Any(c => c.Name == "CodeValue")) qualifyingColumns++;
            if (t.Columns.Any(c => c.Name == "ShortDescription")) qualifyingColumns++;
            if (t.Columns.Any(c => c.Name == "Description")) qualifyingColumns++;

            if (qualifyingColumns == 3)
                return true;
            else
                return false;
        }
        private bool IsDescriptorTable(Table t)
        {
            // Abstract table "Descriptor" itself is not considered a lookup table
            if (t.Name == "Descriptor")
                return false;

            if (t.Name.EndsWith("Descriptor") && t.PrimaryKeyColumns.Count == 1 && t.PrimaryKeyColumns[0].Name == t.Name + "Id")
                return true;

            return false;
        }


        public bool IsLookupTypeColumn(IDictionary<string, Table> tablesByName, Table table, string columnName)
        {
            string lookupTableName;

            var visitedColumns = new HashSet<Tuple<string, string>>();

            return this.IsLookupTypeColumn(tablesByName, table, columnName, visitedColumns, out lookupTableName);
        }

        public bool IsLookupTypeColumn(IDictionary<string, Table> tablesByName, Table table, string columnName, HashSet<Tuple<string, string>> visitedColumns, out string lookupTableName)
        {
            lookupTableName = null;

            // Don't re-evaluate already visisted columns (prevent infinite recursion)
            if (visitedColumns.Contains(Tuple.Create(table.Name, columnName)))
                return false;

            // Find column in the FKs
            var otherTableInfos =
                (from fkt in table.FKTables
                 where !fkt.IsPrimaryTable && fkt.ThisColumns.Contains(columnName)
                 let index = fkt.ThisColumns.IndexOf(columnName)
                 select new { OtherTable = tablesByName[fkt.OtherTable], OtherColumnName = fkt.OtherColumns[index] })
                    .ToList();

            // No more foreign keys to crawl? This is not a type column
            if (!otherTableInfos.Any())
                return false;

            // Try to find the Type table
            lookupTableName =
                (from fkt in otherTableInfos
                 where this.IsLookupTable(fkt.OtherTable)
                 select fkt.OtherTable.Name)
                    .FirstOrDefault();

            // Did we find it defined in a Type table?
            if (lookupTableName != null)
                return true;

            // Mark this column as visited
            visitedColumns.Add(Tuple.Create(table.Name, columnName));

            // Walk all the FK relationships recursively until we know whether any end in a lookup table.
            bool isLookupTypeColumn = false;

            foreach (var otherTableInfo in otherTableInfos)
            {
                isLookupTypeColumn |= this.IsLookupTypeColumn(tablesByName, otherTableInfo.OtherTable, otherTableInfo.OtherColumnName, visitedColumns, out lookupTableName);

                if (isLookupTypeColumn)
                    break;
            }

            return isLookupTypeColumn;
        }

        public void AppendPropertiesWithCollidingJsonNames(List<string> collidingPropertyNames, Table tbl, Table baseTbl, IEnumerable<FKTable> navigableChildFKs, IEnumerable<FKTable> baseNavigableChildFKs, IEnumerable<FKTable> navigableOneToOneChildFKs, IEnumerable<FKTable> baseNavigableOneToOneChildFKs)
        {
            //Write("// DEBUG: Searching for JSON collisions in " + tbl.Name + "\r\n");

            // List<string> collidingPropertyNames = new List<string>();
            var propertyNameByJsonName = new Dictionary<string, string>();

            // Determine the JSON names for the columns of the derived table
            foreach (var column in tbl.Columns)
            {
                string jsonName = this.ApplyPropertyNameConventions(column.CleanName, tbl.Name, null);
                propertyNameByJsonName[jsonName] = column.CleanName;

                //Write("// DEBUG: Deriving JSON column name (" + tbl.Name + "): " + column.CleanName + " -> " + jsonName + "\r\n");
            }

            // Determine the JSON names for the navigable children of the derived table
            foreach (var fkt in navigableChildFKs)
            {
                string jsonName = this.ApplyPropertyNameConventions(fkt.OtherQueryable, tbl.Name, null);
                propertyNameByJsonName[jsonName] = fkt.OtherQueryable;
            }

            foreach (var fkt in navigableOneToOneChildFKs)
            {
                string jsonName = this.ApplyPropertyNameConventions(fkt.OtherClass, tbl.Name, null);

                string columnName = null;

                if (propertyNameByJsonName.TryGetValue(jsonName, out columnName))
                {
                    //Write("// WARNING: JSON collision detected: " + columnName + "\r\n");

                    // Add both columns to the list, using <tbl>.<col> syntax
                    collidingPropertyNames.Add(tbl.Name + "." + fkt.OtherClass);
                    collidingPropertyNames.Add(tbl.Name + "." + columnName);
                }
                else
                {
                    propertyNameByJsonName[jsonName] = fkt.OtherClass;
                }
            }

            if (baseTbl != null)
            {
                // Determine the JSON names for the columns of the base table
                foreach (var column in baseTbl.Columns)
                {
                    string jsonName = this.ApplyPropertyNameConventions(column.CleanName, baseTbl.Name, null);

                    //Write("// DEBUG: Deriving JSON column name (" + baseTbl.Name + "): " + column.CleanName + " -> " + jsonName + "\r\n");

                    string columnName = null;

                    if (propertyNameByJsonName.TryGetValue(jsonName, out columnName))
                    {
                        //Write("// WARNING: JSON collision detected: " + columnName + "\r\n");

                        // Add both columns to the list, using <tbl>.<col> syntax
                        collidingPropertyNames.Add(baseTbl.Name + "." + column.Name);
                        collidingPropertyNames.Add(tbl.Name + "." + columnName);
                    }
                    else
                    {
                        propertyNameByJsonName[jsonName] = column.CleanName;
                    }
                }
            }

            // Determine the JSON names for the navigable children of the base table
            foreach (var fkt in baseNavigableChildFKs)
            {
                string jsonName = this.ApplyPropertyNameConventions(fkt.OtherQueryable, baseTbl.Name, null);

                //Write("// DEBUG: Deriving JSON column name (" + baseTbl.Name + "): " + fkt.OtherQueryable + " -> " + jsonName + "\r\n");

                string otherQueryable = null;

                if (propertyNameByJsonName.TryGetValue(jsonName, out otherQueryable))
                {
                    //Write("// WARNING: JSON collision detected: " + otherQueryable + "\r\n");

                    // Add both columns to the list, using <tbl>.<col> syntax
                    collidingPropertyNames.Add(baseTbl.Name + "." + fkt.OtherQueryable);
                    collidingPropertyNames.Add(tbl.Name + "." + otherQueryable);
                }
                else
                {
                    propertyNameByJsonName[jsonName] = fkt.OtherQueryable;
                }
            }

            foreach (var fkt in baseNavigableOneToOneChildFKs)
            {
                string jsonName = this.ApplyPropertyNameConventions(fkt.OtherClass, tbl.Name, null);

                string columnName = null;

                if (propertyNameByJsonName.TryGetValue(jsonName, out columnName))
                {
                    //Write("// WARNING: JSON collision detected: " + columnName + "\r\n");

                    // Add both columns to the list, using <tbl>.<col> syntax
                    collidingPropertyNames.Add(baseTbl.Name + "." + fkt.OtherClass);
                    collidingPropertyNames.Add(tbl.Name + "." + columnName);
                }
                else
                {
                    propertyNameByJsonName[jsonName] = fkt.OtherClass;
                }
            }

            //return collidingPropertyNames;
        }

        public string ApplyPropertyNameConventions(string columnName, string tableName, List<string> collidingProperties)
        {
            return this.ApplyPropertyNameConventions(columnName, tableName, collidingProperties, false);
        }

        public string ApplyPropertyNameConventions(string columnName, string tableName, List<string> collidingProperties, bool skipCamelCasing)
        {
            string fqn = tableName + "." + columnName;

            if (collidingProperties != null && collidingProperties.Contains(fqn))
                return skipCamelCasing ? columnName : this.CamelCase(columnName);

            if (columnName.StartsWith(tableName))
            {
                if (columnName.EndsWith("USI") || columnName.EndsWith("TypeId") || columnName.EndsWith("Id") || columnName.EndsWith("X")) // Descriptor Id as well?
                {
                    return skipCamelCasing ? columnName : this.CamelCase(columnName.TrimEnd('X'));
                }

                return skipCamelCasing ? columnName.Substring(tableName.Length) : this.CamelCase(columnName.Substring(tableName.Length));
            }

            return skipCamelCasing ? columnName : this.CamelCase(columnName);
        }

        public bool IsColumnValueAutoAssigned(Table tbl, Column pkCol, Table baseTbl)
        {
            // Is the column an identity column?
            if (pkCol.AutoIncrement)
                return true;

            // Is the table not derived from another?
            if (baseTbl == null)
                return false;

            // Check the base table's PK column definition
            var baseFkt = tbl.FKTables.Where(fkt => fkt.OtherTable == baseTbl.Name).SingleOrDefault();

            if (baseFkt == null)
                throw new Exception(string.Format("Unable to find FK relationship from table '{0}' to base table '{1}'.", tbl.Name, baseTbl.Name));

            int colPos = baseFkt.ThisColumns.IndexOf(pkCol.Name);

            if (colPos < 0)
                return false;

            string basePKColName = baseFkt.OtherColumns[colPos];

            Column basePKCol = baseTbl.Columns.Single(c => c.Name == basePKColName);

            return basePKCol.AutoIncrement;
        }
        public string CamelCase(string propertyName)
        {
            if (propertyName.Length == 1)
                return propertyName.ToLower();

            int leadingUpperChars = propertyName.TakeWhile(c => char.IsUpper(c)).Count();
            int prefixLength = leadingUpperChars - 1;

            if (propertyName.Length == leadingUpperChars
                || (propertyName.Length == leadingUpperChars + 1 && propertyName.EndsWith("s"))) // Handles the case of an acronym with a trailing "s" (e.g. "URIs" -> "uris" not "urIs")
                // Convert entire name to lower case
                return propertyName.ToLower();
            else if (prefixLength > 0)
                // Apply lower casing to leading acronym
                return propertyName.Substring(0, prefixLength).ToLower()
                       + propertyName.Substring(prefixLength);
            else
            // Apply camel casing
                return propertyName.Substring(0, 1).ToLower() + propertyName.Substring(1);
        }

        public List<FKTable> GetNavigableParentRelationships(Table tbl, List<string> aggregateTableNames)
        {
            var parentFKs = tbl.FKTables.Where(fkt => !fkt.IsPrimaryTable && this.IsInAggregate(aggregateTableNames, fkt.OtherTable) && !this.IsExcluded(fkt.OtherTable)
                                                      && fkt.OtherTable != fkt.ThisTable).ToList();
            // ^^^^^^^-------- Stop self recursive relationships from being mapped/serialized through object references 

            // Identify multiple FKs to the same table
            var groupedFKs =
                (from fk in parentFKs
                 group fk by fk.OtherTable into g
                 where g.Count() > 1
                 select g)
                    .ToList();

            // Filter out multiple FKs to the same table
            if (groupedFKs.Count() > 0)
            {
                // We need to remove the FK that results in a role name
                foreach (var grouping in groupedFKs)
                {
                    foreach (var fk in grouping)
                    {
                        string sortedThisColumns = string.Join("|", Enumerable.OrderBy(fk.ThisColumns, n => n));
                        string sortedOtherColumns = string.Join("|", Enumerable.OrderBy(fk.OtherColumns, n => n));

                        if (sortedThisColumns != sortedOtherColumns)
                        {
                            // It's a reference with a role name, so remove it from our parentFKs list.
                            parentFKs.Remove(fk);
                        }
                    }
                }
            }

            return parentFKs;
        }

        // Get relationships where the target is the primary and isn't in the current aggregate, except for self references
        public List<FKTable> GetNonNavigableParentRelationships(Table tbl, List<string> aggregateTableNames)
        {
            var navigableParentRelationship = GetNavigableParentRelationships(tbl, aggregateTableNames);
            // Parent FKs where the parent is not in the aggregate.
            var parentFKs = tbl.FKTables.Where(fkt => !fkt.IsPrimaryTable && !this.IsExcluded(fkt.OtherTable)
                                                      && !navigableParentRelationship.Contains(fkt) // Filter out the navigable parent relationships
                                                      ).ToList();
            return parentFKs;
        }

        public string GetRoleName(FKTable fkt)
        {
            var nameParts = this.GetForeignKeyNameParts(fkt);

            if (nameParts == null)
                return null;

            return nameParts.RoleName;
        }

        ForeignKeyNameParts GetForeignKeyNameParts(FKTable fkt)
        {
            if (fkt.IsPrimaryTable)
            {
                // Only allowing role name processing for parent side of 1->1 relationships
                var otherTable = GetTable(fkt.OtherTable);

                if (otherTable.PKs.Count != fkt.OtherColumns.Count
                    || otherTable.PKs.Any(pkCol => !fkt.OtherColumns.Contains(pkCol.Name)))
                {
                    throw new Exception(
                        string.Format(
                            "Unable to process the parent side of a foreign key ({0}.{1} --> {2}.{3}) for role name or qualifier.",
                            fkt.ThisTableSchema,
                            fkt.ThisTable,
                            fkt.OtherTableSchema,
                            fkt.OtherTable));
                }
            }

            for (int i = 0; i < fkt.ThisColumns.Count; i++)
            {
                if (fkt.ThisColumns[i] != fkt.OtherColumns[i])
                {
                    string coreName = this.TrimSuffix(fkt.OtherColumns[i], "Id");
                    string pattern = @"^(?<RoleName>\w*)" + coreName + @"(?<Qualifier>\w*)$";
                    var match = Regex.Match(fkt.ThisColumns[i], pattern);

                    if (match.Success)
                    {
                        var parts = new ForeignKeyNameParts
                        {
                            RoleName = match.Groups["RoleName"].Value,
                            Core = coreName,
                            Qualifier = match.Groups["Qualifier"].Value
                        };

                        // Don't let role name be the same as the parent table, or strangeness in naming will occur
                        if (parts.RoleName == fkt.OtherTable)
                            parts.RoleName = null;

                        return parts;
                    }
                }
            }

            return null;
        }

        string TrimSuffix(string text, string suffix)
        {
            return Regex.Replace(text, @"^(\w+)" + suffix + "$", "$1");
        }

        public bool IsFKPartOfPK(Table table, FKTable fk)
        {
            return table.PKs.Any(c => fk.ThisColumns.Contains(c.Name));
        }

        public string CleanForSwaggerAttribute(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            return
                Regex.Replace(text, @"[^ -~]", " ") // Remove characters that break Swagger presentation (anything outside of {space} through {tilde}
                     .Replace(@"""", @"\""");                // Escape embedded quotes
        }

        public string GetPropertyType(Column column)
        {
            if (column.IsNullable && column.SysType != "string")
                return column.SysType + "?";

            return column.SysType;
        }

        public Column GetOtherColumn(IDictionary<string, Table> tablesByName, FKTable fkt, string thisColumn)
        {
            for (int i = 0; i < fkt.ThisColumns.Count; i++)
            {
                if (fkt.ThisColumns[i] == thisColumn)
                {
                    string otherColumn = fkt.OtherColumns[i];
                    return
                        (from c in tablesByName[fkt.OtherTable].Columns
                         where c.Name == otherColumn
                         select c)
                            .Single();
                }
            }

            throw new Exception(string.Format("Unable to find column '{0}' in foreign key constraint '{1}' between tables '{2}' and '{3}'.",
                thisColumn, fkt.ConstraintName, fkt.ThisTable, fkt.OtherTable));
        }

        public string GetFKOtherColumnName(FKTable fkt, string thisColumnName)
        {
            int index = fkt.ThisColumns.IndexOf(thisColumnName);

            if (index < 0)
                throw new Exception(string.Format("Unable to find column '{0}' in foreign key from table '{1}' to table '{2}' ({3}).", thisColumnName, fkt.ThisTable, fkt.OtherTable, fkt.ConstraintName));

            return fkt.OtherColumns[index];
        }

        public bool ShouldGenerateAggregate(string aggregateRootName, string context)
        {
            return true;
        }

        public bool ShouldGenerateEntity(string aggregateRootName, string entityName, string context)
        {
            return true;
        }

        public bool ShouldGenerateMember(string aggregateRootName, string entityName, string memberName, string context)
        {
            return true;
        }

        public bool ShouldGenerateReference(string aggregateRootName, string entityName, FKTable incomingFK, string context)
        {
            return true;
        }
    }
}
