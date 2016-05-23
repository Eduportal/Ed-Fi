namespace EdFi.Ods.CodeGen.DatabaseSchema
{
    public interface IDatabaseTypeTranslator
    {
        string GetSysType(string sqlType);
        string GetNHType(string sqlType);
        DbType GetDbType(string sqlType);
    }

    public class DatabaseTypeTranslator : IDatabaseTypeTranslator
    {
        public string GetSysType(string sqlType)
        {
            string sysType = "string";
            switch (sqlType)
            {
                case "bigint":
                    sysType = "long";
                    break;
                case "tinyint":
                case "smallint":
                    sysType = "short";
                    break;
                case "int":
                    sysType = "int";
                    break;
                case "uniqueidentifier":
                    sysType = "Guid";
                    break;
                case "smalldatetime":
                case "datetime":
                case "date":
                    sysType = "DateTime";
                    break;
                case "float":
                    sysType = "double";
                    break;
                case "real":
                case "numeric":
                case "smallmoney":
                case "decimal":
                case "money":
                    sysType = "decimal";
                    break;
                case "bit":
                    sysType = "bool";
                    break;
                case "image":
                case "binary":
                case "varbinary":
                    sysType = "byte[]";
                    break;
                case "time":
                    sysType = "TimeSpan";
                    break;
            }
            return sysType;
        }
        public string GetNHType(string sqlType)
        {
            string sysType = "string";
            switch (sqlType)
            {
                case "bigint":
                    sysType = "long";
                    break;
                case "tinyint":
                case "smallint":
                    sysType = "short";
                    break;
                case "int":
                    sysType = "int";
                    break;
                case "uniqueidentifier":
                    sysType = "Guid";
                    break;
                case "datetimeoffset":
                    sysType = "datetimeoffset";
                    break;
                case "smalldatetime":
                case "datetime":
                    sysType = "timestamp"; //"datetime";
                    break;
                case "date":
                    sysType = "date";
                    break;
                case "float":
                    sysType = "double";
                    break;
                case "real":
                case "numeric":
                case "smallmoney":
                case "decimal":
                case "money":
                    sysType = "decimal";
                    break;
                case "bit":
                    sysType = "bool";
                    break;
                case "image":
                case "binary":
                case "varbinary":
                    sysType = "byte[]";
                    break;
                case "time":
                    sysType = "TimeAsTimeSpan";
                    break;
            }
            return sysType;
        }

        public  DbType GetDbType(string sqlType)
        {
            switch (sqlType)
            {
                case "varchar":
                    return DbType.AnsiString;
                case "nvarchar":
                    return DbType.String;
                case "int":
                    return DbType.Int32;
                case "uniqueidentifier":
                    return DbType.Guid;
                case "datetime":
                    return DbType.DateTime;
                case "bigint":
                    return DbType.Int64;
                case "binary":
                    return DbType.Binary;
                case "bit":
                    return DbType.Boolean;
                case "char":
                    return DbType.AnsiStringFixedLength;
                case "decimal":
                    return DbType.Decimal;
                case "float":
                    return DbType.Double;
                case "image":
                    return DbType.Binary;
                case "money":
                    return DbType.Currency;
                case "nchar":
                    return DbType.String;
                case "ntext":
                    return DbType.String;
                case "numeric":
                    return DbType.Decimal;
                case "real":
                    return DbType.Single;
                case "smalldatetime":
                    return DbType.DateTime;
                case "smallint":
                    return DbType.Int16;
                case "smallmoney":
                    return DbType.Currency;
                case "sql_variant":
                    return DbType.String;
                case "sysname":
                    return DbType.String;
                case "text":
                    return DbType.AnsiString;
                case "timestamp":
                    return DbType.Binary;
                case "tinyint":
                    return DbType.Byte;
                case "varbinary":
                    return DbType.Binary;
                case "xml":
                    return DbType.Xml;
                default:
                    return DbType.AnsiString;
            }

        }
    }
}