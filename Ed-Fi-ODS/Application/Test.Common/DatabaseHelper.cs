using System;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Test.Common
{
    public class DatabaseHelper
    {
        public void CopyDatabase(string originalDatabaseName, string newDatabaseName)
        {
            string datafile, logfile;
            var backup = Path.Combine(GetBackupDirectory(), originalDatabaseName + ".bak");
            GetDatabaseFiles(newDatabaseName, out datafile, out logfile);

            var sql = string.Format(@"BACKUP DATABASE [{0}] TO DISK = '{1}' WITH INIT;", originalDatabaseName, backup);
            ObjectContext.ExecuteStoreCommand(TransactionalBehavior.DoNotEnsureTransaction, sql);

            sql = string.Format(@"RESTORE FILELISTONLY FROM DISK = '{0}';", backup);
            var files = ObjectContext.ExecuteStoreQuery<FileListResult>(sql, TransactionalBehavior.DoNotEnsureTransaction).ToArray();
            var data = files[0].LogicalName;
            var log = files[0].LogicalName;

            sql = string.Format(@"RESTORE DATABASE [{0}] FROM DISK = '{1}' WITH MOVE '{2}' TO '{4}', MOVE '{3}_log' TO '{5}';", newDatabaseName, backup, data, log, datafile, logfile);
            ObjectContext.ExecuteStoreCommand(TransactionalBehavior.DoNotEnsureTransaction, sql);
        }

        public void DropDatabase(string databaseName)
        {

            var sql = string.Format(@"ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{0}]", databaseName);
            ObjectContext.ExecuteStoreCommand(TransactionalBehavior.DoNotEnsureTransaction, sql);
        }

        private class FileListResult
        {
            public string LogicalName { get; set; }
            public string PhysicalName { get; set; }
            public string Type { get; set; }
        }
        private class DbResult
        {
            public string Value { get; set; }
            public string Data { get; set; }
        }

        private string _connectionString;
        private ObjectContext _context;
        private ObjectContext ObjectContext
        {
            get
            {
                if (_context == null)
                {
                    var tmp = new DbContext("EdFi_master");
                    _connectionString = tmp.Database.Connection.ConnectionString;
                    _context = (tmp as IObjectContextAdapter).ObjectContext;
                }

                return _context;
            }
        }
        
        private string GetBackupDirectory()
        {
            return GetSqlRegistryValue(@"HKEY_LOCAL_MACHINE", @"Software\Microsoft\MSSQLServer\MSSQLServer", @"BackupDirectory");
        }

        private string GetSqlRegistryValue(string subtree, string folder, string key)
        {
            var sql = string.Format(@"EXEC master.dbo.xp_instance_regread N'{0}', N'{1}',N'{2}'", subtree, folder, key);
            var path = ObjectContext.ExecuteStoreQuery<DbResult>(sql).FirstOrDefault();
            return (path != null) ? path.Data : null;
        }

        private string GetSqlDataPath()
        {
            var path = GetSqlRegistryValue(@"HKEY_LOCAL_MACHINE", @"Software\Microsoft\MSSQLServer\MSSQLServer", @"DefaultData");

            if (string.IsNullOrWhiteSpace(path))
                path = GetSqlRegistryValue(@"HKEY_LOCAL_MACHINE", @"Software\Microsoft\MSSQLServer\Setup", @"SQLDataRoot");
            
            if (string.IsNullOrWhiteSpace(path))
                throw new Exception("Failed to get a result querying for the default SQL data path");
            
            var rex = new Regex(@"(\\DATA[\\]?$)|(\\$)", RegexOptions.IgnoreCase);

            path = rex.Replace(path, string.Empty) + @"\Data";
            
            return path;
        }

        private void GetDatabaseFiles(string dbName, out string data, out string log)
        {
            var basePath = GetSqlDataPath();
            data = Path.Combine(basePath, string.Format("{0}.mdf", dbName));
            log = Path.Combine(basePath, string.Format("{0}.ldf", dbName));
        }
    }
}
