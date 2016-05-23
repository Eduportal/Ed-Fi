function global:Initialize-IIS() {
	Write-CommandMessage "Initialize-IIS"

	function Initialize-IISApplication([string]$portNumber, [string]$appPoolName,	[string]$webSiteName) {
		$baseDeployPath = $localDeploymentSettings.baseDeployPath
		$windows = $env:windir
		$appcmd = "$windows\system32\inetsrv\appcmd.exe"

		Write-Host "Initializing IIS -->  Port: [$portNumber]  AppPool: [$appPoolName]  WebSite: [$webSiteName]" -ForegroundColor Cyan

		function EnsureAppPoolExists() {
			$getAppPools = [scriptblock]::Create("$appcmd list apppool")
			$appPoolResult = Invoke-Command -ScriptBlock $getAppPools

			[bool] $hasAppPool = $FALSE

			$appPoolResult -split [environment]::NewLine  | ForEach {
				$p = ($_ -split '"')[1].ToUpper()
				if ($p -eq $appPoolName.ToUpper()) {
					$hasAppPool = $TRUE
				}
			}

			if (-not $hasAppPool) {
				$createAppPool = [scriptblock]::Create("$appcmd add apppool /name:$appPoolName /managedRuntimeVersion:v4.0")
				Write-Host "Creating IIS Application Pool $appPoolName"
				Invoke-Command -ScriptBlock $createAppPool
			}
			else {
				$recycleAppPool = [scriptblock]::Create("$appcmd recycle apppool /apppool.name:$appPoolName")
				Write-Host
				Write-Host "Recycling existing application pool [$appPoolName] [$recycleAppPool]" -ForegroundColor Cyan
				Write-Host
				Invoke-Command -ScriptBlock $recycleAppPool
			}
		}

		function EnsureWebSiteExists() {
			$getWebSites = [scriptblock]::Create("$appcmd list site")
			$webSitesResult = Invoke-Command -ScriptBlock $getWebSites

			[bool] $hasWebSite = $FALSE

			$webSitesResult -split [environment]::NewLine  | ForEach {
				$ws = ($_ -split '"')[1].ToUpper()
				if ($ws -eq $webSiteName.ToUpper()) {
					$hasWebSite = $TRUE
				}
			}

			if (-not $hasWebSite) {
				$createWebSite = [scriptblock]::Create("$appcmd add site /name:$webSiteName /physicalPath:`"$baseDeployPath`" /bindings:http/*:$($portNumber):")
				Write-Host
				Write-Host "Creating Web Site: '$createWebSite'"
				Invoke-Command -ScriptBlock $createWebSite
        
				$setAppPool = [scriptblock]::Create("$appcmd set app $webSiteName/ /applicationPool:$appPoolName")
				Invoke-Command -ScriptBlock $setAppPool

				$startWebSite = [scriptblock]::Create("$appcmd start site `"$webSiteName`"")
				Invoke-Command -ScriptBlock $startWebSite
			}
			else {
				Write-Host "Found existing web site $webSiteName"
			}
		}

		function EnsureApplicationExists([string] $appName) {
			$getApplications = [scriptblock]::Create("$appcmd list app")
			$applicationsResult = Invoke-Command -ScriptBlock $getApplications
			$fullAppName = "$webSiteName/$appName".ToUpper()

			[bool] $hasApplication = $FALSE

			$applicationsResult -split [environment]::NewLine  | ForEach {
				$ap = ($_ -split '"')[1].ToUpper()
				if ($ap -eq $fullAppName) {
					$hasApplication = $TRUE
				}
			}

			if (-not $hasApplication) {
				$createApplication = [scriptblock]::Create("$appcmd add app /site.name:$webSiteName /path:/$appName /physicalPath:$baseDeployPath\$appName")
				Write-Host
				Write-Host "Creating Application '$createApplication'"
				Invoke-Command -ScriptBlock $createApplication

				$setAppPool = [scriptblock]::Create("$appcmd set app $fullAppName /applicationPool:$appPoolName")
				Invoke-Command -ScriptBlock $setAppPool
			}
			else {
				Write-Host "Found existing application $fullAppName"
			}
		}

		function EnsureAppsExist() {
			foreach($app in $localDeploymentSettings.applications.Keys) {
				EnsureApplicationExists $app
			}
		}

		Function EnsureDirectoryExists([string] $path) {
			if (-not (Test-Path $path)) {
				New-Item -ItemType directory -Path $path
			}
			else {
				Write-Host "Found existing path $path"
			}
		}

		function CreateDeploymentDirectories() {
			foreach($app in $localDeploymentSettings.applications.Keys) {
				EnsureDirectoryExists "$baseDeployPath\$app"
			}
		}

		CreateDeploymentDirectories
		EnsureAppPoolExists
		EnsureWebSiteExists
		EnsureAppsExist


		foreach($app in $localDeploymentSettings.applications.Keys) {
			Write-DeploymentInfo $app
		}

		Write-Host
	}

	foreach ($iisConfig in $localDeploymentSettings.IisConfigurations.Values) {
		$port = $iisConfig["Port"]
		$appPool = $iisConfig["AppPool"]
		$webSite = $iisConfig["WebSiteName"]

		Initialize-IISApplication $port $appPool $webSite
	}
}
