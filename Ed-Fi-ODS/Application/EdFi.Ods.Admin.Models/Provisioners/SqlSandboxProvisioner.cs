using System;
using System.Data.Entity;
using System.IO;
using System.Linq;

namespace EdFi.Ods.Admin.Models
{
    using System.Globalization;
    using System.Text.RegularExpressions;

    public class SqlSandboxProvisioner : AzureSandboxProvisioner
    {
        protected override void CopySandbox(string orig, string copy)
        {
            string datafile, logfile;
            var backup = Path.Combine(GetBackupDirectory(), orig + ".bak");
            GetDatabaseFiles(copy, out datafile, out logfile);

            var sql = string.Format(@"BACKUP DATABASE [{0}] TO DISK = '{1}' WITH INIT;", orig, backup);
            ObjectContext.ExecuteStoreCommand(TransactionalBehavior.DoNotEnsureTransaction, sql);

            sql = string.Format(@"RESTORE FILELISTONLY FROM DISK = '{0}';", backup);
            var files = ObjectContext.ExecuteStoreQuery<FileListResult>(sql, TransactionalBehavior.DoNotEnsureTransaction).ToArray();
            var data = files[0].LogicalName;
            var log = files[0].LogicalName;

            sql = string.Format(@"RESTORE DATABASE [{0}] FROM DISK = '{1}' WITH MOVE '{2}' TO '{4}', MOVE '{3}_log' TO '{5}';",
                                copy, backup, data, log, datafile, logfile);
            ObjectContext.ExecuteStoreCommand(TransactionalBehavior.DoNotEnsureTransaction, sql);
        }

        private class FileListResult
        {
            public string LogicalName { get; set; }
            public string PhysicalName { get; set; }
            public string Type { get; set; }
        }

        private class DBResult
        {
            public string Value { get; set; }
            public string Data { get; set; }
        }

        private string GetBackupDirectory()
        {
            return GetSqlRegistryValue(@"HKEY_LOCAL_MACHINE", @"Software\Microsoft\MSSQLServer\MSSQLServer", @"BackupDirectory");
        }

        private string GetSqlRegistryValue(string subtree, string folder, string key)
        {
            var sql = string.Format(@"EXEC master.dbo.xp_instance_regread N'{0}', N'{1}',N'{2}'", subtree, folder, key);
            var path = ObjectContext.ExecuteStoreQuery<DBResult>(sql).FirstOrDefault();
            if (path == null)
                return null;
            return path.Data;
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
