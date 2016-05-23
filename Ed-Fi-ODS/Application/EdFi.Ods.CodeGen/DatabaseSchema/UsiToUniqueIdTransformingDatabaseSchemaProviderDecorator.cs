using System.Collections.Generic;
using System.Linq;

namespace EdFi.Ods.CodeGen.DatabaseSchema
{
    public class UsiToUniqueIdTransformingDatabaseSchemaProviderDecorator : DatabaseSchemaProviderDecoratorBase
    {
        private readonly IDatabaseSchemaProvider _decoratedProvider;

        public UsiToUniqueIdTransformingDatabaseSchemaProviderDecorator(IDatabaseSchemaProvider decoratedProvider)
            : base(decoratedProvider)
        {
            _decoratedProvider = decoratedProvider;
        }

        public override List<Table> LoadTables()
        {
            var tables = _decoratedProvider.LoadTables();

            foreach (var table in tables)
            {
                table.PrimaryKeyColumns = this.ReplaceUsiColumns(table.PrimaryKeyColumns);
                table.Columns = this.ReplaceUsiColumns(table.Columns);

                foreach (var fkTable in table.FKTables)
                {
                    fkTable.ThisColumns = this.ReplaceUsiColumns(fkTable.ThisColumns);
                    fkTable.OtherColumns = this.ReplaceUsiColumns(fkTable.OtherColumns);
                }
            }

            return tables;
        }

        private List<string> ReplaceUsiColumns(List<string> columns)
        {
            return columns.Select(x => CodeGenSpecifications.IsUsiColumn(x) ? GetUniqueIdPropertyName(x) : x)
                          .ToList();
        }
        
        private List<Column> ReplaceUsiColumns(List<Column> columns)
        {
            var usiColumns = columns.Where(x => CodeGenSpecifications.IsUsiColumn(x.Name)).ToList();

            foreach (var usiColumn in usiColumns)
            {
                var uniqueIdPropertyName = GetUniqueIdPropertyName(usiColumn.Name);
                var resource = uniqueIdPropertyName.ToLower().Replace("uniqueid", string.Empty);

                var uniqueIdColumn = new Column(usiColumn.TableSchema, usiColumn.TableName, uniqueIdPropertyName)
                {
                    CleanName = uniqueIdPropertyName,
                    SysType = "string",
                    NHType = "string",
                    DataType = "varchar",
                    DbType = DatabaseSchema.DbType.AnsiString,
                    AutoIncrement = false,
                    IsPK = usiColumn.IsPK,
                    MaxLength = 32,
                    Precision = 0,
                    Scale = 0,
                    IsNullable = usiColumn.IsNullable,
                    IsForeignKey = usiColumn.IsForeignKey,
                    Description = string.Format("A unique alpha-numeric code assigned to a {0}.", resource)
                };
                if (columns.Select(c => c.Name).Contains(uniqueIdColumn.Name))
                    columns.RemoveAt(columns.IndexOf(usiColumn));
                else
                    columns[columns.IndexOf(usiColumn)] = uniqueIdColumn;
            }

            return columns;
        }

        private static string GetUniqueIdPropertyName(string usiColumnName)
        {
            return usiColumnName.Replace("USI", "UniqueId");
        }
    }
}