param (
    [Parameter(Mandatory = $true)]
    $deploymentConfiguration,
    [Parameter(Mandatory = $true)]
    [string[]]
    [alias("localEducationAgencyNames")]
    $specificEntityNames,
    [Parameter(Mandatory = $true)]
    $sourceBuildConfig,
    $schoolYear,
    [string[]] $typesToRestore,
	$targetBuildConfig
)

#if no $targetBuildConfig provided, use deploymentConfiguration as target
if ($targetBuildConfig -eq $null -or $target -eq "") {$targetBuildConfig = $deploymentConfiguration }

# Relative paths (to this script)
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
. $scriptDir\..\..\..\modules\utility\build-utility.ps1

$scriptsDir = $($folders.scripts)
$baseDir   = $($folders.base)

# Imports
Import-Module "$($folders.modules.invoke('database\database-management.psm1'))"
Import-Module "$($folders.modules.invoke('utility\credential-management.psm1'))" -force
. "$($folders.activities.invoke(`"deployment\$deploymentConfiguration.vars.ps1`"))"
Import-Module "$($folders.activities.invoke('common.psm1'))"

# Initialize credentials
. "$($folders.activities.invoke('deployment\database\restore-database-from-backup-credentials.ps1'))"
Initialize-Credentials "$($folders.activities.invoke('deployment'))"

# Credentials
$dbAdminSecureCreds = Get-PSCredential "DB Admin ($sqlDataSourceName)"
$networkCreds = Get-NetworkCredential "DB Admin ($sqlDataSourceName)"
$applicationUser = Get-PSCredential "Application ($deploymentConfiguration)"

. "$($folders.activities.invoke(`"deployment\database\restore-database-from-backup.vars.ps1`"))"

#Pull types to restore from config if it was not passed in.
$typesToRestore = if($typesToRestore) {$typesToRestore} else {$dbTypesToRestore}


#TODO: Break these out into a single function that accepts the source and target db names.
# This may require a conversion to psake so that the lines above can be in context for the "tasks"
foreach ($dbType in $typesToRestore) {
    if ($dbType -eq "Application") {
        $sourceDbName = (Get-ProjectSpecificDatabaseName $appTypePrefix "" "$dbType" "$sourceBuildConfig" -versionSuffix "$legacyVersionNum") 
        $targetDbName = $applicationDbName 

        $targetDbExists = Test-DatabaseExists (Get-Server $sqlDataSourceName $dbAdminSecureCreds.UserName $dbAdminSecureCreds.Password) $targetDbName
        if ($targetDbExists) {
            $initialTargetDbName = $targetDbName + (Get-Date -format yyyyMMddHHmmsszz)
        }
        else {
            $initialTargetDbName = $targetDbName
        }
        
        # Find appropriate backup in backup folder
        $bakFilename = [IO.Path]::Combine($msSqlBackupPath, $sourceDbName + ".bak")

        Write-Host "Restoring database from $bakFilename to $initialTargetDbName ..."
        Restore-Database $sqlDataSourceName $initialTargetDbName $dbAdminSecureCreds.UserName $dbAdminSecureCreds.Password `
            $bakFilename $usersToReinitialize $sqlDataFilesPath $sqlLogFilesPath		
        
        if($supportUserPrefix) {
            $server = Get-Server $sqlDataSourceName $dbAdminSecureCreds.UserName $dbAdminSecureCreds.Password
            $dbConnStr = Get-SqlConnectionString $sqlDataSourceName $initialTargetDbName $networkCreds
            $logins = @()
            $server.Logins | % { $logins += $_.Name }
            foreach ($login in $logins) {
                if($login -like "$($supportUserPrefix)*") {
                    Write-Host "Creating support db user '$login' ..."
                    $query = "IF NOT EXISTS (SELECT name FROM sys.database_principals WHERE name = `'$login`')`n"
                    $query += "BEGIN`nCREATE USER $login FOR LOGIN $login`nEND`nGO`n"
                    $query += "EXEC sp_addrolemember N'db_datareader', N'$login'`nGO"
                    Invoke-SqlScript $dbConnStr $query
                }
            }
        }
        
        if ($targetDbExists) {
            $oldDbName = $targetDbName + "_Restoring"
            #Rename old db, overwrite destination db if present. This does fail prior to the clobber if the $targetDbName is not found.
            if (Rename-Database $sqlDataSourceName $targetDbName $oldDbName $dbAdminSecureCreds.UserName $dbAdminSecureCreds.Password -clobber) {
                #Succeded, Rename new db
                if(Rename-Database $sqlDataSourceName $initialTargetDbName $targetDbName $dbAdminSecureCreds.UserName $dbAdminSecureCreds.Password) {
                    #Succeded, Remove old db
                    Remove-Database $sqlDataSourceName $oldDbName $dbAdminSecureCreds.UserName $dbAdminSecureCreds.Password
                }
                else {
                    #if the rename of inital to target failed put the old one back.
                    $restoredOriginal = Rename-Database $sqlDataSourceName $oldDbName $targetDbName $dbAdminSecureCreds.UserName $dbAdminSecureCreds.Password
                    #Remove newly restored database to prevent clutter (we obviously still have the backup as we just restored for this db).
                    Remove-Database $sqlDataSourceName $initialTargetDbName $dbAdminSecureCreds.UserName $dbAdminSecureCreds.Password
                    if ($restoredOriginal) {
                        $errorMessage += "Rename failed for new db: $initialTargetDbName `r`n"
                        $errorMessage += "Rename to restore the original db to: $targetDbName succeeded. `r`n"
                        $errorMessage += "Restoration of $bakFilename to $targetDbName failed!"
                        throw $errorMessage
                    }
                    else {
                        $errorMessage = "CRITICAL:`r`n"
                        $errorMessage += "Rename failed for new db: $initialTargetDbName `r`n"
                        $errorMessage += "Rename to restore the original db to: $targetDbName failed. `r`n"
                        $errorMessage += "Restoration of $bakFilename to $targetDbName failed! `r`n"
                        $errorMessage += "MANUAL ACTION: Rename $oldDbName to $targetDbName"
                        throw $errorMessage
                    }
                }
            }
            else {
                #Remove newly restored dtabase to prevent clutter (we obviously still have the backup as we just restored).
                Remove-Database $sqlDataSourceName $initialTargetDbName $dbAdminSecureCreds.UserName $dbAdminSecureCreds.Password
                $errorMessage = "Rename failed for active instance."
                $errorMessage += "Restoration of $bakFilename to $targetDbName failed!"
                throw $errorMessage
            } 
        }

        if ($remoteMsSqlBackupPath) {
            Write-Host "Copying '$bakFilename' to $remoteMsSqlBackupPath ..."
            copy (Resolve-Path $bakFilename) $remoteMsSqlBackupPath
        }
    }
    else {
        foreach ($specificEntityName in $specificEntityNames) {
            $normalizedSpecificEntityName = $specificEntityName.Replace(" ", "")

            # Derive original source database, and target database (based on configurations)
            $sourceDbName = (Get-ProjectSpecificDatabaseName $appTypePrefix $normalizedSpecificEntityName $dbType $sourceBuildConfig $schoolyear $legacyVersionNum) 
            $targetDbName = (Get-ProjectSpecificDatabaseName $appTypePrefix $normalizedSpecificEntityName $dbType $targetBuildConfig $schoolyear $legacyVersionNum) 
            
            $targetDbExists = Test-DatabaseExists (Get-Server $sqlDataSourceName $dbAdminSecureCreds.UserName $dbAdminSecureCreds.Password) $targetDbName
            if ($targetDbExists) {
                $initialTargetDbName = $targetDbName + (Get-Date -format yyyyMMddHHmmsszz)
            }
            else {
                $initialTargetDbName = $targetDbName
            }

            # Find appropriate backup in backup folder
            $bakFilename = [IO.Path]::Combine($msSqlBackupPath, $sourceDbName + ".bak")

            Write-Host "Restoring database from $bakFilename to $initialTargetDbName ..."
            Restore-Database $sqlDataSourceName $initialTargetDbName $dbAdminSecureCreds.UserName $dbAdminSecureCreds.Password `
                $bakFilename $usersToReinitialize $sqlDataFilesPath $sqlLogFilesPath		
            
            if($supportUserPrefix) {
                $server = Get-Server $sqlDataSourceName $dbAdminSecureCreds.UserName $dbAdminSecureCreds.Password
                $dbConnStr = Get-SqlConnectionString $sqlDataSourceName $initialTargetDbName $networkCreds
                $logins = @()
                $server.Logins | % { $logins += $_.Name }
                foreach ($login in $logins) {
                    if($login -like "$($supportUserPrefix)*") {
                        Write-Host "Creating support db user '$login' ..."
                        $query = "IF NOT EXISTS (SELECT name FROM sys.database_principals WHERE name = `'$login`')`n"
                        $query += "BEGIN`nCREATE USER $login FOR LOGIN $login`nEND`nGO`n"
                        $query += "EXEC sp_addrolemember N'db_datareader', N'$login'`nGO"
                        Invoke-SqlScript $dbConnStr $query
                    }
                }
            }
            
            if ($targetDbExists) {
                $oldDbName = $targetDbName + "_Restoring"
                #Rename old db, overwrite destination db if present. This does fail prior to the clobber if the $targetDbName is not found.
                if (Rename-Database $sqlDataSourceName $targetDbName $oldDbName $dbAdminSecureCreds.UserName $dbAdminSecureCreds.Password -clobber) {
                    #Succeded, Rename new db
                    if(Rename-Database $sqlDataSourceName $initialTargetDbName $targetDbName $dbAdminSecureCreds.UserName $dbAdminSecureCreds.Password) {
                        #Succeded, Remove old db
                        Remove-Database $sqlDataSourceName $oldDbName $dbAdminSecureCreds.UserName $dbAdminSecureCreds.Password
                    }
                    else {
                        #if the rename of inital to target failed put the old one back.
                        $restoredOriginal = Rename-Database $sqlDataSourceName $oldDbName $targetDbName $dbAdminSecureCreds.UserName $dbAdminSecureCreds.Password
                        #Remove newly restored database to prevent clutter (we obviously still have the backup as we just restored for this db).
                        Remove-Database $sqlDataSourceName $initialTargetDbName $dbAdminSecureCreds.UserName $dbAdminSecureCreds.Password
                        if ($restoredOriginal) {
                            $errorMessage += "Rename failed for new db: $initialTargetDbName `r`n"
                            $errorMessage += "Rename to restore the original db to: $targetDbName succeeded. `r`n"
                            $errorMessage += "Restoration of $bakFilename to $targetDbName failed!"
                            throw $errorMessage
                        }
                        else {
                            $errorMessage = "CRITICAL:`r`n"
                            $errorMessage += "Rename failed for new db: $initialTargetDbName `r`n"
                            $errorMessage += "Rename to restore the original db to: $targetDbName failed. `r`n"
                            $errorMessage += "Restoration of $bakFilename to $targetDbName failed! `r`n"
                            $errorMessage += "MANUAL ACTION: Rename $oldDbName to $targetDbName"
                            throw $errorMessage
                        }
                    }
                }
                else {
                    #Remove newly restored dtabase to prevent clutter (we obviously still have the backup as we just restored).
                    Remove-Database $sqlDataSourceName $initialTargetDbName $dbAdminSecureCreds.UserName $dbAdminSecureCreds.Password
                    $errorMessage = "Rename failed for active instance."
                    $errorMessage += "Restoration of $bakFilename to $targetDbName failed!"
                    throw $errorMessage
                } 
            }

            if ($remoteMsSqlBackupPath) {
                Write-Host "Copying '$bakFilename' to $remoteMsSqlBackupPath ..."
                copy (Resolve-Path $bakFilename) $remoteMsSqlBackupPath
            }
        }
    }
}