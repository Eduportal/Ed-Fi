#this should be imported after the deployment specific configuration file.
$websiteName = "$deploymentNamePrefix$deploymentNameSuffix"
$loadUserProfile = $true
$deploymentPackageName = "$($deploymentPackageNameRoot).zip"
$deploymentProjectName = "Edfi.Dashboards.Presentation.Web"
$applyHostHeader = if("$deploymentNamePrefix" -eq "") {$false} else {$true}

Function Set-WebsiteCustomConfigurations {
    Assert-Variables "artifactPath", "deploymentWebsite", "topRightCornerLegendText", "appTypePrefix", "isWebAppDeployment"
    
    if($isWebAppDeployment) {
        $stsUrl  = "https://$deploymentNamePrefix$deploymentNameSuffix/$($webAppName)_STS"
        $siteUrl = "https://$deploymentNamePrefix$deploymentNameSuffix/$webAppName"
    }
    else {
        $stsUrl  = "https://$($deploymentNamePrefix)STS.$deploymentNameSuffix"
        $siteUrl = "https://$deploymentNamePrefix$deploymentNameSuffix"
    }
    
    $artifactSpecificFolderName = if($isWebAppDeployment) {"$($deploymentWebsite.name)-$webAppName"} else {"$($deploymentWebsite.name)"} 
    
    # Modify app settings to insert appropriate data state message.
    Poke-Xml $artifactPath\Web.config "/configuration/appSettings/add[@key='TopRightCornerLegend']/@value" "$topRightCornerLegendText" 

    #Using LocalApplicationData path for persisted repository.
    $repositoryPath = [Environment]::GetFolderPath("CommonApplicationData") + "\$appTypePrefix\$artifactSpecificFolderName"
    if(-not (Test-Path $repositoryPath)) {
        md $repositoryPath | Out-Null
    }
    # Modify app settings to point to the appropriate directory
    Poke-Xml $artifactPath\Web.config "/configuration/appSettings/add[@key='PersistedRepositoryDirectory']/@value" "$repositoryPath" 
            
    $tempChartDir = "$env:SystemDrive\Temp\AspNetCharting\$artifactSpecificFolderName"
    if(Test-Path $tempChartDir) {
        Remove-Item $tempChartDir -Recurse -Force
    }
    md $tempChartDir | Out-Null
    Add-DirectoryPermissions "$tempChartDir" "IIS_IUSRS" "Modify"
    # Modify app settings to seperate chart caching directories
    Poke-Xml $artifactPath\Web.config    "/configuration/appSettings/add[@key='ChartImageHandler']/@value" "storage=file;timeout=20;dir=$tempChartDir\;deleteAfterServicing=false;"

    Poke-Xml $artifactPath\Web.config    "/configuration/microsoft.identityModel/service/audienceUris/add/@value"                               "$siteUrl"
    Poke-Xml $artifactPath\Web.config    "/configuration/microsoft.identityModel/service/federatedAuthentication/wsFederation/@realm"           "$siteUrl"
    Poke-Xml $artifactPath\Web.config    "/configuration/microsoft.identityModel/service/federatedAuthentication/wsFederation/@issuer"          "$stsUrl"
    Poke-Xml $artifactPath\Web.config    "/configuration/microsoft.identityModel/service/federatedAuthentication/wsFederation/@requireHttps"    "$siteIsSSL"
    Poke-Xml $artifactPath\Web.config    "/configuration/microsoft.identityModel/service/federatedAuthentication/cookieHandler/@requireSsl"     "$siteIsSSL"
    Poke-Xml $artifactPath\Web.config    "/configuration/microsoft.identityModel/service/issuerNameRegistry/trustedIssuers/add/@name"           "$stsUrl"
    #Poke in DW Db connection info
    
    
    
    if ($environmentCSBsByDbType -and $environmentCSBsByDbType.$dashboardDwDbNameSeed)
    {
    $environmentCSBsByDbType.$dashboardDwDbNameSeed.add("Uid", "$sqlUser")
    $environmentCSBsByDbType.$dashboardDwDbNameSeed.add("Pwd", "$sqlPass")
    $environmentCSBsByDbType.$dashboardDwDbNameSeed.add("ApplicationIntent", "ReadOnly")
    
    $dashboardDwDbNewCSB = Get-SqlConnectionString $environmentCSBsByDbType.$dashboardDwDbNameSeed
    Poke-Xml $artifactPath\Web.config    "/configuration/connectionStrings/add[@name='DataWarehouse']/@connectionString" $dashboardDwDbNewCSB
    }
    else
    {
     Poke-Xml $artifactPath\Web.config    "/configuration/connectionStrings/add[@name='DataWarehouse']/@connectionString" "Data Source=$dashboardDwSqlServerName;Database=$dashboardDwDbName;User Id=$sqlUser;Password=$sqlPass;"   
    }
    
    
}
