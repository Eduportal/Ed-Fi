-- RUN THIS FIRST
-- Create the database

IF (NOT EXISTS (
	SELECT name
	FROM master.dbo.sysdatabases 
	WHERE ('[' + name + ']' = 'EdFi_Admin' OR name = 'EdFi_Admin')
	))
BEGIN
	CREATE DATABASE [EdFi_Admin]
END