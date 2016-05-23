USE [EdFi_Admin]

DECLARE @dbName varchar(15)
SET @dbName = 'EdFi_Admin'

DECLARE @edfiAdmin varchar(15)
SET @edfiAdmin = 'EdFiAdmin'

DECLARE @edfiAdminPassword varchar(15)
SET @edfiAdminPassword = '***REMOVED***1!'

DECLARE @CreateLoginSql nvarchar(500)
DECLARE @CreateUserSql nvarchar(128)

-- Create the login
IF not Exists (select loginname from master.dbo.syslogins where loginname = @edfiAdmin)
BEGIN -- Create user for SQL Authentication
	SELECT @CreateLoginSql = 'CREATE LOGIN [' + @edfiAdmin + '] WITH PASSWORD = ''' + @edfiAdminPassword + ''', DEFAULT_DATABASE = [' + @dbName + ']'
	PRINT @CreateLoginSql
	EXEC sp_executesql @CreateLoginSql
END

-- Add user to database
IF not Exists (select * from sys.database_principals where name = @edfiAdmin)
BEGIN
	SELECT @CreateUserSql = 'CREATE USER [' + @edfiAdmin + '] FOR LOGIN [' + @edfiAdmin + ']'
	PRINT @CreateUserSql
	EXEC sp_executesql @CreateUserSql
	EXEC sp_addrolemember 'db_datareader', @edfiAdmin
	EXEC sp_addrolemember 'db_datawriter', @edfiAdmin
END

--
--	edfiLoader User
--

DECLARE @edfiLoader varchar(15)
SET @edfiLoader = 'edfiLoader'

-- Create the login
IF not Exists (select loginname from master.dbo.syslogins where loginname = @edfiLoader)
BEGIN -- Create user for SQL Authentication
	SELECT @CreateLoginSql = 'CREATE LOGIN [' + @edfiLoader + '] WITH PASSWORD = ''' + @edfiLoader + ''', DEFAULT_DATABASE = [' + @dbName + ']'
	PRINT @CreateLoginSql
	EXEC sp_executesql @CreateLoginSql
END

-- Add user to database
IF not Exists (select * from sys.database_principals where name = @edfiLoader)
BEGIN
	SELECT @CreateUserSql = 'CREATE USER [' + @edfiLoader + '] FOR LOGIN [' + @edfiLoader + ']'
	PRINT @CreateUserSql
	EXEC sp_executesql @CreateUserSql
	EXEC sp_addrolemember 'db_datareader', @edfiLoader
	EXEC sp_addrolemember 'db_datawriter', @edfiLoader
END
