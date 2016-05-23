# *************************************************************************
# ©2013 Ed-Fi Alliance, LLC. All Rights Reserved.
# *************************************************************************
param(
    #DbTypeNames should match the connection string names contained in the packages
    #The connection strings are pointed to the correct type of database based on this convention.
	[string[]] $dbTypeNames = @("EdFi","Dashboard"),
    
    [Parameter(Mandatory=$true)]
    [alias("localEducationAgencyName")]
	[string] $specificEntityType,
    
    [Parameter(Mandatory=$true)]
	[string] $buildConfigurationName,
    
    [Parameter(Mandatory=$true)]
	[string] $environment,
    
    [string[]] $tasks = @(),
    # etlUnitSkipPattern: set this to "EdFi.Etl.DW" to disable skipping
    # it is a comma-delimited string. 
    [string] $etlUnitSkipPattern=$null,
    [Parameter(Mandatory=$false)]
	[string] $schoolyear
)

if (-not (get-module |? { $_.Name -eq "path-resolver" })) {
    Import-Module $PSScriptRoot\..\..\..\modules\path-resolver.psm1
}
Import-Module "$($folders.modules.invoke('psake\psake.psm1'))"

invoke-psake "$($folders.activities.invoke('build\load-database\load-database-psake.ps1'))" $tasks `
	-parameters @{ 
		dbTypeNames = $dbTypeNames
        specificEntityType = $specificEntityType
        buildConfigurationName = $buildConfigurationName
        environment = $environment
        schoolyear = $schoolyear
        folders = $folders
        etlUnitSkipPattern = $etlUnitSkipPattern
	}

if ($Error -ne '') {
	write-host "ERROR: $error" -fore RED
	exit $error.Count
}