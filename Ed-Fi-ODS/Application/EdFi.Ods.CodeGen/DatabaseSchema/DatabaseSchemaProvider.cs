using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using EdFi.Common.Inflection;

namespace EdFi.Ods.CodeGen.DatabaseSchema
{
    public interface IDatabaseSchemaProvider
    {
        List<Table> LoadTables();
        List<Index> GetIndices();
        void EnsureAllConnectionsClosed();
    }

    public class DatabaseSchemaProvider : IDatabaseSchemaProvider
    {
        private readonly string templatesPath;
        private readonly string currentProjectFileName;

        private List<SqlConnection> connections = new List<SqlConnection>();
        private List<Table> tables;
        private List<Index> indices = null;
        private Dictionary<string, List<FKTable>> allFkTablesByFQTableName;
        private Dictionary<string, List<Column>> allColumnsByFQTableName;
        private Dictionary<string, List<string>> allPKColumnsByFQTableName;

        //this is a list of schemas you want generated
        readonly string[] IncludedSchemas =
        {
            "edfi", "extension"
        };

        public DatabaseSchemaProvider(string currentProjectFileName, string templatesPath)
        {
            this.currentProjectFileName = currentProjectFileName;
            this.templatesPath = templatesPath;
        }

        private const string ConnectionStringName = "Ods";

        private string _connectionString = "";

        private string ConnectionString
        {
            get
            {
                if (String.IsNullOrEmpty(this._connectionString))
                {
                    string environmentConnectionString = (string)Environment.GetEnvironmentVariables()["EdFiOdsConnectionString"];

                    if (environmentConnectionString != null)
                        this._connectionString = environmentConnectionString;
                    else
                        this._connectionString = this.GetConnectionString(ConnectionStringName);
                }

                if (this._connectionString.Contains("|DataDirectory|"))
                {
                    //have to replace it
                    string dataFilePath = this.GetDataDirectory();
                    this._connectionString = this._connectionString.Replace("|DataDirectory|", dataFilePath);
                }

                return this._connectionString;
            }
        }

        private string GetConnectionString(string connectionStringName)
        {
            //var _CurrentProject = GetCurrentProject();

            string result = "";
            ExeConfigurationFileMap configFile = new ExeConfigurationFileMap();
            configFile.ExeConfigFilename = this.GetConfigPath();

            if (string.IsNullOrEmpty(configFile.ExeConfigFilename))
                throw new ArgumentNullException("The project does not contain App.config or Web.config file.");


            var config = System.Configuration.ConfigurationManager.OpenMappedExeConfiguration(configFile,
                ConfigurationUserLevel.None);
            var connSection = config.ConnectionStrings;

            //if the connectionString is empty - which is the defauls
            //look for count-1 - this is the last connection string
            //and takes into account AppServices and LocalSqlServer
            if (string.IsNullOrEmpty(connectionStringName))
            {
                if (connSection.ConnectionStrings.Count > 1)
                {
                    result = connSection.ConnectionStrings[connSection.ConnectionStrings.Count - 1].ConnectionString;
                }
            }
            else
            {
                try
                {
                    result = connSection.ConnectionStrings[connectionStringName].ConnectionString;
                }
                catch
                {
                    result = "There is no connection string name called '" + connectionStringName + "'";
                }
            }

            return result;
        }

        private string GetConfigPath()
        {
            string configFilePath = System.IO.Path.Combine(this.templatesPath, "app.config");

            if (!System.IO.File.Exists(configFilePath))
                throw new FileNotFoundException(string.Format("Template configuration file was not found at '{0}'.",
                    configFilePath));

            return configFilePath;

            /*
            EnvDTE.Project project = GetCurrentProject();
            foreach (EnvDTE.ProjectItem item in project.ProjectItems)
            {
                // if it is the app.config file, then open it up
                if (item.Name.Equals("App.config",StringComparison.InvariantCultureIgnoreCase) || item.Name.Equals("Web.config",StringComparison.InvariantCultureIgnoreCase))
                    return GetProjectPath() + "\\" + item.Name;
            }
            return String.Empty;
            */
        }

        public string GetDataDirectory()
        {
            return Path.GetDirectoryName(this.currentProjectFileName) + "\\App_Data\\";
        }

        public List<Table> LoadTables()
        {
            if (this.tables != null)
                return this.tables;

            var result = new List<Table>();

            //pull the tables in a reader
            using (IDataReader rdr = this.GetReader(this.GetTableSQL()))
            {
                while (rdr.Read())
                {
                    Table tbl = new Table();
                    tbl.Name = rdr["TABLE_NAME"].ToString();
                    tbl.Schema = rdr["TABLE_SCHEMA"].ToString();
                    tbl.Description = rdr["TABLE_DESCRIPTION"].ToString();
                    tbl.Columns = this.LoadColumns(tbl);
                    tbl.PrimaryKeyColumns = this.GetPKColumns(tbl);
                    tbl.CleanName = this.CleanUp(tbl.Name);
                    tbl.ClassName = CompositeTermInflector.MakeSingular(tbl.CleanName);
                    tbl.QueryableName = CompositeTermInflector.MakePlural(tbl.ClassName);

                    //set the PK for the columns
                    //var pkColumn = tbl.PrimaryKeyColumns.Any(pkc => pkc.Name == //tbl.Columns.SingleOrDefault(x=>x.Name.ToLower().Trim()==tbl.PrimaryKey.ToLower().Trim());
                    //if(pkColumn!=null)
                    //    pkColumn.IsPK=true;

                    tbl.FKTables = this.LoadFKTables(tbl);

                    result.Add(tbl);
                }
            }

            foreach (Table tbl in result)
            {
                //loop the FK tables and see if there's a match for our FK columns
                foreach (Column col in tbl.Columns)
                {
                    col.IsForeignKey = tbl.FKTables.Any(
                        x => x.ThisColumns.Contains(col.Name) //, StringComparison.InvariantCultureIgnoreCase)
                        );
                }
            }

            this.tables = result; // TODO: Should we filter excluded tables here? .Where(t => !IsExcluded(t.Name)).ToList();

            return this.tables;
        }

        private List<Column> LoadColumns(Table tbl)
        {
            if (this.allColumnsByFQTableName == null)
            {
                //allColumnsByFQTableName = new Dictionary<string, List<Column>>();

                var allColumns = new List<Column>();

                var cmd = this.GetCommand(COLUMN_SQL);
                //cmd.Parameters.AddWithValue("@tableName",tbl.Name);
                //cmd.Parameters.AddWithValue("@tableSchema",tbl.Schema);

                using (IDataReader rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    int precisionCol = rdr.GetOrdinal("Precision");
                    int scaleCol = rdr.GetOrdinal("Scale");
                    int maxLengthCol = rdr.GetOrdinal("MaxLength");

                    while (rdr.Read())
                    {
                        Column col = new Column(rdr["Owner"].ToString(), rdr["TableName"].ToString(), rdr["ColumnName"].ToString());
                        col.CleanName = this.CleanUp(col.Name == col.TableName ? col.Name + "X" : col.Name);
                        col.DataType = rdr["DataType"].ToString();
                        col.SysType = this.GetSysType(col.DataType);
                        col.NHType = this.GetNHType(col.DataType);
                        col.DbType = this.GetDbType(col.DataType);
                        col.AutoIncrement = rdr["IsIdentity"].ToString() == "1";
                        col.IsNullable = rdr["IsNullable"].ToString() == "YES";
                        col.Precision = rdr.IsDBNull(precisionCol) ? 0 : rdr.GetByte(precisionCol);
                        col.Scale = rdr.IsDBNull(scaleCol) ? 0 : rdr.GetInt32(scaleCol);
                        col.MaxLength = rdr.IsDBNull(maxLengthCol) ? 0 : rdr.GetInt32(maxLengthCol);

                        col.Description = rdr["Description"].ToString();

                        allColumns.Add(col);
                    }
                }

                this.allColumnsByFQTableName =
                    (from c in allColumns
                     let fqn = c.TableSchema + "." + c.TableName
                     group c by fqn into g
                     select g)
                        .ToDictionary(x => x.Key, x => x.ToList());
            }

            return this.allColumnsByFQTableName[tbl.Schema + "." + tbl.Name];
        }

        public List<Index> GetIndices()
        {
            if (this.indices != null)
                return this.indices;

            var result = new List<Index>();
            Index currentIndex = null;

            //pull the tables in a reader
            using (IDataReader rdr = this.GetReader(INDEXES_SQL))
            {
                string previousIndexName = string.Empty;

                while (rdr.Read())
                {
                    string currentIndexName = rdr["IndexName"].ToString();

                    if (previousIndexName != currentIndexName)
                    {
                        if (currentIndex != null)
                            result.Add(currentIndex);

                        currentIndex = new Index();
                        currentIndex.IndexName = currentIndexName;
                        currentIndex.TableName = rdr["TableName"].ToString();
                        currentIndex.SchemaName = rdr["SchemaName"].ToString();
                    }

                    currentIndex.ColumnNames.Add(rdr["ColumnName"].ToString());

                    previousIndexName = currentIndexName;
                }

                if (currentIndex != null)
                    result.Add(currentIndex);
            }

            return result;
        }

        public void EnsureAllConnectionsClosed()
        {
            try
            {
                foreach (var conn in this.connections)
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
            catch { }
        }

        private string GetFKSqlSQL()
        {
            return @"
    SELECT
        ThisTable  = FK.TABLE_NAME,
        ThisColumn = CU.COLUMN_NAME,
        OtherTable  = PK.TABLE_NAME,
        OtherColumn = PT.COLUMN_NAME, 
        Constraint_Name = C.CONSTRAINT_NAME,
        Owner = FK.TABLE_SCHEMA,
        OtherOwner = PK.TABLE_SCHEMA
    FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS C
    INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS FK ON C.CONSTRAINT_NAME = FK.CONSTRAINT_NAME
    INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS PK ON C.UNIQUE_CONSTRAINT_NAME = PK.CONSTRAINT_NAME
        and C.UNIQUE_CONSTRAINT_SCHEMA = PK.TABLE_SCHEMA
    INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE CU ON C.CONSTRAINT_NAME = CU.CONSTRAINT_NAME
    INNER JOIN
        (    
            SELECT i1.TABLE_SCHEMA, i1.TABLE_NAME, i2.CONSTRAINT_SCHEMA, i2.COLUMN_NAME, i2.ORDINAL_POSITION
            FROM  INFORMATION_SCHEMA.TABLE_CONSTRAINTS i1
            INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE i2 ON i1.CONSTRAINT_NAME = i2.CONSTRAINT_NAME
            WHERE i1.CONSTRAINT_TYPE = 'PRIMARY KEY'
        ) 
    PT ON PT.TABLE_NAME = PK.TABLE_NAME AND PT.TABLE_SCHEMA = PK.TABLE_SCHEMA AND PT.ORDINAL_POSITION = CU.ORDINAL_POSITION
        AND PT.CONSTRAINT_SCHEMA = PK.CONSTRAINT_SCHEMA
    WHERE PK.TABLE_SCHEMA in ('" + String.Join("','", this.IncludedSchemas) + @"')     
    ORDER BY FK.TABLE_NAME, CU.CONSTRAINT_NAME, CU.ORDINAL_POSITION ";
        }

        private const string COLUMN_SQL = @"SELECT 
        TABLE_CATALOG AS [Database],
        TABLE_SCHEMA AS Owner, 
        TABLE_NAME AS TableName, 
        COLUMN_NAME AS ColumnName, 
        ORDINAL_POSITION AS OrdinalPosition, 
        COLUMN_DEFAULT AS DefaultSetting, 
        c.IS_NULLABLE AS IsNullable, DATA_TYPE AS DataType, 
        CHARACTER_MAXIMUM_LENGTH AS MaxLength, 
        DATETIME_PRECISION AS DatePrecision,
        NUMERIC_PRECISION AS Precision,
        NUMERIC_SCALE AS Scale,
        COLUMNPROPERTY(object_id('[' + TABLE_SCHEMA + '].[' + TABLE_NAME + ']'), COLUMN_NAME, 'IsIdentity') AS IsIdentity,
        COLUMNPROPERTY(object_id('[' + TABLE_SCHEMA + '].[' + TABLE_NAME + ']'), COLUMN_NAME, 'IsComputed') as IsComputed,
        x.value AS Description
    FROM  INFORMATION_SCHEMA.COLUMNS c
        LEFT JOIN sys.tables st ON c.TABLE_NAME = st.name AND c.TABLE_SCHEMA = SCHEMA_NAME(st.schema_id) 
        LEFT JOIN sys.columns sc ON st.object_id = sc.object_id AND c.COLUMN_NAME = sc.name
        LEFT JOIN sys.extended_properties x ON st.object_id = x.major_id AND sc.column_id = x.minor_id AND x.name = 'MS_Description'
    ORDER BY Owner, TableName, OrdinalPosition ASC
";

        private const string INDEXES_SQL = @"
  SELECT 
       ind.name as IndexName
      ,t.name as TableName
      ,col.name as ColumnName
      ,s.name as SchemaName
  FROM sys.indexes ind 

  INNER JOIN sys.index_columns ic 
      ON  ind.object_id = ic.object_id and ind.index_id = ic.index_id 

  INNER JOIN sys.columns col 
      ON ic.object_id = col.object_id and ic.column_id = col.column_id 

  INNER JOIN sys.tables t 
      ON ind.object_id = t.object_id 

  INNER JOIN sys.objects o
      ON ind.object_id = o.object_id

  INNER JOIN sys.schemas s
      ON o.schema_id = s.schema_id

  ORDER BY t.name, ind.name, ic.index_column_id
";



        private string GetTableSQL()
        {
            return @"
  SELECT 
      DB_NAME() AS TABLE_CATALOG,
      s.name AS TABLE_SCHEMA,
      t.name AS TABLE_NAME,
      t.type_desc AS TABLE_TYPE,
      x.value AS TABLE_DESCRIPTION
  FROM
      sys.tables t 
          INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
          LEFT JOIN sys.extended_properties x ON t.object_id = x.major_id AND x.name = 'MS_Description' AND x.minor_id = 0
  WHERE
      t.type_desc='USER_TABLE' and s.name in ('" + String.Join("','", this.IncludedSchemas) + @"')     
  ORDER BY s.name, t.name";
        }

        private List<Column> GetPKColumns(Table table)
        {
            var columns = this.LoadColumns(table);

            if (this.allPKColumnsByFQTableName == null)
            {
                string sql = @"SELECT KCU.TABLE_SCHEMA, KCU.TABLE_NAME, KCU.COLUMN_NAME 
            FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE KCU
            JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS TC
            ON KCU.CONSTRAINT_NAME=TC.CONSTRAINT_NAME
            WHERE TC.CONSTRAINT_TYPE='PRIMARY KEY'
            ORDER BY KCU.TABLE_SCHEMA, KCU.TABLE_NAME, KCU.ORDINAL_POSITION;
            --AND KCU.TABLE_NAME=@tableName";

                var cmd = this.GetCommand(sql);

                var allPKColumnData = new List<Tuple<string, string, string>>();

                using (IDataReader rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    while (rdr.Read())
                    {
                        var dataItem = new Tuple<string, string, string>(
                            rdr["table_schema"].ToString(), rdr["table_name"].ToString(), rdr["column_name"].ToString());

                        allPKColumnData.Add(dataItem);
                    }
                }

                this.allPKColumnsByFQTableName =
                    (from pkc in allPKColumnData
                     let fqn = pkc.Item1 + "." + pkc.Item2
                     group pkc by fqn into g
                     select g)
                        .ToDictionary(x => x.Key, x => x.Select(y => y.Item3).ToList());
            }

            List<string> pkColumnNames;

            if (!this.allPKColumnsByFQTableName.TryGetValue(table.Schema + "." + table.Name, out pkColumnNames))
                pkColumnNames = new List<string>();

            List<Column> pkColumns =
                (from c in columns
                 where pkColumnNames.Contains(c.Name)
                 select c)
                    .ToList();

            foreach (Column pkColumn in pkColumns)
            {
                pkColumn.IsPK = true;
            }

            return pkColumns;
        }

        private List<FKTable> LoadFKTables(Table table)
        {
            if (this.allFkTablesByFQTableName == null)
            {
                var allFKs = new List<FKTable>();
                var cmd = this.GetCommand(this.GetFKSqlSQL());
                cmd.CommandTimeout = 0; //no timeout
                //cmd.Parameters.AddWithValue("@tableName",table.Name);
                using (IDataReader rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    string mostRecentConstraintName = null;
                    FKTable fk = null, inverseFK = null;

                    while (rdr.Read())
                    {
                        string currentConstraintName = rdr["Constraint_Name"].ToString();

                        if (currentConstraintName != mostRecentConstraintName)
                        {
                            if (fk != null)
                            {
                                allFKs.Add(fk);
                                allFKs.Add(inverseFK);
                            }

                            fk = new FKTable();
                            inverseFK = new FKTable();

                            fk.ThisTable = rdr["ThisTable"].ToString();
                            fk.ThisTableSchema = rdr["Owner"].ToString();
                            fk.OtherTable = rdr["OtherTable"].ToString();
                            fk.OtherTableSchema = rdr["OtherOwner"].ToString();
                            fk.IsPrimaryTable = false;
                            fk.OtherClass = CompositeTermInflector.MakeSingular(this.CleanUp(fk.OtherTable));
                            fk.OtherQueryable = CompositeTermInflector.MakePlural(fk.OtherClass);
                            fk.ConstraintName = rdr["Constraint_Name"].ToString();

                            inverseFK.ThisTable = rdr["OtherTable"].ToString();
                            inverseFK.ThisTableSchema = rdr["OtherOwner"].ToString();
                            inverseFK.OtherTable = rdr["ThisTable"].ToString();
                            inverseFK.OtherTableSchema = rdr["Owner"].ToString();
                            inverseFK.IsPrimaryTable = true;
                            inverseFK.OtherClass = CompositeTermInflector.MakeSingular(this.CleanUp(inverseFK.OtherTable));
                            inverseFK.OtherQueryable = CompositeTermInflector.MakePlural(inverseFK.OtherClass);
                            inverseFK.ConstraintName = rdr["Constraint_Name"].ToString();
                        }

                        fk.ThisColumns.Add(rdr["ThisColumn"].ToString());
                        fk.OtherColumns.Add(rdr["OtherColumn"].ToString());

                        inverseFK.ThisColumns.Add(rdr["OtherColumn"].ToString());
                        inverseFK.OtherColumns.Add(rdr["ThisColumn"].ToString());

                        mostRecentConstraintName = currentConstraintName;
                    }

                    // Add the final one
                    if (fk != null)
                    {
                        allFKs.Add(fk);
                        allFKs.Add(inverseFK);
                    }
                }

                this.allFkTablesByFQTableName =
                    (from fk in allFKs
                     let fqn = (fk.ThisTableSchema + "." + fk.ThisTable).ToLower()
                     group fk by fqn
                         into g
                         select g)
                        .ToDictionary(x => x.Key, x => x.ToList());
            }

            //this is a "bi-directional" scheme
            //which pulls both 1-many and many-1

            List<FKTable> result;
            string fqnLookup = (table.Schema + "." + table.Name).ToLower();

            if (!this.allFkTablesByFQTableName.TryGetValue(fqnLookup, out result))
                return new List<FKTable>();

            return result;
        }

        private IDataReader GetReader(string sql)
        {
            SqlConnection conn = new SqlConnection(this.ConnectionString);
            this.connections.Add(conn);
            SqlCommand cmd = new SqlCommand(sql, conn);
            conn.Open();
            return cmd.ExecuteReader(CommandBehavior.CloseConnection);
        }

        private SqlCommand GetCommand(string sql)
        {
            SqlConnection conn = new SqlConnection(this.ConnectionString);
            this.connections.Add(conn);
            SqlCommand cmd = new SqlCommand(sql, conn);
            conn.Open();
            return cmd;
        }

        string GetSysType(string sqlType)
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
        string GetNHType(string sqlType)
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

        private DatabaseSchema.DbType GetDbType(string sqlType)
        {
            switch (sqlType)
            {
                case "varchar":
                    return DatabaseSchema.DbType.AnsiString;
                case "nvarchar":
                    return DatabaseSchema.DbType.String;
                case "int":
                    return DatabaseSchema.DbType.Int32;
                case "uniqueidentifier":
                    return DatabaseSchema.DbType.Guid;
                case "datetime":
                    return DatabaseSchema.DbType.DateTime;
                case "date":
                    return DbType.Date;
                case "bigint":
                    return DatabaseSchema.DbType.Int64;
                case "binary":
                    return DatabaseSchema.DbType.Binary;
                case "bit":
                    return DatabaseSchema.DbType.Boolean;
                case "char":
                    return DatabaseSchema.DbType.AnsiStringFixedLength;
                case "decimal":
                    return DatabaseSchema.DbType.Decimal;
                case "float":
                    return DatabaseSchema.DbType.Double;
                case "image":
                    return DatabaseSchema.DbType.Binary;
                case "money":
                    return DatabaseSchema.DbType.Currency;
                case "nchar":
                    return DatabaseSchema.DbType.String;
                case "ntext":
                    return DatabaseSchema.DbType.String;
                case "numeric":
                    return DatabaseSchema.DbType.Decimal;
                case "real":
                    return DatabaseSchema.DbType.Single;
                case "smalldatetime":
                    return DatabaseSchema.DbType.DateTime;
                case "smallint":
                    return DatabaseSchema.DbType.Int16;
                case "smallmoney":
                    return DatabaseSchema.DbType.Currency;
                case "sql_variant":
                    return DatabaseSchema.DbType.String;
                case "sysname":
                    return DatabaseSchema.DbType.String;
                case "text":
                    return DatabaseSchema.DbType.AnsiString;
                case "timestamp":
                    return DatabaseSchema.DbType.Binary;
                case "tinyint":
                    return DatabaseSchema.DbType.Byte;
                case "varbinary":
                    return DatabaseSchema.DbType.Binary;
                case "xml":
                    return DatabaseSchema.DbType.Xml;
                default:
                    return DatabaseSchema.DbType.AnsiString;
            }

        }

        string CleanUp(string tableName)
        {
            string result = tableName;

            //strip blanks
            result = result.Replace(" ", "");

            //put your logic here...

            return result;
        }
    }
}
