/*
IF EXISTS (SELECT 1 from master.dbo.syslogins where loginname='edfiAdminApp' union select 1 from sysusers where name='edfiAdminApp')
BEGIN
  IF NOT EXISTS
    (SELECT name
     FROM sys.database_principals
     WHERE name = 'edfiAdminApp')
  BEGIN
*/
	CREATE USER [edfiAdminApp] FOR LOGIN [edfiAdminApp] WITH DEFAULT_SCHEMA=[EdFi]
	GO
/*
  END
*/
  EXEC sp_addrolemember N'db_datawriter', N'edfiAdminApp'
  EXEC sp_addrolemember N'db_datareader', N'edfiAdminApp'
  EXEC sp_addrolemember N'db_ddladmin', N'edfiAdminApp'
  EXEC sp_addrolemember N'db_backupoperator', N'edfiAdminApp'
  GRANT EXECUTE TO [edfiAdminApp]
/*
END
*/
GO
