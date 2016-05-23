function global:Rebuild-Solution() {
    Param([string]$buildConfiguration = "Debug")
	Write-CommandMessage "Rebuild-Solution"

	$path = $solutionPaths.solutionFile.Path

	Invoke-MsBuild "`"$path`" `"/verbosity:minimal`" `"/target:rebuild`" `"/property:Configuration=$buildConfiguration`"" 
}