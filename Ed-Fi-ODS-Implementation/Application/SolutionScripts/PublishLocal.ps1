function global:Publish-Local() {
	Param([string]$appName = "")

	function Publish-LocalApplication([string] $appName) {


		Write-Host
		Write-Host "Deploying '$appName'" -ForegroundColor Cyan
		$applicationSettings = $localDeploymentSettings.applications.$appName
		$project = $applicationSettings.Project
		$publishProfile = $applicationSettings.PublishProfile
		$success = Invoke-MsBuild "`"$($localDeploymentSettings.solution)`" `"/p:DeployOnBuild=true;PublishProfile=$publishProfile;SuppressDllCopyOnDeploy=true`" `"/t:$project`" `"/property:Configuration=$publishProfile`" `"/verbosity:minimal`""

		if ($applicationSettings.PostDeployment) {
			$script = [scriptblock]::Create($applicationSettings.PostDeployment)
			Invoke-Command -ScriptBlock $script
		}

		if ($applicationSettings.FileSystemQueues) {
			Initialize-QueuesForApplication $appName
		}

		Write-DeploymentInfo $appName
		Write-Host "Successfully deployed application '$appName'" -ForegroundColor Green
	}

	Initialize-IIS

	if ($appName.Length -gt 0) {
		Assert-AppNameExists $appName

		Write-CommandMessage "Publish-Local"
		Publish-LocalApplication $appName
	} 
	else {
		$OrderedApps = [array]$localDeploymentSettings.applications.values | where-object -filterscript {"$($_.DeployOrderIndex)"}
		$UnorderedApps += [array]$localDeploymentSettings.applications.values | where-object -filterscript {-not "$($_.DeployOrderIndex)"}
		$SortedApps = @($OrderedApps | sort-object -property { $_.DeployOrderIndex }) + @($UnorderedApps)
		Write-Host "-appName parameter not specified.  Deploying all applicatons in order: $($SortedApps.Name)" -ForegroundColor Yellow
		foreach ($app in $SortedApps) {
			Write-CommandMessage "Publish-Local $($app.name)"
			Publish-LocalApplication($app.name)
		}
	}
}
