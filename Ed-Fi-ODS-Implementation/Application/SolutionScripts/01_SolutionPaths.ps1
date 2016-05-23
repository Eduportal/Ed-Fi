# requires -version 3
function Find-ParentPathForFile {
    param(
        [parameter(mandatory=$true)] [string] $filename,
        [string] $path = $pwd
    )
    $currentPath = $path
    while($currentPath) {
        $checkPath = Join-Path -Path $currentPath -ChildPath $fileName
        if (Test-Path $checkPath) {
            return $currentPath
        }
        $parent = Split-Path -Path $currentPath -Parent
        $currentPath = $parent
    }
    throw "Could not find file '$fileName' in parents of '$path'"
}

<#
The PathResolverRepositoryOverride variable must contain the names of the folders of each repository used by the project. In this case, we use Ed-Fi-Ods and Ed-Fi-Ods-Implementation. Note that these two repositories must exist in the same parent folders. 
Developers had asked for a way to rename the folder containing the repositories; that's what Find-ParentPathForFile is doing. The Ed-Fi-Common repository name, however, is a non-flexible convention at this time. Note that this convention is consistent what is found on TeamCity and in Octopus.
#>
$ThisRepositoryName = split-path (Find-ParentPathForFile ThisFileMustExistAtTheRepositoryRoot.txt $PSScriptRoot ) -leaf
$env:PathResolverRepositoryOverride = "Ed-Fi-Common;Ed-Fi-Ods;$ThisRepositoryName"
$erroractionpreference = "stop" 

#if (-not (get-module |? { $_.Name -eq "path-resolver" })) {
    Import-Module $PSScriptRoot\..\..\logistics\scripts\modules\path-resolver.psm1 -force
#}

$global:solutionPaths = @{}
$solutionPaths.solutionFile = $folders.base.invoke('Application/Ed-Fi-Ods.sln')
$solutionPaths.src = split-path $solutionPaths.solutionFile
$solutionPaths.repositoryRoot = $folders.base.invoke('')