param([string]$fileArchiveToProcess)

if (-not (get-module |? { $_.Name -eq "path-resolver" })) {
    Import-Module $PSScriptRoot\..\..\..\modules\path-resolver.psm1
}

Import-Module "$($folders.modules.invoke('psake\psake.psm1'))"
. "$($folders.activities.invoke('build\process-files\process-files.vars.ps1'))"
. "$($folders.activities.invoke('build\' + $environment + '.vars.ps1'))"

if (-not (Test-Path $processFilesLogPath)) {
    md $processFilesLogPath
}

Start-Transcript "$processFilesLogPath\$(Get-date -f "yyyy-MM-dd").log" -Append -Force

Write-Host $fileArchiveToProcess -Foreground Yellow 

if (Test-Path $fileArchiveToProcess) {
    #Get needed parameter values from file name.
    $fileArchiveToProcess -match $regex 
    $publishType = $matches[2]
    $isPartial = ("Partial" -eq $matches[3])
    if ($machineFileTypes -eq $null -or $machineFileTypes[$Env:computername] -eq $publishType) {
        $parms = @{
            fileArchive = $fileArchiveToProcess;
            localEducationAgencyName = $matches[1];
            publishType = $publishType;
            folders = $folders;
            environment = $environment
        }
        
        if($isPartial){
            Invoke-Psake "$($folders.activities.invoke('build\process-files\process-files-psake.ps1'))" ProcessPartial -parameters $parms 
        }
        else {
            Invoke-Psake "$($folders.activities.invoke('build\process-files\process-files-psake.ps1'))" -parameters $parms
        }
    }
}
else {
    throw "File $fileArchiveToProcess Not Found!`r`nFile Cannot be processed." 
}
Stop-Transcript
if ($error.count -gt 0 -or ($LASTEXITCODE -ne $null -and $LASTEXITCODE -gt 0)) {
    Exit 1
}
else {
    Exit 0
}