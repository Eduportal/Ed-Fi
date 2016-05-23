$etlUnitProjectId = 1
$dbTypeNames = @("EdFi","Dashboard","DashboardDW","Application","Acceptance")
$edfiStandardVersionDb = if ($edfiStandardVersion) {"$($edfiStandardVersion.Replace('.',''))"}
$testExecutionDatabaseName = Get-ProjectSpecificDatabaseName $appTypePrefix "" "Acceptance" $edfiStandardVersionDb -versionSuffix "$legacyVersionNum"

#Dynamically find the etl folders to pass across all repos.
#This logic is also in load-database-psake and might should be considered for a move to a module.
#Or at least a standardized location for using ETLUnit.
$packagePaths = Select-CumulativeRepositoryResolvedFiles "ETL\" | select -ExpandProperty FullName | Split-Path | select -Unique
$orderedPackageSources = Format-ArrayByRepositories $packagePaths
$packagePathArgs = ""
foreach ($packageSource in $orderedPackageSources) {
    $packagePathArgs += "$packageSource;"
}
$packagePathArgs = $packagePathArgs.Trim(";")
$etlUnitPath = $packagePathArgs