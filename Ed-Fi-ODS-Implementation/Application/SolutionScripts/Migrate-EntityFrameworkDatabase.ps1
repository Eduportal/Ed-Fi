Function global:Migrate-EntityFrameworkDatabase() {
	Param(
		[Parameter(Mandatory=$True)] [string]$databaseName,
		[switch] [alias('NoCompile')] $DoNotRecompile,
		[switch] [alias('IPromiseIHaveAlreadyBackedUpTheDatabase')] $ForcePushToProduction
	)
	if ($databaseName.ToLower().Contains('prod') -and -not $ForcePushToProduction) {
	  throw "Refusing to migrate production database.  Make sure the database is first backed up, then use the switch '-IPromiseIHaveAlreadyBackedUpTheDatabase' to call this script"
	}
	$databaseSettings = $remoteDeploymentSettings.databases[$databaseName]
	if (-not $databaseSettings) {
		throw "Could not locate production database setting with name $databaseName"
	}
	$VSBuildConfiguration = "Release"
	if (-not $DoNotRecompile) {
		Build-Project $databaseSettings["MigrationProject"] $VSBuildConfiguration
	}

	$configProject = $databaseSettings["ConfigProject"]
	$configFile = $databaseSettings["ConfigFile"]
	$connectionStringName = $databaseSettings["ConnectionStringName"]

	$connStrTokens = Get-ConnectionStringTokensByProject $configProject $configFile $connectionStringName
	
	$parms = @{
		dbTypeNames = $databaseSettings["dbTypeName"]
		databaseName = $connStrTokens.databaseName
		buildConfigurationName = $VSBuildConfiguration
		environment = $databaseSettings["Environment"]
	}

	. $folders.activities.invoke('build/initialize-database/initialize-database.ps1') @parms
	if ($error -or $LASTEXITCODE) { throw $error }
}

