IF (NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'uid'))  
BEGIN 
    EXEC ('CREATE SCHEMA [uid] AUTHORIZATION [dbo]') 
END 
GO

CREATE TABLE [uid].UniqueIdPersonMapping
(
       Id uniqueidentifier NOT NULL,
       UniqueId nvarchar(32) NOT NULL,
);
GO

ALTER TABLE [uid].UniqueIdPersonMapping ADD CONSTRAINT PK_UniqueIdMapping PRIMARY KEY CLUSTERED (Id);
ALTER TABLE [uid].UniqueIdPersonMapping ADD CONSTRAINT UI_UniqueIdPersonMapping_UniqueId UNIQUE NONCLUSTERED (UniqueId) ;
GO
