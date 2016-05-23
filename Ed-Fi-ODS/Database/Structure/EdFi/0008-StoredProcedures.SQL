SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_ssis_addlogentry]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[sp_ssis_addlogentry] AS' 
END
GO



ALTER PROCEDURE [dbo].[sp_ssis_addlogentry]  
	@event sysname,  
	@computer nvarchar(128),  
	@operator nvarchar(128),  
	@source nvarchar(1024),  
	@sourceid uniqueidentifier,  
	@executionid uniqueidentifier,  
	@starttime datetime,  
	@endtime datetime,  
	@datacode int,  
	@databytes image,  
	@message nvarchar(2048)
  AS  
	INSERT INTO sysssislog (      
		event,      
		computer,      
		operator,      
		source,      
		sourceid,      
		executionid,      
		starttime,      
		endtime,      
		datacode,      
		databytes,      
		message )  
	VALUES (      
		@event,      
		@computer,      
		@operator,      
		@source,      
		@sourceid,      
		@executionid,      
		@starttime,      
		@endtime,      
		@datacode,      
		@databytes,      
		@message )  
RETURN 0
--
--
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[util].[BuildSummaryEmail]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [util].[BuildSummaryEmail] AS' 
END
GO



ALTER PROCEDURE [util].[BuildSummaryEmail] (
	@RecipientEmailAddress nvarchar(255),
	@ServerName nvarchar(255) = NULL
)
AS
BEGIN
	SET NOCOUNT ON
	
	DECLARE @ServerNameInSubjectLine nvarchar(255)
	SELECT @ServerNameInSubjectLine = ISNULL(@ServerName, @@SERVERNAME)

	IF OBJECT_ID('tempdb..#BuildSummary') IS NOT NULL
		DROP TABLE #BuildSummary
	CREATE TABLE #BuildSummary (
		BuildSummaryId					int identity(1,1) primary key,
		ServerName						nvarchar(255),
		DistrictName					nvarchar(255),
		TeamCityBuildTypeId				nvarchar(255),
		TeamCityBuildId					bigint,
		TeamCityBuildNumber				nvarchar(255),
		BuildStatus						nvarchar(255),
		BuildFinishTime					datetime,
		ElapsedMinutes					int,
		ElapsedStatus					nvarchar(255),
		EdFiExceptionErrors				nvarchar(255),
		EdFiExceptionWarnings			nvarchar(255),
		EdFiExceptionEntityNotFounds	nvarchar(255),
		EdFiExceptionMaxErrorRate		nvarchar(255),
		FailedQAScripts					nvarchar(255),
		FailedSanityCheck				nvarchar(255)
	)


	-----------------------------------
	-- Collect data
	-----------------------------------
	
	INSERT INTO #BuildSummary(TeamCityBuildTypeId, TeamCityBuildId, TeamCityBuildNumber, BuildStatus, BuildFinishTime, ElapsedMinutes)
	SELECT TeamCityBuildTypeId, TeamCityBuildId, TeamCityBuildNumber, BuildStatus, BuildFinishTime, DATEDIFF(minute, BuildFinishTime, GETDATE()) 
	FROM (
		SELECT 
			build_type_id as TeamCityBuildTypeId, 
			build_id as TeamCityBuildId, 
			build_number as TeamCityBuildNumber, 
			status_text as BuildStatus,
			-- add -5 during daylight saving time or -6 otherwise
			DATEADD(hour, -6, DATEADD(second, build_finish_time_server/1000, '1970/1/1')) as BuildFinishTime,
			ROW_NUMBER() OVER (PARTITION BY build_type_id ORDER BY build_finish_time_server DESC) as ranking
		FROM TeamCity.dbo.history a WITH (NOLOCK)
		WHERE DATEADD(hour, -5, DATEADD(second, build_finish_time_server/1000, '1970/1/1')) > GETDATE() - 7
		AND EXISTS (
			SELECT 1 
			FROM EdFi_LoadReport.util.TeamCityStatisticHistory b WITH (NOLOCK)
			WHERE a.build_id = b.TeamCityBuildId
		) 
	) temp
	WHERE ranking = 1


	IF OBJECT_ID('tempdb..#TeamCityStatisticHistoryPivot') IS NOT NULL
		DROP TABLE #TeamCityStatisticHistoryPivot
	SELECT
		TeamCityBuildId,
		MIN(ServerName) as ServerName,
		MIN(DatabaseName) as DistrictName,
		MIN(CASE WHEN StatisticKey = 'EdFiExceptionErrors' THEN StatisticValue END) as EdFiExceptionErrors,
		MIN(CASE WHEN StatisticKey = 'EdFiExceptionWarnings' THEN StatisticValue END) as EdFiExceptionWarnings,
		MIN(CASE WHEN StatisticKey = 'EdFiExceptionEntityNotFounds' THEN StatisticValue END) as EdFiExceptionEntityNotFounds,
		MIN(CASE WHEN StatisticKey = 'EdFiExceptionMaxErrorRate' THEN StatisticValue END) as EdFiExceptionMaxErrorRate,
		MIN(CASE WHEN StatisticKey = 'FailedQAScripts' THEN StatisticValue END) as FailedQAScripts,
		MIN(CASE WHEN StatisticKey = 'FailedSanityCheck' THEN StatisticValue END) as FailedSanityCheck
	INTO #TeamCityStatisticHistoryPivot
	FROM EdFi_LoadReport.util.TeamCityStatisticHistory a WITH (NOLOCK)
	WHERE EXISTS (
		SELECT 1
		FROM #BuildSummary b WITH (NOLOCK)
		WHERE a.TeamCityBuildId = b.TeamCityBuildId
	)
	GROUP BY TeamCityBuildId


	UPDATE a
	SET
		ServerName					= b.ServerName,
		DistrictName				= b.DistrictName,
		EdFiExceptionErrors			= b.EdFiExceptionErrors,
		EdFiExceptionWarnings		= b.EdFiExceptionWarnings,
		EdFiExceptionEntityNotFounds= b.EdFiExceptionEntityNotFounds,
		EdFiExceptionMaxErrorRate	= b.EdFiExceptionMaxErrorRate,
		FailedQAScripts				= b.FailedQAScripts,
		FailedSanityCheck			= b.FailedSanityCheck
	FROM
		#BuildSummary a WITH (NOLOCK) INNER JOIN
		#TeamCityStatisticHistoryPivot b
			ON a.TeamCityBuildId = b.TeamCityBuildId
	
	UPDATE #BuildSummary
	SET 
		DistrictName = (SELECT ISNULL(temp.list.value('.', 'varchar(MAX)'), '') FROM (SELECT Token FROM util.Split(DistrictName, '_') WHERE Id IN (2, 5) ORDER BY Id FOR XML PATH(''), TYPE ) as temp(list)),
		ElapsedStatus = CASE 
			WHEN ElapsedMinutes BETWEEN 0 AND (60*24 - 1)		THEN 'Last 24 hours'
			WHEN ElapsedMinutes BETWEEN 60*24 AND (60*48 - 1)	THEN 'One day ago'
			ELSE CAST(ElapsedMinutes/(60*24) as nvarchar) + ' days ago'
		END

	-----------------------------------
	-- Email
	-----------------------------------
	
	DECLARE @HTML nvarchar(MAX), @Subject nvarchar(1000), @DosCommand nvarchar(4000)
	
	SELECT DISTINCT @Subject = 'Build Summary: ' + @ServerNameInSubjectLine + ' (' + CAST(MONTH(GETDATE()) as varchar) + '/' + CAST(DAY(GETDATE()) as varchar) + ')'
	
	SELECT @HTML = '<table border=''1'' cellpadding=''3'' cellspacing=''0''><tr><th rowspan=''2'' valign=''top''>District Name</th><th rowspan=''2'' valign=''top''>Build#</th><th rowspan=''2'' valign=''top''>Status</th><th rowspan=''2'' valign=''top''>Elapsed</th><th rowspan=''2'' valign=''top''>Build Date</th><th rowspan=''2'' valign=''top''>Finished</th><th colspan=''3''>EdFi Exception</th><th colspan=''2''>Failed Scripts</th></tr>'
	SELECT @HTML = @HTML + '<tr><th># Error</th><th># Warning</th><th># Entity</th><th>QA Script</th><th>Sanity Check</th></tr>'
	SELECT @HTML = @HTML + '<tr>' +
		'<td>' + DistrictName + '</td>' +
		'<td><a href=''http://' + ServerName + ':8080/viewLog.html?tab=buildLog&buildTypeId=' + TeamCityBuildTypeId + '&buildId=' + CAST(TeamCityBuildId as varchar) + '''>' + TeamCityBuildNumber + '</a></td>' +
		'<td>' + CASE WHEN BuildStatus LIKE '%Failure%' THEN '<font color=''red''>' + BuildStatus + '</font>' ELSE BuildStatus END + '</td>' +
		'<td>' + ElapsedStatus + '</td>' +
		'<td>' + CAST(CAST(BuildFinishTime as date) as nvarchar) + '</td>' +
		'<td align=''right''>' + RIGHT(CONVERT(nvarchar(25), BuildFinishTime, 100), 7) + '</td>' +
		'<td align=''right''>' + REPLACE(CONVERT(nvarchar, CAST(ISNULL(EdFiExceptionErrors, 0) as money), 1), '.00', '') + '</td>' +
		'<td align=''right''>' + REPLACE(CONVERT(nvarchar, CAST(ISNULL(EdFiExceptionWarnings, 0) as money), 1), '.00', '') + '</td>' +
		'<td align=''right''>' + REPLACE(CONVERT(nvarchar, CAST(ISNULL(EdFiExceptionEntityNotFounds, 0) as money), 1), '.00', '') + '</td>' +
		'<td align=''right''>' + REPLACE(CONVERT(nvarchar, CAST(ISNULL(FailedQAScripts, 0) as money), 1), '.00', '') + '</td>' +
		'<td align=''right''>' + REPLACE(CONVERT(nvarchar, CAST(ISNULL(FailedSanityCheck, 0) as money), 1), '.00', '') + '</td>' +
		'</tr>'
	FROM #BuildSummary
	ORDER BY DistrictName
	SELECT @HTML = @HTML + '</table><br><br>'	
	SELECT @HTML = @HTML + 'This report includes the latest build status for districts that have been built in the last 7 days.'	
	
	EXEC msdb.dbo.sp_send_dbmail 
		@profile_name = 'DLP Notification',
		@body_format = 'HTML',
		@recipients = @RecipientEmailAddress,
		@body = @HTML,
		@subject = @Subject


	SET NOCOUNT OFF	
END
--
--
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[util].[DashboardAssociation]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [util].[DashboardAssociation] AS' 
END
GO



ALTER PROCEDURE [util].[DashboardAssociation] (
	@AssociationFlag bit = 1
) AS
BEGIN

	SET NOCOUNT ON

	-----------------------------------
	-- Prep steps
	-----------------------------------

	DECLARE @DashboardDatabaseName nvarchar(255)
	DECLARE @DashboardSchemaTableId int, @TableSchema nvarchar(255), @TableName nvarchar(255)
	DECLARE @SQLCommand nvarchar(4000)

	TRUNCATE TABLE util.DashboardSchemaTable

	SELECT @DashboardDatabaseName = REPLACE(DB_NAME(), '_EdFi_', '_Dashboard_')

	-----------------------------------
	-- Create synonym for tables in the DashboardIntegration database
	-----------------------------------

	IF EXISTS (SELECT 1 FROM sys.databases WHERE name = @DashboardDatabaseName) AND @AssociationFlag = 1
	BEGIN
		IF SCHEMA_ID('domain') IS NULL
			EXEC ('CREATE SCHEMA domain authorization dbo')

		IF SCHEMA_ID('metric') IS NULL
			EXEC ('CREATE SCHEMA metric authorization dbo')

		SELECT @SQLCommand = 'INSERT INTO util.DashboardSchemaTable(TableSchema, TableName) ' +
		'SELECT TABLE_SCHEMA, TABLE_NAME ' +
		'FROM ' + @DashboardDatabaseName + '.INFORMATION_SCHEMA.TABLES ' +
		'WHERE TABLE_SCHEMA IN (''domain'', ''metric'') ' +
		'AND TABLE_TYPE = ''BASE TABLE'''
		EXEC sp_executesql @SQLCommand

		WHILE EXISTS (SELECT 1 FROM util.DashboardSchemaTable)
		BEGIN
			SELECT 
				@DashboardSchemaTableId = DashboardSchemaTableId,
				@TableSchema = TableSchema,
				@TableName = TableName
			FROM util.DashboardSchemaTable
			ORDER BY DashboardSchemaTableId
			
			SELECT @SQLCommand = 'IF OBJECT_ID(''[' + @TableSchema + '].[' + @TableName + ']'', ''SN'') IS NOT NULL DROP SYNONYM [' + @TableSchema + '].[' + @TableName + ']'
			EXEC sp_executesql @SQLCommand

			SELECT @SQLCommand = 'CREATE SYNONYM [' + @TableSchema + '].[' + @TableName + '] FOR ' + @DashboardDatabaseName + '.[' + @TableSchema + '].[' + @TableName + ']'
			IF @DashboardDatabaseName <> DB_NAME()
				EXEC sp_executesql @SQLCommand
			
			DELETE util.DashboardSchemaTable
			WHERE DashboardSchemaTableId = @DashboardSchemaTableId
		END
	END


	-----------------------------------
	-- Drop synonym for tables in the DashboardIntegration database
	-----------------------------------

	ELSE BEGIN

		INSERT INTO util.DashboardSchemaTable(TableSchema, TableName)
		SELECT SCHEMA_NAME(schema_id), [name]
		FROM sys.all_objects
		WHERE type = 'sn'
		AND SCHEMA_NAME(schema_id) IN ('domain', 'metric')		

		WHILE EXISTS (SELECT 1 FROM util.DashboardSchemaTable)
		BEGIN
			SELECT 
				@DashboardSchemaTableId = DashboardSchemaTableId,
				@TableSchema = TableSchema,
				@TableName = TableName
			FROM util.DashboardSchemaTable
			ORDER BY DashboardSchemaTableId
			
			SELECT @SQLCommand = 'IF OBJECT_ID(''[' + @TableSchema + '].[' + @TableName + ']'', ''SN'') IS NOT NULL DROP SYNONYM [' + @TableSchema + '].[' + @TableName + ']'
			EXEC sp_executesql @SQLCommand
			
			DELETE util.DashboardSchemaTable
			WHERE DashboardSchemaTableId = @DashboardSchemaTableId
		END

		IF SCHEMA_ID('domain') IS NOT NULL
		BEGIN TRY
			EXEC ('DROP SCHEMA domain')
		END TRY
		BEGIN CATCH
			-- Ignore error
		END CATCH

		IF SCHEMA_ID('metric') IS NOT NULL
		BEGIN TRY
			EXEC ('DROP SCHEMA metric')
		END TRY
		BEGIN CATCH
			-- Ignore error
		END CATCH
	END
	
	SET NOCOUNT OFF

END
--
--
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[util].[EdFiExceptionEnhancement]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [util].[EdFiExceptionEnhancement] AS' 
END
GO



ALTER PROCEDURE [util].[EdFiExceptionEnhancement] (
	@TimeoutInSeconds int = 2147483647
) AS
BEGIN

	SET NOCOUNT ON

	DECLARE @SQL nvarchar(MAX), @ErrorMessage nvarchar(MAX)
	DECLARE @EdFiExceptionId int, @TableName nvarchar(255), @ComponentName nvarchar(255), @ColumnNameList nvarchar(4000), @ColumnValueList nvarchar(4000)
	DECLARE @LookupCondition nvarchar(4000)
	DECLARE @LookupUnionTag nvarchar(4000), @LookupConditionAND nvarchar(4000), @LookupConditionOR nvarchar(4000)
	DECLARE @StartTime datetime = GETDATE()
	DECLARE @LookupErrorCode int = -1071607778


	-----------------------------------
	-- Handle cases where the Destination component that writes to the EdFiException table is not mapped properly
	-----------------------------------

	-- If ErrorCode is not mapped in the Destination component, we will need to fix the package.
	-- In the meantime, deduce the ErrorCode from ErrorDescription if possible.
	UPDATE edfi.EdFiException
	SET ErrorCode = CASE WHEN ErrorDescription LIKE 'Row yielded no match during lookup%' THEN @LookupErrorCode ELSE 0 END
	WHERE ErrorCode IS NULL
	AND ErrorDescription IS NOT NULL

	UPDATE edfi.EdFiException
	SET ErrorMessage = 'At least one of these column is null: TableName, ColumnNameList, ColumnValueList, and ErrorCode. Please check the Destination component that writes to the edfi.EdFiException table.'
	WHERE ErrorCode IS NULL
	OR TableName + ColumnNameList + ColumnValueList IS NULL


	-----------------------------------
	-- Lookup exceptions
	-----------------------------------
	
	-- Hardcode ErrorMessage for lookup exceptions
	UPDATE edfi.EdFiException
	SET ErrorMessage = 'No match during lookup'
	WHERE ErrorCode = @LookupErrorCode

	-- Replace ExceptionLevel for lookup for Staff and Student
	UPDATE edfi.EdFiException
	SET ExceptionLevel = 'EntityNotFound'
	WHERE TableName IN ('edfi.Staff', 'edfi.Student')
	AND ErrorCode = @LookupErrorCode


	-----------------------------------
	-- Run insert statements to capture error messages for non Lookup exceptions
	-----------------------------------

	DECLARE @EdFiExceptionTable TABLE (
		EdFiExceptionId			int,
		TableName				nvarchar(255),
		ColumnNameList			nvarchar(4000),
		ColumnValueList			nvarchar(4000)
	)

	INSERT INTO @EdFiExceptionTable
	SELECT EdFiExceptionId, TableName, ColumnNameList, ColumnValueList
	FROM edfi.EdFiException
	WHERE ErrorCode <> @LookupErrorCode
	AND ErrorMessage IS NULL

	WHILE EXISTS (SELECT 1 FROM @EdFiExceptionTable) AND DATEDIFF(SECOND, @StartTime, GETDATE()) < @TimeoutInSeconds
	BEGIN
		SELECT TOP 1 
			@EdFiExceptionId = EdFiExceptionId,
			@TableName = TableName,
			@ColumnNameList = ColumnNameList,
			@ColumnValueList = ColumnValueList
		FROM @EdFiExceptionTable

		SELECT @SQL = 'INSERT INTO ' + @TableName + '(' + @ColumnNameList + ') SELECT ' + @ColumnValueList

		-----------------------------------
		-- Run the insert statement in a TRY block to trap the error. Roll back if it goes through.
		-----------------------------------
		BEGIN TRANSACTION
		BEGIN TRY
			SELECT @ErrorMessage = 'No Error'
			EXEC sp_executesql @SQL
		END TRY
		BEGIN CATCH
			SELECT @ErrorMessage = ERROR_MESSAGE()
		END CATCH
		ROLLBACK


		-----------------------------------
		-- Remove row-specific information so that ErrorMessage remains generic
		-----------------------------------
		IF @ErrorMessage LIKE '%The duplicate key value is%'
			SELECT @ErrorMessage = RTRIM(LEFT(@ErrorMessage, CHARINDEX(' The duplicate key value is', @ErrorMessage)))

		IF @ErrorMessage LIKE '%The conversion of the varchar value % overflowed an int column%'
			SELECT @ErrorMessage = 'The conversion of the varchar value overflowed an int column'


		-----------------------------------
		-- final
		-----------------------------------
		UPDATE edfi.EdFiException
		SET ErrorMessage = @ErrorMessage
		WHERE EdFiExceptionId = @EdFiExceptionId
		
		DELETE @EdFiExceptionTable
		WHERE EdFiExceptionId = @EdFiExceptionId
	END
	

	-----------------------------------
	-- Populate LookupCondition from ColumnNameList and ColumnValueList for Lookup exceptions
	-----------------------------------

	UPDATE edfi.EdFiException
	SET LookupCondition = ColumnNameList + CASE ColumnValueList WHEN 'NULL' THEN ' IS ' ELSE '=' END  + ColumnValueList
	WHERE ErrorCode = @LookupErrorCode
	AND ColumnNameList NOT LIKE '%,%'

	DECLARE @LookupExceptionTable TABLE (
		EdFiExceptionId			int,
		ComponentName			nvarchar(255),
		ColumnNameList			nvarchar(4000),
		ColumnValueList			nvarchar(4000)
	)

	INSERT INTO @LookupExceptionTable
	SELECT EdFiExceptionId, ComponentName, ColumnNameList, ColumnValueList
	FROM edfi.EdFiException
	WHERE ErrorCode = @LookupErrorCode
	AND LookupCondition IS NULL

	DECLARE @LookupConditionClause TABLE(ColumnName nvarchar(255), ConditionClause nvarchar(4000))

	WHILE EXISTS (SELECT 1 FROM @LookupExceptionTable) AND DATEDIFF(SECOND, @StartTime, GETDATE()) < @TimeoutInSeconds
	BEGIN
		SELECT TOP 1 
			@EdFiExceptionId = EdFiExceptionId,
			@ComponentName = ComponentName,
			@ColumnNameList = ColumnNameList,
			@ColumnValueList = ColumnValueList,
			@LookupCondition = '',
			@LookupUnionTag = '',
			@LookupConditionAND = '',
			@LookupConditionOR = ''
		FROM @LookupExceptionTable
		
		DELETE @LookupConditionClause
		INSERT INTO @LookupConditionClause(ColumnName, ConditionClause)
		SELECT a.ColumnName, a.ColumnName + CASE b.ColumnValue WHEN 'NULL' THEN ' IS ' ELSE '=' END + b.ColumnValue as ConditionClause
		FROM 
			(SELECT Id, Token as ColumnName FROM util.Split((SELECT ColumnNameList FROM edfi.EdFiException WHERE EdFiExceptionId = @EdFiExceptionId), ',')) a INNER JOIN
			(SELECT Id, Token as ColumnValue FROM util.Split((SELECT ColumnValueList FROM edfi.EdFiException WHERE EdFiExceptionId = @EdFiExceptionId), ',')) b
				ON a.Id = b.Id

		IF @ComponentName NOT LIKE '%@LookupUnion%'
		BEGIN
			SELECT @LookupCondition = @LookupCondition + ConditionClause + ' AND '
			FROM @LookupConditionClause

			SELECT @LookupCondition = LEFT(@LookupCondition, NULLIF(LEN(@LookupCondition), 0) - 4)		-- remove trailing AND
		END
		ELSE BEGIN
			SELECT @LookupUnionTag = (SELECT TOP 1 Token FROM util.Split(@ComponentName, ' ') WHERE Token LIKE '%@LookupUnion%')			

			SELECT 
				@LookupConditionOR  = @LookupConditionOR  + CASE WHEN b.ColumnName IS NULL THEN + a.ConditionClause + ' OR ' ELSE '' END,
				@LookupConditionAND = @LookupConditionAND + CASE WHEN b.ColumnName IS NOT NULL THEN + a.ConditionClause + ' AND ' ELSE '' END
			FROM 
				@LookupConditionClause a LEFT OUTER JOIN
				(SELECT Token as ColumnName FROM util.Split(@LookupUnionTag, '-') WHERE Token <> '@LookupUnion') b
					ON a.ColumnName = b.ColumnName

			SELECT @LookupConditionOR  = LEFT(@LookupConditionOR,  NULLIF(LEN(@LookupConditionOR), 0)  - 3)		-- remove trailing OR
			SELECT @LookupConditionAND = LEFT(@LookupConditionAND, NULLIF(LEN(@LookupConditionAND), 0) - 4)		-- remove trailing AND
			SELECT @LookupCondition = ISNULL(@LookupConditionAND + ' AND ', '') + ISNULL(NULLIF('(' + @LookupConditionOR + ')', '()'), '')

		END

		UPDATE edfi.EdFiException
		SET LookupCondition = ISNULL(@LookupCondition, '')
		WHERE EdFiExceptionId = @EdFiExceptionId
		
		DELETE @LookupExceptionTable
		WHERE EdFiExceptionId = @EdFiExceptionId
	END
	
	UPDATE edfi.EdFiException
	SET LookupCondition = ''
	WHERE LookupCondition IS NULL
	
	SET NOCOUNT OFF
END
--
--
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[util].[EdFiPopulationAnalysis]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [util].[EdFiPopulationAnalysis] AS' 
END
GO



ALTER PROCEDURE [util].[EdFiPopulationAnalysis] (
	@IntegrationTempKeyword nvarchar(255) = 'Temp',
	@StagingKeyword nvarchar(255) = ''
)
AS
BEGIN

	SET NOCOUNT ON

	-----------------------------------
	-- Prep steps
	-----------------------------------

	DECLARE @EdFiPopulationId int
	DECLARE @SQLCommand nvarchar(4000), @RecordCount int, @MostRecentDate date
	DECLARE @PriorSQLCommand nvarchar(4000), @PriorRecordCount int, @PriorMostRecentDate date
	DECLARE @PriorDatabaseName nvarchar(255)
	DECLARE @EdFiPopulation TABLE (EdFiPopulationId int primary key, TableSchema nvarchar(255), TableName nvarchar(255))
	
	SELECT @PriorDatabaseName = REPLACE(DB_NAME(), @IntegrationTempKeyword, @StagingKeyword)
	IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = @PriorDatabaseName)
		SELECT @PriorDatabaseName = DB_NAME()
	
	TRUNCATE TABLE util.EdFiPopulation
	
	IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'StudentSectionAttendanceEvent')
		SELECT 
			@SQLCommand		 = 'SELECT @MostRecentDate = MAX(EventDate) FROM edfi.StudentSectionAttendanceEvent WITH (NOLOCK)',
			@PriorSQLCommand = 'SELECT @PriorMostRecentDate = MAX(EventDate) FROM [' + @PriorDatabaseName + '].edfi.StudentSectionAttendanceEvent WITH (NOLOCK)'
	ELSE
		SELECT 
			@SQLCommand		 = 'SELECT @MostRecentDate = MAX(EventDate) FROM edfi.AttendanceEvent WITH (NOLOCK)',
			@PriorSQLCommand = 'SELECT @PriorMostRecentDate = MAX(EventDate) FROM [' + @PriorDatabaseName + '].edfi.AttendanceEvent WITH (NOLOCK)'
	
	EXEC sp_executesql @SQLCommand, N'@MostRecentDate date output',  @MostRecentDate output
	EXEC sp_executesql @PriorSQLCommand, N'@PriorMostRecentDate date output', @PriorMostRecentDate output

	INSERT INTO util.EdFiPopulation(TableSchema, TableName, MostRecentDate, PriorMostRecentDate, DatabaseName, PriorDatabaseName)
	SELECT TABLE_SCHEMA, TABLE_NAME, @MostRecentDate, @PriorMostRecentDate, DB_NAME(), @PriorDatabaseName
	FROM INFORMATION_SCHEMA.TABLES
	WHERE TABLE_NAME NOT LIKE '%Type'
	AND TABLE_NAME NOT IN ('EdFiException', 'sysssislog')
	AND TABLE_TYPE = 'BASE TABLE'
	AND TABLE_SCHEMA = 'edfi'
	ORDER BY TABLE_NAME

	INSERT INTO @EdFiPopulation
	SELECT EdFiPopulationId, TableSchema, TableName 
	FROM util.EdFiPopulation


	-----------------------------------
	-- Loop through each table and select count(1)
	-----------------------------------

	WHILE EXISTS (SELECT 1 FROM @EdFiPopulation)
	BEGIN
		SELECT TOP 1 
			@EdFiPopulationId = EdFiPopulationId,
			@SQLCommand = 'SELECT @RecordCount = COUNT(1) FROM [' + TableSchema + '].[' + TableName + '] WITH (NOLOCK)',
			@PriorSQLCommand	= 'SELECT @PriorRecordCount = COUNT(1) FROM [' + @PriorDatabaseName + '].[' + TableSchema + '].[' + TableName + '] WITH (NOLOCK)',
			@RecordCount = 0,
			@PriorRecordCount = 0
		FROM @EdFiPopulation
		ORDER BY EdFiPopulationId

		BEGIN TRY
		EXEC sp_executesql @SQLCommand, N'@RecordCount int output',  @RecordCount output
		EXEC sp_executesql @PriorSQLCommand, N'@PriorRecordCount int output',  @PriorRecordCount output
		END TRY
		BEGIN CATCH
			-- Ignore error
		END CATCH
		
		UPDATE util.EdFiPopulation
		SET 
			RecordCount = @RecordCount,
			PriorRecordCount = @PriorRecordCount
		WHERE EdFiPopulationId = @EdFiPopulationId
		
		DELETE @EdFiPopulation
		WHERE EdFiPopulationId = @EdFiPopulationId
	END


	-----------------------------------
	-- Add summary to Comment
	-----------------------------------

	UPDATE util.EdFiPopulation
	SET PercentChanged = CAST(ISNULL((RecordCount - PriorRecordCount)*100.00/NULLIF(PriorRecordCount, 0), 0) as decimal(10,2))

	UPDATE util.EdFiPopulation
	SET Comment = 
		CASE
			WHEN PriorRecordCount = 0 AND RecordCount = 0 THEN '8 - No data'
			WHEN PriorRecordCount = 0 AND RecordCount > 0 THEN '4 - No prior data'
			WHEN PriorRecordCount > 0 AND RecordCount = 0 THEN '3 - No current data'
			WHEN RecordCount = PriorRecordCount AND RecordCount <> 0 THEN '7 - No change'		
			WHEN PercentChanged < -10 THEN '1 - Decreased by more than 10%'	
			WHEN PercentChanged BETWEEN -10 AND 0 THEN '5 - Decreased'		
			WHEN PercentChanged BETWEEN 0 AND 10 THEN '6 - Increased'
			WHEN PercentChanged > 10 THEN '2 - Increased by more than 10%'
			ELSE '0 - Unknown'
		END

	SET NOCOUNT OFF

END
--
--
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[util].[EdFiReportPersistence]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [util].[EdFiReportPersistence] AS' 
END
GO



ALTER PROCEDURE [util].[EdFiReportPersistence] (
	@TeamCityBuildId int
)
AS
BEGIN
	SET NOCOUNT ON
	
	DECLARE @ServerName nvarchar(255), @DatabaseName nvarchar(255)
	SELECT @ServerName = @@SERVERNAME, @DatabaseName = DB_NAME()

	IF EXISTS (SELECT 1 FROM sys.databases WHERE name = 'EdFi_LoadReport')
	BEGIN
		-----------------------------------
		-- Persist the data
		-----------------------------------

		IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'EdFiException' AND TABLE_SCHEMA = 'edfi')
			INSERT INTO EdFi_LoadReport.util.EdFiExceptionHistory
			SELECT @ServerName, @DatabaseName, @TeamCityBuildId, EdFiExceptionId, TableName, ColumnNameList, ColumnValueList, ErrorMessage, IdentifierCondition, LookupCondition, ExceptionLevel, StartTime, PackageName, TaskName, ComponentName, ErrorCode, ErrorDescription, ErrorColumn, ErrorColumnName
			FROM edfi.EdFiException WITH (NOLOCK)
		
		IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'EdFiPopulation' AND TABLE_SCHEMA = 'util')
			INSERT INTO EdFi_LoadReport.util.EdFiPopulationHistory
			SELECT @ServerName, @DatabaseName, @TeamCityBuildId, EdFiPopulationId, TableSchema, TableName, RecordCount, PriorRecordCount, PercentChanged, MostRecentDate, PriorMostRecentDate, Comment
			FROM util.EdFiPopulation WITH (NOLOCK)
		
		IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'QAResults' AND TABLE_SCHEMA = 'util')
			INSERT INTO EdFi_LoadReport.util.QAResultsHistory
			SELECT @ServerName, @DatabaseName, @TeamCityBuildId, QAResultsId, ScriptFileName, ScriptResult, NumberOfDiscrepancies, Comment, ResultFileName, CreateDateTime
			FROM util.QAResults WITH (NOLOCK)

		IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'EdFiSanity' AND TABLE_SCHEMA = 'util')
			INSERT INTO EdFi_LoadReport.util.EdFiSanityHistory
			SELECT @ServerName, @DatabaseName, @TeamCityBuildId, EdFiSanityId, BeginLine, EndLine, InterChangeName, ScriptResult, ExpectedResult, ActualResult, Description, Script, ParserCompletionFlag, ExecCompletionFlag
			FROM util.EdFiSanity WITH (NOLOCK)

		IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TeamCityStatistic' AND TABLE_SCHEMA = 'util')
			INSERT INTO EdFi_LoadReport.util.TeamCityStatisticHistory
			SELECT @ServerName, @DatabaseName, @TeamCityBuildId, TeamCityStatisticId, StatisticKey, StatisticValue
			FROM util.TeamCityStatistic WITH (NOLOCK)
	END

	SET NOCOUNT OFF

END
--
--
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[util].[EdFiSanityCheck]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [util].[EdFiSanityCheck] AS' 
END
GO



ALTER PROCEDURE [util].[EdFiSanityCheck] (
	@EdFiSanityScript nvarchar(max) 
) AS
BEGIN

	SET NOCOUNT ON

	-----------------------------------
	-- Prep steps
	-----------------------------------

	EXEC util.DashboardAssociation

	DECLARE @SQLCommand nvarchar(4000)
	DECLARE @FileContent TABLE (FileContentId int identity(1, 1) primary key, Content nvarchar(MAX))
	DECLARE @EdFiSanityId int, @EndLine int, @InterChangeName nvarchar(255), @ExpectedResult nvarchar(255), @Description nvarchar(MAX), @Script nvarchar(MAX)
	DECLARE @Operator char(2), @ExpectedNumber float
	DECLARE @ActualResult TABLE (ActualResult nvarchar(MAX))

	TRUNCATE TABLE util.EdFiSanity
	
	-----------------------------------
	-- Import the script file to a table
	-----------------------------------

	INSERT INTO @FileContent(Content)
	SELECT REPLACE(Token, CHAR(13), '')
	FROM util.Split(@EdFiSanityScript, CHAR(10))
	ORDER BY Id	
				
	IF (SELECT COUNT(1) FROM @FileContent WHERE LEN(LTRIM(RTRIM(Content))) > 0) = 0
	BEGIN
		INSERT INTO util.EdFiSanity
		SELECT 0, 0, '', 'Failed', 'Make sure that the EdFiSanity.txt file exists', '', '', '', '', NULL, 1, 1
	END


	INSERT INTO util.EdFiSanity(BeginLine)
	SELECT FileContentId
	FROM @FileContent
	WHERE Content LIKE '--%Interchange:%'

	UPDATE a
	SET EndLine = ISNULL(b.BeginLine - 1, (SELECT MAX(FileContentId) FROM @FileContent))
	FROM 
		util.EdFiSanity a LEFT OUTER JOIN
		util.EdFiSanity b
			ON a.EdFiSanityId = b.EdFiSanityId - 1

	
	-----------------------------------
	-- Parse the script file
	-----------------------------------

	WHILE EXISTS (SELECT 1 FROM util.EdFiSanity WHERE ParserCompletionFlag = 0)
	BEGIN
		SELECT 
			@EdFiSanityId = EdFiSanityId,
			@EndLine = -1,
			@InterChangeName = '',
			@ExpectedResult = '',
			@Description = '',
			@Script = '',
			@Operator = '',
			@ExpectedNumber = NULL
		FROM util.EdFiSanity 
		WHERE ParserCompletionFlag = 0
		ORDER BY EdFiSanityId
		
		SELECT 
			@InterChangeName	= MAX(LTRIM(RTRIM(REPLACE(CASE WHEN b.Content LIKE '-- Interchange:%' THEN b.Content END, '-- Interchange:', '')))),
			@ExpectedResult		= MAX(LTRIM(RTRIM(REPLACE(CASE WHEN b.Content LIKE '-- ExpectedResult:%' THEN b.Content END, '-- ExpectedResult:', '')))),
			@Description		= MAX(LTRIM(RTRIM(REPLACE(CASE WHEN b.Content LIKE '-- Description:%' THEN b.Content END, '-- Description:', ''))))
		FROM 
			util.EdFiSanity a INNER JOIN
			@FileContent b
				ON b.FileContentId BETWEEN a.BeginLine AND a.EndLine
		WHERE a.EdFiSanityId = @EdFiSanityId
		GROUP BY a.EdFiSanityId

		SELECT @Script = @Script + ' ' + ISNULL(b.Content, '')
		FROM 
			util.EdFiSanity a INNER JOIN
			@FileContent b
				ON b.FileContentId BETWEEN a.BeginLine AND a.EndLine
		WHERE b.Content NOT LIKE '%--%'
		AND a.EdFiSanityId = @EdFiSanityId

		-- @ExpectedNumber will remain NULL if the expected result is not number
		BEGIN TRY
			IF LEFT(LTRIM(@ExpectedResult), 1) IN ('<', '>')
				SELECT @ExpectedNumber = CAST(REPLACE(REPLACE(REPLACE(@ExpectedResult, '<', ''), '>', ''), '=', '') as float)
		END TRY
		BEGIN CATCH
		END CATCH

		UPDATE util.EdFiSanity
		SET	
			ParserCompletionFlag = 1,
			InterChangeName = @InterChangeName,
			ExpectedResult = @ExpectedResult,
			[Description] = @Description,
			Script = @Script,
			ExpectedNumber = @ExpectedNumber,
			Operator = REPLACE(REPLACE(@ExpectedResult, CAST(@ExpectedNumber as varchar), ''), ' ', '')
		WHERE EdFiSanityId = @EdFiSanityId

	END



	-----------------------------------
	-- Loop through the EdFiSanity and execute the query
	-----------------------------------

	WHILE EXISTS (SELECT 1 FROM util.EdFiSanity WHERE ExecCompletionFlag = 0)
	BEGIN
		SELECT 
			@EdFiSanityId = EdFiSanityId,
			@Script = Script
		FROM util.EdFiSanity 
		WHERE ExecCompletionFlag = 0
		ORDER BY EdFiSanityId
		
		DELETE @ActualResult

		BEGIN TRY
			INSERT INTO @ActualResult(ActualResult)
			EXEC sp_executesql @Script
		END TRY
		BEGIN CATCH
			INSERT INTO @ActualResult(ActualResult)
			SELECT 'Error: ' + ERROR_MESSAGE()
		END CATCH

		UPDATE util.EdFiSanity
		SET 
			ExecCompletionFlag = 1,
			ActualResult = (SELECT MAX(ActualResult) FROM @ActualResult)
		WHERE EdFiSanityId = @EdFiSanityId

	END



	-----------------------------------
	-- Evaluation (Passed/Failed)
	-----------------------------------

	-- Evaluation for numeric results
	UPDATE util.EdFiSanity
	SET ScriptResult =
		CASE Operator
			WHEN '>'  THEN CASE WHEN CAST(ActualResult as float) >  ExpectedNumber THEN 'Passed' ELSE 'Failed' END
			WHEN '<'  THEN CASE WHEN CAST(ActualResult as float) <  ExpectedNumber THEN 'Passed' ELSE 'Failed' END
			WHEN '>=' THEN CASE WHEN CAST(ActualResult as float) >= ExpectedNumber THEN 'Passed' ELSE 'Failed' END
			WHEN '<=' THEN CASE WHEN CAST(ActualResult as float) <= ExpectedNumber THEN 'Passed' ELSE 'Failed' END
		END
	WHERE ExpectedNumber IS NOT NULL

	-- Evaluation for empty results	
	UPDATE util.EdFiSanity
	SET ScriptResult = CASE WHEN ActualResult = '' THEN 'Passed' ELSE 'Failed' END
	FROM util.EdFiSanity
	WHERE ExpectedResult IN ('<empty>', '')

	-- This evaluates everything else
	UPDATE util.EdFiSanity
	SET ScriptResult = CASE WHEN ActualResult = ExpectedResult THEN 'Passed' ELSE 'Failed' END
	FROM util.EdFiSanity
	WHERE ScriptResult IS NULL


	-- Remove synonym to objects in the Dashboard database
	EXEC util.DashboardAssociation 0		

	SET NOCOUNT OFF

END
--
--
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[util].[TeamCityIntegration]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [util].[TeamCityIntegration] AS' 
END
GO



-- OutputOption: EdFiException, EdFiPopulation, EdFiSanityCheck, TeamCityStatistic
ALTER PROCEDURE [util].[TeamCityIntegration] (
	@OutputOption nvarchar(255)
)
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @OutputTable TABLE (
		OutputId		int identity(1,1) primary key,
		OutputOption	nvarchar(255) not null,
		OutputIndex		nvarchar(3) default '', 
		OutputText		nvarchar(MAX)
	)

	-----------------------------------
	-- Export the main report and top 100 records for each EdFiException group
	-----------------------------------

	IF @OutputOption = 'EdFiException'
	BEGIN
		DELETE @OutputTable

		IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'EdFiException' AND TABLE_SCHEMA = 'edfi')
		BEGIN
			DECLARE @EdFiExceptionTable TABLE (
				EdFiExceptionTableId	int identity(1,1) primary key,
				ExceptionLevel			nvarchar(20), 
				ErrorMessage			nvarchar(MAX), 
				TableName				nvarchar(255), 
				PackageName				nvarchar(255),
				RecordCount				int,
				CompletionFlag			bit
			)
			
			INSERT INTO @EdFiExceptionTable
			SELECT ExceptionLevel, ErrorMessage, TableName, PackageName, COUNT(1) as RecordCount, 0 as CompletionFlag
			FROM edfi.EdFiException WITH (NOLOCK)
			GROUP BY ExceptionLevel, ErrorMessage, TableName, PackageName
			ORDER BY MIN(StartTime)

			INSERT INTO @OutputTable(OutputOption, OutputText) SELECT @OutputOption, '<html><body>'
			INSERT INTO @OutputTable(OutputOption, OutputText) SELECT @OutputOption, '<h3>EdFiException Summary</h3><table border="1" cellpadding="3" cellspacing="0"><tr><th>Package Name</th><th>Table Name</th><th>Exception Level</th><th>Error Message</th><th>Record Count</th></tr>'

			INSERT INTO @OutputTable(OutputOption, OutputText)
			SELECT @OutputOption, '<tr>' +
				'<td>' + ISNULL(PackageName, 'Unknown') + '</td>' +
				'<td>' + ISNULL(TableName, 'Unknown') + '</td>' +
				'<td>' + ISNULL(ExceptionLevel, 'Unknown') + '</td>' +
				'<td><a href = "EdFiException' + CAST(EdFiExceptionTableId as nvarchar) + '.html">' + ISNULL(ErrorMessage, 'Unknown') + '</a></td>' +
				'<td align="right">' + REPLACE(CONVERT(nvarchar, CAST(RecordCount as money), 1), '.00', '') + '</td>' +
				'</tr>'
			FROM @EdFiExceptionTable
			ORDER BY ExceptionLevel, RecordCount DESC

			INSERT INTO @OutputTable(OutputOption, OutputText) SELECT @OutputOption, '</table><br/><br/><br/>'
			INSERT INTO @OutputTable(OutputOption, OutputText) SELECT @OutputOption, '</body></html>'


			DECLARE @EdFiExceptionTableId int, @ExceptionLevel nvarchar(20), @ErrorMessage nvarchar(MAX), @TableName nvarchar(255), @PackageName nvarchar(255), @RecordCount int
			WHILE EXISTS (SELECT 1 FROM @EdFiExceptionTable WHERE CompletionFlag = 0)
			BEGIN
				SELECT
					@EdFiExceptionTableId = EdFiExceptionTableId,
					@ExceptionLevel = ISNULL(ExceptionLevel, 'Unknown'),
					@ErrorMessage = ISNULL(ErrorMessage, 'Unknown'),
					@TableName = ISNULL(TableName, 'Unknown'),
					@PackageName = ISNULL(PackageName, 'Unknown'),
					@RecordCount = RecordCount
				FROM @EdFiExceptionTable
				WHERE CompletionFlag = 0
				ORDER BY EdFiExceptionTableId
				
				INSERT INTO @OutputTable(OutputOption, OutputIndex, OutputText) SELECT @OutputOption, @EdFiExceptionTableId, '<html><body>'
				INSERT INTO @OutputTable(OutputOption, OutputIndex, OutputText) SELECT @OutputOption, @EdFiExceptionTableId, '<h3>EdFiException Sample' + CASE WHEN @RecordCount > 100 THEN ' (top 100 records)' ELSE '' END + '</h3>'			
				INSERT INTO @OutputTable(OutputOption, OutputIndex, OutputText) SELECT @OutputOption, @EdFiExceptionTableId, '<b>ErrorMessage:</b> ' + @ErrorMessage + '<br>'
				INSERT INTO @OutputTable(OutputOption, OutputIndex, OutputText) SELECT @OutputOption, @EdFiExceptionTableId, '<b>TableName:</b> ' + @TableName + '<br>'
				
				INSERT INTO @OutputTable(OutputOption, OutputIndex, OutputText)
				SELECT TOP 1 @OutputOption, @EdFiExceptionTableId, '<b>ColumnNameList:</b> ' + ISNULL(ColumnNameList, 'Unknown') + '<br><br>'
				FROM edfi.EdFiException WITH (NOLOCK)
				WHERE ExceptionLevel = @ExceptionLevel
				AND ErrorMessage = @ErrorMessage
				AND TableName = @TableName
				AND PackageName = @PackageName

				INSERT INTO @OutputTable(OutputOption, OutputIndex, OutputText) SELECT @OutputOption, @EdFiExceptionTableId, '<table border="1" cellpadding="3" cellspacing="0"><tr><th>ColumnValueList</th><th>IdentifierCondition</th><th>LookupCondition</th></tr>'

				INSERT INTO @OutputTable(OutputOption, OutputIndex, OutputText)
				SELECT TOP 100 @OutputOption, @EdFiExceptionTableId, '<tr>' +
					'<td>' + ISNULL(ColumnValueList, 'Unknown') + '</td>' +
					'<td>' + ISNULL(NULLIF(IdentifierCondition, ''), '&nbsp;') + '</td>' +
					'<td>' + ISNULL(NULLIF(LookupCondition, ''), '&nbsp;') + '</td>' +
					'</tr>'
				FROM edfi.EdFiException WITH (NOLOCK)
				WHERE ExceptionLevel = @ExceptionLevel
				AND ErrorMessage = @ErrorMessage
				AND TableName = @TableName
				AND PackageName = @PackageName

				INSERT INTO @OutputTable(OutputOption, OutputIndex, OutputText) SELECT @OutputOption, @EdFiExceptionTableId, '</table>'
				INSERT INTO @OutputTable(OutputOption, OutputIndex, OutputText) SELECT @OutputOption, @EdFiExceptionTableId, '</body></html>'
			
				UPDATE @EdFiExceptionTable 
				SET CompletionFlag = 1
				WHERE EdFiExceptionTableId = @EdFiExceptionTableId
			END
		END		
	END


	-----------------------------------
	-- Reports for EdFiPopulation
	-----------------------------------

	IF @OutputOption = 'EdFiPopulation'	
	BEGIN
		DELETE @OutputTable

		IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'EdFiPopulation' AND TABLE_SCHEMA = 'util')
		BEGIN
			INSERT INTO @OutputTable(OutputOption, OutputText) SELECT @OutputOption, '<html><body>'
			
			INSERT INTO @OutputTable(OutputOption, OutputText)
			SELECT @OutputOption, '<table border="1" cellpadding="3" cellspacing="0">' + 
				'<tr><th>Environment</th><th>Database Name</th><th>Max Attendance Date</th></tr>' +
				'<tr><td>New Build</td><td>' + ISNULL((SELECT TOP 1 DatabaseName FROM util.EdFiPopulation), 'Unknown') + '</td><td>' + ISNULL(CAST((SELECT TOP 1 MostRecentDate FROM util.EdFiPopulation WITH (NOLOCK)) as varchar), 'Unknown') + '</td></tr>' +
				'<tr><td>Referenced Build</td><td>' + ISNULL((SELECT TOP 1 PriorDatabaseName FROM util.EdFiPopulation), 'Unknown') + '</td><td>' + ISNULL(CAST((SELECT TOP 1 PriorMostRecentDate FROM util.EdFiPopulation WITH (NOLOCK)) as varchar), 'Unknown') + '</td></tr>' +
				'</table><br>'

			INSERT INTO @OutputTable(OutputOption, OutputText) SELECT @OutputOption, '<h3>EdFi Population Summary</h3><table border="1" cellpadding="3" cellspacing="0"><tr><th>Schema</th><th>Table Name</th><th>Record Count</th><th>Prior Count</th><th>Percent Changed</th><th>Comment</th></tr>'

			INSERT INTO @OutputTable(OutputOption, OutputText)
			SELECT @OutputOption, '<tr>' +
				'<td>' + TableSchema + '</td>' +
				'<td>' + TableName + '</td>' +
				'<td align="right">' + REPLACE(CONVERT(nvarchar, CAST(RecordCount as money), 1), '.00', '') + '</td>' +
				'<td align="right">' + REPLACE(CONVERT(nvarchar, CAST(PriorRecordCount as money), 1), '.00', '') + '</td>' +
				'<td align="right">' + CAST(PercentChanged as varchar) + '%</td>' +
				'<td>' + CASE WHEN LEFT(Comment, 1) IN (1, 2) THEN '<font color="red">' + Comment + '</font>' ELSE Comment END + '</td>' +
				'</tr>'
			FROM util.EdFiPopulation WITH (NOLOCK)
			ORDER BY Comment, TableSchema, TableName
			
			INSERT INTO @OutputTable(OutputOption, OutputText) SELECT @OutputOption, '</table><br/><br/><br/>'
			INSERT INTO @OutputTable(OutputOption, OutputText) SELECT @OutputOption, '</body></html>'
		END
	END


	-----------------------------------
	-- Reports for EdFiSanityCheck
	-----------------------------------

	IF @OutputOption = 'EdFiSanityCheck'
	BEGIN
		DELETE @OutputTable
		
		IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'EdFiSanity' AND TABLE_SCHEMA = 'util')
		BEGIN
			INSERT INTO @OutputTable(OutputOption, OutputText) SELECT @OutputOption, '<html><body>'
			INSERT INTO @OutputTable(OutputOption, OutputText) SELECT @OutputOption, 'Number of scripts: ' + CAST(ISNULL((SELECT COUNT(1) FROM util.EdFiSanity WITH (NOLOCK)), 0) as nvarchar) + '<br><br>'
			INSERT INTO @OutputTable(OutputOption, OutputText) SELECT @OutputOption, '<table border="1" cellpadding="3" cellspacing="0"><tr><th>InterChange</th><th>Description</th><th>Result</th><th>Expected Result</th><th>Actual Result</th><th>Script</th></tr>'

			INSERT INTO @OutputTable(OutputOption, OutputText)
			SELECT @OutputOption, '<tr>' + 
				'<td valign="top">' + InterChangeName + '</td>' +
				'<td valign="top">' + [Description] + '</td>' +
				'<td valign="top">' + CASE ScriptResult WHEN 'Failed' THEN '<font color="red">' + ScriptResult + '</font>' ELSE ScriptResult END + '</td>' +
				'<td valign="top" width="200">' + ISNULL(NULLIF(ExpectedResult, ''), '&nbsp;') + '</td>' + 
				'<td valign="top" width="200">' + ISNULL(NULLIF(ActualResult, ''), '&nbsp;') + '</td>' + 
				'<td valign="top"><font size="-2">' + Script + '</font></td>' +
				'</tr>'
			FROM util.EdFiSanity WITH (NOLOCK)
			ORDER BY InterchangeName, ScriptResult

			INSERT INTO @OutputTable(OutputOption, OutputText) SELECT @OutputOption, '</table><br/><br/><br/>'
			INSERT INTO @OutputTable(OutputOption, OutputText) SELECT @OutputOption, '</body></html>'
		END
	END


	-----------------------------------
	-- Publish stats to TeamCity
	-----------------------------------
	
	IF @OutputOption = 'TeamCityStatistic'
	BEGIN
		DELETE @OutputTable
		
		IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'QAResults' AND TABLE_SCHEMA = 'util')
			INSERT INTO util.TeamCityStatistic(StatisticKey, StatisticValue)
			SELECT 'FailedQAScripts', CAST(COUNT(1) as nvarchar) FROM util.QAResults WITH (NOLOCK) WHERE ScriptResult = 'Failed'
		
		IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'EdFiSanity' AND TABLE_SCHEMA = 'util')
			INSERT INTO util.TeamCityStatistic(StatisticKey, StatisticValue)
			SELECT 'FailedSanityCheck', CAST(COUNT(1) as nvarchar) FROM util.EdFiSanity WITH (NOLOCK) WHERE ScriptResult = 'Failed'

		IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'EdFiException' AND TABLE_SCHEMA = 'edfi')
		BEGIN
			INSERT INTO util.TeamCityStatistic(StatisticKey, StatisticValue)
			SELECT 'EdFiExceptionErrors', CAST(COUNT(1) as nvarchar) FROM edfi.EdFiException WITH (NOLOCK) WHERE ExceptionLevel = 'Error'

			INSERT INTO util.TeamCityStatistic(StatisticKey, StatisticValue)
			SELECT 'EdFiExceptionWarnings', CAST(COUNT(1) as nvarchar) FROM edfi.EdFiException WITH (NOLOCK) WHERE ExceptionLevel = 'Warning'

			INSERT INTO util.TeamCityStatistic(StatisticKey, StatisticValue)
			SELECT 'EdFiExceptionEntityNotFounds', CAST(COUNT(1) as nvarchar) FROM edfi.EdFiException WITH (NOLOCK) WHERE ExceptionLevel = 'EntityNotFound'
		END

		IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'EdFiException' AND TABLE_SCHEMA = 'edfi')
		AND EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'EdFiPopulation' AND TABLE_SCHEMA = 'util')
			INSERT INTO util.TeamCityStatistic(StatisticKey, StatisticValue)
			SELECT 'EdFiExceptionMaxErrorRate', CAST(MAX(CAST(ErrorRate as decimal(10, 2))) as nvarchar)
			FROM (
				SELECT a.TableName, ISNULL(COUNT(1)*100.00/NULLIF(b.RecordCount, 0), 100) as ErrorRate
				FROM 
					edfi.EdFiException a WITH (NOLOCK) LEFT OUTER JOIN
					util.EdFiPopulation b WITH (NOLOCK)
						ON a.TableName = b.TableSchema + '.' + b.TableName
				WHERE a.ErrorCode <> -1071607778
				AND a.ErrorMessage NOT LIKE '%Cannot insert duplicate key in object%'
				GROUP BY a.TableName, b.RecordCount
			) as temp

			
		DELETE @OutputTable
		
		INSERT INTO @OutputTable(OutputOption, OutputText)
		SELECT @OutputOption, '##teamcity[buildStatisticValue key=''' + StatisticKey + ''' value=''' + ISNULL(StatisticValue, 0) + ''']'
		FROM util.TeamCityStatistic
	END



	-----------------------------------
	-- Print out reports 
	-----------------------------------

	IF @OutputOption <> 'TeamCityStatistic'
	BEGIN
		SELECT
			'FileName=' + OutputOption + ISNULL(OutputIndex, '') + '.html',
			    OutputText,
				OutputId
				FROM @OutputTable 
				ORDER BY OutputOption, OutputIndex, OutputId
	END
	ELSE
	BEGIN
		-- (Powershell will simply Out-Host the data to build log)
		SELECT OutputText
		FROM @OutputTable
	END
	
	SET NOCOUNT OFF

END
--
--
GO
