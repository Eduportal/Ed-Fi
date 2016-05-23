<#
.Synopsis
Resolve paths across specified repositories

.Description
Resolve paths across specified repositories. 

A folder anchor is used to allow resolution across repositories. For overrides to work, the relative folder paths for the override and the core implementation must be the same. 

.Parameter repositoryNames
The RepositoryNames argument has a default value of Ed-Fi-Core and Ed-Fi-Apps. The value can be overridden by either passing the repository names when the module is imported or by specifying and environment variable 'PathResolverRepositoryOverride'. If both are specified, the repositories passed directly to the module during import have preeminence. The format for the environment variable is semicolon separated values.

Important: Names of the repositories do not matter when they are specified. However the order is important. Repositories will be interpreted with the first repository name as the most general or 'core' and the last repository specified as the most implementation specific.

NOTE: When passing in repository names to Import-Module with -ArgumentList, some non-obvious syntax is required. In short, it must be invoke like this: 

    import-module path-resolve.psm1 -ArgumentList @(,@('root1','root2','rootN'))

The $repositoryNames parameter is an array, and -ArgumentList expects an array; in order for -ArgumentList to interpret the $repositoryNames parameter as a single argument, rather than expand it as a list of multiple positional arguments, it must be passed as an array within an array. 
#>
param(
    [parameter(position=0)] [string[]] $repositoryNames
)

#Resolve if overrides or defaults are required
if ([string]::IsNullOrWhiteSpace($repositoryNames)) {
    if ([string]::IsNullOrWhiteSpace($env:PathResolverRepositoryOverride)) {
        $repositoryNames = @("Ed-Fi-Core","Ed-Fi-Apps")
    }
    else {
        Write-Host "Using repositories from environment variable: $env:PathResolverRepositoryOverride"
        $repositoryNames = @($env:PathResolverRepositoryOverride.Split(";"))
    }
}

#Create new object in memory (not just a pointer)
$invertedRepositoryNames = $repositoryNames.Clone()
#Invert the values
[Array]::Reverse($invertedRepositoryNames)


#Works up the $path until it finds the first incidence of the $itemName in the $path.
#Then trims the end of the $path off at that point and returns the path. (does include $itemName)
#in the value returned.
function Get-AncestorItemPath ([string]$path, [string]$itemName) {
    #Remove Case Sensitivity
    $path = $path.ToLower()
    $itemName = $itemName.ToLower()
    
    $pos = $path.LastIndexOf("\$itemName\")
    
    if ($pos -lt 0) {
        if ($path.EndsWith("\$itemName")) {
            $pos = $path.LastIndexOf("\$itemName")
        } else {
            throw "Unable to locate '$itemName' in path '$path'."
        }
    }
    
    $newPath = $path.Substring(0, $pos) + "\$itemName"
    return (Resolve-Path $newPath)
}


function Get-RootPath {
    #Look up from this script's location to find the highest level common named folder.
    $logisticsBasePath = Get-AncestorItemPath $PSScriptRoot "logistics"
    #Jump up two levels to get the root.
    return Resolve-Path "$logisticsBasePath\..\..\"
}

#Using the Root path for the repositories, return back the path for the $repositoryName specified.
function Get-RootBasedRepositoryPath ([string]$repositoryName) {
    $root = Get-RootPath
    $repositoryPath = "$($root)$repositoryName"
    
    If (-not (Test-Path $repositoryPath)) {
        throw ("$repositoryName path not found under $($root)!")
    }
    return $repositoryPath
}

<#
.synopsis
Get the root of the specified repository.
.parameter RepositoryName
The name(s) of the repositories to find the root of.
If this is not specified, find the roots of all repositories in the path resolver.
#>
function Get-RepositoryRoot {
    [cmdletbinding()]
    param(
        [string[]]$repositoryName
    )
    if (-not $repositoryName) {
        $repositoryName = $repositoryNames
    }
    foreach ($repo in $repositoryName) {
        if (-not ($repositoryNames -contains $repo)) {
            throw "Tried to get the root of a repository named $repo, but the path resolver doesn't know about a repository with that name."
        }
        write-verbose "Getting repository root for repository: $repo"
        Get-RootBasedRepositoryPath $repo
    }
}
function Get-RepositoryNames {
    return $script:repositoryNames
}

#Knowing that core is in the first position of the repositoryNames provided 
#Get the root based repository path for the repository name that is in that position.
function Get-CoreBasePath {
    return Get-RootBasedRepositoryPath $repositoryNames[0]
}

<#
.Synopsis
Return paths to specified files, checking all repositories, and including only the most specific version of each file. 

.Description
Return paths to specified files, checking all repositories, and including only the most specific version of each file. 

For example, in a system with three repositories, laid out like this: 

    \Ed-Fi-Core
        \1234.txt
        \qwer.txt
    \Ed-Fi-Plugins
        \qwer.txt
        \asdf.txt
    \Ed-Fi-Apps
        \asdf.txt
        \zxcv.txt

Seraching for zxcv.txt or asdf.txt returns the version in Ed-Fi-Apps; searching for qwer.txt returns the version in Ed-Fi-Plugins, and searching for 1234.txt returns the version in Ed-Fi-Core. 

.Parameter PathSuffix
The path suffix to append to the repository path. E.g. if $pathSuffix is 'logistics', find files in <most specific repository>\logistics.
#>
function Get-RepositoryResolvedPath ([string]$pathSuffix) {
    foreach ($repositoryName in $invertedRepositoryNames) {
        $repositoryPath = Get-RootBasedRepositoryPath $repositoryName
        If (test-path "$repositoryPath\$pathSuffix") {
            return Resolve-Path "$repositoryPath\$pathSuffix"
        }
    }
    $errorMsg = "$pathSuffix was not found in the following repositories:`r`n{0}" -f ($invertedRepositoryNames -join "`r`n")
	throw ($errorMsg)
}

<#
.Synopsis
Return the most general repository path with the specified path suffix.

.Description 
Return the most general repository path with the specified path suffix. As long as there are more than 1 repositories in $repositoryNames, do not look in the least general repository at all.

For example, if there are three repositories: Ed-Fi-Core, Ed-Fi-Plugins, and Ed-Fi-Apps, look first in Ed-Fi-Core, and then in Ed-Fi-Plugins, for the specified path suffix. Ignore the path in Ed-Fi-Apps even if it does exist.

.Notes 
This is useful when dot-sourcing or importing core scripts in an extension, so that the core functionality can be inherited if it exists, but it will not cause an infinite loop by attempting to import itself.

.Parameter PathSuffix
The path suffix to append to the repository path. E.g. if $pathSuffix is 'logistics', find files in <most general repository>\logistics.
#>
function Get-CorePath ([string]$pathSuffix) {
    #This has been switched to a dynamic lookup so that not every "core" repo has to have every folder.
    #Just give a single repo a pass on this check.
    if ($repositoryNames.Count -gt 1) {
        $implementationRepo = ($repositoryNames | select -Last 1)
    }
    else { 
        Write-Host "Warning: Core path lookups are not typically used in single repository implementations."
        $implementationRepo = [string]::Empty 
    }
    $checkedRepos = @()
    foreach ($repositoryName in $repositoryNames) {
        #skip the implementation repo. It is not ever going to be "core".
        if ($repositoryName -eq $implementationRepo) { continue; }
        $checkedRepos += $repositoryName
        $repositoryPath = Get-RootBasedRepositoryPath $repositoryName
        If (test-path "$repositoryPath\$pathSuffix") {
            return Resolve-Path "$repositoryPath\$pathSuffix"
        }
    }
    $errorMsg = "$pathSuffix was not found in the following repositories:`r`n{0}" -f ($checkedRepos -join "`r`n")
	throw ($errorMsg)
}


<#
.Synopsis
Select files in the repositories. Use files recursively from Apps to Core. 

.Description 
Iterate over all the repository paths, going from most specific to least. If the path exists with the specified path suffix select all the files listed that have not been resolved in a previous repository. Apply the filter to the file listing, aggregate, and return the result.

NOTE: The indexing utilized looks at the relative paths from the repository root when determining if a file has previously been selected.

.Parameter PathSuffix
The path suffix to append to the repository path. E.g. if $pathSuffix is 'logistics', find files in '<repository roots>\logistics'. 

.Parameter Filter
Use a Get-ChildItem when selecting files, such as '*.sql'.

.Parameter Recurse
Select files recursively.
#>
function Select-RepositoryResolvedFiles {
    param(
        [string] $pathSuffix, 
        [string] $filter,
        [switch] $recurse
    )
    $output = @()
    $namesTypes = @{}
    #Using inverted listing so core goes last.
    foreach ($repositoryName in $invertedRepositoryNames) {
        $outputHolder = @()
        $repositoryPath = Get-RootBasedRepositoryPath $repositoryName
        If (Test-Path "$repositoryPath\$pathSuffix") {
            $repositoryResolvedPath = Resolve-Path "$repositoryPath\$pathSuffix"
            #Select all the files in the path that match the pattern where they are not listed in the index
            $outputHolder += gci -recurse:$recurse $repositoryResolvedPath -filter $filter | where {if (($namesTypes.Keys -contains ($_.FullName -Replace [regex]::Escape("$repositoryPath"),"")) -and ($namesTypes[($_.FullName -Replace [regex]::Escape("$repositoryPath"),"")] -eq $_.GetType())) {$false} else {$true} }
            #Add new files to index
            $outputHolder | % { $namesTypes.Add(($_.FullName -Replace [regex]::Escape("$repositoryPath"),""),$_.GetType()) }
            #Add new files to output
            $output += $outputHolder
        }
    }
    return $output
}


<#
.Synopsis
Select files from all repositories. Return all files that match.

.Description 
Iterate over all the repository paths, if the path exists with the specified path suffix, apply the filter to the file listing, aggregate, and return the result.

.Parameter PathSuffix
The path suffix to append to the repository path. E.g. if $pathSuffix is 'logistics', find files in 'C:\path\to\Ed-Fi-Core\logistics'. 

.Parameter Filter
Use a Get-ChildItem filter when selecting files, such as '*.sql'.

.Parameter Recurse
Select files recursively.
#>
function Select-CumulativeRepositoryResolvedFiles {
    param(
        [string] $pathSuffix, 
        [string] $filter,
        [switch] $recurse
    )
    $output = @()
    foreach ($repositoryName in $repositoryNames) {
        $repositoryPath = Get-RootBasedRepositoryPath $repositoryName
        If (Test-Path "$repositoryPath\$pathSuffix") {
            $repositoryResolvedPath = Resolve-Path "$repositoryPath\$pathSuffix"
            #Select all the files in the path that match the pattern
            $output += gci -recurse:$recurse $repositoryResolvedPath -filter $filter 
        }
    }
    return $output
}

#Sets a global folders variable. Is the primary access mechanism for resolving paths. Utilizes a delegate.
if (-not ($script:folders)) {
    $script:folders = @{}
    #This is used in remote web deployments to find this path resolver and CANNOT be a delegate.
    $folders.core = Get-CoreBasePath
    $folders.scripts = [System.Func[Object,Object]]{ return (Get-RepositoryResolvedPath "logistics\scripts\$($args[0])") }
    $folders.base = [System.Func[Object,Object]]{ return (Get-RepositoryResolvedPath "$($args[0])") }
    $folders.tools = [System.Func[Object,Object]]{ return (Get-RepositoryResolvedPath "tools\$($args[0])") }
    $folders.modules = [System.Func[Object,Object]]{ return (Get-RepositoryResolvedPath "logistics\scripts\modules\$($args[0])") }
    $folders.activities = [System.Func[Object,Object]]{ return (Get-RepositoryResolvedPath "logistics\scripts\activities\$($args[0])") }
}

#Set aliases to maintain previous functionality
Set-Alias -Name Select-EdFiFiles -Value Select-RepositoryResolvedFiles -Description "Legacy passthru for Ed-Fi script calls"
Set-Alias -Name Select-EdFiDatabaseFiles -Value Select-CumulativeRepositoryResolvedFiles -Description "Legacy passthru for Ed-Fi script calls"
Set-Alias -Name Select-CumaltiveRepositoryResolvedFiles -Value Select-CumulativeRepositoryResolvedFiles -Description "Correct spelling error in previous version"

$exportFunction = @(
    'Select-RepositoryResolvedFiles'
    'Select-CumulativeRepositoryResolvedFiles'
    'Get-CorePath'
    'Get-RepositoryResolvedPath'
    'Get-RepositoryRoot'
    'Get-RepositoryNames'
)
$exportVariable = @(
    "folders"
)
$exportAlias = @(
    'Select-EdFiFiles'
    'Select-EdFiDatabaseFiles'
    'Select-CumaltiveRepositoryResolvedFiles'
)
Export-ModuleMember -Function $exportFunction -variable $exportVariable -alias $exportAlias
