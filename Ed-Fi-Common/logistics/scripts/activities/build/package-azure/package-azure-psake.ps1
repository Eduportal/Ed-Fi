properties {
    $erroractionpreference = "stop"
    . $folders.scripts.Invoke("activities\build\$environment.vars.ps1")
    import-module -force $folders.modules.invoke("azure-tools")
    import-module -force $folders.modules.invoke("utility\build-utility.ps1")
}

task default -depends PackageAzure

task PackageAzure {
        
	#Below, setting the VisualStudioVersion=10.0 forces us to build with VS2010 .target files, even if the file has been upgraded to 2012 format.
	#  I learned of this solution from http://blogs.msdn.com/b/webdev/archive/2012/08/22/visual-studio-project-compatability-and-visualstudioversion.aspx
	#  When we switch to copying over the VS2012 .target files instead of the 2010 target files, remove this property.
    $capparms = @{
        BuildConfiguration = $global:AzurePackageCreationConfig
        projectFile = $ccprojLocation
        visualStudioVersion = $visualStudioVersion
        verbose = $true
    }
    $teamCityLoggingPath = "$($env:teamcity_dotnet_nunitlauncher_msbuild_task)"
    if ($teamCityLoggingPath -ne "") {
       $capparms.LoggingProvider =  "JetBrains.BuildServer.MSBuildLoggers.MSBuildLogger,$teamCityLoggingPath;Verbosity=minimal"
    }
    New-AzurePackage @capparms
}