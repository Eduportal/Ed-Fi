# *************************************************************************
# ©2013 Ed-Fi Alliance, LLC. All Rights Reserved.
# *************************************************************************
param(
	[string[]] $dbTypeNames,
	[alias("localEducationAgencyName")] [string] $specificEntityType,
	[string] $buildConfigurationName,
    [string] $databaseName,
    [string[]] $tasks = @(),
    [Parameter(Mandatory=$true)] [string] $environment,
    [string] $schoolyear,
    [string] $edfiStandardVersion
)

if (-not (get-module |? { $_.Name -eq "path-resolver" })) {
    Import-Module $PSScriptRoot\..\..\..\modules\path-resolver.psm1
}
Import-Module "$($folders.modules.invoke('psake\psake.psm1'))"

#remove old initialize database for testing task.
#If the function is still needed it should be put under the acceptance database type.
if ($tasks -Contains "InitializeDatabaseForTesting") {
    Write-Host "Removing 'InitializeDatabaseForTesting' task. If the function is still needed, it should be added as a script under the Database\Data\Acceptance database type."
    $newTasks =@()
    foreach ($task in $tasks) {
     if ($task -ne"InitializeDatabaseForTesting") {
        $newTasks += $task
     }
    }
    $tasks = $newTasks
}
#Create database has been fully deprecated. It no longer runs scripts in the correct order. 
invoke-psake "$($folders.activities.invoke('build\initialize-database\initialize-database-psake.ps1'))" $tasks `
	-parameters @{ 
		dbTypeNames             = $dbTypeNames; 
		specificEntityType = "$specificEntityType"; 
		buildConfigurationName  = "$buildConfigurationName"; 
        databaseName            = "$databaseName";
        environment             = "$environment";
        schoolyear              = "$schoolyear";
        folders                 = $folders;
        edfiStandardVersion     = $edfiStandardVersion;
	}

if ($Error -ne '' -and $Error[0].Message -eq `
	"Windows PowerShell Workflow is not supported in a Windows PowerShell x86-based console. Open a Windows PowerShell x64-based console, and then try again.")
{ 
	$Error.Clear();
}
else
{
	write-host "ERROR: $error" -fore RED
	exit $error.Count
}