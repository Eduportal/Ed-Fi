param(
    [Parameter(Mandatory=$true)] [string] $environment,
    [string[]] $tasks = @()
)
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
if (-not (get-module |? { $_.Name -eq "path-resolver" })) {
    Import-Module $PSScriptRoot\..\..\..\modules\path-resolver.psm1
}
Import-Module $folders.modules.invoke('psake/psake.psm1')

$psakeParameters = @{
    environment = $environment
    folders = $folders
}
$psakeScript = $folders.activities.invoke('build\remove-sandboxen\remove-sandboxen-psake.ps1')
invoke-psake -buildFile $psakeScript -taskList $tasks -parameters $psakeParameters

if ($Error) {
	write-host "ERROR: $error" -fore RED
	exit $error.Count
}
