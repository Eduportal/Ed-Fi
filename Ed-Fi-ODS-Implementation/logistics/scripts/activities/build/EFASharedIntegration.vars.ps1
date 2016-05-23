#This file is being renamed becuase it is confilisting with Apps when we
#are trying to build the packages. 
#Depending on the implementation of the implementation repository, 
#this will probably need to move or be renamed again.
$appTypePrefix = "EdFiOds"
$scriptDrive = Split-Path -Qualifier $MyInvocation.MyCommand.Path

. "$($folders.activities.invoke('common.ps1'))"

#Note: the presence of this path enables QA Reporting.
#$qaReportPath = "$($folders.base.invoke('..\'))QaReporting\"
$msSqlBackupPath         = "$scriptDrive\MSSQL\Backup\"

$etlUnitSaveResultOption = ""

#all transient dbs are backed up.
$tempDbTypeBackUpExceptions = @()

if ("$legacyVersionNum" -eq ""){
    $Script:legacyVersionNum = "$env:versionSuffix"
}
else {
    $Script:legacyVersionNum = "$legacyVersionNum"
}

#This is the pattern that matched for script $specificEntitySubTypeRegExs to indicate that it is an LEA $specificEntitySubTypeRegEx for additonal filtering.
$specificEntitySubTypeRegEx = "\d+\.\w+ISD"

# Always drop all databases before creating them in TDOEIntegration
# [ODS-433] Required to be Rest_Api due to client dependencies on folder name, rename later to EdFi_Bulk
$dbTypeTransient += @("EdFi","EdFiAdmin","EduId","EduId.Database","SSO_Integration","EdFiPopulated", "Rest_Api")

$global:AzurePackageCreationConfig = "Release"

# odsType must be set to 'sandbox' or 'sharedinstance' for databases to get deployed properly
$odsType = "Sandbox"

$structureScriptSqlTimeout = 120

$sqlServerName = "dunlop.doubleline.us"
$defaultOleDbProvider = "SQLNCLI11"

$edfiAdminCSB = New-Object System.Data.Common.DbConnectionStringBuilder $true
$edfiAdminCSB['Server'] = $sqlServerName
$edfiAdminCSB['Provider'] = $defaultOleDbProvider
$edfiAdminCSB['Database'] = "EFASharedIntegration_EdFi_Admin"

$edfiBulkCSB = New-Object System.Data.Common.DbConnectionStringBuilder $true
$edfiBulkCSB['Server'] = $sqlServerName
$edfiBulkCSB['Provider'] = $defaultOleDbProvider
$edfiBulkCSB['Database'] = "EFASharedIntegration_EdFi_Bulk"

$edfiOdsCSB = New-Object System.Data.Common.DbConnectionStringBuilder $true
$edfiOdsCSB['Server'] = $sqlServerName
$edfiOdsCSB['Provider'] = $defaultOleDbProvider
$edfiOdsCSB['Database'] = "EdFi_Ods_2015"

$environmentCSBsByDbType = @{  
                               # [ODS-433] Required to be restApiDbNameSeed due to client dependencies, rename later to edfiBulkDbNameSeed
                               "$restApiDbNameSeed"     = $edfiBulkCSB;
                               "$edfiAdminDbNameSeed"   = $edfiAdminCSB;
                               "$edfiDbNameSeed"        = $edfiOdsCSB;
                            }
