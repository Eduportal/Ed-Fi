param(
    [string] $PromotionEnvironment = $OctopusParameters["PromotionEnvironment"],
    [string] $PathResolverRepositoryOverride = $OctopusParameters["PathResolverRepositoryOverride"],
    [string] $CertPrefix = $OctopusParameters["CertPrefix"],
    [string] $InstallType = $OctopusParameters["InstallType"],
	[switch] $WhatIf
)

foreach ($var in "PromotionEnvironment","PathResolverRepositoryOverride","CertPrefix","InstallType") {
    if (-not (test-path variable:/$var)) {
        throw "Missing Octopus parameter '$var'. This script should be run from an Octopus Tentacle. Either it is being run from outside of a Tentacle, or Octopus is not correctly configured."
    }
}

. $PSScriptRoot\DLPDeployment.ps1 -PromotionEnvironment $PromotionEnvironment -PathResolverRepositoryOverride $PathResolverRepositoryOverride -CertPrefix $CertPrefix -InstallType $InstallType
