import-module $PSScriptRoot\..\path-resolver.psm1
import-module $folders.modules.invoke('utility\build-utility.psm1')

<#
NOTE: We would like to pull this back out and make it a real set of functions in this module. 
#>
set-alias Publish-CloudService $PSScriptRoot\PublishCloudService.ps1


<#
    .synopsis 
    Create an Azure package
    .description 
    Use MSBuild to create an Azure package. 
    Optionally poke variables into the web.config files before the package is created. 
    (Note that, because Azure packages are signed, the web.config files cannot be changed after the package is created.)
    .parameter BuildConfiguration
    The Visual Studio build configuration to use when building
    .parameter TargetProfile
    The profile to use when building
    .parameter projectFile
    The .projectFile file to build
    .parameter VisualStudioVersion
    The version of visual studio to pass to MSBuild. Leave blank to allow MSBuild to use its own default.
#>
function New-AzurePackage {
    [cmdletbinding()]
    param(
        #[parameter(mandatory=$true)] [validateset('local','dev','prod','test','demo')] $TargetEnv,
        [parameter(mandatory=$true)] $BuildConfiguration,
        [parameter(mandatory=$true)] $projectFile,
        $TargetProfile,
        $visualStudioVersion,
        $LoggingProvider
    )
    $projectFile = resolve-path $projectFile
    $MSBuildDir = (Get-ItemProperty -Path "Registry::HKLM\Software\Microsoft\.NETFramework" -Name InstallRoot).InstallRoot + '\v4.0.30319'
    $MSBuildExe = resolve-path "$MSBuildDir\msbuild.exe"
    write-verbose "Using msbuild at $MSBuildExe"
    $MSBuildArgs = @("$projectFile", "/t:Publish", "/p:Configuration=$BuildConfiguration")
    if ($TargetProfile) {
        $MSBuildArgs += @("/p:TargetProfile=$TargetProfile")
    }
    if ($visualStudioVersion) { 
        $MSBuildArgs += @("/p:VisualStudioVersion=$visualStudioVersion") 
    }
    if ($loggingProvider) {
        $MSBuildArgs += @("/l:$LoggingProvider")
    }
    write-verbose "Running MSBuild: $MSBuildExe $MSBuildArgs"
    Invoke-ProcessAndWait -command $MSBuildExe -argumentList $MSBuildArgs -CheckExitCode
}

export-modulemember -alias Publish-CloudService -function New-AzurePackage