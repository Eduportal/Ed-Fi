# *************************************************************************
# ©2013 Ed-Fi Alliance, LLC. All Rights Reserved.
# *************************************************************************
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
if (-not (get-module |? { $_.Name -eq "path-resolver" })) {
    Import-Module $PSScriptRoot\..\..\..\modules\path-resolver.psm1
}
Import-Module "$($folders.modules.invoke('psake\psake.psm1'))"
$psakeScriptPath = "$($folders.activities.invoke('build\compile\compile-webapp-psake.ps1'))"
#set environment var for nuget to work without pre-configuration:
$Env:EnableNuGetPackageRestore = $true
invoke-psake "$psakeScriptPath" `
	-parameters @{ 
		folders        = $folders; 
	}

if ($Error -ne '') {
	write-host "ERROR: $error" -fore RED
	exit $error.Count
}