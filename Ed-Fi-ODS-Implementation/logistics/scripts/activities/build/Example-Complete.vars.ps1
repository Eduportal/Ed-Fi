# Import some variables for use later
. "$($folders.activities.invoke('common.ps1'))"

# Should not be changed
$appTypePrefix = "EdFiOds"

# If this is set, enable QA reporting
#$qaReportPath = "$($folders.base.invoke('..\'))QaReporting\"

# Should be set to a directory on the database server that the MSSQL service can read from
# If empty, attempt to use the default backup directory (may have permission problems)
#$msSqlBackupPath = "D:\MSSQL\Backup"
#
# This is frequently set to a folder \MSSQL\Backup on the same drive as the build agent uses. 
# Doing so lets us tell clients they need to create directories for the build agent and MSSQL 
# backup on the same drive, but they don't have to tell us what that drive is or if it changes.
#$scriptDrive = split-path -Qualifier $PSScriptRoot
#$msSqlBackupPath = "$scriptDrive\MSSQL\Backup\"

# This is the pattern that matched for script $specificEntitySubTypeRegExs to indicate that it is an LEA $specificEntitySubTypeRegEx for additonal filtering.
# Pattern used to determine scripts to run against LEA 
# Used only in dashboard loads
$specificEntitySubTypeRegEx = "\d+\.\w+ISD"

# Transient databases will get dropped and redeployed from scratch as empty databases. Add to the list of
# transient databases by appending to this array.
# 
# Note that some database types are set to transient in initialize-database.vars.ps1. This setting allows 
# the definition of *additional* transient databases that differ per promotion environment. 
#
# For example, in development you might want to add *all* database types here, in order to get pristine
# databases for every build.
#
# This also means that in an environment vars file, a list of databases must be *added* with "+=", rather 
# than *assigned* with "=". 
#$dbTypeTransient += @()

# Transient databases whose types are in this list will get backed up before getting dropped. 
# If this array is empty, all transient databases will get backed up. 
$tempDbTypeBackUpExceptions = @()

# odsType is only relevant for Ed-Fi ODS implementations. 
# It must be set to 'sandbox' or 'sharedinstance' for databases to get deployed properly.
# Sandboxed databases use a completely separate ODS per API key and are useful in development and for SIS 
# vendors to test their integration, while sharedInstance databases are what's used in real production.
$odsType = "Sandbox"

# Timeout for SQL scripts. 
# This remains for backwards compatibility only. 
# The default timeout of 30 seconds is set in the database-management module
# SQL scripts which need a longer timeout should request it as a variable at the top of the file
#$structureScriptSqlTimeout = 120

### Database settings
# Note that some changes in this section (such as to $sqlServerName or a database name) may need to be reflected in <connectionStrings> sections for config transforms.

# Hostname of the database server
$sqlServerName = "localhost"

# OLE DB provider for the database server. (Changes per version of MSSQL.)
$defaultOleDbProvider = "SQLNCLI11"

# Connection string builders. Probably don't need to change these unless you want to change the database name.

$edfiAdminCSB = New-Object System.Data.Common.DbConnectionStringBuilder $true
$edfiAdminCSB['Server'] = $sqlServerName
$edfiAdminCSB['Provider'] = $defaultOleDbProvider
$edfiAdminCSB['Database'] = "EdFi_Admin"

$edfiBulkCSB = New-Object System.Data.Common.DbConnectionStringBuilder $true
$edfiBulkCSB['Server'] = $sqlServerName
$edfiBulkCSB['Provider'] = $defaultOleDbProvider
$edfiBulkCSB['Database'] = "EdFi_Bulk"

# Note that, in SharedInstance environments, the ODS database name is special. It must be "EdFi_Ods_YYYY", where YYYY is the school year. 
# ODS database names in Sandbox environments can be anything. 
$edfiOdsCSB = New-Object System.Data.Common.DbConnectionStringBuilder $true
$edfiOdsCSB['Server'] = $sqlServerName
$edfiOdsCSB['Provider'] = $defaultOleDbProvider
$edfiOdsCSB['Database'] = "EdFi_Ods"

# This hashtable mus be populated
# The database name seeds are found in common.ps1
$environmentCSBsByDbType = @{  
    # [ODS-433] Required to be restApiDbNameSeed due to client dependencies, rename later to edfiBulkDbNameSeed
    "$restApiDbNameSeed"   = $edfiBulkCSB;
    "$edfiAdminDbNameSeed" = $edfiAdminCSB;
    "$edfiDbNameSeed"      = $edfiOdsCSB;
}
