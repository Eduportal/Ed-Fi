# Import some variables for use later
. "$($folders.activities.invoke('common.ps1'))"

# Should not be changed for ODS implementations
$appTypePrefix = "EdFiOds"

# Should be set to a directory on the database server that the MSSQL service can read from
# If empty, attempt to use the default backup directory (may have permission problems)
#$msSqlBackupPath         = "$scriptDrive\MSSQL\Backup\"

# odsType must be set to 'sandbox' or 'sharedinstance' for databases to get deployed properly.
# Sandboxed databases use a completely separate ODS per API key and are useful in development and for SIS vendors to test their integration, while sharedInstance databases are what's used in real production
$odsType = "Sandbox"

# Timeout for SQL scripts. 120 is a good default.
$structureScriptSqlTimeout = 120

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
# ODS database names in Sandbox environments can be anything, but is typically "EdFi_Ods". 
$edfiOdsCSB = New-Object System.Data.Common.DbConnectionStringBuilder $true
$edfiOdsCSB['Server'] = $sqlServerName
$edfiOdsCSB['Provider'] = $defaultOleDbProvider
$edfiOdsCSB['Database'] = "EdFi_Ods"

$environmentCSBsByDbType = @{  
    # [ODS-433] Required to be restApiDbNameSeed due to client dependencies, rename later to edfiBulkDbNameSeed
    "$restApiDbNameSeed"   = $edfiBulkCSB;
    "$edfiAdminDbNameSeed" = $edfiAdminCSB;
    "$edfiDbNameSeed"      = $edfiOdsCSB;
}
