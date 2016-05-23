# *************************************************************************
# ©2013 Ed-Fi Alliance, LLC. All Rights Reserved.
# *************************************************************************
#MOVE TO BAT - rv * -Exclude args -ea SilentlyContinue; rmo *; $error.Clear(); cls
param(  
[Parameter(Mandatory=$true)]
[String]$pathToMappings,
[String]$leaName,
[String]$mappingToRegenerate,
[switch]$promptForSubType
) 
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
. $scriptDir\..\..\..\modules\utility\build-utility.ps1
Import-Module "$($folders.modules.invoke('psake\psake.psm1'))"

if ("$mappingToRegenerate" -eq "") {
    Write-Host "Enter name of mapping to regenerate (ENTER = All Mappings): " -NoNewLine -Fore Yellow
    [string]$mappingToRegenerate = Read-Host
}
if("$mappingToRegenerate" -ne "" -and $promptForSubType) {
    Write-Host "Enter the TYPE of $mappingToRegenerate mapping to regenerate (ENTER = All Types): " -NoNewLine -Fore Yellow
    [string]$mappingSubType = Read-Host
}

$scriptPath = "$($folders.activities.invoke('adhoc-processes\generate-mapping-src\generate-mapping-src-psake.ps1'))"
$parameters = @{
    folders = $folders;
    leaName = "$leaName";
    mappingSearchDirectory = "$pathToMappings";
    mappingToRegenerate = "$mappingToRegenerate";
    mappingSubType = "$mappingSubType";
}

invoke-psake $scriptPath -parameters $parameters

if ($Error -ne '') {
	write-host "ERROR: $error" -fore RED
	exit $error.Count
}