<Query Kind="Statements">
  <Connection>
    <ID>7e6a7ca2-d38a-4b55-b0c1-1258c4d50577</ID>
    <Server>(local)</Server>
    <Database>master</Database>
    <ShowServer>true</ShowServer>
  </Connection>
</Query>

const string databaseName = "EdFi_Ods_Empty";
const string sqlFilesBasePath = @"C:\Program Files\Microsoft SQL Server\MSSQL11.MSSQLSERVER\MSSQL";

bool skipDelete = true;

string basePath = Path.GetDirectoryName(Path.GetDirectoryName(Util.CurrentQueryPath));
string bakFolder = basePath + @"\Application\packages\EdFi.Samples.Ods.1.2.1.3";

// Get current user's name
AppDomain myDomain = Thread.GetDomain();
myDomain.SetPrincipalPolicy(System.Security.Principal.PrincipalPolicy.WindowsPrincipal);
var x = (System.Security.Principal.WindowsPrincipal) Thread.CurrentPrincipal;
string username = x.Identity.Name.Dump();

if (!skipDelete)
{
	ExecuteCommand(@"
EXEC msdb.dbo.sp_delete_database_backuphistory @database_name = N'" + databaseName + @"'
;
USE [master]
;
ALTER DATABASE [" + databaseName + @"] SET  SINGLE_USER WITH ROLLBACK IMMEDIATE
;
USE [master]
;
/****** Object:  Database [" + databaseName + @"]    Script Date: 08/28/2013 22:44:02 ******/
DROP DATABASE [" + databaseName + @"]
;
");
}

	// Restore database
	ExecuteCommand(@"
RESTORE DATABASE [" + databaseName + @"] FROM  DISK = N'" + bakFolder + @"\EdFi_GrandBendISD_EdFi_IntegrationTemp.bak' WITH  FILE = 1,  MOVE N'EdFi_GrandBendISD_EdFi_IntegrationTemp' TO N'" + sqlFilesBasePath + @"\DATA\" + databaseName + @".mdf',  MOVE N'EdFi_GrandBendISD_EdFi_IntegrationTemp_log' TO N'" + sqlFilesBasePath + @"\Log\" + databaseName + @".LDF',  NOUNLOAD,  REPLACE,  STATS = 10
;
EXEC [" + databaseName + @"].dbo.sp_changedbowner @loginame = N'" + username + @"', @map = false
;
");

ExecuteCommand(@"
USE [" + databaseName + @"];
;
ALTER USER edfiLoader WITH LOGIN=edfiLoader
;
");