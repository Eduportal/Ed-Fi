IF NOT EXISTS (SELECT * FROM sys.synonyms WHERE name = N'LocalEducationAgencyIdToParentGuid' AND schema_id = SCHEMA_ID(N'auth'))
CREATE SYNONYM [auth].[LocalEducationAgencyIdToParentGuid] FOR [auth].[LocalEducationAgencyIdToParent]
GO
IF NOT EXISTS (SELECT * FROM sys.synonyms WHERE name = N'LocalEducationAgencyIdToParentUniqueId' AND schema_id = SCHEMA_ID(N'auth'))
CREATE SYNONYM [auth].[LocalEducationAgencyIdToParentUniqueId] FOR [auth].[LocalEducationAgencyIdToParent]
GO
IF NOT EXISTS (SELECT * FROM sys.synonyms WHERE name = N'LocalEducationAgencyIdToStaffGuid' AND schema_id = SCHEMA_ID(N'auth'))
CREATE SYNONYM [auth].[LocalEducationAgencyIdToStaffGuid] FOR [auth].[LocalEducationAgencyIdToStaff]
GO
IF NOT EXISTS (SELECT * FROM sys.synonyms WHERE name = N'LocalEducationAgencyIdToStaffUniqueId' AND schema_id = SCHEMA_ID(N'auth'))
CREATE SYNONYM [auth].[LocalEducationAgencyIdToStaffUniqueId] FOR [auth].[LocalEducationAgencyIdToStaff]
GO
IF NOT EXISTS (SELECT * FROM sys.synonyms WHERE name = N'LocalEducationAgencyIdToStudentGuid' AND schema_id = SCHEMA_ID(N'auth'))
CREATE SYNONYM [auth].[LocalEducationAgencyIdToStudentGuid] FOR [auth].[LocalEducationAgencyIdToStudent]
GO
IF NOT EXISTS (SELECT * FROM sys.synonyms WHERE name = N'LocalEducationAgencyIdToStudentUniqueId' AND schema_id = SCHEMA_ID(N'auth'))
CREATE SYNONYM [auth].[LocalEducationAgencyIdToStudentUniqueId] FOR [auth].[LocalEducationAgencyIdToStudent]
GO
IF NOT EXISTS (SELECT * FROM sys.synonyms WHERE name = N'ParentGuidToStudentGuid' AND schema_id = SCHEMA_ID(N'auth'))
CREATE SYNONYM [auth].[ParentGuidToStudentGuid] FOR [auth].[ParentToStudent]
GO
IF NOT EXISTS (SELECT * FROM sys.synonyms WHERE name = N'ParentGuidToStudentUniqueId' AND schema_id = SCHEMA_ID(N'auth'))
CREATE SYNONYM [auth].[ParentGuidToStudentUniqueId] FOR [auth].[ParentToStudent]
GO
IF NOT EXISTS (SELECT * FROM sys.synonyms WHERE name = N'ParentUniqueIdToStudentGuid' AND schema_id = SCHEMA_ID(N'auth'))
CREATE SYNONYM [auth].[ParentUniqueIdToStudentGuid] FOR [auth].[ParentToStudent]
GO
IF NOT EXISTS (SELECT * FROM sys.synonyms WHERE name = N'ParentUniqueIdToStudentUniqueId' AND schema_id = SCHEMA_ID(N'auth'))
CREATE SYNONYM [auth].[ParentUniqueIdToStudentUniqueId] FOR [auth].[ParentToStudent]
GO
IF NOT EXISTS (SELECT * FROM sys.synonyms WHERE name = N'SchoolIdToStaffGuid' AND schema_id = SCHEMA_ID(N'auth'))
CREATE SYNONYM [auth].[SchoolIdToStaffGuid] FOR [auth].[SchoolIdToStaff]
GO
IF NOT EXISTS (SELECT * FROM sys.synonyms WHERE name = N'SchoolIdToStaffUniqueId' AND schema_id = SCHEMA_ID(N'auth'))
CREATE SYNONYM [auth].[SchoolIdToStaffUniqueId] FOR [auth].[SchoolIdToStaff]
GO
IF NOT EXISTS (SELECT * FROM sys.synonyms WHERE name = N'SchoolIdToStudentGuid' AND schema_id = SCHEMA_ID(N'auth'))
CREATE SYNONYM [auth].[SchoolIdToStudentGuid] FOR [auth].[SchoolIdToStudent]
GO
IF NOT EXISTS (SELECT * FROM sys.synonyms WHERE name = N'SchoolIdToStudentUniqueId' AND schema_id = SCHEMA_ID(N'auth'))
CREATE SYNONYM [auth].[SchoolIdToStudentUniqueId] FOR [auth].[SchoolIdToStudent]
GO
