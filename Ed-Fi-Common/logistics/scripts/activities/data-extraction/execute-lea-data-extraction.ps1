#This is the controller script for all LEA data extraction. 
#The trunk version of this script is the version that is run on the LEA system. 
#By default this file is excluded from production branch packaging.
Param([parameter(Mandatory=$true)]$localEducationAgencyName, [switch]$developmentOnlyNoPreExtract)
#Expected deployed location is C:\EdFi\Development\Ed-Fi-Apps\Logistics\Scripts\data-extraction\
$normalizedLocalEducationAgencyName = $localEducationAgencyName.Replace(" ","")
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
if (-not (get-module |? { $_.Name -eq "path-resolver" })) {
    Import-Module $PSScriptRoot\..\..\..\modules\path-resolver.psm1
}

$devType  = "Development"
#Resolve all Development paths here:
$developmentPackageUpdate    = "$($folders.activities.invoke('data-extraction\extract-compress-send-data.ps1')) `"$localEducationAgencyName`" `"GetLatestDeploymentPackage`" $devType;"
$developmentPreExtractScript = "$($folders.activities.invoke('data-extraction\extract-compress-send-data.ps1')) `"$localEducationAgencyName`" `"ExecutePreExtractScript`" $devType;"
$developmentExtract          = "$($folders.activities.invoke('data-extraction\extract-compress-send-data.ps1')) `"$localEducationAgencyName`" ExtractSisData,CompressAndSendData $devType;"
$developmentCd               = "cd $($folders.activities.invoke('data-extraction\'));"

#Resolve all production paths here:
$prodType = "Production"
Import-Module $folders.base.invoke('..\..\Production\Ed-Fi-Apps\logistics\scripts\modules\path-resolver.psm1') -PreFix Prd
$productionPackageUpdate     = "$((Select-PrdEdfiFiles 'logistics\scripts\activities\data-extraction\extract-compress-send-data.ps1').FullName) `"$localEducationAgencyName`" `"GetLatestDeploymentPackage`" $prodType;"
$productionExtract           = "$((Select-PrdEdfiFiles 'logistics\scripts\activities\data-extraction\extract-compress-send-data.ps1').FullName) `"$localEducationAgencyName`" ExtractSisData,CompressAndSendData $prodType;"
$productionCd                = "cd $(Split-Path -Parent (Select-PrdEdfiFiles 'logistics\scripts\activities\data-extraction\')[0].FullName);"


# Add eventlog entry for the start of this processing
[Diagnostics.EventLog]::WriteEntry("Edfi Data Extraction", "Edfi Data Extraction process started.", "Information")
$startTime = [DateTime]::Now

$logPath = "$($folders.base.invoke('..\..\'))Logs"

# Create the log folder if it doesn't already exist
if (-not(Test-Path $logPath)){
	md $logPath | Out-Null
}

# Create a log file for the current run based on a timestamp
$logFileName = "DataExtraction - " + [DateTime]::Now.ToString("yyyy-MM-dd HH-mm-ss") + ".txt"

if ($developmentOnlyNoPreExtract) {
    $extractionSteps = @(
                        "$developmentCd $developmentPackageUpdate",
                        "$developmentCd $developmentExtract"
                        )
}
else{
    #1)Pull down latest development package
    #2)Pull down latest production package
    #3)Execute any pre-extract scripts from development (currently these change very infrequently and any adjustments would need to be made for both extraction lines anyway)
    #4)Execute production data extraction and transfer to host
    #5)Execute development data extraction and transfer to host
    $extractionSteps = @(
                        "$developmentCd $developmentPackageUpdate",
                        "$productionCd $productionPackageUpdate",
                        "$developmentCd $developmentPreExtractScript",
                        "$productionCd $productionExtract",
                        "$developmentCd $developmentExtract"
                        )
}
foreach ($step in $extractionSteps) {
	$command = $step

	# Encode the command to be executed
	$bytes = [System.Text.Encoding]::Unicode.GetBytes($command)
	$encodedCommand = [Convert]::ToBase64String($bytes)

	# Launch powershell to execute the process (so we can capture all output)
	powershell.exe -NonInteractive -encodedCommand $encodedCommand >> "$logPath\$logFileName"
    #Stop processing on error
    if ($lastexitcode -ne 0) { break }
}
$logOutText = [IO.File]::ReadAllText("$logPath\$logFileName")
$logOutText = if($logOutText.Length -ge 32700) { $logOutText.Substring(0,32700) } Else { $logOutText }
if ($lastexitcode -eq 0) {
	[Diagnostics.EventLog]::WriteEntry("Edfi Data Extraction", $logOutText, "Information")
} else {
	[Diagnostics.EventLog]::WriteEntry("Edfi Data Extraction", $logOutText, "Error")
}

# Add eventlog entry for the end of this processing
$duration = ([DateTime]::Now - $startTime)
[Diagnostics.EventLog]::WriteEntry("Edfi Data Extraction", "Edfi Data Extraction process finished in $duration.", "Information")