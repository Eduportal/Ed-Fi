/*
IF EXISTS (SELECT 1 from master.dbo.syslogins where loginname='edfiLoader' union select 1 from sysusers where name='edfiLoader')
BEGIN
  IF NOT EXISTS
    (SELECT name
     FROM sys.database_principals
     WHERE name = 'edfiLoader')
  BEGIN
*/
	CREATE USER [edfiLoader] FOR LOGIN [edfiLoader] WITH DEFAULT_SCHEMA=[EdFi]
	GO
/*
  END
*/
  EXEC sp_addrolemember N'db_datawriter', N'edfiLoader'
  EXEC sp_addrolemember N'db_datareader', N'edfiLoader'
  EXEC sp_addrolemember N'db_ddladmin', N'edfiLoader'
  EXEC sp_addrolemember N'db_backupoperator', N'edfiLoader'
  GRANT EXECUTE TO [edfiLoader]
/*
END
*/
GO
