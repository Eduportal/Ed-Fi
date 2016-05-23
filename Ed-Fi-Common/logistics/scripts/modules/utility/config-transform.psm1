param ([string] [string]$configTransformationRunnerPath,[switch]$useAzureLogging)
#Config transformation Module
$ErrorActionPreference = "stop"
if (-not (get-module |? { $_.Name -eq "path-resolver" })) {   
    Import-Module $PSScriptRoot\..\modules\path-resolver.psm1
}
if ($useAzureLogging) {
    . $folders.activities.Invoke('azure-startup\SetupEventLog.ps1')
}
function Log-Local ($eventMessage, $entryType) {
    $eventHeader = "$(Split-Path $PSCommandPath -Leaf)"
    if($entryType -eq "Error") {
        if ($useAzureLogging) {
            Write-DlpAzureStartupEventLog -entryType Error -message $eventMessage -header $eventHeader
            exit
        }
        else {
            throw ("{0}-{1}" -f $eventHeader,$eventMessage)
        }
    }
    else {
        if ($useAzureLogging) {
            Write-DlpAzureStartupEventLog -message $eventMessage -header $eventHeader
        }
        else {
            Write-Output ("{0}-{1}" -f $eventHeader,$eventMessage)
        }
    }
    return
}

#If we're given a path, make sure it's good.
if ("$configTransformationRunnerPath" -ne "" -and (Test-Path $configTransformationRunnerPath)) {
    throw "Invalid Path specified for `$configTransformationRunnerPath: $configTransformationRunnerPath"
}

function Merge-ApplicationConfig {
param(
    [string]$projectName,
    [string]$baseConfigPath,
    [string]$transformationSuffix,
    $whatif
)
    try {
        Log-Local "Begin processing Merge-ApplicationConfig"
        $wctrPath = $configTransformationRunnerPath
        if (!$wctrPath) {
            Log-Local "ConfigTransformationRunnerPath was not provided on module registration. Search based on base directory."
            $possibleRunnerPaths = @(dir -path "$($folders.base.Invoke(".."))\**\WebConfigTransformRunner.exe" -recurse)
            #use what we find.
            if ($possibleRunnerPaths.Count -gt 0) {
                $wctrPath = $possibleRunnerPaths[0].FullName
            } 
            else {
                Log-Local -entryType Error -eventMessage "ConfigTransformationRunnerPath is unknown."
            }
        }
        else {
            $wctrPath = $configTransformationRunnerPath
        }
        Log-Local "Found WebConfigTransformRunner at $wctrPath."
        
        $baseFile = Split-Path -Leaf $baseConfigPath
        $transformFile = "$(Split-Path $baseConfigPath)\$projectName.exe.$transformationSuffix.config"

        if (-not (test-path $baseConfigPath)) {
            $failMessage = "ERROR. failed to retrieve base config file at '$baseConfigPath'"
            Log-Local -entryType Error -eventMessage $failMessage
        }
        $OutputConfigs = @((get-item $baseConfigPath).fullname)
        $transformFile = (get-item $transformFile).fullname

        foreach ($outconf in $OutputConfigs) {
            # Copy the configuration to a pristine version before doing the transform. 
            # Only do it once, though, so if this script gets run multiple times it doesn't copy the transformed version on top.
            $pristine = "$outconf.PRISTINE"
            if (-not (test-path $pristine)) {
                Log-Local "Did not find existing pristine file; copying '$outconf' to '$pristine'."
                copy-item $outconf $pristine
            }
            else {
                Log-Local "Found pre-existing pristine file at '$pristine'."
            }

            Log-Local "Beginning transform of '$pristine' by '$transformFile' into '$outconf'"
            if (-not $whatif) {
                $process = Start-Process -Wait -NoNewWindow -PassThru -FilePath $wctrPath -ArgumentList @($pristine,$transformFile,$outconf)
                $wctrExit = $process.ExitCode
                $finishMessage = "WebConfigTransformRunner exited with $wctrExit"
                if ($wctrExit -ne 0) {
                    Log-Local -entryType Error -eventMessage $finishMessage
                }
                else {
                    Log-Local $finishMessage
                }
            }
        }
    }
    catch {
        Log-Local -entryType Error -eventMessage "Error processing script! `r`n`r`n$_"
    }
}

Export-ModuleMember Merge-ApplicationConfig