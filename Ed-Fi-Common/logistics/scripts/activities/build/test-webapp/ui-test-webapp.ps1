# *************************************************************************
# ©2013 Ed-Fi Alliance, LLC. All Rights Reserved.
# *************************************************************************
#The deployment configuration of the website that is the target of te UITest.
#Root directory for the test. Uses one level above the base directory by default. (e.g. a peer to Apps/Core) It is expected the test data will be present one level down in a folder
#called "UITesting"
#Overides the images destination folder.
param([Parameter(Mandatory=$true)][string]$deploymentConfiguration,[string]$rootPath,[string]$imagesOutputPath)
if (-not (get-module |? { $_.Name -eq "path-resolver" })) {
    Import-Module $PSScriptRoot\..\..\..\modules\path-resolver.psm1
}

#Get website deployment configs
. $folders.activities.invoke("deployment\$deploymentConfiguration.vars.ps1")
#Add Poke Function
. $folders.modules.Invoke("utility\build-utility.ps1")

if ("$rootPath" -eq "") {
    #Set the root dir one level up from base
    $rootPath = $folders.base.Invoke('..\')
}
#Add the expected directory to the root path.
$rootPath = "$rootPath\UITesting"
if(-not (Test-Path $rootPath)) {
    md $rootPath
}

if ("$imagesOutputPath" -eq "") {
   $imagesOutputPath = "$rootPath\Images" 
}

#Clean and Create images destination folder:
$imagesOutputPath | where {Test-Path $_} | Remove-item -recurse -force
md $imagesOutputPath

#Poke Config Values
Poke-Xml "$rootPath\UITesting.config" "configuration/appSettings/add[@key='serverAddress']/@value" "$deploymentNamePrefix$deploymentNameSuffix"
Poke-Xml "$rootPath\UITesting.config" "configuration/appSettings/add[@key='applicationPath']/@value" "$webAppName"
Poke-Xml "$rootPath\UITesting.config" "configuration/appSettings/add[@key='screenshotImagePath']/@value" "$imagesOutputPath"