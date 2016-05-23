# *************************************************************************
# ©2013 Ed-Fi Alliance, LLC. All Rights Reserved.
# *************************************************************************

#no longer in use...

$ErrorActionPreference = "Stop"
properties { 
    #"environment" "deploymentConfiguration" "dropAppDb" are passed in.
    #NOTE: This script is being used in R10 to backup the production Application_Db for deployment to staging.
    #Any LoadRoles task should either take this into account or expose a backup only option.
    
    #Import here so functions are in scope.
    . "$($folders.modules.invoke('utility\build-utility.ps1'))"
    $dbTypeName = "Application";
	Assert-Variables "environment" "deploymentConfiguration"
    
    Import-Module "$($folders.modules.invoke('utility\credential-management.psm1'))" -force
    . "$($folders.activities.invoke(`"build\$environment.vars.ps1`"))"
    #This needs to come after the build vars so that the applicationDbName Returned is correct.
    . "$($folders.activities.invoke(`"deployment\$deploymentConfiguration.vars.ps1`"))"
    Import-Module "$($folders.activities.invoke('common.psm1'))"
    Import-Module "$($folders.modules.invoke('database\database-management.psm1'))"
    Import-Module "$($folders.modules.invoke('database\database-migrations.psm1'))"
    
    #Assure vars from imports
    Assert-Variables "applicationDbName", "msSqlBackupPath", "sqlServerName"
    
    Add-CredentialMetadata `
	@(
        @{Name = "DB Admin($sqlServerName)";  DefaultUsername = $null;    Description = "Credentials for full control of the SQL Server instance."; IntegratedSecuritySupported = $true}
      )

	# Initialize credentials
	Initialize-Credentials $folders.activities.Invoke('build\')

	# Credentials used in this script
	$dbAdminSecureCreds  = Get-PSCredential      "DB Admin($sqlServerName)"

    #Get the main type based connection strings
    #$appSqlDbNameInfix = #GET VALUE from environment config! Typically, $buildConfigurationName
    $sqlDbName = $applicationDbName

    #Will need this later for any dtsx package execution:
    #$dbLoaderCreds       = Get-NetworkCredential "DB Loader"
    #$etlPackagesBasePath      = "$($folders.base)\Etl"
    #$oleConnString = $(Get-OleDbConnectionString $sqlServerName $sqlDbName $dbLoaderCreds))

    $connString = Get-SqlConnectionString $sqlServerName $sqlDbName $dbAdminSecureCreds

    #Get SqlServer Object for db existence checks
    $sqlServer = Get-Server $sqlServerName $dbAdminSecureCreds.UserName $dbAdminSecureCreds.Password

    $parms = @{ dbTypeNames   = @($dbTypeName);
               databaseName  = $sqlDbName;
               folders       = $folders;
               subTypeNames = ($specificEntityTypeNames | % { $_.Replace(" ","")})
             } 
}

task default -depends BackUpApplicationDb,DropApplicationDb,CreateAndSyncDb,SyncDb,ExecuteEnvironmentSpecificConfig

#Always take a backup first, just in case.
task BackUpApplicationDb -precondition {return (Test-DatabaseExists $sqlServer $sqlDbName)} {
    Write-Host "Backing up temporary database $sqlDbName to $msSqlBackupPath ..."
    Backup-Database $sqlServerName $sqlDbName $dbAdminSecureCreds.UserName $dbAdminSecureCreds.Password "$msSqlBackupPath\" | out-null
}
#Based on variable from deployment configuration determine whether the application DB should be dropped.
task DropApplicationDb -precondition {return ($dropAppDb -eq $true)} {
    Remove-Database $sqlServerName $sqlDbName $dbAdminSecureCreds.UserName $dbAdminSecureCreds.Password -safe
}

#if the application database does not exist, create and initalize it.
task CreateAndSyncDb -precondition {return -not (Test-DatabaseExists $sqlServer $sqlDbName)} {
    Invoke-Psake "$($folders.activities.invoke('build\create-database\create-database-psake.ps1'))" -parameters $parms
    if ($error) {
        Write-Host "An error has occured during Creation/Sync of database type: $dbTypeName"
        exit $error.Count 
    }
}

#Perform database updates to current level if we didn't just recreate it.
task SyncDb -precondition {return ($dropAppDb -eq $false)} {
    Invoke-Psake "$($folders.activities.invoke('build\create-database\create-database-psake.ps1'))" SyncDatabaseScripts -parameters $parms
}

task ExecuteEnvironmentSpecificConfig -precondition {return (Test-Function "Update-ApplicationDatabase")} {
    Update-ApplicationDatabase $connString
}
<# Waiting on Requirements!
task LoadRoles {
    #Check if role data exists for a district.?
    #Execute if not?

    #need to import dtsx exec module.
    #re-enable $connectionString
}
#>