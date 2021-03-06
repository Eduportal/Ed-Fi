/*
   Friday, June 14, 20139:33:55 AM
   User: 
   Server: (local)
   Database: EdFi_Ods2
   Application: 
*/

/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE edfi.StudentAssessmentItem
	DROP CONSTRAINT REL_59
GO
ALTER TABLE edfi.StudentObjectiveAssessment SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'edfi.StudentObjectiveAssessment', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'edfi.StudentObjectiveAssessment', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'edfi.StudentObjectiveAssessment', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
GO
ALTER TABLE edfi.StudentAssessmentItem SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'edfi.StudentAssessmentItem', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'edfi.StudentAssessmentItem', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'edfi.StudentAssessmentItem', 'Object', 'CONTROL') as Contr_Per 