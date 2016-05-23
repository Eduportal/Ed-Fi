#Params-
#$deploymentConfiguration: Production,Staging
#$districtName:            <The name of the district the website is being deployed for>
#$shareName:			   <The name of the directory that is shared on this WebApp server for deployments (not the path)>
#$remoteCredentials:	   <A HashTable of variables/values that are credentials for this WebApp instance>
#$customWebSiteName:       <The name of the website on this server to deploy under. Default is "Default Web Site">
Param(
    $deploymentConfiguration, 
    #$districtNames, 
    $specificEntityTypes,
    $shareName, 
    #$leaRemoteCredentials, 
    $credentialsList,
    $folders = $null, 
    $legacyVersionNum, 
    [string[]]$websiteTypes
)
$Script:legacyVersionNum = $legacyVersionNum
#This is the WebApp deployment script that runs on the remote machine.
#It is intended to be run as Invoke-Command -FilePath <deploymentServerPath\this file> etc..
<#
This script:
1) Extracts the WebApp.zip to the specified website location
2) Modifies the extracted config files for the local machine configuration
3) Creates and starts the website/apppools in IIS
#>
Set-ExecutionPolicy -ExecutionPolicy Unrestricted -Scope Process -Force

#For some servers .net 3.5 is missing even if 4.0 is installed.
#The webserver role does not include the .net 3.5 framework by default for WS2K8
if(-not $($(Test-Path $Env:SystemRoot\microsoft.net\Framework\v3.5) -or $(Test-Path $Env:SystemRoot\microsoft.net\Framework64\v3.5))) { 
    #if this is WindowsServer2008 (the most likely scenario) this module path will exist
    if(test-path servermanager) {
        Import-Module Servermanager
        #Add .net3.5 to the role list.
        Add-WindowsFeature NET-Framework-Core
        Remove-Module Servermanager -force
    }
    else {
        Write-Host ".NET 3.5 is not available and the ServerManager module was unavailable. This currently means that deployment will fail. `
        `r`n The installation will need to be performed manually."
    }
}

if (-not ([string]::IsNullOrEmpty($shareName))){
    #Remote execution.
    #Otherwise If a sharename was provided, use it as the source
    [string]$sharePath = Get-WmiObject -class Win32_Share | where {$_.Name -eq $shareName} | % {$_.Path}
    #to get the folders for this runspace.
    $folders = $null
    Import-Module "$sharePath\Ed-Fi-Apps\logistics\scripts\modules\path-resolver.psm1"
    $localDeploymentPath = "$($folders.base.invoke('..\'))"
}
elseif ($folders -ne $null) {
    #If the folders hash table was provided use it. The caller should have already been aware of the paths where this script is invoked.
    #However with the delegates we will still have to tell it to re-import the module (Using one that is NOT a delegate).
    $modulePath = "$($folders.core)\logistics\scripts\modules\path-resolver.psm1"
    Import-Module $modulePath
    $localDeploymentPath = "$($folders.base.invoke('..\'))"
}
else {
    throw 'Either $folders or a $shareName containing the scripts folder must be provided.'
}
#if (-not (Test-Path "$($folders.base)")) { throw "`$folders.base path: $($folders.base) is invalid." }

Import-Module "$($folders.modules.invoke('psake\psake.psm1'))" -force

$psakeScriptPath = "$($folders.activities.invoke('deployment\web\remote-deploy-psake.ps1'))"

foreach ($webSiteType in $webSiteTypes) {
    invoke-psake "$psakeScriptPath" `
        -parameters @{ 
            folders                 = $folders; 
            websiteType             = $webSiteType;
            deploymentConfiguration = $deploymentConfiguration;
            #leaNames                = $districtNames;
            #leaRemoteCredentials    = $leaRemoteCredentials;
            specificEntityTypes     = $specificEntityTypes;
            credentialsList         = $credentialsList;
            localDeploymentPath     = $localDeploymentPath;}
    if ($error) { 
        Write-Host "Error during $webSiteType deployment! `r`n $error"
        throw $error 
    }
}