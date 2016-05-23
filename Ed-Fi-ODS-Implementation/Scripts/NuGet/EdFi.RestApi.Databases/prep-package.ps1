<#
.synopsis
Prepare the EdFi.RestApi.Databases nuspec file for use with 'nuget pack'

.parameter VisualStudioBuildConfig
The VS build configuration that was used to build the various database utilities (the bulk loader, any EntityFramework migrations, etc)

.notes
The format of the EdFi Database Deployment NuGet package: 

Everything we add belongs in the lib\ folder. As we are not a tool intended to be invoked from with Visual Studio, this is correct per the NuGet package spec.

That said, at this time we are not breaking out by .NET version, even for executables and DLLs we place in the lib\ folder. This is acceptable and in line with the spec as well.

Powershell and database scripts belong in a repository-specific folder, and should retain their relative paths to that folder. So, 'Database\Data\EduId' in an Ed-Fi-Apps repository would go in 'lib\Ed-Fi-Apps\Database\Data\EduId'.

Binaries belong in their own folders inside lib. So, the way to include the EdFi.Ods.BulkLoad.Console executables and DLLs is to create a 'lib\EdFi.Ods.BulkLoad.Console' folder.

As only one .nuspec file can be used to generate a given NuGet package, the least generic repository (traditionally called Ed-Fi-Apps) must contain references to all files, even ones in other, less generic repositories (such as Ed-Fi-Core).

This way, Powershell and database scripts can be referenced using the path resolver in a way that permits normal overrides,
while binaries are invoked directly.
#>
param(
    [parameter(mandatory=$true)] $packageName,
    $outputDirectory
)

# Note: this cannot be done in the param block. $PSScriptRoot must not be available at that time.
if (-not $outputDirectory) {
    $outputDirectory = $PSScriptRoot
}

Import-Module "$PSScriptRoot\..\..\..\logistics\scripts\modules\path-resolver.psm1" -force
Import-Module $folders.modules.invoke('packaging') -force
$nuspecPath = "$outputDirectory\$packageName.nuspec"

# Create a Nuspec file with an empty <files> element
New-Nuspec -forceOverwrite -nuspecPath $nuspecPath -id $packageName -description $packageName -authors "Double Line" -owners "Double Line"

# We require the populate template package to be in logistics/bin before we Add-RepositoryFileToNuspec:
. $folders.activities.invoke('build/copy-populatedtemplate/copy-populatedtemplate.ps1')

# Add all of the in-repository files which have a relative path that should be preserved in the target
$repoNuspecFiles = @(
    Select-CumulativeRepositoryResolvedFiles -recurse "logistics/scripts"
    Select-CumulativeRepositoryResolvedFiles -recurse "logistics/bin"
    Select-CumulativeRepositoryResolvedFiles -recurse "Database"
    Select-CumulativeRepositoryResolvedFiles "credentials-*.xml"
)
Add-RepositoryFileToNuspec -nuspecPath $nuspecPath -file $repoNuspecFiles

# Add all of the out of repository files which will specify a custom target path
$nonrepoNuspecFiles = @( 
    @{ source=Select-RepositoryResolvedFiles "Scripts\NuGet\EdFi.RestApi.Databases\PostDeploy.ps1"; target="." }
    @{ source=Select-RepositoryResolvedFiles "Scripts\NuGet\EdFi.RestApi.Databases\chocolateyInstall.ps1"; target="." }
    @{ source=Select-RepositoryResolvedFiles "Scripts\NuGet\EdFi.RestApi.Databases\DLPDeployment.ps1"; target="." }
    @{ source=$SamplesOdsFullPath; target="$SamplesOdsPackageName" }
)
Add-FileToNuspec -nuspecPath $nuspecPath -sourceTargetPair $nonrepoNuspecFiles

