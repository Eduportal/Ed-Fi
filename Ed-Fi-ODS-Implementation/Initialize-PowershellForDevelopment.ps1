<#
This file emulates the SolutionScripts NuGet package, but it works outside of the NuGet Package Manager console in Visual Studio
#>

$SolutionScriptsDir = resolve-path "$(split-path $MyInvocation.MyCommand.path)\Application\SolutionScripts"
$NuGetPackagesDir = resolve-path "$(split-path $MyInvocation.MyCommand.path)\Application\packages"

function Get-SortableVersionText([string]$nugetFolderName) {
    # Extract the numeric values from the name, and pad each to the left to 10 places
    $versionParts = ([regex]"\.([0-9]+)").Matches("$nugetFolderName") | % { "{0:X10}" -f [int] $_.Groups[1].Value }; 
    
    # Create padding to the right for 4 versions
    $paddedVersionParts = 1..4 | % { "{0:X10}" -f 0 } 
    
    # Concatenate the arrays and generate a 4-part version, with 0 values for missing version parts
    [string]::Join(".", $versionParts + $paddedVersionParts).Substring(0, 43)
}
function Get-LatestNugetPackagePath([string]$nugetPackageBaseName) {
    gci $NuGetPackagesDir -filter "$nugetPackageBaseName*"  | 
        select -Property @(@{Name="Path";Expression={$_.FullName}}, @{Name="Version";Expression={(Get-SortableVersionText $_.Name)}}) | 
        sort Version -Descending | 
        select -first 1 -expand Path
}

write-host "Solution Scripts Console" -foregroundcolor Magenta
$SolutionScriptsPackage = Get-LatestNugetPackagePath SolutionScripts
<#try { #>
    . $SolutionScriptsPackage\tools\init.ps1 -package $NuGetPackagesDir 
<#}
catch {
    throw "Could not find SolutionScripts NuGet package. Launch Visual Studio and check NuGet packages, and re-run this script."
}#>

