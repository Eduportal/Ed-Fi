if (-not (get-module |? { $_.Name -eq "path-resolver" })) {
    Import-Module $PSScriptRoot\..\path-resolver.psm1
}
import-module $folders.modules.invoke('utility/build-utility.psm1')

function New-Nuspec {
    param(
        [parameter(mandatory=$true)] $nuspecPath,
        [parameter(mandatory=$true)] [string] $id,
        [parameter(mandatory=$true)] [string] $description,

        [string] $version = "0.0.0",
        [string] $title,
        [string] $authors,
        [string] $owners,
        [bool] $requireLicenseAcceptance = $false,
        [string] $releaseNotes,
        [string] $copyright = "Copyright $((get-date).year)",
        [hashtable] $dependencies,
        [switch] $forceOverwrite
    )
    if (test-path $nuspecPath) {
        if (-not $forceOverwrite) {
            throw "Nuspec file at '$nuspecPath' already exists"
        }
        else {
            rm $nuspecPath
        }
    }
    '<?xml version="1.0"?>' | out-file $nuspecPath
    $nuspecPath = resolve-path $nuspecPath
    [xml]$xml = Get-Content $nuspecPath

    $package = $xml.AppendChild($xml.CreateElement("package")) 
    $metadata = $package.AppendChild($xml.CreateElement("metadata")) 
    $metadata.AppendChild($xml.CreateElement("id")).AppendChild($xml.CreateTextNode("$id")) | out-null
    $metadata.AppendChild($xml.CreateElement("version")).AppendChild($xml.CreateTextNode("$version")) | out-null
    $metadata.AppendChild($xml.CreateElement("title")).AppendChild($xml.CreateTextNode("$title")) | out-null
    $metadata.AppendChild($xml.CreateElement("authors")).AppendChild($xml.CreateTextNode("$authors")) | out-null
    $metadata.AppendChild($xml.CreateElement("owners")).AppendChild($xml.CreateTextNode("$owners")) | out-null
    $metadata.AppendChild($xml.CreateElement("requireLicenseAcceptance")).AppendChild($xml.CreateTextNode("$requireLicenseAcceptance".tolower())) | out-null
    $metadata.AppendChild($xml.CreateElement("description")).AppendChild($xml.CreateTextNode("$description")) | out-null
    $metadata.AppendChild($xml.CreateElement("releaseNotes")).AppendChild($xml.CreateTextNode("$releaseNotes")) | out-null
    $metadata.AppendChild($xml.CreateElement("copyright")).AppendChild($xml.CreateTextNode("$copyright")) | out-null
    $metadata.AppendChild($xml.CreateElement("tags")).AppendChild($xml.CreateTextNode("$tags")) | out-null
    if ($dependencies) {
        $metaDependencies = $metadata.AppendChild($xml.CreateElement("dependencies"))
        $dependenciesGroup = $metaDependencies.AppendChild($xml.CreateElement("group"))
        foreach ($packageVersionPair in $dependencies.GetEnumerator()) {
            $dependency = $dependenciesGroup.AppendChild($xml.CreateElement("dependency"))
            $dependencyAttribute = $xml.CreateAttribute("$($packageVersionPair.Key)")
            $dependencyAttribute.Value = "$($packageVersionPair.Value)"
            $dependency.Attibutes.Append($dependencyAttribute) | Out-Null 
        }
    }
    $package.AppendChild($xml.CreateElement("files")) | out-null

    $xml.Save($nuspecPath)
}

<#
.synopsis
Include a file in a .nuspec NuGet package specification
.parameter SourceTargetPair
An array of hashtables, where each hashtable must contain a .source and a .target attribute. 

The .target attribute is the relative path within the NuGet package, and might look like "lib/net45/" or "lib/net45/thing.dll".

The .source attribute is an array containing one or more references to files (whether FileInfo objects, relative path strings, or absolute path strings). DirectoryInfo objects and paths to directories are ignored. 
#>
function Add-FileToNuspec {
    param(
        [parameter(mandatory=$true)] [hashtable[]] [ValidateScript({
            if ([bool]$_.target -and [bool]$_.source) { return $true }
            else { write-host "SourceTargetPair entry failed validation: @{ source = $($_.source); target = $($_.target) } "; return $false }
            })] $sourceTargetPair,
        [parameter(mandatory=$true)] $nuspecPath    
    )
    $nuspecPath = resolve-path $nuspecPath
    [xml]$xml = Get-Content $nuspecPath

    $filesElem = $xml.GetElementsByTagName('files')[0]
    foreach ($pair in $sourceTargetPair) {
        $resolvedTarget = $pair.target -replace '/','\' -replace '\\\\','\'
        foreach ($source in $pair.source) {
            if ((get-item $source).gettype() -match "DirectoryInfo") {
                continue
            }
            $resolvedSource = resolve-path $source

            $newFile = $xml.CreateElement("file")
            $newFile.SetAttribute('src',$resolvedSource) 
            $newFile.SetAttribute('target',$resolvedTarget)

            Write-Verbose "Adding new XML element for file '$resolvedPath'"
            $filesElem.AppendChild($newFile) | out-null
        }
    }

    $xml.Save("$nuspecPath")        
}

<#
.synopsis
Add a file from one of the $repositoryNames to a Nuspec package specification
#>
function Add-RepositoryFileToNuspec {
    param(
        [parameter(mandatory=$true)] [Object[]] $filePath,
        [parameter(mandatory=$true)] $nuspecPath    
    )

    $sourceTargetPair = @()
    foreach ($file in $filePath) {
        if ($file.GetType().name -match '(File|Directory)Info$') {
            $fullname = $file.fullname
        }
        else {
            $fullname = resolve-path $file
        }
        $foundRepoRoot = $false
        foreach ($repoRoot in Get-RepositoryRoot) {
            $repoName = split-path -leaf $repoRoot

            $escRegex = [Regex]::Escape("$($repoRoot.tolower())\")
            if ($fullname -match "^$escRegex")  { 
                $relPath = $fullname -replace $escRegex,''
                $sourceTargetPair += @(@{ source = $fullname; target = "$repoName\$relPath"})
                $foundRepoRoot = $true
                break
            }
        }
        if (-not $foundRepoRoot) {
            throw "File '$($artifact.fullname)' does not appear to be in one of the repository roots."
        }
    }
    Add-FileToNuspec -nuspecPath $nuspecPath -sourceTargetPair $sourceTargetPair
}

<#
    .synopsis 
    Get sortable version text from a string
    .description
    NuGet packages names typicaly include a version string at the end. For example: MyPackage.1.0.0.0.nupkg
    This scheme works great until version 1.0.0.9 gets incremented to 1.0.0.10. 
    This function returns a string containing just the version number, which each field padded to ten digits. 
#>
Function Get-SortableVersionText([string]$nugetPackageName) {
    # Extract the numeric values from the name, and pad each to the left to 10 places
    $versionParts = ([regex]"\.([0-9]+)").Matches("$nugetPackageName") | % { "{0:X10}" -f [int] $_.Groups[1].Value }; 
    
    # Create padding to the right for 4 versions
    $paddedVersionParts = 1..4 | % { "{0:X10}" -f 0 } 
    
    # Concatenate the arrays and generate a 4-part version, with 0 values for missing version parts
    [string]::Join(".", $versionParts + $paddedVersionParts).Substring(0, 43)
}

<#
    .synopsis
    Given a nuget package name, find the most recent version installed in the package directory
    .parameter NuGetPackageName
    Find nuget packages with this base name.
    .parameter PackageDir
    Find packages in this directory. Use either a solution's "packages" directory, or "C:\Chocolatey\lib".

#>
Function Get-LatestNugetPackagePath {
    param(
        [parameter(mandatory=$true)] [string] $nugetPackageName,
        [parameter(mandatory=$true)] [string] $packageDir
    )
    if (-not (test-path $packageDir)) {
        throw "No such package directory '$packageDir'"
    }
    $allMatches = gci $packageDir -filter "$nugetPackageName*"
    $allVersionedMatches = $allMatches | select -Property @("FullName", @{Name="Version";Expression={(Get-SortableVersionText $_.Name)}}) 
    $latestVersion = $allVersionedMatches | sort Version -Descending | select -first 1 -expand FullName
    return $latestVersion
}

Function Get-VersionedNugetPackagePath {
    param(
        [parameter(mandatory=$true)] [string] $nugetPackageName,
        [parameter(mandatory=$true)] [string] $packageDir,
		[parameter(mandatory=$true)] [string] $version
    )
    if (-not (test-path $packageDir)) {
        throw "No such package directory '$packageDir'"
    }
    $allMatches = gci $packageDir -filter "$nugetPackageName*"
    $specificMatch = $allMatches | ?{$_.Name -like "*.$version"}
	if ($specificMatch.Count -gt 1) { throw "Too many matches. $specificMatch" }
	if ($specificMatch.Count -lt 1) { throw "No such version $version of $nugetPackageName found in $packageDir $allMatches" }
	
	return $specificMatch.FullName
	
}

<#
.synopsis
Return the best guess for the location of the Chocolatey install directory.
.description
Return the best guess for the location of the Chocolatey install directory.

The %ChocolateyInstall% environment variable is not always present. This function checks for this variable, but if it isn't present, it also looks in other well-known places.
#>
function Get-ChocolateyInstallDir {
    $possibleChoco = @(
        "$env:ChocolateyInstall"
        "$env:ProgramData\Chocolatey"
        "$env:SystemDrive\Chocolatey"
        "C:\Chocolatey"
    )
    $Chocolatey = $null
    foreach ($location in $possibleChoco) {
        if ($location -and (test-path $location)) {
            $Chocolatey = $location
        }
    }
    if ($Chocolatey) {
        return $Chocolatey
    }
    else {
        throw "Could not find Chocolatey install directory."
    }
}

<#
.synopsis
Invoke a Chocolatey executable.
.description
Invoke a Chocolatey executable. Can invoke Chocolatey itself, or any executable from the $env:ChocolateyInstall\bin directory.
#>
function Invoke-ChocolateyExecutable {
    [cmdletbinding()]
    param(
        [string] $command = "chocolatey",
        [array] $argumentList
    )
    $chocoDir = Get-ChocolateyInstallDir
    $commandBase = "$chocoDir\bin\${command}"
    foreach ($potentialCommand in @("$commandBase.exe","$commandBase.bat","$commandBase")) {
        if (test-path $potentialCommand) {
            $commandPath = resolve-path $potentialCommand
            break
        }
    }
    Invoke-ProcessAndWait -command $commandPath -argumentList $argumentList 
}

$exportFunction = @(
    'New-Nuspec'
    'Add-FileToNuspec'
    'Add-RepositoryFileToNuspec'
    'Get-LatestNugetPackagePath'
    'Get-ChocolateyInstallDir'
    'Invoke-ChocolateyExecutable'
)
Export-ModuleMember -Function $exportFunction 
