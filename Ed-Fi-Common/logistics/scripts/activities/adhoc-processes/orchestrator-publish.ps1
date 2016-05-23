#Please make sure to set the artifact path for this Build Configuration to: EtlPublish\*.zip =>
#Please ensure that TeamCity is providing env.TeamCityAgentWorkDir set to: %system.teamcity.build.checkoutDir%
#Also, please make sure to establish an apporporiate clean up policy or published packages may consume too much storage or be cleaned up (deleted) prematurely.
Param([string]$orchestratorServerName)

$sharePath = "\\$orchestratorServerName\EtlPublish\"
$ErrorActionPreference = "Stop"
if (-not (Test-Path $env:TeamCityAgentWorkDir)) {
	throw "Team City Agent Working Directory: `r`n $env:TeamCityAgentWorkDir `r`nwas not valid!"
}

#Refresh Destination Dir
$agentEtlPublish = "$env:TeamCityAgentWorkDir\EtlPublish\"
if (Test-Path $agentEtlPublish) {
	rm -r $agentEtlPublish
}
md $agentEtlPublish

if (-not (Test-Path $sharePath)) {
	throw "Promotion Share Path: `r`n $sharePath `r`nwas not valid!"
}

#Find the archive:
$archive = @()
$archive += dir $sharePath -filter "*.zip"
if ($archive.Count -gt 1) {
	throw "More than one zip found for promotion. Aborting."
}
if ($archive.Count -eq 0) {
	throw "No zip file found."
}

$archive | Move-Item -Destination $agentEtlPublish
$archiveName = Split-Path -Leaf $archive[0]
Write-Host "Archive: $archiveName `r`nPublished Successfully."