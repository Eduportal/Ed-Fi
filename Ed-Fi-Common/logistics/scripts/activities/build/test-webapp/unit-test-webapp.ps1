# *************************************************************************
# ©2013 Ed-Fi Alliance, LLC. All Rights Reserved.
# *************************************************************************
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
if (-not (get-module |? { $_.Name -eq "path-resolver" })) {
    Import-Module $PSScriptRoot\..\..\..\modules\path-resolver.psm1
}
Import-Module "$($folders.modules.invoke('psake\psake.psm1'))"
$psakeScriptPath = "$($folders.activities.invoke('build\test-webapp\test-webapp-core-psake.ps1'))"
invoke-psake "$psakeScriptPath" `
	-parameters @{ 
		folders        = $folders; 
        testType       = "UnitTesting";
        testFilter     = "*.Tests.dll"
	}

if ($Error -ne '') {
	write-host "ERROR: $error" -fore RED
	exit $error.Count
}