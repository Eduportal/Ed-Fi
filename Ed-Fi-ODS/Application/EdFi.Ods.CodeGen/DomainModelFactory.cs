using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

using EdFi.Ods.CodeGen.DatabaseSchema;
using EdFi.Ods.CodeGen.Models;

namespace EdFi.Ods.CodeGen
{
    public class DomainModelFactory
    {
        private readonly ICodeGenHelper _codeGenHelper;
        private readonly XDocument _domainMetadataXDocument;
        private readonly IEnumerable<XElement> _aggregateElts;

        public DomainModelFactory(ICodeGenHelper codeGenHelper, string extensionDomainMetadataFolderName)
        {
            _codeGenHelper = codeGenHelper;
            _domainMetadataXDocument = MetadataHelper.GetDomainMetadata(extensionDomainMetadataFolderName);
            _aggregateElts = _domainMetadataXDocument.Descendants("Aggregate");
        }

        public XDocument DomainMetadata { get { return _domainMetadataXDocument; } }

        public T4ResourceModel GetModel()
        {
            var model = new T4ResourceModel();
            var tablesByName = _codeGenHelper.TablesByName;

            // Determine what tables in the database have not been explicitly handled
            var unhandledTableNames =
                _codeGenHelper.TablesByName.Keys
                .Except(_domainMetadataXDocument.Descendants("Entity").Select(x => x.Attribute("table").Value))
                .Where(x => !_codeGenHelper.IsExcluded(x) && !x.EndsWith("Extension")) // Extension tables don't need to be explicitly handled, they are joined by convention
                .ToList();

            if (unhandledTableNames.Any())
                throw new Exception(string.Format("The following tables have not been explicitly handled:\r\n{0}", string.Join("\r\n\t", unhandledTableNames)));

            foreach (var aggregateElt in _aggregateElts)
            {
                var aggregateTableNames =
                    (from e in aggregateElt.Descendants("Entity")
                     select e.Attribute("table").Value)
                        .ToList();

                string rootTableName = aggregateElt.AttributeValue("root");
                bool allowPrimaryUpdates = Convert.ToBoolean(aggregateElt.AttributeValue("allowPrimaryKeyUpdates"));

                foreach (var tbl in aggregateTableNames.Where(x => !_codeGenHelper.IsExcluded(x)).Select(aggregateTableName => _codeGenHelper.GetTable(aggregateTableName)))
                {
                    var codeGenClassModel = ToCodeGenClassModel(tbl, aggregateElt, aggregateTableNames, tablesByName);

                    if (allowPrimaryUpdates && tbl.Name == rootTableName)
                        codeGenClassModel.AllowPrimaryKeyUpdates = true;

                    model.Classes.Add(codeGenClassModel);
                   
                }
            }

            return model;
        }

        private CodeGenClassModel ToCodeGenClassModel(Table table, XElement element, List<string> aggregateTableNames,
            IDictionary<string, Table> tablesByName)
        {
            var aggregateRootName = element.Attribute("root").Value;
            string baseTableName = GetBaseTableName(element, table);

            Table aggregateTable = null;
            _codeGenHelper.TablesByName.TryGetValue(aggregateRootName, out aggregateTable);
            var isBaseTable = table.Name == GetBaseTableName(element, aggregateTable);
            var baseAggregateTableNames = new List<string>();
            
            if (isBaseTable)
            {
                baseAggregateTableNames = _aggregateElts.First(elt =>
                    elt.Attribute("root").Value == table.Name)
                    .Descendants("Entity")
                    .Select(entity => entity.Attribute("table").Value)
                    .ToList();
            }
            var isRootTable = table.Name.Equals(aggregateRootName, StringComparison.InvariantCultureIgnoreCase);

            var tm = new CodeGenClassModel
            {
                IsDerived = baseTableName != null,
                IsAbstract = IsAbstract(table, element),
                IsRootTable = isRootTable,
                Name = table.Name,
                Schema = table.Schema,
                AggregateRootName = aggregateRootName,
                AggregateTableNames = (isRootTable) ? aggregateTableNames : (isBaseTable ? baseAggregateTableNames : new List<string>()),
                UnderlyingTable = table
            };

            tm.PKs = GetPKs(tm, tablesByName, baseTableName, table);
            tm.ParentFkTable = tm.IsRootTable
                ? null
                : _codeGenHelper.GetNavigableParentRelationships(table, aggregateTableNames).FirstOrDefault();
            tm.NonPrimaryKeyColumns = GetNonPrimaryKeyColumns(tablesByName, table);
            tm.ChildTables = GetChildTables(tablesByName, table, aggregateTableNames);

            Table baseTbl = null;

            if (baseTableName != null)
                _codeGenHelper.TablesByName.TryGetValue(baseTableName, out baseTbl);

            tm.BaseChildTables = GetBaseChildTables(tablesByName, baseTableName, baseTbl);
            tm.DerivedTables = GetDerivedTables(tablesByName, table);
            tm.IsBaseTable = tm.DerivedTables.Any();
            tm.OneToOneChildFKs = GetOneToOneChildFKs(table, aggregateTableNames, tablesByName);
            tm.IsParentFkPartOfPrimary = IsParentFkPartOfPrimary(tm, table);

            if (baseTbl != null)
            {
                tm.BaseTableColumns = GetBaseTableColumns(baseTbl, tablesByName);
                tm.BaseClass = ToCodeGenClassModel(baseTbl, element, aggregateTableNames, tablesByName);
            }

            tm.IsBaseTableAbstract = IsBaseTableAbstract(tm, baseTbl);
            tm.Implements = GetInterfaces(tm);
            tm.IsLookup = _codeGenHelper.IsLookupTable(table);

            tm.AllCascadingChildConstraints = new Lazy<List<FKTable>>(() => GetAllCascadingChildConstraints(table, null, tablesByName));

            return tm;
        }

        private List<FKTable> GetAllCascadingChildConstraints(Table tbl, FKTable cascadeContextFK, IDictionary<string, Table> tablesByName)
        {
            // Don't process excluded tables
            if (_codeGenHelper.IsExcluded(tbl.Name))
                return new List<FKTable>();

            // Get child FK constraints that have columns impacted by the incoming context
            List<FKTable> affectedChildFKs;

            if (cascadeContextFK == null)
                affectedChildFKs = tbl.ChildTables.ToList();
            else
                // Find all child tables that are affected by the incoming cascade context 
                // (there may be child tables of this table that are unaffected by the incoming FK)
                affectedChildFKs =
                    (from childFkt in tbl.ChildTables
                     where childFkt.ThisColumns.Any(thisCol => cascadeContextFK.OtherColumns.Contains(thisCol))
                     select childFkt)
                    .ToList();

            var cascadingConstraints = new List<FKTable>(affectedChildFKs);

            foreach (var fkt in affectedChildFKs)
            {
                // Get the child table
                var otherTable = tablesByName[fkt.OtherTable];

                // Add results of recursive calls to this stack
                var cascadingChildConstraints = GetAllCascadingChildConstraints(otherTable, fkt, tablesByName);
                cascadingConstraints.AddRange(cascadingChildConstraints);
            }

            return cascadingConstraints;
        }

        private List<ColumnModel> GetBaseTableColumns(Table baseTbl, IDictionary<string, Table> tablesByName)
        {
            var cols = baseTbl.NonPrimaryKeyColumns.Where(c => c.Name != "Id" && c.Name != "CreateDate" && c.Name != "LastModifiedDate").Select(
                c =>
                    new ColumnModel(c)
                    {
                        PropertyType = _codeGenHelper.GetPropertyType(c),
                        IsLookup = _codeGenHelper.IsLookupTypeColumn(tablesByName, baseTbl, c.Name),
                        IsUniqueId = _codeGenHelper.IsUniqueId(baseTbl, c)
                    }).ToList();
            cols.ForEach(c => c.ColNameToUse = ConvertColumnName(c));
            return cols;
        }


        private bool IsAbstract(Table table, XContainer element)
        {
            return element.Descendants("Entity").Any(e => e.Attribute("table").Value == table.Name
                                                          &&
                                                          e.Attributes("isAbstract")
                                                              .FirstOrDefault(a => a.Value == "true") != null);
        }

        private string GetBaseTableName(XElement element, Table table)
        {
            var baseTableNameAttr = element.Descendants("Entity")
                .Where(e => e.Attribute("table").Value == table.Name)
                .Select(e => e.Attribute("isA"))
                .SingleOrDefault();

            return baseTableNameAttr == null ? null : baseTableNameAttr.Value;
        }

        private List<ColumnModel> GetPKs(CodeGenClassModel tm, IDictionary<string, Table> tablesByName, string baseTableName, Table table)
        {
            var pks = table.PKs.Select(c => new ColumnModel(c)
            {
                PropertyType = _codeGenHelper.GetPropertyType(c),
                IsLookup = _codeGenHelper.IsLookupTypeColumn(tablesByName, table, c.Name),
                IsAutoIncrement = IsAutoIncrement(c, tm.IsDerived, tablesByName, table, baseTableName),
                IsUniqueId = _codeGenHelper.IsUniqueId(table, c),
            }).ToList();

            pks.ForEach(c => c.ColNameToUse = ConvertColumnName(c));
            return pks;
        }

        private List<ColumnModel> GetNonPrimaryKeyColumns(IDictionary<string, Table> tablesByName, Table table)
        {
            var cols =
                table.NonPrimaryKeyColumns.Where(
                    c => c.Name != "Id" && c.Name != "CreateDate" && c.Name != "LastModifiedDate").Select(
                        c =>
                            new ColumnModel(c)
                            {
                                PropertyType = _codeGenHelper.GetPropertyType(c),
                                IsLookup = _codeGenHelper.IsLookupTypeColumn(tablesByName, table, c.Name),
                                IsUniqueId = _codeGenHelper.IsUniqueId(table, c)
                            }).ToList();
            cols.ForEach(c => c.ColNameToUse = ConvertColumnName(c));
            return cols;
        }

        private List<FKTable> GetChildTables(IDictionary<string, Table> tablesByName, Table table, List<string> aggregateTableNames)
        {
            return table.GetNavigableChildTables(tablesByName)
                .Where(
                    ct =>
                        _codeGenHelper.IsInAggregate(aggregateTableNames, ct.OtherTable) &&
                        !_codeGenHelper.IsExcluded(ct.OtherTable))
                .ToList();
        }

        private List<Table> GetDerivedTables(IDictionary<string, Table> tablesByName, Table table)
        {
            return _domainMetadataXDocument.Descendants("Entity")
                .Where(e => e.Attributes("isA").FirstOrDefault(a => a.Value == table.Name) != null)
                .Select(e => tablesByName[e.Attribute("table").Value])
                .ToList();
        }

        private bool IsParentFkPartOfPrimary(CodeGenClassModel tm, Table table)
        {
            return (tm.ParentFkTable != null) && _codeGenHelper.IsFKPartOfPK(table, tm.ParentFkTable);
        }

        private bool IsBaseTableAbstract(CodeGenClassModel tm, Table baseTbl)
        {
            return tm.IsDerived
                   && _domainMetadataXDocument.Descendants("Entity").Any(e => e.Attribute("table").Value == baseTbl.Name
                                                           && e.Attributes("isAbstract").FirstOrDefault(a => a.Value == "true") != null);
        }

        private List<FKTable> GetOneToOneChildFKs(Table table, List<string> aggregateTableNames, IDictionary<string, Table> tablesByName)
        {
            return
                table.GetNavigableOneToOneChildTables(tablesByName)
                    .Where(
                        ct =>
                            _codeGenHelper.IsInAggregate(aggregateTableNames, ct.OtherTable) &&
                            !_codeGenHelper.IsExcluded(ct.OtherTable))
                    .ToList();
        }

        private List<FKTable> GetBaseChildTables(IDictionary<string, Table> tablesByName, string baseTableName, Table baseTbl)
        {
            var baseAggregateRootName =
                (from a in _aggregateElts
                 from e in a.Descendants("Entity")
                 where e.Attribute("table").Value == baseTableName
                 select a.Attribute("root").Value)
                    .SingleOrDefault();

            var baseAggregateTableNames =
                (from a in _aggregateElts
                 where a.Attribute("root").Value == baseAggregateRootName
                 from e in a.Descendants("Entity")
                 select e.Attribute("table").Value)
                    .ToList();

            return baseTbl == null
                ? new List<FKTable>()
                : baseTbl.GetNavigableChildTables(tablesByName)
                    .Where(
                        ct =>
                            _codeGenHelper.IsInAggregate(baseAggregateTableNames, ct.OtherTable) &&
                            !_codeGenHelper.IsExcluded(ct.OtherTable))
                    .ToList();
        }


        private string GetInterfaces(CodeGenClassModel tm)
        {
            var interfaceBuilder = new StringBuilder();
            if (tm.IsDerived)
                AddInterface(interfaceBuilder, "I" + tm.BaseClass.Name);
            if (!tm.IsAbstract) AddInterface(interfaceBuilder, "ISynchronizable, IMappable");
            if (tm.IsRootTable) AddInterface(interfaceBuilder, "IHasIdentifier");
            if (tm.PKs.Any(x => x.IsUniqueId)) AddInterface(interfaceBuilder, "IIdentifiablePerson");
            return interfaceBuilder.ToString();
        }

        static string TrimSuffix(string text, string suffix)
        {
            return Regex.Replace(text, @"^(\w+)" + suffix + "$", "$1");
        }

        private static string ConvertColumnName(ColumnModel col)
        {
            return col.IsLookup ? TrimSuffix(col.CleanName, "Id") : col.CleanName;
        }

        private bool IsAutoIncrement(Column column, bool isDerived, IDictionary<string, Table> tablesByName, Table table,
            string baseTableName)
        {
            return column.AutoIncrement ||
                   (isDerived &&
                    _codeGenHelper.GetOtherColumn(tablesByName, table.FKTables.Single(fkt => fkt.OtherTable == baseTableName),
                        column.Name).AutoIncrement);
        }

        private static void AddInterface(StringBuilder sb, string interfaceName)
        {
            if (sb.Length > 0)
                sb.Append(", " + interfaceName);
            else
                sb.Append(" : " + interfaceName);
        }
    }
}