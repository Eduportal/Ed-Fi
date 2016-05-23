# *************************************************************************
# ©2013 Ed-Fi Alliance, LLC. All Rights Reserved.
# *************************************************************************
#Parms:
# $webAppServerName - The Web-App server to whence the deployment shall take place
# $webAppShareName  - The name of the shared folder to deploy to on the Web-App server.
# $deploymentConfiguration - The configuration of the deployment to take place (Production, Staging)
# $specificEntityTypes    - The name of the districts for which the deployment is occuring
# $ldapEntries - names used to identify LDAP credentialsd that need to be loaded 
Param(
    [string[]] $webAppServerNames,
    $webAppShareName = "",
    $deploymentConfiguration,
    [alias("districtNames")] [string[]] $specificEntityTypes,
    [string[]]$ldapEntries,
    [string]$publishType = ""
)

if (-not (get-module |? { $_.Name -eq "path-resolver" })) {
    Import-Module $PSScriptRoot\..\..\..\modules\path-resolver.psm1
}
Import-Module "$($folders.modules.invoke('psake\psake.psm1'))"
. "$($folders.activities.invoke(`"deployment\$deploymentConfiguration.vars.ps1`"))"
$psakeScriptPath = "$($folders.activities.invoke('deployment\web\invoke-remote-deploy-psake.ps1'))"

# Backwards compatibility for existing environments: 
# Former versions of this script didn't have an $ldapEntries argument, and ldap connection strings
# were automaticaly generated based on $districtNames (which is the former name of $specificEntityTypes)
# To specify no $ldapEntries, just pass an empty array.
if ($ldapEntries -eq $null) {
    $ldapEntries = $specificEntityTypes
}

foreach ($webAppServerName in $webAppServerNames) {
    $parametersToPass = @{
        folders                 = $folders;
        webAppServerName        = $webAppServerName;
        webAppShareName         = $webAppShareName;
        deploymentConfiguration = $deploymentConfiguration;
        specificEntityTypes     = [string[]]$specificEntityTypes;
        ldapEntries             = $ldapEntries;
        publishType             = $publishType;
    }
    #Assume that generally the latest is already provided.
    #$preformGetLatest is in the deploymentconfiguration variables

    #Default is if the server we're on is not the server we're deploying to then we should setup the remote structures
    $preformSetup = ($env:ComputerName -ne $webAppServerName)
     if ($serverSetupExclusions -ne $null) {
        #unless we have explicitly specified we should not (eg your deploying to "localhost" which doesn't match the computername)
         foreach ($server in $serverSetupExclusions) {
            if ($server -eq $webAppServerName) {$preformSetup = $false}
         }
     }
    if($preformSetup -and ([String]::IsNullOrEmpty($webAppShareName))) { Throw 'Remote Setup is indicated but no $webAppShareName was provided!'}
    if ($preformGetLatest -and $preformSetup){
        invoke-psake "$psakeScriptPath" -parameters $parametersToPass -taskList GetLatestSetupDeploy
    }
    elseif ($preformSetup) {
        invoke-psake "$psakeScriptPath" -parameters $parametersToPass -taskList RemoteSetup,Deploy
    }
    elseif ($preformGetLatest) {
        invoke-psake "$psakeScriptPath" -parameters $parametersToPass -taskList GetLatestPackage,Deploy
    }
    else {
        invoke-psake "$psakeScriptPath" -parameters $parametersToPass
    }

    if ($Error -ne '') {
        #Reset this if there is an error when we are in a deployment config that gets latest, the file exists and we were trying to deploy.
        $versionFilePath = "$($folders.base.invoke(''))Hosting-WebApp-Version.txt"
        if ((Test-Path $versionFilePath) -and $global:shouldDeploy) {
            Write-Host "Removing version file to allow for re-deployment..."
            Remove-Item $versionFilePath -Force
        }
        write-host "ERROR: $error" -fore RED
        exit $error.Count
    }
    #Don't keep going if we don't have anything to deploy.
    if (-not $global:shouldDeploy) {break}
    #Only get lastest on the first iteration
    $preformGetLatest = $false
}