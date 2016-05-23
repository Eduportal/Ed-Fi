#this should be imported after the deployment specific configuration file.
if($isWebAppDeployment) {
    $websiteName = "$deploymentNamePrefix$deploymentNameSuffix"
    $webAppName = "$($webAppName)_STS"
} 
else {
    $websiteName = "$($deploymentNamePrefix)STS.$deploymentNameSuffix"
}
$loadUserProfile = $false
$deploymentPackageName = "$($deploymentPackageNameRoot)-STS.zip"
$deploymentProjectName = "Edfi.Dashboards.SecurityTokenService.Web"
$applyHostHeader = $true

Function Set-WebsiteSETSpecificCustomConfigurations {
    Assert-Variables "normSpecificEntityType", "artifactPath"
    
    #region Web Config Pokes
    if (($ldapUser -ne $null) -and ($ldapPass -ne $null)) {
        # Modify app settings to include the approporiate ldapUser
        Poke-Xml $artifactPath\Web.config "/configuration/ldapConfiguration/directories/add[@name='$normSpecificEntityType']/@appUsername" "$ldapUser"
        Poke-Xml $artifactPath\Web.config "/configuration/ldapConfiguration/directories/add[@name='$normSpecificEntityType']/@appPassword" "$ldapPass"
    }
}