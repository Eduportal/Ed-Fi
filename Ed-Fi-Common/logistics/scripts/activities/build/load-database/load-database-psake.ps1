# *************************************************************************
# ©2013 Ed-Fi Alliance, LLC. All Rights Reserved.
# *************************************************************************
$ErrorActionPreference = "Stop"
properties { 
    if (-not $folders) {
        $scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
        Import-module $scriptDir\..\..\..\modules\path-resolver.psm1
    }
    . "$($folders.modules.invoke('utility\build-utility.ps1'))"
    
	Assert-Variables "specificEntityType", "buildConfigurationName", "environment"
	$normSpecificEntityType	  = $specificEntityType.Replace(" ", "")
    $subTypeNames += @($normSpecificEntityType)
    Import-Module "$($folders.modules.invoke('utility\credential-management.psm1'))" -force
    . "$($folders.activities.invoke('common.ps1'))"
    . "$($folders.activities.invoke(`"build\$environment.vars.ps1`"))"
    . "$($folders.activities.invoke('build\load-database\load-database-credentials.ps1'))"
    Import-Module "$($folders.activities.invoke('common.psm1'))"
    Import-Module "$($folders.modules.invoke('database\database-management.psm1'))"
    Import-Module "$($folders.modules.Invoke('database\dtsx-execution.psm1'))"
    Import-Module "$($folders.modules.Invoke('database\database-utility.psm1'))"
    
    # Include build environment-specific credentials
    if (Test-Path "$($folders.activities.invoke('build\'))$environment.credentials.ps1") {
        . "$($folders.activities.build.invoke(`"$environment.credentials.ps1`"))"
    }
    $infrastructureDbPath     = "Database\"
	
    #Get all the ETL Directories in all the Repos
    $packagePaths             = Select-CumulativeRepositoryResolvedFiles "ETL\" | select -ExpandProperty FullName | Split-Path | select -Unique
	$longitudinalPackagesPath = "$($folders.base.invoke('Etl\EdFi.Etl.Dashboard\'))" # For ExecuteLongitudinalPackages
	$etlUnitExePath           = "$($folders.tools.invoke('EtlUnit\EtlUnit.exe'))"
    #$postLoadPath             = "$($folders.activities.build.invoke\load-database"
	$districtSourceDataPath   = "$sourceDataBasePath\$normSpecificEntityType" # Path for source XML data
    $script:databasesToPublish       = @()

    #Clean and setup QAReporting folder if needed.
    if ($qaReportPath)
    { 
        if(test-path $qaReportPath) {del -force -recurse $qaReportPath}
        md $qaReportPath | Out-Null
    }

	# Initialize credentials
	Initialize-Credentials $folders.activities.invoke('build\')

	# Credentials used in this script
	$dbLoaderCreds                     = Get-NetworkCredential "DB Loader"
    $testDataLoaderCreds               = Get-NetworkCredential "DB Loader (Test Data Server)"
	$dbAdminCreds                      = Get-NetworkCredential "DB Admin"
    $dbAdminSecureCreds                = Get-PSCredential      "DB Admin"
    $loadedDatabaseBackupPSCredentials = Get-PSCredential      "Loaded Db Backup Destination"
    
    . "$($folders.activities.invoke('build\load-database\load-database-vars.ps1'))"
}

task default -depends CreateAndSyncDb, ExecutePackages, ExecuteLongitudinalPackages, ExecutePostloads, BackupAndArchiveTransientDatabaseTypes, ExecuteQAStep, ExecuteQA, DropTransientDatabaseTypes, PublishDatabaseBackup

function Remove-FalseErrorsFromScriptStack {
	#local errors.
	for ($i = 0; $i -lt $error.count; $i++)
	{ 
		if($error[$i].ToString() -eq `
		"Windows PowerShell Workflow is not supported in a Windows PowerShell x86-based console. Open a Windows PowerShell x64-based console, and then try again.") 
		{
			$error.RemoveAt($i)
            #if there are still errors.  we need to call ourself again.
            if ($error.Count -gt 0) { Remove-FalseErrorsFromScriptStack }
			break
		}
	}
}

function Remove-FalseErrorsFromGlobalStack {
	#local errors.
	for ($i = 0; $i -lt $global:error.count; $i++)
	{ 
		if($global:error[$i].ToString() -eq `
		"Windows PowerShell Workflow is not supported in a Windows PowerShell x86-based console. Open a Windows PowerShell x64-based console, and then try again.") 
		{
			$global:error.RemoveAt($i)
            #if there are still errors.  we need to call ourself again.
            if ($global:error.Count -gt 0) { Remove-FalseErrorsFromGlobalStack }
			break
		}
	}
}

Function Get-EtlUnitExecuteItemsCommandLineArgs {
    Assert-Variables "packagePaths", "districtSourceDataPath", "threadCount", "connectionStrings", "packageVariables"
    $connectionParms = "`""
    foreach ($dbName in $connectionStrings.Keys) {
        $connectionParms += "$dbName;$($connectionStrings[$dbName])||"
    }
    $connectionParms = $connectionParms.Trim("||")
    $connectionParms += "`""
    
    if ($packageVariables.Count -gt 0) {
        $variableParms = "-vars `""
        foreach ($varName in $packageVariables.Keys) {
            $variableParms += "$varName;$($packageVariables[$varName])||"
        }
        $variableParms = $variableParms.Trim("||")
        $variableParms += "`""
    }
    
    $orderedPackageSources = Format-ArrayByRepositories $packagePaths
    $packagePathArgs = ""
    foreach ($packageSource in $orderedPackageSources) {
        $packagePathArgs += "$packageSource;"
    }
    $packagePathArgs = $packagePathArgs.Trim(";")
    
    Write-Verbose "Impl ETL base path: $etlPackagesBasePath"
    Write-Verbose "Core ETL base path: $etlCorePackagesBasePath"
    $skipPatternArg = if ($etlUnitSkipPattern) { "-skip `"$etlUnitSkipPattern`"" } else {""}
    $versionArg = if ($edfiStandardVersion) {"-version $edfiStandardVersion"} else {""}
    $dataPathArg = if ($districtSourceDataPath) {"-dataPath=$districtSourceDataPath"} else {""}
    $executionArgs = "ExecuteItems -xplan='$($folders.base.invoke('Etl\ExecutionGraph.xml'))' $skipPatternArg -conn $connectionParms $variableParms -path=$packagePathArgs -threads=$threadCount $dataPathArg $versionArg"
    return $executionArgs
}

function Test-CurrentlyBusinessHours {
	# Get "business day", from 8am-6pm, excluding weekends
	$dayOfWeek = [DateTime]::Today.DayOfWeek
	if ($dayOfWeek -eq "Saturday" -or $dayOfWeek -eq "Sunday") {
		$dayStart = [DateTime]::MinValue
		$dayEnd = [DateTime]::MinValue
	} else {
		$dayStart = [DateTime]::Today + "8:00"
		$dayEnd = [DateTime]::Today + "18:00"
	}

	if ((Get-Date) -gt $dayStart -and (Get-Date) -lt $dayEnd) {
        $true
    } else {
        $false
    }
}

task CreateAndSyncDb {
    foreach ($dbTypeName in $dbTypeNames) {
        #for the load we want one database per type.
        $initializeParms = @{ 
                              dbTypeNames            = @($dbTypeName);
                              specificEntityName     = $specificEntityType;
                              buildConfigurationName = $buildConfigurationName;
                              folders                = $folders;
                            }
        if ($edfiStandardVersion) {$initializeParms += @{edfiStandardVersion = $edfiStandardVersion;}}               
        Invoke-Psake "$($folders.activities.invoke('build\initialize-database\initialize-database-psake.ps1'))" -parameters $initializeParms
        
		#clear the false positives.
		Remove-FalseErrorsFromScriptStack 
		Remove-FalseErrorsFromGlobalStack
		if ($global:error.Count -gt 0) { 
            Write-Host "An error has occurred during Creation/Initialization of database type: $dbTypeName"
            exit $global:error.Count 
        }
    }
}

task RefreshIntegrationData {
    if (Test-Function "Update-IntegrationData") {
        Update-IntegrationData
    }
}

task ExecutePackages -depends RefreshIntegrationData { 
    # Get environment-specific command-line args for EtlUnit
    $commandLineArgs = Get-EtlUnitExecuteItemsCommandLineArgs 
	$commandLine = "$etlUnitExePath $commandLineArgs"

	# Check to see if we're debugging
	<# For possible future inclusion if feature is actually used
    if ($debug_project_item -ne $null) {
		$command_line += " -debug=$debug_project_item"
	}
	#>
    
    #Write-Host "DEBUG: Command line Args: $commandLineArgs" -Fore Blue

    $etlProcess = Start-Process "$etlUnitExePath" -NoNewWindow -PassThru -Wait -WorkingDirectory "$(Resolve-Path $folders.base.invoke('..\'))" -ArgumentList "$commandLineArgs" 
    $script:etlExitCode = $etlProcess.ExitCode

	# Check to see if EtlUnit returned a failure code
	if (($script:etlExitCode) -ne 0) {
		#If we have an error we still need to run the first set of postloads all the way through so
        #that the error table can get populated, even if some of them fail so set the Erraction preference to allow them all to run in that case.
        $ErrorActionPreference = "Continue"
	}
}

# TODO: This task will be removed once the logitudinal packages are updated to execute using the execution metadata
task ExecuteLongitudinalPackages {
    $longitudinalPath = if ($edfiStandardVersion) { "$longitudinalPackagesPath\$edfiStandardVersion"} else { $longitudinalPackagesPath }
    $longitudinalPackageName = "$longitudinalPath\SchoolLongitudinalMetrics.dtsx"

	# Execute the package
	$success = $false
    $success = Invoke-Package "$longitudinalPackageName" $connectionStrings

# Seting etlExitCode to 1 will fail the build after post loads.
	if (-not $success) {
		$script:etlExitCode = 1
		$ErrorActionPreference = "Continue"
		Write-Host "Package execution failed."
	} else {
		Write-Host "Package executed successfully."
	}
}

task ExecutePostloads {
    # Postloads by Db Type
    foreach ($dbTypeName in $dbTypeNames) {
        #Get scripts
        $postloadFiles = @()
        $postloadFiles += Select-EdFiDatabaseFiles "$infrastructureDbPath\PostLoad\$dbTypeName\*\" "*.sql" | where { -not ( $_.Directory -match $specificEntitySubtypeRegex) }
        foreach ($spEnType in $subTypeNames) {
            $postloadFiles += Select-EdFiDatabaseFiles "$infrastructureDbPath\PostLoad\$dbTypeName\*\" "*.sql" | where { ($_.Directory -match $specificEntitySubtypeRegex) -and (-not [string]::IsNullOrWhiteSpace($spEnType)) -and ($_.Directory -match ".+\.$spEnType") }
        }
        foreach ($script in $postloadFiles) {
            #TODO: Look at CSB's in the future. Current scope is for TX that doesn't have the new CSB work.
            $postLoadScript = New-SqlScript -scriptPath $script.FullName $sqlVars
            if ($postLoadScript.permission -eq "Admin") {
                $postLoadScript.connectionString = $adminConnectionStrings[$dbTypeName]
            }
            else {
                $postLoadScript.connectionString = $transientSqlConnectionStrings[$dbTypeName]
            }
            #Set default timeout if we haven't been given one.
            if ($postLoadScript.timeOut -eq $null) {
                #TODO:Set script level var for this default.
                $postLoadScript.timeOut = 30
            }
            #Execute
            Write-Host "Executing postload step $($script.FullName) ..."
            Invoke-SqlScript $postLoadScript
        }
    }
    # Check to see if EtlUnit returned a failure code
	if (($script:etlExitCode) -ne 0) {
        #Set back the error action prefence that was modified earlier so that the subsequent throw will stop execution.
        $ErrorActionPreference = "Stop"
		throw "EtlUnit indicated package execution failed."
	}
}

task BackupAndArchiveTransientDatabaseTypes {
    Write-Host "Before Performing datase backups..."
    $recycle = Test-Function "RecycleSqlServer"
    if ($recycle) {
        RecycleSqlServer
        #Make sure the server can be reached.
        $serverIsUnreachable = $true
        $timeOut = (Get-Date) + (New-TimeSpan 0:00:00 0:01:00)
        while ($serverIsUnreachable -and (Get-Date) -lt $timeOut) {
            try {
                $serverIsUnreachable = $false
                $sqlServer.Databases.Refresh($true)
            }
            catch {
                $serverIsUnreachable = $true
                $global:error.RemoveAt(0)
                Sleep 5
            }
        }
    }
    # Back up the transient databases
    foreach ($dbTypeName in $transientConnectionStrings.Keys) {
        if (-not ($tempDbTypeBackUpExceptions -eq "$dbTypeName")) {
            $dbToBackUp = Get-ProjectSpecificDatabaseName $appTypePrefix $normSpecificEntityType $dbTypeName $buildConfigurationName $schoolyear $legacyVersionNum
            if ($recycle -and (Test-DatabaseExists $sqlServer $dbToBackUp)) {
                $baseTime = Get-date
                $thresholdTime = ($baseTime) + (New-TimeSpan 0:00:00 0:03:00)
                $trySetOnlineTime = ($baseTime) + (New-TimeSpan 0:00:00 0:01:30)
                #This loop is to help make sure the database is online before trying to backup after a SQL serverRecycle
                while(($sqlServer.Databases.Item("$dbToBackUp")).Status -notMatch "Normal" -and (Get-Date) -lt $thresholdTime) {
                    Write-Host "Database Status: $(($sqlServer.Databases.Item("$dbToBackUp")).Status)"
                    #If we've waited to the trySetOnlineTime then start trying to set the database online.
                    #Otherwise just wait
                    if ((Get-Date) -gt $trySetOnlineTime) {
                        try{
                            Write-Host "Before attempt to set db online."
                            ($sqlServer.Databases.Item("$dbToBackUp")).SetOnline()
                        }
                        catch {
                            Write-Host "Error setting database $dbToBackUp online:`n$($_ | select *)"
                            $global:error.RemoveAt(0)
                            Sleep 15
                        }
                    }
                    else {
                        Sleep 10
                    }
                    #Update database statuses.
                    $sqlServer.Databases.Refresh($true)
                }
            } 
			$logfiles = ($sqlServer.Databases.Item("$dbToBackUp")).logfiles
			foreach ($lf in $logfiles)  
			{
			     $lf.shrink(1,[Microsoft.SqlServer.Management.Smo.ShrinkMethod]'Default')   
			}
            Write-Host "Backing up temporary database $dbToBackUp to $msSqlBackupPath ..."
            $backupFilePath = Backup-Database $sqlServerName $dbToBackUp $dbAdminSecureCreds.UserName $dbAdminSecureCreds.Password $msSqlBackupPath #$true
            $script:databasesToPublish += @("$backupFilePath")
        }
    }
}

function Process-QaReportRows {
    <#
    $rows is a hashtable that we get from Invoke-SqlReader. 
    It has a numerical index for .keys (which we ignore)

    We check for rows that have 3 columns and where the first column is a string beginning with "FileName=". 
    In that case, we expect that the array looks like this: (<filename>, <filecontent>, <index>)
    Since the <filename> item is actually in the form "Filename=SOMEFILENAME", so we have to strip out the first 9 characters.
    The <filecontent> item is the contents of one line in the file.
    The <index> item is the order that the line should be inserted into the file. Note that this index
    resets for each file; the index in $rows.keys doesn't. 

    If the row does NOT have the properties we check for, we assume that it is a message of type 

    Return a hashtable where the key is a filename and the value is the file contents, in order
    #>
    param(
        [parameter(mandatory=$true)] $rows
    )
    $outfiles = @{}
    if ($rows) {
        $files = @{}
        foreach ($row in [array]$rows.values) {
            if (($row.length -eq 3) -and ($row[0] -match "^FileName=(?<FileName>.+)$")) {
                $filename = "$($matches.FileName)"
                $linecontent = $row[1]
                $linenum = $row[2]

                # this works whether $files.$filename exists or not
                $files.$filename += @(,@($linecontent,$linenum))
            }
            elseif ($row[0]) { 
                write-host $row[0] 
            }
        }
        foreach ($filename in $files.keys) {
            $sorted = $files.$filename | sort-object @{ Expression= {[int]$_[1]}; Ascending=$true }
            foreach ($row in $sorted) {
                $outfiles.$filename += $row[0]
            }
        }
    }
    return $outfiles
}

task ExecuteQA {
    # QA by Db Type
    foreach ($dbTypeName in $dbTypeNames) {
        #perform association only if this is the edfi database and we have a dashboard db in context.
        if($dbTypeName -eq $edFiDbNameSeed -and $dbTypeNames -contains $dashboardDbNameSeed) {
            Invoke-SqlScript $adminConnectionStrings[$dbTypeName] "EXEC util.DashboardAssociation"
        }
        #Get scripts
        $qaFiles = @()
        $qaFiles += Select-EdFiDatabaseFiles "$infrastructureDbPath\QA\$dbTypeName\*\" "*.sql"
        
        foreach ($script in $qaFiles) {
            #TODO: Look at CSB's in the future. Current scope is for TX that doesn't have the new CSB work.
            $qaScript = New-SqlScript -scriptPath $script.FullName $sqlVars
            if ($qaScript.permission -eq "Admin") {
                $qaScript.connectionString = $adminConnectionStrings[$dbTypeName]
            }
            else {
                $qaScript.connectionString = $transientSqlConnectionStrings[$dbTypeName]
            }
            #Set default timeout if we haven't been given one.
            if ($qaScript.timeOut -eq $null) {
                #TODO:Set script level var for this default.
                $qaScript.timeOut = 30
            }
            #Execute
            Write-Host "Executing QA step $($script.FullName) ..."
            $outputs = Invoke-SqlReader $qaScript
            $outfiles = Process-QaReportRows $outputs
            foreach ($filename in $outfiles.keys) {
                $outfiles.$filename | out-file -filepath "$qaReportPath\$filename"
            }
        }
    }
}

task ExecuteQAStep {    
    try {
        #TODO: Move this to using new SqlScript functions.
        Import-Module "$($folders.modules.invoke('database\sqlps'))" -DisableNameChecking

        Invoke-SqlCmd -ServerInstance $sqlServerName -Database $edFiDbName -UserName $dbAdminCreds.UserName -Password $dbAdminCreds.Password -Query "EXEC util.DashboardAssociation"

        #Get scripts
        $qaFiles = @()
        foreach ($scriptDir in $sqlVars.QAScriptDir.split("|")) {
            $qaFiles += dir $scriptDir -filter "*.sql"
        }

        #Execute and store results
        $sqlResults = @()
        foreach ($script in $qaFiles) {
            try {
                $outputFileName = "$qaReportPath\$($script -replace ".sql", ".txt")"
                $sqlResult = Invoke-SqlCmd -ServerInstance $sqlServerName -Database $edFiDbName -UserName $dbAdminCreds.UserName -Password $dbAdminCreds.Password -QueryTimeout 3600 -InputFile $script.FullName
                $sqlResults += , ($script, $sqlResult.Length)
                if ($sqlResult.Length -gt 0) {
                    Invoke-SqlCmd -ServerInstance $sqlServerName -Database $edFiDbName -UserName $dbAdminCreds.UserName -Password $dbAdminCreds.Password -InputFile $script.FullName | Out-File $outputFileName
                }
                Invoke-SqlCmd -ServerInstance $sqlServerName -Database $edFiDbName -UserName $dbAdminCreds.UserName -Password $dbAdminCreds.Password -Query "INSERT INTO util.QAResults(ScriptFileName, NumberOfDiscrepancies) SELECT '$script', $($sqlResult.Length)"
            }
            catch {
				$escapedExceptionValue = $($_.Exception.Message) -replace "'","''"
				$queryText = "INSERT INTO util.QAResults(ScriptFileName, Comment) SELECT '$script', '$escapedExceptionValue'"
                Invoke-SqlCmd -ServerInstance $sqlServerName -Database $edFiDbName -UserName $dbAdminCreds.UserName -Password $dbAdminCreds.Password -Query $queryText
                $sqlResults += , ($script, -2147483647)
                $_.Exception.Message | Out-File $outputFileName
                $global:error.RemoveAt(0)
            }
        }
        $sqlResults = $sqlResults | Sort-Object @{Expression={[Math]::Abs($_[1])}; Ascending=$false}
        
        #Export report
        $report = "<html><body>`n"
        $report += "Number of QA scripts: $($qaFiles.length)<br><br>`n"
        $report += "<table border=`"1`" cellpadding=`"3`" cellspacing=`"0`"><tr><th>Script Name</th><th>Result</th><th># Discrepancies</th></tr>`n"

        foreach ($sqlResult in $sqlResults) {
            switch ($sqlResult[1]) {
                {$_ -eq -2147483647} { $report += "<tr><td>$($sqlResult[0])</td><td><a href=`"$($sqlResult[0] -replace ".sql", ".txt")`">Error</a></td><td align=`"right`">0</td></tr>`n" }
                {$_ -gt  0} { $report += "<tr><td>$($sqlResult[0])</td><td><a href=`"$($sqlResult[0] -replace ".sql", ".txt")`">Failed</a></td><td align=`"right`">$("{0:#,#}" -f $sqlResult[1])</td></tr>`n" }
                default {$report += "<tr><td>$($sqlResult[0])</td><td>Passed</td><td align=`"right`">0</td></tr>`n"}
            }
        }
        $report += '</table><br/><br/><br/></body></html>'
        $report | Out-File "$qaReportPath\QAScripts.html"

        Invoke-SqlCmd -ServerInstance $sqlServerName -Database $edFiDbName -UserName $dbAdminCreds.UserName -Password $dbAdminCreds.Password -Query "EXEC util.DashboardAssociation 0"
    }
    finally {
        Remove-Module SqlPs
    }
}

task DropTransientDatabaseTypes -depends BackupAndArchiveTransientDatabaseTypes {
    #Drop temp DBs.
    foreach ($dbTypeName in $transientConnectionStrings.Keys) {
            $dbToRemove = Get-ProjectSpecificDatabaseName $appTypePrefix $normSpecificEntityType $dbTypeName $buildConfigurationName $schoolyear $legacyVersionNum
            if (Test-DatabaseExists $sqlServer $dbToRemove ) {
                Write-Host "Removing temporary database: $dbToRemove"
                Remove-Database $sqlServerName $dbToRemove $dbAdminSecureCreds.UserName $dbAdminSecureCreds.Password
            }
            else {
                Write-Host "Database $dbToRemove not found on server $($sqlServerName). Nothing to Remove."
            }
    }
}

task PublishDatabaseBackup {
    if (Test-Function "Publish-DatabaseBackup") {
        foreach ($backupFile in $script:databasesToPublish) {
            Publish-DatabaseBackup $backupFile
        }
    }
}