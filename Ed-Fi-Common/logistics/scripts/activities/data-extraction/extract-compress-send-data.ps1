# *************************************************************************
# ©2013 Ed-Fi Alliance, LLC. All Rights Reserved.
# *************************************************************************
Param([parameter(Mandatory=$true)]$localEducationAgencyName, $tasksToRun, $publishType)
if (-not (get-module |? { $_.Name -eq "path-resolver" })) {
    Import-Module $PSScriptRoot\..\..\..\modules\path-resolver.psm1
}
Import-Module "$($folders.modules.invoke('psake\psake.psm1'))"

$excludes = @() # Packages to skip execution on for troubleshooting purposes

invoke-psake "$($folders.activities.invoke('data-extraction\extract-compress-send-data-psake.ps1'))" $tasksToRun  `
  -Parameters @{ 
    localEducationAgencyName     = $localEducationAgencyName;
    excludes                     = $excludes;
    folders                      = $folders;
    publishType                  = $publishType;
    }

if ($Error -ne '') {
	write-host "ERROR: $error" -fore RED
	exit $error.Count
}