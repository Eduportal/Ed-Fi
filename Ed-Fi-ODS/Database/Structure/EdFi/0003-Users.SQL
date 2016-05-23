IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = N'edfiLoader')
CREATE USER [edfiLoader] FOR LOGIN [edfiLoader] WITH DEFAULT_SCHEMA=[EdFi]
GO
