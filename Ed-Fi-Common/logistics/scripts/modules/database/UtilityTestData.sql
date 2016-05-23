--$sqltestparms = @{Doublethis = "C:\Projects\TennesseeDOE\Ed-Fi-Common\logistics\scripts\modules\database\database-management.psm1"; FatHat = 23421; JamBgo = "dfadfadf"; ThatsOneBadPath="Harry:\Nonexistant"}
--#ps Permission:Admin
--#ps Variable:JamBgo
--#ps Variable:NotHere
--#ps VariableFileContent:NorThere
--#ps VariableFileContent:Doublethis
CREATE NONCLUSTERED INDEX IX_TeacherSection_Staff_School ON domain.TeacherSection (StaffUSI, SchoolId)
GO
 
sp_rename '[domain].[StudentList]', 'StudentListView'
GO
 
SELECT * INTO domain.StudentList
FROM domain.StudentListView
GO
--#ps Variable:FatHat
CREATE NONCLUSTERED INDEX IX_StudentList ON domain.StudentList
                (
                StudentUSI, SchoolId
                )
--#ps VariableFileContent:ThatsOneBadPath
GO
 
CREATE NONCLUSTERED INDEX IX_StudentList_SchoolId ON domain.StudentList
                (
                SchoolId
                )
GO

/*
 select thisShouldNotBeParsed from AsAStatement
 where thisWholeThingIs = 'In a comment block'
 GO
*/

/*
--jkj /* gdgdg
 select thisShouldNotBeParsed from AsAStatement
 where thisWholeThingIs = 'In a comment block'
 GO
*/   --  svdgsfgssfgfsgsf ---sfgsffsfsgfsfsffgsa
select * from sys.Tables /****** Script for All ONE Line Check GO ******/ select * from sys.columns 
*/
--Nested
select* from sys.Tables /**** --/** Script for SelectTopNRows command from SSMS  /***/***/ select * from sys.columns GO
/* /*
SELECT TOP 1000 [TEST_ARTIFACT_ID]
      ,[TEST_ARTIFACT_NAME]
      ,[ARTIFACT_TYPE]
      ,[DISPLAY_ORDER]
      ,[VISIBLE]
      ,[TEST_ID] */
      ,[FILTER_CLAUSE_FOR_ACTUAL_OUTPUT]
  FROM [NeDOE_TestData].[TMETA].[T_TEST_ARTIFACT]
*/

CREATE NONCLUSTERED INDEX IX_StudentRecordCurrentCourse
ON [domain].[StudentRecordCurrentCourse] ([SchoolId])
INCLUDE ([StudentUSI],[TermTypeId],[TermType],[LocalCourseCode],[CourseTitle],[SubjectArea],[CreditsToBeEarned],[Instructor],[GradeLevel],[GradingPeriod],[LetterGradeEarned],[NumericGradeEarned],[TrendDirection])
GO
--#ps Timeout:1800

-- Test that commented out sections get ignored
/*
CREATE INDEX IX_LocalEducationAgencyToSchool_ChildId
    ON auth.LocalEducationAgencyIdToSchool (ChildId);
GO
*/



Select * from dummy.table