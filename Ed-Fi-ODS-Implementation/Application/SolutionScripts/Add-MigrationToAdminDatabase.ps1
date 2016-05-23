Function global:Add-MigrationToAdminDatabase([string]$migrationName) 
{
    if ([string]::IsNullOrEmpty($migrationName))
    {
        $migrationName = read-host "Migration name: "
    }

    # Make sure the database is clean
    Reset-AdminDatabase

	$migrationsProject = "EdFi.Ods.Admin.Models"
	$projectToPullConfigFrom = "EdFi.Ods.Admin"
	$connectionStringName = "EdFi_Admin"

    # Generate the migration
    Add-Migration -Name $migrationName -ProjectName $migrationsProject -StartupProjectName $projectToPullConfigFrom -ConnectionStringName $connectionStringName
}
