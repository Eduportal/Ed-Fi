IF EXISTS (SELECT 1 from master.dbo.syslogins where loginname='edfiAdminApp' union select 1 from sysusers where name='edfiAdminApp')
BEGIN
  IF NOT EXISTS
    (SELECT name
     FROM sys.database_principals
     WHERE name = 'edfiAdminApp')
  BEGIN
     CREATE USER [edfiAdminApp] FOR LOGIN [edfiAdminApp] WITH DEFAULT_SCHEMA=[dbo]
  END

    CREATE ROLE [db_executor]
  GRANT EXECUTE ON SCHEMA::[dbo] TO db_executor

  EXEC sp_addrolemember 'db_ddladmin', 'edfiAdminApp'
  EXEC sp_addrolemember 'db_datareader', 'edfiAdminApp'
  EXEC sp_addrolemember 'db_datawriter', 'edfiAdminApp'
  EXEC sp_addrolemember 'db_executor', 'edfiAdminApp'
  EXEC sp_addrolemember 'db_backupoperator', 'edfiAdminApp'

END
GO

