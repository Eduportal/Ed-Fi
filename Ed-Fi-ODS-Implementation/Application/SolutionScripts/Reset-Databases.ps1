Function global:Reset-EmptyDatabase() {
	Write-CommandMessage "Reset-EmptyDatabase"
	$initdbArgs = @{
		dbTypeNames = "EdFi"
		environment = "Development"
		databaseName = "EdFi_Ods_Empty"
	}
	. $folders.activities.invoke('build/initialize-database/initialize-database.ps1') @initdbArgs
	if ($error) {
		throw "$($error.count) errors"
	}
}

# [ODS-433] Required to be Rest_Api due to client dependencies, rename later to EdFi_Bulk
function global:Reset-RestApiWorkingDatabase() {
	param(
		[switch] [alias('NoCompile')] $DoNotRecompile
	)

	Write-CommandMessage "Reset-BulkWorkingDatabase"
	$parms = @{
		# [ODS-433] Required to be Rest_Api due to client dependencies on folder name, rename later to EdFi_Bulk
		dbTypeNames = "Rest_Api"
		databaseName = "EdFi_Bulk"
		#buildConfigurationName = "Debug"
		environment = "Development"
		tasks = @("default","ApplyEntityFrameworkMigrations")
	}

	if (-not $DoNotRecompile) {
		Write-RunMessage "Building EdFi.Ods.Api.Data"
		Build-Project "EdFi.Ods.Api.Data" #$parms.buildConfigurationName
	}

	. $folders.activities.invoke('build/initialize-database/initialize-database.ps1') @parms
	if ($error) { throw $error }
}


Function global:Reset-EmptyTemplateDatabase() {
	Write-CommandMessage "Reset-EmptyTemplateDatabase"

	$initdbArgs = @{
		dbTypeNames = "EdFi"
		environment = "Development"
		databaseName = "EdFi_Ods_Empty_Template"
	}
	. $folders.activities.invoke('build/initialize-database/initialize-database.ps1') @initdbArgs
	if ($error) {
		throw "$($error.count) errors"
	}
}

function global:Compile-LoaderExecutables {
	Write-RunMessage "Compiling Loader executables..."
	Build-Project -projectName EdFi.Ods.BulkLoad.Console -buildConfiguration $localDeploymentSettings.BulkLoadConsole.BuildConfiguration
}

Function global:Reset-MinimalTemplateDatabase() {
	param(
		[switch] [alias('NoCompile')] $DontRecompileLoaderExecutables
	)
	if (-not $DontRecompileLoaderExecutables) {
		Compile-LoaderExecutables
	}
	Write-RunMessage "Applying types and descriptors..."

	$initdbArgs = @{
		dbTypeNames = "EdFi"
		environment = "Development"
		databaseName = "EdFi_Ods_Minimal_Template"
		tasks = @("default","ApplyDescriptors","ApplyEdOrgs")
	}
	. $folders.activities.invoke('build/initialize-database/initialize-database.ps1') @initdbArgs
	if ($error) {
		write-host $error
		throw "$($error.count) errors"
	}
}

Function global:Reset-SharedInstanceDatabase() {
	param(
		[switch] [alias('NoCompile')] $DontRecompileLoaderExecutables,
		[string] $SchoolYear = "2014"
	)

	if (-not $DontRecompileLoaderExecutables) {
		Compile-LoaderExecutables
	}
	Write-RunMessage "Applying types and descriptors..."

	$DatabaseName = "EdFi_Ods_" + $SchoolYear

	$initdbArgs = @{
		dbTypeNames = "EdFi"
		environment = "Development"
		databaseName = $DatabaseName
		tasks = @("default","ApplyDescriptors")
	}
	. $folders.activities.invoke('build/initialize-database/initialize-database.ps1') @initdbArgs
	if ($error) {
		write-host $error
		throw "$($error.count) errors"
	}
}

Function global:Reset-SecurityDatabase() {
	param(
		[switch] [alias('NoCompile')] $DoNotRecompile,
		[string] $DatabaseName = "EdFi_Security"
	)

	Write-CommandMessage "Reset-SecurityDatabase ($DatabaseName)"

	$parms = @{
		dbTypeName = "EdFiSecurity";
		DatabaseName = $DatabaseName;
		environment = "Development"
		tasks = @("default","ApplyEntityFrameworkMigrations")
	}

	if (-not $DoNotRecompile) {
		Build-Project "EdFi.Ods.Security.Metadata" $localDeploymentSettings.BulkLoadConsole
	}

	. $folders.activities.invoke('build/initialize-database/initialize-database.ps1') @parms
	if ($error) { throw $error }
}


Function global:Reset-AdminDatabase() {
	param(
		[switch] [alias('NoCompile')] $DoNotRecompile,
		[string] $DatabaseName = "EdFi_Admin"
	)

	Write-CommandMessage "Reset-AdminDatabase ($DatabaseName)"

	$parms = @{
		dbTypeName = "edfiAdmin";
		DatabaseName = $DatabaseName;
		#BuildConfiguration = "Development";
		environment = "Development"
		tasks = @("default","ApplyEntityFrameworkMigrations")
	}

	if (-not $DoNotRecompile) {
		Build-Project "EdFi.Ods.Admin.Models" $localDeploymentSettings.BulkLoadConsole
	}

	. $folders.activities.invoke('build/initialize-database/initialize-database.ps1') @parms
	if ($error) { throw $error }
}


Function global:Reset-SsoIntegrationDatabase() {
	Write-CommandMessage "Reset-SsoIntegrationDatabase"

	$parms = @{
		dbTypeName = "SSO_Integration"
		environment = "Development"
		specificEntityName = " "
		#buildConfigurationName = "Development"
		databaseName = "SSO_Integration"
	}
	. $folders.activities.invoke('build/initialize-database/initialize-database.ps1') @parms
	if ($error) { throw $error }
}


<#
#	This function should be updated to build a populated demo database when that process is defined
#>
Function global:Reset-PopulatedDatabases() {
	Write-CommandMessage "Reset-PopulatedDatabases"

	. $folders.activities.invoke('build/copy-populatedtemplate/copy-populatedtemplate.ps1')
	$initdbArgs = @{
		dbTypeNames = "EdFi"
		environment = "Development"
		databaseName = "EdFi_Ods_Populated_Template"
		tasks = @("RestorePopulatedDbTemplate")
	}
	. $folders.activities.invoke('build/initialize-database/initialize-database.ps1') @initdbArgs
	if ($error) {
		write-host $error
		throw "$($error.count) errors"
	}
	
	$initdbArgs = @{
		dbTypeNames = "EdFi"
		environment = "Development"
		databaseName = "EdFi_Ods"
		tasks = @("RestorePopulatedDbTemplate")
	}
	. $folders.activities.invoke('build/initialize-database/initialize-database.ps1') @initdbArgs
	if ($error) {
		write-host $error
		throw "$($error.count) errors"
	}

}

Function global:Reset-EduIdDatabase() {
<#  
EduId database is unique in that it is built from a Visual Studio SQL Server Database Project, instead of being
restored from a BAK file. So the project must be built prior to its deployment, whereas in all other databases, we are deploying
databases first and then building and publishing websites afterwards. So creating a custom step here which does both, i.e. invokes
MS build to create the project, and then deploy via scripts (not using dapac to deploy, because we need data also)
#>
<#
	#refactor this script to get database settings from DeploymentSettings script
	Write-CommandMessage "Reset-EduIdDatabaseForLocalDevOnly"
	$projectFile =  Select-RepositoryResolvedFiles "Database\Structure\EduId.Database\EduId.Database.sqlproj"
	$serverName = "(local)" 
	$DatabaseName = "EduID_Db"
	$connectionStringTokens = "EduIdContext"
	$localConnectionString = Get-SqlConnectionString $serverName "master"

	Write-RunMessage "Removing database [$DatabaseName]..."
    Remove-Database $serverName $DatabaseName
	Invoke-MsBuild "--% /t:rebuild,deploy /p:TargetDatabase=$DatabaseName /p:TargetConnectionString=`"$localConnectionString`" `"$projectFile`" "
	Write-CommandMessage "Reset-EduId"
	$initdbArgs = @{
		dbTypeNames = "EduId", "UniqueId_Integration"
		environment = "Development"
		databaseName = "EduId_Db"
		tasks = @("default")
	}
	. $folders.activities.invoke('build/initialize-database/initialize-database.ps1') @initdbArgs
	if ($error) {
		throw "$($error.count) errors"
	}
#>
}
