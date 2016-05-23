namespace EdFi.Ods.CodeGen.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using EdFi.Ods.CodeGen.DatabaseSchema;

    public class CodeGenClassModel
    {
        public CodeGenClassModel()
        {
            PKs = new List<ColumnModel>();
            OneToOneChildFKs = new List<FKTable>();
            NonPrimaryKeyColumns = new List<ColumnModel>();
            ChildTables = new List<FKTable>();
            BaseChildTables = new List<FKTable>();
            DerivedTables = new List<Table>();
            NavigableParentFkTables = new List<FKTable>();
            BaseTableColumns = new List<ColumnModel>();
            AllCascadingChildConstraints = new Lazy<List<FKTable>>(() => new List<FKTable>());
            AggregateTableNames = new List<string>();
        }

        public string Name { get; set; }
        public Table UnderlyingTable { get; set; }
        public List<ColumnModel> PKs { get; set; }
        public List<ColumnModel> NonPrimaryKeyColumns { get; set; }
        public ColumnModel ForeignKeyColumn { get; set; }
        public FKTable ParentFkTable { get; set; }
        public bool IsParentFkPartOfPrimary { get; set; }
        public bool IsPkAutoIncrement { get; set; }
        public List<FKTable> ChildTables { get; set; }
        public List<FKTable> BaseChildTables { get; set; }
        public List<Table> DerivedTables { get; set; }
        public List<FKTable> NavigableParentFkTables { get; set; }
        public bool IsDerived { get; set; }
        public bool IsRootTable { get; set; }
        public bool IsLookup { get; set; }
        public bool IsAbstract { get; set; }
        public bool IsBaseTable { get; set; }
        public bool IsBaseTableAbstract { get; set; }
        public string AggregateRootName { get; set; }
        public List<string> AggregateTableNames { get; set; } 
        public bool AllowPrimaryKeyUpdates { get; set; }
        public CodeGenClassModel BaseClass { get; set; }
        public List<ColumnModel> BaseTableColumns { get; set; }
        public List<FKTable> OneToOneChildFKs { get; set; }
        public bool HasAnyOneToOneChildFKs { get { return this.OneToOneChildFKs.Any(); } }
        public string Schema { get; set; }
        public string Implements { get; set; }

        public List<ColumnModel> NonReferencedPKs
        {
            get
            {
                return this.PKs.Where(x => this.ParentFkTable == null || !this.ParentFkTable.ThisColumns.Contains(x.Name)).ToList();
            }
        }
        public List<ColumnModel> NonInheritedPKs
        {
            get
            {
                return this.PKs.Where(x => (this.ParentFkTable == null || !this.ParentFkTable.ThisColumns.Contains(x.Name))
                                      && (!this.IsDerived || this.BaseClass.PKs.All(c => c.Name != x.Name))).ToList();
            }
        }
        public List<ColumnModel> InheritedNonPkColumns
        {
            get
            {
                return
                    this.BaseTableColumns.Where(c => c.Name != "Id" && c.Name != "CreateDate" && c.Name != "LastModifiedDate")
                        .ToList();
            }
        }
        public List<ColumnModel> NonPrimaryKeyColumnsToMap
        {
            get
            {
                return
                    this.NonPrimaryKeyColumns.Where(
                        c =>
                            c.Name != "Id" && c.Name != "CreateDate" && c.Name != "LastModifiedDate")
                        .ToList();
            }
        }
        public List<ColumnModel> NonPrimaryKeyColumnsToShow
        {
            get
            {
                return
                    this.NonPrimaryKeyColumnsToMap.Where(c =>!this.PKs.Select(pk => pk.Name).Contains(c.Name))
                        .ToList();
            }
        }

        public Lazy<List<FKTable>> AllCascadingChildConstraints { get; set; }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return string.Format("{0}.{1}", Schema, Name);
        }
    }
}