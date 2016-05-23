# *************************************************************************
# ©2014 Ed-Fi Alliance, LLC. All Rights Reserved.
# *************************************************************************

# this script is should not be being used any more,  it's left here only for legacy purposes.

# you really should be using the initialize-database.ps1 file.  however there's one catch.
# initialize-database does not have a dropAppDb flag.  
#  if you want your database type to be dropped then you should add it into the $environment.vars file
#  like so:  $dbTypeTransient += @("Application")
#also it should be noted here that when using initialize-database it asks for specificEntityName, 
# you can still give it the array like you do for specificEntityTypeNames and everything will work ok.

param(
    [Parameter(Mandatory=$true)] [string] $environment,

    #For environments with more than one build configuration.
    [Parameter(Mandatory=$true)] [string] $deploymentConfiguration,

    [alias("localEducationAgencies")] [string[]]$specificEntityTypeNames,

    [boolean]$dropAppDb = $false
)

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
if (-not (get-module |? { $_.Name -eq "path-resolver" })) {
    Import-Module $PSScriptRoot\..\..\..\modules\path-resolver.psm1
}

. "$($folders.activities.invoke(`"build\$environment.vars.ps1`"))"
#This needs to come after the build vars so that the applicationDbName Returned is correct.
. "$($folders.activities.invoke(`"deployment\$deploymentConfiguration.vars.ps1`"))"


if ($dropAppDb) { $dbTypeTransient += @("Application") }

Import-Module "$($folders.modules.invoke('psake\psake.psm1'))"

Invoke-psake "$($folders.activities.invoke('build\initialize-database\initialize-database-psake.ps1'))" `
  -parameters @{
	dbTypeNames = @("Application");
	databaseName = $applicationDbName; #from the $deploymentConfiguration.vars file
	specificEntitySubTypeNames = ($specificEntityTypeNames | % { $_.Replace(" ","")});
	buildConfigurationName = " ";
    environment = $environment;
}


if ($Error -ne '') {
	write-host "ERROR: $error" -fore RED
	exit $error.Count
}