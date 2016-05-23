# *************************************************************************
# ©2012 Ed-Fi Alliance, LLC. All Rights Reserved.
# *************************************************************************
param(
    [Parameter(Mandatory=$true)]
    [string[]] $dbTypeNames,
    [alias("localEducationAgencyName")]
    [string] $specificEntityName,
    [string] $buildConfigurationName,
    [string] $databaseName,
    [string[]] $tasks = @(),
    [Parameter(Mandatory=$true)] 
    [string] $environment,
    [string] $schoolyear,
    [string] $edfiStandardVersion,
    [string] $databaseServerName
)

if (-not (get-module |? { $_.Name -eq "path-resolver" })) {
    Import-Module $PSScriptRoot\..\..\..\modules\path-resolver.psm1
}
Import-Module "$($folders.modules.invoke('psake\psake.psm1'))"

invoke-psake "$($folders.activities.invoke('build\initialize-database\initialize-database-psake.ps1'))" $tasks `
	-parameters @{ 
		dbTypeNames             = $dbTypeNames; 
		specificEntityName      = "$specificEntityName"; 
		buildConfigurationName  = "$buildConfigurationName"; 
        databaseName            = "$databaseName";
        environment             = "$environment";
        schoolyear              = "$schoolyear";
        folders                 = $folders;
        edfiStandardVersion     = $edfiStandardVersion;
		databaseServerName		= $databaseServerName;
	}

if ($Error -ne '') {
	write-host "ERROR: $error" -fore RED
	exit $error.Count
}