$env:certPrefix = "Development"
$global:remoteDeploymentSettings = @{}

$remoteDeploymentSettings.databases = @{
	"DevAdmin" = @{
		"ConfigProject" = "EdFi.Ods.Admin.Web";
		"ConfigFile" = "Web.AzureDev.config";
		"ConnectionStringName" = "EdFi_Admin";
		"MigrationProject" = "EdFi.Ods.Admin.Models";
		"DbTypeName" = "EdFi_Admin";
		"Environment" = "SandboxDev";
	}
	
	"DevEdFiBulk" = @{
		"ConfigProject" = "EdFi.Ods.Api.Data";
		"ConfigFile" = "Web.AzureDev.config";
		"ConnectionStringName" = "BulkOperationDbContext";
		"MigrationProject" = "EdFi.Ods.Api.Data";
		# [ODS-433] Required to be Rest_Api due to client dependencies on folder name, rename later to EdFi_Bulk
		"DbTypeName" = "Rest_Api";
		"Environment" = "SandboxDev";
	}

	"ProductionEdFiBulk" = @{
		"ConfigProject" = "EdFi.Ods.WebApi";
		"ConfigFile" = "Web.AzureProd.config";
		"ConnectionStringName" = "BulkOperationDbContext";
		"MigrationProject" = "EdFi.Ods.Api.Data";
		# [ODS-433] Required to be Rest_Api due to client dependencies on folder name, rename later to EdFi_Bulk
		"DbTypeName" = "Rest_Api";
		"Environment" = "SandboxProd";
	}

	"LocalAdmin" = @{
		"ConfigProject" = "EdFi.Ods.Admin";
		"ConfigFile" = "Web.LocalTest.config";
		"ConnectionStringName" = "EdFi_Admin";
		"MigrationProject" = "EdFi.Ods.Admin.Models";
		"DbTypeName" = "EdFi_Admin"
	}

	"ProductionAdmin" = @{
		"ConfigProject" = "EdFi.Ods.Admin";
		"ConfigFile" = "Web.AzureProd.config";
		"ConnectionStringName" = "EdFi_Admin";
		"MigrationProject" = "EdFi.Ods.Admin.Models";
		"DbTypeName" = "EdFi_Admin"
		"Environment" = "SandboxProd";
	}
}



$global:localDeploymentSettings = @{}

$localDeploymentSettings.baseDeployPath = "C:\inetpub\EdFiRest.Test"
$localDeploymentSettings.solution = $solutionPaths.solutionFile
$localDeploymentSettings.LogisticsBin = "$($folders.base.invoke('logistics'))/bin"

$localDeploymentSettings.IisConfigurations = @{
	"Apps" = @{"Port" = "8088"; "AppPool" = "EdFiRest"; "WebSiteName" = "EdFiRest"};
	"SwaggerUI" = @{"Port" = "8089"; "AppPool" = "EdFiRest"; "WebSiteName" = "SwaggerUI"};
}

$localDeploymentSettings.QueueGroups = @{
	"BulkUploads" = @("DeadLetter", "CommitUploadCommand", "StartOperationCommand")
}

$localDeploymentSettings.applications = @{
	"Admin" = @{
		"Name"="Admin";
		"Project"="EdFi_Ods_Admin_Web";
		"AppUrl" = "Admin/TestUsers/Initialize";
		"IIS" = "Apps";
		"PublishProfile" = "Debug"
		"PostDeployment" = "Ping-Site Admin"
		DeployOrderIndex = 0
	};
	
	"SwaggerUI" = @{
		"Name"="SwaggerUI";
		"Project"="EdFi_Ods_SwaggerUI";
		#"AppUrl"="SwaggerUI/SwaggerUI/";
		"IIS" = "SwaggerUI";
		"PublishProfile" = "Debug"
	};
	
	"WebApi-Empty" = @{
		"Name"="WebApi-Empty";
		"Project"="EdFi_Ods_WebApi";
		"IIS" = "Apps";
		"PublishProfile" = "Debug";
		"FileSystemQueues" = @{
			"BaseDirectory" = "MessagingQueueData";
			"Queues" = $localDeploymentSettings.QueueGroups.BulkUploads;
		};
	}
}

$localDeploymentSettings.databases = @{
	"EmptyOds" = @{
		"Name" = "Empty ODS";
		"AppConfigProject" = "EdFi.Ods.WebService.Tests";
		"ConnectionStringName" = "EdFi_Ods_Empty";
		"DatabaseName" = "EdFi_Ods_Empty";
	};

	"EdFiBulkWorkingDatabase" = @{
		"Name" = "Empty Bulk";
		"AppConfigProject" = "EdFi.Ods.WebApi";
		"ConnectionStringName" = "EdFi_Ods";
		"DatabaseName" = "EdFi_Bulk";
	};

	"EmptyOdsTemplate" = @{
		"Name" = "Empty ODS Template";
		"AppConfigProject" = "EdFi.Ods.WebApi";
		"ConnectionStringName" = "EdFi_Ods";
		"DatabaseName" = "EdFi_Ods_Empty_Template"
	};

	#"MinimalODSTemplate" = @{
	#	"Name" = "ODS With Types and Descriptors, for SQL Azure template";
	#	"AppConfigProject" = "EdFi.Ods.WebApi";
	#	"ConnectionStringName" = "EdFi_Ods";
	#	"DatabaseName" = "EdFi_Ods_Minimal_Template";
	#	"ApplyTypesAndDescriptors" = "true"
	#};

	"PopulatedOds" = @{
		"Name" = "Populated ODS";
		"AppConfigProject" = "EdFi.Ods.WebApi";
		"ConnectionStringName" = "EdFi_master";
		"CustomDatabaseDeployment" = "Reset-PopulatedDatabases";
		"RestoredDatabaseNames" = @("EdFi_Ods", "EdFi_Ods_Populated_Template") #this will include "EdFi_Ods" after we have a way to create a demo database
	};

	"Admin" = @{
		"Name" = "Backing database for Admin site";
		"AppConfigProject" = "EdFi.Ods.Admin.Test";
		"MigrationProjectName" = "EdFi.Ods.Admin.Models";
		"ConnectionStringName" = "EdFi_Admin";
		"DatabaseName" = "EdFi_Admin";
	};
}

$localDeploymentSettings.databaseLogins = @{
	"edfiLoader" = @{
		"AppConfigProject" = "EdFi.Ods.Admin.Web";
		"AppConfigName" = "web.config";
		"ConnectionStringName" = "EdFi_master";
		"ServerRoles" = @("dbcreator")
	};
	"edfiAdmin" = @{
		"AppConfigProject" = "EdFi.Ods.Admin.Web";
		"AppConfigName" = "web.config";
		"ConnectionStringName" = "EdFi_Admin";
		"ServerRoles" = @()
	};
	"edfiSecurity" = @{
		"AppConfigProject" = "EdFi.Ods.Admin.Web";
		"AppConfigName" = "web.config";
		"ConnectionStringName" = "EdFi_Security";
		"ServerRoles" = @()
	};
}

$localDeploymentSettings.databaseUsers = @{
	"edfiLoader" = @{
		"Login" = "edfiLoader";
		"ServerRoles" = @("db_datawriter","db_datareader","db_ddladmin")
	};
}

$localDeploymentSettings.BulkLoadConsole = @{
	"ProjectName" = "EdFi.Ods.BulkLoad.Console";
	"BuildConfiguration" = "Debug";
	"EXE" = "EdFi.Ods.BulkLoad.Console.exe";
	"SourceFolder" = "$($solutionPaths.repositoryRoot)\Database\Data\EdFi\Descriptors";
	"Manifest" = "$($solutionPaths.repositoryRoot)\Database\Data\EdFi\Descriptors\Manifest.xml";
}

$msbuildPath2012 = "C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild.EXE" 
$msbuildPath2013 = "C:\Program Files (x86)\MSBuild\12.0\bin\MSBuild.exe" 

if (Test-Path $msbuildPath2013) {
	$global:msbuildEXE = $msbuildPath2013
} else {
	$global:msbuildEXE = $msbuildPath2012
}

# Find the highest version of TransformText.exe available
$global:t4EXE = dir TextTransform.EXE -path "C:\Program Files (x86)\Common Files\microsoft shared\" -recurse | sort FullName -Descending | % {$_.FullName} | select -first 1

$global:AllEncryptConfigSections = @(
    @{ relpath = 'EdFi.Ods.Admin.Web\Web.config'; type = "base"; sections = @("connectionStrings")}
    @{ relpath = 'EdFi.Ods.WebApi\Web.config'; type='base'; sections=@('connectionStrings')}
    @{ relpath = 'EdFi.Ods.BulkLoad.Console\App.config'; type = "base"; sections = @("connectionStrings") }
)
foreach ($item in $AllEncryptConfigSections) {
	$item.fullpath = (get-item $folders.base.invoke("Application\$($item.relpath)")).fullname
}

function global:Get-ApplicationWebAddress([string] $appName) {
	Assert-AppNameExists $appName

	$app = $localDeploymentSettings.applications[$appName]
	$url = $appName
	if ($app.ContainsKey("AppUrl")) {
		$url = $app["AppUrl"]
	}

	$iisSettings = $localDeploymentSettings.IisConfigurations[$app["IIS"]]

	return "http://$($env:COMPUTERNAME):$($iisSettings["Port"])/$url"
}

function global:Get-ApplicationPhysicalAddress([string] $appName) {
	Assert-AppNameExists $appName
	
	return "$($localDeploymentSettings.baseDeployPath)\$appName"
}

function global:Assert-AppNameExists([string] $appName) {
	$foundKey = $localDeploymentSettings.applications.ContainsKey($appName)
	if (-not $foundKey) {
		throw "Cannot find application with name [$appName].  Please pick from [$($localDeploymentSettings.applications.Keys)]"
	}
}

function global:Assert-DatabaseSettingsExist([string] $name) {
	$foundKey = $localDeploymentSettings.databases.ContainsKey($name)
	if (-not $foundKey) {
		throw "Cannot find database with name [$name].  Please pick from [$($localDeploymentSettings.databases.Keys)]"
	}
}

function global:Assert-DatabaseLoginExists([string] $name) {
	$foundKey = $localDeploymentSettings.databaseLogins.ContainsKey($name)
	if (-not $foundKey) {
		throw "Cannot find database login with name [$name].  Please pick from [$($localDeploymentSettings.databaseLogins.Keys)]"
	}
}

function global:Assert-DatabaseUserExists([string] $name) {
	$foundKey = $localDeploymentSettings.databaseUsers.ContainsKey($name)
	if (-not $foundKey) {
		throw "Cannot find database user with name [$name].  Please pick from [$($localDeploymentSettings.databaseUsers.Keys)]"
	}
}

function global:Get-DatabaseSettings([string]$databaseName) {
	Assert-DatabaseSettingsExist $databaseName
	return $localDeploymentSettings.databases[$databaseName]
}

function global:Get-DatabaseUserInformation([string]$databaseUser) {
	Assert-DatabaseUserExists $databaseUser
	return $localDeploymentSettings.databaseUsers[$databaseUser]
}

function global:Get-DatabaseLoginInformation([string]$loginName) {
	Assert-DatabaseLoginExists $loginName
	return $localDeploymentSettings.databaseLogins[$loginName]
}

function global:Write-DeploymentInfo() {
	Param([string]$appName = "")

	if ($appName.Length -gt 0) {
		Write-Host 
		Write-Host "Application [$appName] at:"
		Write-Host "     Physical Path: $(Get-ApplicationPhysicalAddress($appName))"
		Write-Host "     Web Address:   $(Get-ApplicationWebAddress($appName))"
	} else {
		foreach($key in $localDeploymentSettings.applications.Keys) {
			Write-DeploymentInfo($key)
		}
	}
}