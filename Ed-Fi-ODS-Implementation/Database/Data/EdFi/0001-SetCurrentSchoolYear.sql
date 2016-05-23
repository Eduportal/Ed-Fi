UPDATE [edfi].[SchoolYearType]
   SET [CurrentSchoolYear] = 1
 WHERE SchoolYear=2015
GO

UPDATE [edfi].[SchoolYearType]
   SET [CurrentSchoolYear] = 0
 WHERE SchoolYear<>2015
GO


