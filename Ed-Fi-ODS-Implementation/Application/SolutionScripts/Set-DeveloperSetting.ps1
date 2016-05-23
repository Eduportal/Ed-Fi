$global:nDevConfig = @{}

$projectName = "EdFi_Bulk"
$configFolderName = "nDevConfig"
$appDataDirectory = Join-Path -Path $projectName -ChildPath $configFolderName
$repositoryRootFileMarker = ".git"

function Initialize-nDevConfig() {
	$nDevConfig.projectName = $projectName
	
	$nDevConfig.src = $solutionPaths.SolutionFile
	$nDevConfig.repositoryRoot = $solutionPaths.repositoryRoot
	$nDevConfig.appDataSettingsPath = Join-Path -Path $env:APPDATA -ChildPath $appDataDirectory
	$nDevConfig.workingCopySettingsPath = Join-Path -Path $nDevConfig.repositoryRoot -ChildPath $configFolderName
	$nDevConfig.initializedMarkerPath = Join-Path -Path $nDevConfig.repositoryRoot -ChildPath .workspaceInitialized

	#Write-Host "DEBUG::  project name '$($nDevConfig.projectName)'"
	#Write-Host "DEBUG::  src          '$($nDevConfig.src)'"
	#Write-Host "DEBUG::  repo root    '$($nDevConfig.repositoryRoot)'"
	#Write-Host "DEBUG::  app data     '$($nDevConfig.appDataSettingsPath)'"
	#Write-Host "DEBUG::  working copy '$($nDevConfig.workingCopySettingsPath)'"
	#Write-Host "DEBUG::  marker       '$($nDevConfig.initializedMarkerPath)'"

	if (Is-DevelopmentEnvironmentInitialized) {
		return
	}

	Write-Host "Initializing nDevConfig"
	Set-WorkspaceInitialized
}

function Is-DevelopmentEnvironmentInitialized() {
	$path = $nDevConfig.initializedMarkerPath
	#Write-Host "DEBUG::  Checking for initialization marker at '$path'"
	return Test-Path $nDevConfig.initializedMarkerPath
}

function Set-WorkspaceInitialized() {
	New-Item $nDevConfig.initializedMarkerPath -type file
}

function global:Set-DeveloperSetting([string]$key, [string]$value) {

	#####  Internal Functions #####
	

	function Ensure-PathExists ([string]$path) {
		#Write-Host "DEBUG: Checking for path at '$path'"
		if (-not (Test-Path $path)) {
			#Write-Host "Creating directory $path"
			New-Item -ItemType directory -Path $path
		}
	}

	function AddSettingToDirectory([string]$key, [string]$value, [string]$directory) {
		Ensure-PathExists $directory
		$settingFilePath = Join-Path -Path $directory -ChildPath $key

		Write-Host "Setting contents of '$settingFilePath' to '$value'"
		Set-Content -Path $settingFilePath $value
	}

	#####  Usage #####

	if ([string]::IsNullOrEmpty($key)) {
		Write-Host
		Write-Host "    Usage::  Set-DeveloperSetting <key> <value>"
		Write-Host
		return
	}

	##### MAIN #####

	Write-Host "Setting '$key' to '$value'"
	
	AddSettingToDirectory $key $value $nDevConfig.workingCopySettingsPath
	AddSettingToDirectory $key $value $nDevConfig.appDataSettingsPath
}

function global:Remove-DeveloperSetting([string] $key) {
	function RemoveSettingFile([string] $path) {
		if (Test-Path $path) {
			Write-Host "Removing file $path"
			Remove-Item $path
		}
	}
	$workingDirectorySetting = Join-Path -Path $nDevConfig.workingCopySettingsPath -ChildPath $key
	$appDataSetting = Join-Path -Path $nDevConfig.appDataSettingsPath -ChildPath $key
	
	RemoveSettingFile $workingDirectorySetting
	RemoveSettingFile $appDataSetting
}

function global:Remove-AllDeveloperSettings() {

	function RemoveAllContents([string] $path) {
		$allItems = Join-Path -Path $path -ChildPath "*"

		if (Test-Path $path) {
			Write-Host "Removing all contents from $path"
			Remove-Item $allItems -recurse
		}
	}

	RemoveAllContents $nDevConfig.workingCopySettingsPath
	RemoveAllContents $nDevConfig.appDataSettingsPath
}

function global:Get-AllDeveloperSettings([string[]]$paths) {
	if (-not $paths) {
		Get-AllDeveloperSettings @($nDevConfig.workingCopySettingsPath, $nDevConfig.appDataSettingsPath)
		return
	}

	$files = @()
	foreach ($path in $paths) {
		$files += Get-Item (Join-Path -Path $path -ChildPath "*")
	}
	$files = $files  | Sort-Object Name

	foreach($item in $files) {
		Write-Host "--> $item < --" -ForegroundColor Magenta
		$content = Get-Content $item
		Write-Host "    $($item.Name): $content" -ForegroundColor White
		Write-Host
		
	}
}

###  Initialize nDevConfig When Script Is Loaded ###
Initialize-nDevConfig
