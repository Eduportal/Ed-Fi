// // ****************************************************************************
// // Copyright (C) 2014 Ed-Fi Alliance, LLC. All Rights Reserved.
// // ****************************************************************************

using System.Collections.Generic;
using System.Linq;

namespace EdFi.Ods.CodeGen.DatabaseSchema
{
    public class EdFiExtensionsDatabaseSchemaProviderDecorator : DatabaseSchemaProviderDecoratorBase
    {
        private readonly IDatabaseSchemaProvider _decoratedProvider;
        private readonly HashSet<Table> _processedTables = new HashSet<Table>();

        public EdFiExtensionsDatabaseSchemaProviderDecorator(IDatabaseSchemaProvider decoratedProvider)
            : base(decoratedProvider)
        {
            _decoratedProvider = decoratedProvider;
        }

        private class ExtensionTablePair
        {
            public Table EdFiTable { get; set; }
            public Table ExtensionTable { get; set; }
        }

        public override List<Index> GetIndices()
        {
            var pairs = this.GetExtensionTablePairs();

            return
                // Filter out extension indices
                (from i in _decoratedProvider.GetIndices()
                where !pairs.Any(x => 
                    x.ExtensionTable.Name == i.TableName 
                    && x.ExtensionTable.Schema == i.SchemaName)
                select i)
                .ToList();
        }

        private List<ExtensionTablePair> _extensionTablePairs;

        private List<ExtensionTablePair> GetExtensionTablePairs()
        {
            if (this._extensionTablePairs != null)
                return this._extensionTablePairs;

            var tables = _decoratedProvider.LoadTables();

            this._extensionTablePairs =
                (from t in tables
                    from t1 in tables
                    where t.Name + "Extension" == t1.Name 
                          && t.Schema == "edfi"
                          && t1.Schema == "extension"
                    select new ExtensionTablePair {EdFiTable = t, ExtensionTable = t1})
                    .ToList();
            
            return this._extensionTablePairs;
        }

        public override List<Table> LoadTables()
        {
            var tables = _decoratedProvider.LoadTables();

            var pairs = this.GetExtensionTablePairs();

            // Present a merged view of the edfi/extension model to the t4 template
            foreach (var pair in pairs)
            {
                // Don't process/modify the same table more than once (due to multiple calls through the decorator)
                if (!_processedTables.Add(pair.EdFiTable))
                    continue;

                // Add copies of the extension columns to the columns of the Ed-Fi table
                pair.EdFiTable.Columns.AddRange(pair.ExtensionTable.NonPrimaryKeyColumns.Select(c => new Column(c)));

                //Add extension foreign keys
                pair.EdFiTable.FKTables.AddRange(pair.ExtensionTable.FKTables);

                // Remove FKs from edfi table that reference the 1-to-1 extension table
                var fksToRemove = pair.EdFiTable.FKTables.Where(
                    fkt => fkt.ThisTable == fkt.OtherTable && fkt.ThisTableSchema != fkt.OtherTableSchema).ToList();

                foreach (var fkTable in fksToRemove)
                    pair.EdFiTable.FKTables.Remove(fkTable);
            }

            return tables;
        }
    }
}