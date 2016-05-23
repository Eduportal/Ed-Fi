# Relative paths (to this script)
if (-not (get-module |? { $_.Name -eq "path-resolver" })) {
    Import-Module $PSScriptRoot\..\..\..\modules\path-resolver.psm1
}
. "$($folders.modules.invoke('utility\zip.psm1'))"
# Ancestor paths
if (-not ($buildActivitiesDir)) {
    $scriptsDir = Get-AncestorItem (Resolve-path .) "scripts"
    $buildActivitiesDir  = Resolve-Path "$scriptsDir\activities\build"
    $deploymentActivitiesDir  = Resolve-Path "$scriptsDir\activities\deployment"
    $baseDir   = Resolve-Path "$scriptsDir\.."
}

# Imports
Import-Module "$scriptsDir\modules\utility\credential-management.psm1" -force
Import-Module "$scriptsDir\modules\package\get-latest-package.psm1"

# Initialize the primary named credentials needed by this deployment script
Add-CredentialMetadata `
    @(
        @{Name = "Remote Deployer"; DefaultUsername = $null; Description = "Credentials for DOGWOOD for the deploy\$deploymentSourceName folder."; }
    )

Initialize-Credentials $buildActivitiesDir
	
#deployer reads from the build server.
$deployerUsername        = Get-Username             "Remote Deployer"
$deployerPassword        = Get-PlaintextPassword    "Remote Deployer"



#Check for newer WebApp Code   
#This value is used via "dot sourcing" during invocations. 
$operationsUpdated = Get-LatestDeploymentPackage $deployerUsername $deployerPassword "" "Hosting-Operations.$($Env:publishType)" $baseDir
#move this to here so we're not constantly flipping the FWS off and on 
if($operationsUpdated) {
    $fileWatcherServiceName = "File Watcher Windows Service"
    $fileWatcherService = get-service -name $fileWatcherServiceName
    if($fileWatcherService) {
       if($fileWatcherService.Status -ne "Stopped") {
            Stop-Service $fileWatcherServiceName -Force
       }
    }
    
    $confirmDelegate = [ICSharpCode.SharpZipLib.Zip.FastZip+ConfirmOverwriteDelegate] { param($filename); -not($filename.Contains("ICSharpCode.SharpZipLib.dll")) }
	Expand-Zip "$baseDir\Hosting-Operations.zip" $baseDir $null "Prompt" $confirmDelegate
    
    if($fileWatcherService) {
        Start-Service $fileWatcherServiceName
    }
}