# *************************************************************************
# ©2013 Ed-Fi Alliance, LLC. All Rights Reserved.
# *************************************************************************
param(
	[string] $environment = "Integration",
    $edfiStandardVersion
)

if (-not (get-module |? { $_.Name -eq "path-resolver" })) {
    Import-Module $PSScriptRoot\..\..\..\modules\path-resolver.psm1
}
Import-Module "$($folders.modules.invoke('psake\psake.psm1'))"

invoke-psake "$($folders.activities.invoke('build\unit-test-etl\unit-test-etl-psake.ps1'))" `
	-parameters @{
        environment             = $environment
        folders                 = $folders
		edfiStandardVersion		= $edfiStandardVersion
	}

if ($Error -ne '') {
	write-host "ERROR: $error" -fore RED
	exit $error.Count
}