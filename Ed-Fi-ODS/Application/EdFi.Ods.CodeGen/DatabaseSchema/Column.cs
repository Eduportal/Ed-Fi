using System;

namespace EdFi.Ods.CodeGen.DatabaseSchema
{
    public class Column : IEquatable<Column>
    {
        public readonly string TableSchema;
        public readonly string TableName;
        public readonly string Name;
        public string CleanName;
        public string SysType;
        public string NHType;
        public string DataType;
        public DbType DbType;
        public bool AutoIncrement;
        public bool IsPK;
        public int MaxLength;
        public int Precision;
        public int Scale;
        public bool IsNullable;
        public bool IsForeignKey;
        public string Description;

        public Column(string tableSchema, string tableName, string name)
        {
            Name = name;
            TableName = tableName;
            TableSchema = tableSchema;
        }

        public Column(Column source)
        {
            this.Name = source.Name;
            this.CleanName = source.CleanName;
            this.SysType = source.SysType;
            this.NHType = source.NHType;
            this.DataType = source.DataType;
            this.DbType = source.DbType;
            this.AutoIncrement = source.AutoIncrement;
            this.IsPK = source.IsPK;
            this.MaxLength = source.MaxLength;
            this.Precision = source.Precision;
            this.Scale = source.Scale;
            this.IsNullable = source.IsNullable;
            this.IsForeignKey = source.IsForeignKey;
            this.TableName = source.TableName;
            this.TableSchema = source.TableSchema;
            this.Description = source.Description;
        }

        public bool Equals(Column other)
        {
            return TableSchema.Equals(other.TableSchema) && TableName.Equals(other.TableName) && Name.Equals(other.Name);
        }

        public override int GetHashCode()
        {
            return (GetType() + TableSchema + TableName + Name).GetHashCode();
        }
    }

    public static class ColumnExtensions
    {
        public static bool IsDateOnlyProperty(this Column column)
        {
            return column.DbType == DbType.Date;
        }
    }
}