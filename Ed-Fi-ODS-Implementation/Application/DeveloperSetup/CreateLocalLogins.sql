USE [master]

DECLARE @defaultDb varchar(15)
DECLARE @loginName varchar(15)
DECLARE @loginPassword varchar(15)
DECLARE @CreateLoginSql nvarchar(500)
DECLARE @CreateUserSql nvarchar(128)

SET @defaultDb = 'master'
SET @loginName = 'edfiAdmin'
SET @loginPassword = 'edfiAdmin'

-- Create the login
IF not Exists (select loginname from master.dbo.syslogins where loginname = @loginName)
BEGIN -- Create user for SQL Authentication
	PRINT 'Creating login ' + @loginName
	SELECT @CreateLoginSql = 'CREATE LOGIN [' + @loginName + '] WITH PASSWORD = ''' + @loginPassword + ''', DEFAULT_DATABASE = [' + @defaultDb + '], CHECK_POLICY = OFF'
	EXEC sp_executesql @CreateLoginSql
	EXEC sp_addsrvrolemember @loginName, 'sysadmin'
END

SET @defaultDb = 'master'
SET @loginName = 'edfiAdminApp'
SET @loginPassword = 'edfiAdminApp'

-- Create the login
IF not Exists (select loginname from master.dbo.syslogins where loginname = @loginName)
BEGIN -- Create user for SQL Authentication
	PRINT 'Creating login ' + @loginName
	SELECT @CreateLoginSql = 'CREATE LOGIN [' + @loginName + '] WITH PASSWORD = ''' + @loginPassword + ''', DEFAULT_DATABASE = [' + @defaultDb + '], CHECK_POLICY = OFF'
	EXEC sp_executesql @CreateLoginSql
END

SET @defaultDb = 'master'
SET @loginName = 'edfiLoader'
SET @loginPassword = 'edfiLoader'

-- Create the login
IF not Exists (select loginname from master.dbo.syslogins where loginname = @loginName)
BEGIN -- Create user for SQL Authentication
	PRINT 'Creating login ' + @loginName
	SELECT @CreateLoginSql = 'CREATE LOGIN [' + @loginName + '] WITH PASSWORD = ''' + @loginPassword + ''', DEFAULT_DATABASE = [' + @defaultDb + '], CHECK_POLICY = OFF'
	EXEC sp_executesql @CreateLoginSql
	EXEC sp_addsrvrolemember @loginName, 'dbcreator'
END
