. (Get-CorePath "logistics\scripts\activities\build\initialize-database\initialize-database.vars.ps1")
# Database types which have EntityFramework migrations must be in this hashtable, which has a format of "dbTypeName = EFAssemblyName"
# Note that a database type may have EF migrations in addition to EdFi database-migrations.psm1 migrations.
$EFMigrationLibraryNames = @{
    EdfiAdmin = "EdFi.Ods.Admin.Models"
    Rest_Api  = "EdFi.Ods.Api.Data"
	EdFiSecurity = "EdFi.Ods.Security.Metadata"
}

