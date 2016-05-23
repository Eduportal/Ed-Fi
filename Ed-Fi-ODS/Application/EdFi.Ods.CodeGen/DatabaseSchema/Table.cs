namespace EdFi.Ods.CodeGen.DatabaseSchema
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Table
    {
        public List<Column> Columns;
        public List<FKTable> FKTables;
        public string Name;
        public string Description;
        public string CleanName;
        public string ClassName;
        public List<Column> PrimaryKeyColumns = new List<Column>();
        public string Schema;
        public string QueryableName;

        public Column this[string colName]
        {
            get
            {
                return
                    (from c in this.Columns
                     where c.Name.Equals(colName, StringComparison.InvariantCultureIgnoreCase)
                     select c)
                    .SingleOrDefault();
            }
        }

        public bool HasLogicalDelete()
        {
            return this.Columns.Any(x => x.Name.ToLower() == "deleted" || x.Name.ToLower() == "isdeleted");
        }

        public bool HasPrimaryKeyField(string columnName)
        {
            return this.PKs.Any(x => x.Name.Equals(columnName, StringComparison.InvariantCultureIgnoreCase));
        }

        public Column DeleteColumn
        {
            get
            {
                Column result = null;
                if (this.Columns.Any(x => x.Name.ToLower() == "deleted"))
                    result = this.Columns.Single(x => x.Name.ToLower() == "deleted");
                if (this.Columns.Any(x => x.Name.ToLower() == "isdeleted"))
                    result = this.Columns.Single(x => x.Name.ToLower() == "isdeleted");
                return result;
            }
        }
        public List<Column> PKs
        {
            get
            {
                return this.PrimaryKeyColumns; //.SingleOrDefault(x=>x.IsPK) ?? this.Columns[0];
            }
        }

        public IEnumerable<Column> NonPrimaryKeyColumns
        {
            get { return this.Columns.Where(c => !c.IsPK); }
        }


        public IEnumerable<Column> ExampleSpecificationColumns
        {
            get
            {
                var excludedColumnNames = new string[] { "CreateDate", "LastModifiedDate" };
                return PKs.Union(NonPrimaryKeyColumns).Where(x => !excludedColumnNames.Contains(x.CleanName)).Distinct();
            }
        }

        public Column Descriptor
        {
            get
            {
                if (this.Columns.Count == 1)
                {
                    return this.Columns[0];
                }
                else
                {
                    //get the first string column
                    Column result = null;
                    result = this.Columns.FirstOrDefault(x => x.SysType.ToLower().Trim() == "string");
                    if (result == null)
                        result = this.Columns[1];
                    return result;
                }
            }
        }

        public IEnumerable<FKTable> ChildTables
        {
            get { return this.FKTables.Where(fkt => fkt.IsPrimaryTable && !IsExtensionRelationship(fkt)); }
        }

        public IEnumerable<FKTable> ParentTables
        {
            get { return this.FKTables.Where(fkt => !fkt.IsPrimaryTable && !IsExtensionRelationship(fkt)); }
        }

        private bool IsExtensionRelationship(FKTable fkt)
        {
            return 
                fkt.ThisTable.ToLower()
                    .Replace("extension", "")
                    .Equals(fkt.OtherTable.ToLower().Replace("extension", ""));
        }

        public virtual IEnumerable<FKTable> GetNavigableOneToOneParentTables(IDictionary<string, Table> tablesByName)
        {
            return this.GetNavigableOneToOneTables(tablesByName, false);
        }

        public IEnumerable<FKTable> GetNavigableOneToOneChildTables(IDictionary<string, Table> tablesByName)
        {
            return this.GetNavigableOneToOneTables(tablesByName, true);
        }

        private IEnumerable<FKTable> GetNavigableOneToOneTables(IDictionary<string, Table> tablesByName, bool isPrimaryTable)
        {
            var oneToOneFKs =
                (from fkt in this.FKTables
                 let thisColumns = string.Join("|", Enumerable.OrderBy(fkt.ThisColumns, n => n))
                 let otherColumns = string.Join("|", Enumerable.OrderBy(fkt.OtherColumns, n => n))
                 let thisTable = tablesByName[fkt.ThisTable]
                 let otherTable = this.GetTableByNameOrDefault(tablesByName, fkt.OtherTable)
                 let fktWithKeys = new { FKT = fkt, ThisColumns = thisColumns, OtherColumns = otherColumns }
                 where otherTable != null  // This check was done because the CodeGenClassModels are filtered based on excluded, and because the separation isn't clean, we sometimes need to perform operations against the underlying table like this from the code gen model, which results in tables not being found because they've been filtered.
                     && fkt.IsPrimaryTable == isPrimaryTable // Only relationships where current table is the parent/child side of the relationship, as specified by isPrimaryTable
                     && fkt.ThisTable != fkt.OtherTable // No self-referencing relationships
                     && fkt.OtherColumns.All(c => otherTable.HasPrimaryKeyField(c)) // And is entirely contained in the child table's primary key
                     && fkt.OtherColumns.Count() == otherTable.PKs.Count  // And is the same as the child's PK (it's a one-to-one relationship)
                     && thisTable.PKs.Count == otherTable.PKs.Count // Same counts = one to one
                 select fkt)
                .ToList();

            return oneToOneFKs;
        }

        private Table GetTableByName(Dictionary<string, Table> tablesByName, string tableName)
        {
            Table table;

            if (tablesByName.TryGetValue(tableName, out table))
                return table;

            throw new Exception(string.Format("Could not find table '{0}'.", tableName));
        }

        private Table GetTableByNameOrDefault(IDictionary<string, Table> tablesByName, string tableName)
        {
            Table table;

            if (tablesByName.TryGetValue(tableName, out table))
                return table;

            return null;
        }

        public IEnumerable<FKTable> GetNavigableChildTables(IDictionary<string, Table> tablesByName)
        {
            {
                var groupedFKs =
                    (from fkt in this.FKTables
                    let thisColumns = string.Join("|", Enumerable.OrderBy(fkt.ThisColumns, n => n))
                    let otherColumns = string.Join("|", Enumerable.OrderBy(fkt.OtherColumns, n => n))
                    let otherTable = this.GetTableByNameOrDefault(tablesByName, fkt.OtherTable)
                    let fktWithKeys = new { FKT = fkt, ThisColumns = thisColumns, OtherColumns = otherColumns }
                    where otherTable != null  // This check was done because the CodeGenClassModels are filtered based on excluded, and because the separation isn't clean, we sometimes need to perform operations against the underlying table like this from the code gen model, which results in tables not being found because they've been filtered.
                        && fkt.IsPrimaryTable  // Only relationships where current table is the "one" side of the relationship
                        && fkt.OtherColumns.All(c => otherTable.HasPrimaryKeyField(c)) // And is entirely contained in the child table's primary key
                        && fkt.OtherColumns.Count() < otherTable.PKs.Count  // And is a subset of the child's PK (not a one-to-one relationship)
                    group fktWithKeys by fktWithKeys.FKT.OtherTable into g
                    select g)
                    .ToList();

                foreach (var grouping in groupedFKs)
                {
                    // If there is only 1 relationship to the child table, use it
                    if (grouping.Count() == 1)
                    {
                        // Remove self-recursive relationships (only allow levels to be pulled based on "parent" id)
                        if (grouping.Single().FKT.OtherTable == grouping.Single().FKT.ThisTable)
                            continue;

                        yield return grouping.Single().FKT;
                    }

                    if (grouping.Count() >= 2)
                    {
                        // Child FKs are only navigable if they are 
                        //    1) non-self-recursive
                        //    2) have the same column names in both the parent and child table, or...
                        //  3) there are no other FKs to the child table that have matching column names, and the FK to the child table has columns that are entirely part of the child's PK columns

                        // For example, on School -> FeederSchoolAssociation, there is SchoolId and FeederSchoolId.
                        //    SchoolId is the primary relationship, and the one that should be mapped to an ORM.
                        // For example, on CalendarDate -> GradingPeriod, there is Date -> BeginDate AND EndDate, but only BeginDate is part of the PK.  That relationship is deemed navigable.

                        /*
                        var primaryChildFKs =
                            (from fkt in grouping
                            let thisColumns = string.Join("|", Enumerable.OrderBy(fkt.ThisColumns, n => n))
                            let otherColumns = string.Join("|", Enumerable.OrderBy(fkt.OtherColumns, n => n))
                            select new { FKT = fkt, ThisColumns = thisColumns, OtherColumns = otherColumns })
                            .ToList();
                        */

                        var filteredPrimaryChildFKs = grouping
                            .Where(x => x.ThisColumns == x.OtherColumns) // Remove any child relationships that apply "role names". Only include the primary relationship, where column names match exactly.
                            .Where(x => x.FKT.OtherTable != x.FKT.ThisTable); // Remove self-recursive relationships

                        // Found a primary child relationship, so it's navigable
                        if (filteredPrimaryChildFKs.Count() == 1)
                        {
                            yield return filteredPrimaryChildFKs.Single().FKT;
                            continue;
                        }

                        // We did not find a primary child relationship, so we are in a role name situation
                        // We just need to check the remote table now and filter the remaining FKs to only
                        // include those that are part of the child's PK (in their entirety)
                        var fksThatAreChildPKs =
                            (from rel in grouping.Where(x => x.ThisColumns != x.OtherColumns)
                             let otherTable = tablesByName[rel.FKT.OtherTable]
                             where rel.FKT.OtherColumns.All(c => otherTable.HasPrimaryKeyField(c))
                             select rel)
                            .ToList();

                        // For debugging purposes
                        /*
                        foreach (var fk in fksThatAreChildPKs)
                            System.IO.File.AppendAllText(@"D:\temp\junk.txt", 
                                string.Format("{0} -> {1}\r\n\ton {2} -> {3}\r\n", 
                                    fk.FKT.ThisTable, fk.FKT.OtherTable,
                                    fk.ThisColumns, fk.OtherColumns));
                        */

                        foreach (var fk in fksThatAreChildPKs.Select(x => x.FKT))
                            yield return fk;
                    }
                }
            }
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return Schema + "." + Name;
        }
    }
}