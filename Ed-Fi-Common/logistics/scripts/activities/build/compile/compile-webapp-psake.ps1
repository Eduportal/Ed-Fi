# *************************************************************************
# ©2013 Ed-Fi Alliance, LLC. All Rights Reserved.
# *************************************************************************
properties {
	$solutionName          = "Edfi.sln"
    $solutionPath          = "$($folders.base.invoke(`"application\$solutionName`"))"
	$buildDirectory        = "$(Split-Path $solutionPath -Parent)\build"

    $buildWebsiteDirectory = "$buildDirectory\_PublishedWebsites\Edfi.Dashboards.Presentation.Web"
    $buildStsDirectory     = "$buildDirectory\_PublishedWebsites\Edfi.Dashboards.SecurityTokenService.Web"
    $msbuildPath           = (Get-ItemProperty (Get-Item HKLM:\Software\Microsoft\MSBuild\ToolsVersions\4.0).PSPath).MSBuildToolsPath + "msbuild.exe"
    $unitTestFilter        = "*.Tests.dll"
    $uiTestFilter          = "*.UITests.dll"
    $testRegEx             = ".+\.(?!zip)"
    Import-Module "$($folders.modules.invoke('utility\zip.psm1'))"
}

task default -depends Build

task Build -depends Compile, PackageTests, PackageWebApp

task Clean { 
    if (Test-Path $buildDirectory) {
    remove-item -force -recurse $buildDirectory -ErrorAction SilentlyContinue | Out-Null }
}

task Init -depends Clean { 
    new-item $buildDirectory -itemType directory | Out-Null
}

task Compile -depends Init { 
    Write-Host "Starting compilation on $env:ComputerName"
    $teamCityLoggingPath = "$($env:teamcity_dotnet_nunitlauncher_msbuild_task)"
	#Below, setting the VisualStudioVersion=10.0 forces us to build with VS2010 .target files, even if the file has been upgraded to 2012 format.
	#  I learned of this solution from http://blogs.msdn.com/b/webdev/archive/2012/08/22/visual-studio-project-compatability-and-visualstudioversion.aspx
	#  When we switch to copying over the VS2012 .target files instead of the 2010 target files, remove this property.
    if ($teamCityLoggingPath -ne "") {
        &$msbuildPath /verbosity:minimal /l:"JetBrains.BuildServer.MSBuildLoggers.MSBuildLogger,$teamCityLoggingPath;Verbosity=minimal" /p:"SolutionDir=$(Split-Path $solutionPath -Parent)\\;Configuration=Release;Platform=Any CPU;OutDir=$buildDirectory\\;VisualStudioVersion=10.0" "$solutionPath"
	}
    else {
        &$msbuildPath /verbosity:minimal /p:"SolutionDir=$(Split-Path $solutionPath -Parent)\\;Configuration=Release;Platform=Any CPU;OutDir=$buildDirectory\\;VisualStudioVersion=10.0" "$solutionPath" | Out-Host        
    }
    
	if ($LASTEXITCODE -ne 0) {
		throw "Compilation failed."
	}
}
#region Packaging
#NOTE: TeamCity's build artifacts would allow for it to do the packaging, however we're packaging in this script and providing the packages as build artifacts so that it is reuseable without TeamCity. 
task PackageWebApp -depends Compile {
    try {
        $webAppPackageDir = "$($folders.base.invoke(''))..\WebApp"
        if (Test-Path $webAppPackageDir) { Remove-Item $webAppPackageDir -Recurse -Force }
        md $webAppPackageDir | Out-Null
 
        Compress-Zip "$webAppPackageDir\WebApp.zip" "$buildWebsiteDirectory"
        Compress-Zip "$webAppPackageDir\WebApp-Sts.zip" "$buildStsDirectory"
	} catch {
		echo "Error packaging WebApp."
		Exit 1;
	}
}

task ProduceNunitProjects -depends Compile {
	try {		
        $nunitProjContent = "<NUnitProject><Settings activeconfig=""Debug"" /><Config name=""Debug"" binpathtype=""Auto"">%assemblies%</Config><Config name=""Release"" binpathtype=""Auto"" /></NUnitProject>"

        $testAssemblies = Get-ChildItem $buildDirectory -Filter $unitTestFilter
        foreach ($assembly in $testAssemblies) {
            $assemblyName = $assembly.Name
            $nunitProjAssemblies = $nunitProjAssemblies + "<assembly path=""$assemblyName"" />"
        }
        
        $nunitProjContent.Replace("%assemblies%", $nunitProjAssemblies) | Out-File $buildDirectory\UnitTest.nunit
        $nunitProjAssemblies = $null
	} catch {
		echo "Error producing NUnit project for Unit Tests."
		Exit 1;
	}
    try {		
        $nunitProjContent = "<NUnitProject><Settings activeconfig=""Debug"" /><Config name=""Debug"" binpathtype=""Auto"">%assemblies%</Config><Config name=""Release"" binpathtype=""Auto"" /></NUnitProject>"

        $testAssemblies = Get-ChildItem $buildDirectory -Filter $uiTestFilter
        foreach ($assembly in $testAssemblies) {
            $assemblyName = $assembly.Name
            $nunitProjAssemblies = $nunitProjAssemblies + "<assembly path=""$assemblyName"" />"
        }
        
        $nunitProjContent.Replace("%assemblies%", $nunitProjAssemblies) | Out-File $buildDirectory\UITest.nunit
        $nunitProjAssemblies = $null
   	} catch {
		echo "Error producing NUnit projects for Ui Tests."
		Exit 1;
	}
}

task PackageTests -depends ProduceNunitProjects {
	try {
        #This creates the locations for the compiled zips. This is the same location that TeamCity will put the Artifacts.
        #They are being put there by team city even though they are likely already there primarily because since this stucture is outside of a VCS (due to its transient nature)
        #it is the only way to ensure all projects run the same binaries (even if they run on seperate build agents).
        $unitTestPackageDir = "$($folders.base.invoke(''))..\UnitTesting"
        $uiTestPackageDir = "$($folders.base.invoke(''))..\UiTesting"
        
        if(Test-Path $unitTestPackageDir) {Remove-Item $unitTestPackageDir -Recurse -Force}
        md $unitTestPackageDir | Out-Null
        
        if(Test-Path $uiTestPackageDir) {Remove-Item $uiTestPackageDir -Recurse -Force}
        md $uiTestPackageDir | Out-Null
        
        Compress-Zip "$unitTestPackageDir\Tests.zip" "$buildDirectory" "$testRegEx" $false
        Copy-Item "$unitTestPackageDir\Tests.zip" -Destination $uiTestPackageDir -Force
   	} catch {
		echo "Error packaging tests."
		Exit 1;
	}
}
#endregion