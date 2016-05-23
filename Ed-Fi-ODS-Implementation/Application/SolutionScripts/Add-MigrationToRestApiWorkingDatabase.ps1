Function global:Add-MigrationToRestApiWorkingDatabase([string]$migrationName) 
{
    $Error.Clear();

    if ([string]::IsNullOrEmpty($migrationName))
    {
        $migrationName = read-host "Migration name: "
    }

    # Make sure the database is clean
    Reset-RestApiWorkingDatabase

	$migrationsProject = "EdFi.Ods.Api.Data"
	$projectToPullConfigFrom = "EdFi.Ods.WebApi"
	$connectionStringName = "BulkOperationDbContext"

    # Generate the migration
    Add-Migration -Name $migrationName -ProjectName $migrationsProject -StartupProjectName $projectToPullConfigFrom -ConnectionStringName $connectionStringName
}
