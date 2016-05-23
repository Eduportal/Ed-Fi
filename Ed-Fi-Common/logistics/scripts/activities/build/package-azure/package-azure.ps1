Param(
    [parameter(mandatory=$true)] [string] $ccprojLocation,
    [parameter(mandatory=$true)] [string] $environment,
    $visualStudioVersion,
    [string[]] $tasks = @()
)
$global:erroractionpreference = "stop"
import-module "$PSScriptRoot\..\..\..\modules\path-resolver.psm1"
Import-Module "$($folders.modules.invoke('psake\psake.psm1'))" -force
$psakeScriptPath = "$($folders.activities.invoke('build\package-azure\package-azure-psake.ps1'))"
$psakeParms = @{
    ccprojLocation = resolve-path $ccprojLocation
    folders = $folders
    environment = $environment
}
if ($visualStudioVersion) { $psakeParms.VisualStudioVersion = $visualStudioVersion}
invoke-psake $psakeScriptPath -parameters $psakeParms -tasklist $tasks

if ($Error) {
    write-host "ERROR: $error" -fore RED
    exit $error.Count
}
