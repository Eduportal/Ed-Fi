# *************************************************************************
# ©2012 Ed-Fi Alliance, LLC. All Rights Reserved.
# *************************************************************************

#  General Notes about this script:
#  This script needs either a databaseName or (specificEntityName and buildConfigurationName)
#  if you pass in a database name then any specificEntitySubTypeNames needed will need to have been passed in already normalized.
# 

$ErrorActionPreference = "Stop"
properties {
    Assert ($dbTypeNames -ne "" `
        -and (($buildConfigurationName -ne "") -or $databaseName -ne "") `
        -and $environment -ne "" ) `
        "Required parameters: dbTypeNames, environment and `n`ta) buildConfigurationName, or `n`tb) databaseName."
    
    # Relative paths (to this script)
    if (-not (get-module |? { $_.Name -eq "path-resolver" })) {
        Import-Module $PSScriptRoot\..\..\..\modules\path-resolver.psm1
    }
    
    if($specificEntityName)
    {
        $normalizedSpecificEntityName	= $specificEntityName.Replace(" ", "")
        $specificEntitySubTypeNames += @($normalizedSpecificEntityName)
    }
    
    #Load Common Modules
    . "$($folders.modules.invoke('utility\build-utility.ps1'))"
    . "$($folders.activities.invoke('common.ps1'))"
    Import-Module "$($folders.modules.Invoke('utility\credential-management.psm1'))" -force
    Import-Module "$($folders.modules.Invoke('database\database-migrations.psm1'))"
    Import-Module "$($folders.modules.Invoke('database\database-management.psm1'))"
    Import-Module "$($folders.modules.Invoke('database\dtsx-execution.psm1'))"
    Import-Module "$($folders.modules.invoke('utility\common-objects.psm1'))"
    
    #Ensure no accidental database deletes
    #if (Test-function "Delete-Database") { Remove-Item function:\Delete-Database}

    #Load Common Vars into scope
    . "$($folders.activities.Invoke(`"build\$environment.vars.ps1`"))"
    #Environment needs to come before so that environment specific vars can be used in the process specific vars.
    #Use an implementation specific process vars (e.g. initialize-database.vars.ps1) with environment specifc var references
    #to override core functionality. This does mean that any automatic fall through overrides by environment vars are prohibited.
    . "$($folders.activities.Invoke('build\initialize-database\initialize-database.vars.ps1'))"
    Assert ($dbTypeTransient -ne $null -and $dbTypeTransient.GetType() -eq @().GetType()) "Missing `$dbTypeTransient containing a list of transient database types."
	
	# Initialize-database.vars.ps1 is imported second which sets adds default dbTypeTransient list.
	# you can add additional transient types in your $environment.vars.ps1 file like this: 
	#    $dbTypeTransient += @("Application")
	# this will allow you to force a particular database to drop, like for instance the application database in the integration environment.
    # NOTE: To remove a transient type you would specify the types to remove in the environment vars. Then
    # extend initialize-database.vars in "Apps" and loop through $dbTypeTransient and remove those types. 
	# this whole next region is to determine the database server and environment
	
    #New logic. If the database name is not defined, and we only have one database type (Not Acceptance), and the database type is one that is defined in the environmentCSBsByDbType, use that information for this.
    #TODO: Look at using $dbTypeNames -notContains "Acceptance" instead of count one, as we may see scenarios where we want to use the first type
    # to determine the CSB where there are subsequent types to be loaded.
    if ([string]::IsNullOrWhiteSpace($databaseName) -and $dbTypeNames.Count -eq 1 -and $environmentCSBsByDbType.Keys -contains $dbTypeNames[0]) {
        $initializeCSB = Copy-DbConnectionStringBuilder $environmentCSBsByDbType[$dbTypeNames[0]]
    }
    else {
        $initializeCSB = New-Object System.Data.Common.DbConnectionStringBuilder $true
        $initializeCSB["Provider"]            = $defaultOleDbProvider
        
        #region Derive sqlDatabaseName and databaseServerName if not provided.
        if ([string]::IsNullOrWhiteSpace($databaseName)) {
            #Import here so the include is not needed in configurations that don't need it.
            Import-Module "$($folders.scripts.Invoke('activities\common.psm1'))"
            
            $initializeCSB["Database"] = Get-ProjectSpecificDatabaseName $appTypePrefix $normalizedSpecificEntityName "$($dbTypeNames[0])" "$buildConfigurationName" "$schoolyear" "$legacyVersionNum"
            Write-Host "Generated Database Name: $($initializeCSB["Database"])"
            # TODO: Add $currentSchoolyear arg when configuration handling is ready for year-specific databases		
        } 
        else {
            $initializeCSB["Database"] = $databaseName
        }
        
        #if $databaseServerName wasn't given or derived from databaseName, Assume $sqlServerName from $environment.vars file.
        if ([string]::IsNullOrWhiteSpace($databaseServerName)) {
            if ($sqlDatabaseName -like "*application*" -and (Test-Path variable:applicationDbSqlServerName)) {
                $initializeCSB["Server"] = $applicationDbSqlServerName 
            }
            elseif ($sqlDatabaseName -like "*dashboardDW*" -and (Test-Path variable:dashboardDwSqlServerName)) {
                $initializeCSB["Server"] = $dashboardDwSqlServerName 
            }
            else {
                Write-Host "Using $sqlServerName set in build\$environment.vars.ps1"
                $initializeCSB["Server"] = $sqlServerName
            }			
        }
        else {
            $initializeCSB["Server"] = $databaseServerName
        }
    }
		
    #Load Base Credentials
    . "$($folders.activities.Invoke('build\initialize-database\initialize-database.credentials.ps1'))"
    Initialize-Credentials $folders.activities.Invoke('build\')
    
	$dbAdminCreds       = Get-NetworkCredential "DB Admin($($initializeCSB["Server"]))"
	$dbAdminSecureCreds = Get-PSCredential      "DB Admin($($initializeCSB["Server"]))"
    #Using db loader creds for all referenced DBTypes as it is a limited account that must be the same for all
    #servers in an environment.
    #NOTE: These creds are not imported by default. They must be added via an initialize-database.credentials.ps1 extension.
	$dbLoaderCreds      = Get-NetworkCredential "Db Loader"
    
    if ($dbAdminCreds -ne $null) {
        $initializeCSB["Uid"] = $dbAdminCreds.UserName;
        $initializeCSB["Pwd"] = $dbAdminCreds.Password;
    }
    else {
        $initializeCSB["Trusted_Connection"] = "yes"
    }

	#endregion    
	
    #Determine DbLifecycleModel
    #For now, if any db type being initialized is transient the entire database is treated as transient.
    foreach ($dbTypeName in $dbTypeNames){
        if ($dbTypeTransient -contains $dbTypeName) {
            $dbLifecycle = "Transient"
			$dbLogType="Simple"
            break
        }
        else {
            $dbLifecycle = "Persistent"
			$dbLogType="Full"
        }
    }
    	
    foreach ($envCSBdbType in $environmentCSBsByDbType.Keys) {
        $tempCSB = Copy-DbConnectionStringBuilder $environmentCSBsByDbType[$envCSBdbType]
        if ($dbLoaderCreds -ne $null) {
            $tempCSB["Uid"] = $dbLoaderCreds.UserName;
            $tempCSB["Pwd"] = $dbLoaderCreds.Password;
        }
        else {
            $tempCSB["Trusted_Connection"] = "yes"
        }
        #Set up all the dbTypes from for the environemnt to be read during DB initialization if needed.
        #This uses loader creds since they should be the same accross the environemnt.
        $connectionStrings[$envCSBdbType] = (Get-OleDbConnectionString $tempCSB)
    }
    
    #Set all the declared database type's connection strings to this database so they are available for package execution.
    #This also overrides and preadded ones from the environment CSBs from above (for scenarios like acceptance).
    foreach ($dbTypeName in $dbTypeNames) {
        $connectionStrings[$dbTypeName] = (Get-OleDbConnectionString $initializeCSB)
    }
	$connString  = Get-SqlConnectionString $initializeCSB
    $sqlServer   = Get-Server $initializeCSB["Server"] $dbAdminSecureCreds.UserName $dbAdminSecureCreds.Password
}

task default -depends BackupDatabase,DropDatabase,CreateDatabase,InitializeEncryption,SyncDbTypes
task DeployDatabaseProjectFromMSBuild -depends BackupDatabase,DropDatabase,CreateDatabaseFromMsbuildDatabaseProject,InitializeEncryption,SyncDbTypes

#migrationMetadataScriptFiles {{MetaDataHash},@(ScriptFileArray)}
#sqlVars {"variableName", "variableValue"}
function Run-Scripts {
    [cmdletbinding()]
    param( [Parameter(Mandatory=$true)]
           [hashtable]$migrationMetadataScriptFiles,
           [hashtable]$sqlVars
    )
    #TODO:Fix or issue warning/error about random metadata orders for multiple database types.
    #Get-RepositoryNames comes from path-resolver.psm1
    foreach ($scriptSource in Get-RepositoryNames) {
        $scriptFiles = @()
        foreach ($migrationMetadataKey in $migrationMetadataScriptFiles.PSBase.Keys) {
            if ($migrationMetadataKey.ScriptSource -eq $scriptSource) {
                 $scriptFiles += $migrationMetaDataScriptFiles[$migrationMetadataKey]
            }
        }
        Write-Host "For $dbTypeName in $scriptSource preparing to Sync $($scriptFiles.Count) files."
        
        #Sync Per Repo from general to specific
        Sync-Database $scriptFiles $connString $initializeCSB["Database"] $datascriptSqlTimeout $sqlVars
    }    
}

function Get-MigrationMetadataScriptFiles([string]$dbTypeName, [string]$scriptType) {
    $allSqlFilesForDbType = Select-EdFiDatabaseFiles "$infrastructureDbPath\$scriptType\$dbTypeName\*\" "*.sql"
    Write-Verbose "Found $($allSqlFilesForDbType.Count) raw files for $scriptType and $dbTypeName"
    $outputMigrationMetadataScriptFiles = New-HashtableKeyedHashtable
    $migrationMetaSqlFiles = New-HashtableKeyedHashtable
    foreach ($sqlFile in $allSqlFilesForDbType) {
        #Get the metadata for the raw script
        $migrationMetadata = Get-ScriptMigrationMetadata $sqlFile.fullname
        #Add it to the raw collection
        $migrationMetaSqlFiles[$migrationMetadata] += @($sqlFile)
    }
    Write-Verbose "Found $($migrationMetaSqlFiles.PSBase.Count) sets of unique metadata for $scriptType and $dbTypeName"
    #If we have a edfilegacyVersion get it's files and the keys to filter on (e.g. ScriptSource/ScriptType/DatabaseType)
    if ($edfiStandardVersion) {
        foreach ($keymatch in ($migrationMetaSqlFiles.PSBase.Keys | where { $_.SubType -eq $edfiStandardVersion})) {
            $outputMigrationMetadataScriptFiles[$keymatch] = $migrationMetaSqlFiles[$keymatch]
            $legacyKeyMatches += $keymatch  
        }  
    }
    
    #Only get root DB folders (No subtypes) where the keymatch does not match the legacy key matches (so we don't try to run multiple versions of the same DB).
    #NOTE: $edfiStandardVersion is deprecated as of Ed-Fi 1.2 as the version subdirectories are no longer maintatined.
    #where { $_.SubType -ne $edfiStandardVersion -and $specificEntitySubTypeNames -notcontains $_.SubType }
    foreach ($keymatch in ($migrationMetaSqlFiles.PSBase.Keys | where { [string]::IsNullOrWhiteSpace($_.SubType) })) {
        $skipKey = $false
        #This logic skips the parent scripts when using legacy versions.
        foreach ($legacyKey in $legacyKeyMatches) {
            if($legacyKey.ScriptSource -eq $keymatch.ScriptSource -and
               $legacyKey.ScriptType -eq $keymatch.ScriptType -and
               $legacyKey.DatabaseType -eq $keymatch.DatabaseType) {
                    $skipKey = $true
                    break
               }   
        }
        if (-not $skipKey) {
            $outputMigrationMetadataScriptFiles[$keymatch] = $migrationMetaSqlFiles[$keymatch]
        }
    }
    Write-Verbose "Found $($outputMigrationMetadataScriptFiles.PSBase.Keys.Count) sets of unique metadata for $scriptType and $dbTypeName that don't have a subtype."
    #Get and files from subfolders that match the SE type passed in.
    foreach ($seName in $specificEntitySubTypeNames) {
        foreach ($keymatch in ($migrationMetaSqlFiles.PSBase.Keys | where { $_.SubType -eq $seName })) {
            $outputMigrationMetadataScriptFiles[$keymatch] = $migrationMetaSqlFiles[$keymatch]
        }
    }
	
	return $outputMigrationMetadataScriptFiles
}

function BackupDatabase-Precondition {
	# reasons not to back a database up:
	#   * the database does not exist
	#   * the database is transient
	#   * if there are no scripts to apply

	if (-not(Test-DatabaseExists $sqlServer $initializeCSB["Database"])) { 
		Write-Host "$($initializeCSB["Database"]) does not exist on $($initializeCSB["Server"])"
		return $false 
	}
	if ($dbLifecycle -eq "Transient") { 
		Write-Host "$($initializeCSB["Database"]) is Transient, no need to back it up"
		return $false 
	}
	
    #Search for scripts to apply in the order they will be applied.
    #NOTE: This check does not acknowledge database version hacks.
    $scriptTypes = @("structure","data")
    foreach ($scriptType in $scriptTypes) {
        foreach ($dbTypeName in $dbTypeNames) {
            $migrationMetadataScriptFiles = Get-MigrationMetadataScriptFiles $dbTypeName $scriptType
            #Get repo names returns the repo names from path resolver in the order of most general first.
            foreach ($scriptSource in Get-RepositoryNames) {
                foreach ($migrationMetadataKey in $migrationMetadataScriptFiles.PSBase.Keys) {
                    if ($migrationMetadataKey.ScriptSource -eq $scriptSource) {
                        if (Test-SyncDatabase $migrationMetadataScriptFiles[$migrationMetadataKey] $connString $initializeCSB["Database"]) {
                            #Return true as soon as we find scripts to apply.
                            return $true
                        }
                    }
                }
            }
        }
    }

    Write-Host "$($initializeCSB["Database"]) is $dbLifecycle but there are no new scripts to apply."
    return $false 
}


Function Import-PackageData ($dbTypeName) {
    $allDtsxDataFilesForDbType = Select-EdFiDatabaseFiles "$infrastructureDbPath\Data\$dbTypeName\*\" | where { $_.Extension -eq ".dtsx"}
    if ($edfiStandardVersion) {
        $edfiLegacyVersionFiles = $allDtsxDataFilesForDbType | where {$_.Directory -match $edfiStandardVersionRegEx}
        $edfiLegacyVersionRootDirectories = $edfiLegacyVersionFiles | % {$_.Directory -match $edfiStandardVersionRegEx | Out-null; $matches[1]}
    }
    $edfiFiles = $allDtsxDataFilesForDbType | where { -not ($_.Directory -match $specificEntitySubTypeRegEx) -and $_.Directory -notmatch $edfiLegacyVersionsRegEx -and $edfiLegacyVersionRootDirectories -notcontains $_.Directory }
    $edfiDtsxFiles = @()
    $edfiDtsxFiles += $edfiLegacyVersionFiles
    $edfiDtsxFiles += $edfiFiles

    # Get all SSIS packages sorted by file name
    $fileGroups = $edfiDtsxFiles | sort DirectoryName -desc | group DirectoryName

    if ($fileGroups) { 
        foreach ($fileGroup in $fileGroups) {
            # Execute each package in sequence in 32 bit mode (for the sake of the Excel driver)
            $fileGroup.Group | sort Name | % {
                $file = $_
                $variables = @{}
            
                $excelFileName = [IO.Path]::ChangeExtension($file.FullName, ".Data.xls")
                if (Test-Path $excelFileName) {
                    $variables.Add("ExcelFilePath", $excelFileName)
                }
                $partialSourcePathFile = [IO.Path]::ChangeExtension($file.FullName, ".DataPath.txt")
                if (Test-Path $partialSourcePathFile) {
                    $partialSourcePathFileContent = @(Get-Content $partialSourcePathFile)
                    if ($partialSourcePathFileContent.Count -gt 0) {
                        $partialPath = $partialSourcePathFileContent[0]
                    }
                    else {
                        throw "$partialSourcePathFile was defined, but contained no data."
                    }
                    $packageSourcePath = $folders.base.invoke($partialPath)
                    $variables.Add("DataPath", "$packageSourcePath")
                }

                Write-Host "Executing package $($file.Name) ..."
                $success = $false
                $success = Invoke-Package "$($file.FullName)" $connectionStrings $variables
            
                if (-not $success) {
                    throw "Package $($file.Name) failed."
                } else {
                    Write-Host "Package executed successfully."
                }
            }
        }
    }
}

task BackupDatabase -precondition {BackupDatabase-Precondition} {
	Write-Host "Backing up database $($initializeCSB["Database"]) to $msSqlBackupPath ..." #msSqlBackupPath comes from $environment.vars file
    Backup-Database $initializeCSB["Server"] $initializeCSB["Database"] $dbAdminSecureCreds.UserName $dbAdminSecureCreds.Password "$msSqlBackupPath\" | out-null
}

task DropDatabase -precondition {return ($dbLifecycle -eq "Transient")} -depends BackupDatabase {
    Assert ($initializeCSB["Database"] -ne $null) "'sqlDatabaseName' has not yet been assigned.  Review how script is called to make sure the appropriate script arguments have been passed."
	Remove-Database $initializeCSB["Server"] $initializeCSB["Database"] $dbAdminSecureCreds.UserName $dbAdminSecureCreds.Password -safe
}

task InitializeSqlLogins -precondition {Test-Function "Initialize-SqlLogins"} {
    #This functionality should be expanded later.
    Initialize-SqlLogins
}

task CreateDatabase -precondition {(-not (Test-DatabaseExists $sqlServer $initializeCSB["Database"]))} -depends InitializeSqlLogins { 
    $script:db = New-DatabaseWithMigrations $initializeCSB["Server"] $initializeCSB["Database"] $dbAdminSecureCreds.UserName $dbAdminSecureCreds.Password $dbLogType
    $script:dbCreated = $true
}

task InitializeEncryption -precondition {$databaseEncryptorName -and $databaseEncryptionType -and $databaseEncryptionAlgorithm} {
    if ($db -eq $null) {
        $sqlServer.Databases.Refresh($true)
        $db = $sqlServer.Databases.Item("$($initializeCSB["Database"])")
    }
    # Turn on database encryption, if configured
    Enable-DatabaseEncryption $db $databaseEncryptionAlgorithm $databaseEncryptionType $databaseEncryptorName
}

task SyncDbTypes {
    #if we have more than one dbtype run the initiizatation for each (e.g. a testing acceptance database)
    foreach ($dbTypeName in $dbTypeNames) {
        Run-Scripts (Get-MigrationMetadataScriptFiles $dbTypeName "structure") $sqlVars
        if($dbCreated) { Import-PackageData $dbTypeName } #This should only run if the DB is new. Currently there are no subsequent packages for persitent dbs.
        Run-Scripts (Get-MigrationMetadataScriptFiles $dbTypeName "data") $sqlVars
    }
}



<#
.description
Apply entity framework migrations with migrate.exe
.notes
1) Assumes that EF migrations are packaged into logistics/bin/$MigrationName/Migrations
2) Assumes that EF itself (migrate.exe and so forth) is packaged into logistics/bin/$MigrationName/EntityFramework
3) Assumes an $EFMigrationLibraryNames hashtable to correspond a dbTypeName with a migration library name. E.g.:
   $EFMigrationLibraryNames = @{
       EdfiAdmin = "EdFi.Ods.Admin.Models"
       Rest_Api  = "EdFi.Ods.Api.Data"
   }
   This hashtable should be in initialize-databas.vars.ps1 in the implementation repo
#>
task ApplyEntityFrameworkMigrations {
    foreach ($dbTypeName in $dbTypeNames) {
        $MigrationName = $EFMigrationLibraryNames.$dbTypeName
        if ($MigrationName) {
            write-host "Found EntityFramework migration '$MigrationName' for dbType '$dbTypeName'"
            $efPath = $folders.base.invoke("logistics\bin\$MigrationName\EntityFramework")
            $MigrationPath = $folders.base.invoke("logistics\bin\$MigrationName\Migrations")

            $migrateexe = resolve-path "$efPath\migrate.exe"
            Write-Host "Startup $MigrationPath..."
            $migrateArgs = @(
                "`"$MigrationName.dll`"", "/StartUpDirectory=`"$MigrationPath`"", "/connectionString=`"$connString`"",
                "/connectionProviderName=`"System.Data.SqlClient`""
            )
            Invoke-ProcessAndWait -command $migrateexe -argumentList $migrateArgs -CheckExitCode -ShowStandardOutput
            write-host "migrate.exe exit code: $LASTEXITCODE"
        }
        else {
            write-host "No EntityFramework migration found for dbType '$dbTypeName'"
        }
    }
}

<#
.description
Apply descriptors with the bulk loader
.notes
Gets location of console bulk load executable and descriptors from initialize-database.vars.ps1 (or overrides in environment vars files)
#>
task ApplyDescriptors {
    write-host -foreground yellow "Importing descriptors..."
    $bulkLoadArgs = @("/f","`"$edfiDescriptorsFolder`"","/d","`"$($initializeCSB['Database'])`"",
        "/m","`"$edfiDescriptorsManifest`"","/c","`"$connString`"")
    $consoleBulkLoadExe = $folders.base.invoke($consoleBulkLoadRelPath)
    Invoke-ProcessAndWait -command $consoleBulkLoadExe -argumentList $bulkLoadArgs -CheckExitCode
}

<#
.description
Apply edorgs with the bulk loader
.notes
Gets location of console bulk load executable and edorgs from initialize-database.vars.ps1 (or overrides in environment vars files)
#>
task ApplyEdOrgs {
    write-host -foreground yellow "Importing edorgs..."
    $bulkLoadArgs = @("/f","`"$edfiEdOrgsFolder`"","/d","`"$($initializeCSB['Database'])`"",
        "/m","`"$edfiEdOrgsManifest`"","/c","`"$connString`"")
    $consoleBulkLoadExe = $folders.base.invoke($consoleBulkLoadRelPath)
    Invoke-ProcessAndWait -command $consoleBulkLoadExe -argumentList $bulkLoadArgs -CheckExitCode
}

<#
.description
Use MSBuild to deploy a .sqlproj (Visual Studio database project)
.notes
1)  Assumes a $SqlProjectNames hashtable to correspond a dbTypeName with a full path to a database project. E.g.:
    $SqlProjectNames = @{
        EduId = Select-RepositoryResolvedFiles "Database/Structure/EduId.Database/EduId.Database.sqlproj"
    }
    This hashtable should be in initialize-databas.vars.ps1 in the implementation repo
2)  Multiple dbTypeNames are supported, but only *one* of them can have a database project associated with it
#>
task CreateDatabaseFromMsbuildDatabaseProject {
    $masterCSB = Copy-DbConnectionStringBuilder $initializeCSB
    $masterCSB["Database"] = "master"
    $masterConnStr = Get-SqlConnectionString -dbCSB $masterCSB

    # We might have a dbTypeNames array with more than one type. 
    # If so, that might be OK - for example, a database project could be deployed first, and then .sql scripts of other dbTypeNames
    # could be applid. However, only one database project can be deployed to a given database. Make sure there's only one:
    $projectDbType = $dbTypeNames |? { $SqlProjectNames[$_] }
    if ($projectDbType.count -ne 1) {
        throw "Expected 1 dbTypeName to have a database project, but instead found $($projectDbType.count)."
    }
    $foundProjectFile = $SqlProjectNames[$projectDbType]
    write-host "Creating database from database project found at '$foundProjectFile'"

    # Find MSBuild. Look for VS2013 MSBuild; if not, fall back to .NET 4.0 MSBuild (available on any .NET 4.0 machine):
    $msBuildEXE = "${env:WinDir}\Microsoft.NET\Framework\v4.0.30319\msbuild.EXE" 
    if (test-path "${env:ProgramFiles(x86)}\MSBuild\12.0\bin\MSBuild.exe") { 
        $msBuildEXE = "${env:ProgramFiles(x86)}\MSBuild\12.0\bin\MSBuild.exe" 
    }

    # Need to supply MSBuild with a /p:TargetDatabase= that is set to the database name, and /p:TargetConnectionString= that is set to
    # a connection string for the 'master' catalog, as well as a project file
    Invoke-ProcessAndWait -ShowStandardOutput -command $msBuildEXE -argumentList @('/t:rebuild,deploy','/nr:false'
        "`"/p:TargetDatabase=$($initializeCSB['Database'])`"","/p:TargetConnectionString=`"$masterConnStr`"","`"$foundProjectFile`"")

    $script:dbCreated = $true
}
