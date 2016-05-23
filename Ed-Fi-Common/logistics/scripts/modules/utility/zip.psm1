# Get directory for this script file, rather than resolving to the current directory for the calling script (which may be different)
if (-not (get-module |? { $_.Name -eq "path-resolver" })) {
    Import-Module $PSScriptRoot\..\modules\path-resolver.psm1
}

# Load assembly needed for Zip file manipulation
$sharpZipLibPath = "$($folders.tools.Invoke('SharpZipLib\ICSharpCode.SharpZipLib.dll'))"

#ToDo: look at PowerShell 3 native zip support.
[System.Reflection.Assembly]::LoadFrom($sharpZipLibPath) | Out-Null
$script:fastZip = New-Object "ICSharpCode.SharpZipLib.Zip.FastZip"

<#
function Initialize-ZipLib
{
	if ($script:fastZip -eq $null) {
		[System.Reflection.Assembly]::LoadFrom($sharpZipLibPath) | Out-Null
		$script:fastZip = New-Object "ICSharpCode.SharpZipLib.Zip.FastZip"
	}
}
#>

<#
function Extract-Package
{
	param([string]$zipfilename, [string] $destination)

	if(test-path($zipfilename))
	{	
		$shellApplication = new-object -com shell.application
		$zipPackage = $shellApplication.NameSpace($zipfilename)
		$destinationFolder = $shellApplication.NameSpace($destination)

		# See http://msdn.microsoft.com/en-us/library/bb787866(v=vs.85).aspx
		# CopyHere options:  16 = Respond with "Yes to All" for any dialog box that is displayed.
		$destinationFolder.CopyHere($zipPackage.Items(), 16) 
	}
}
#>

function Expand-Zip
{
	param (
		[IO.FileInfo] $fileInfo, 
		[string] $targetFolder, 
		[string] $regexFilter = $null,
		[ICSharpCode.SharpZipLib.Zip.FastZip+Overwrite] $overwrite = "Always",
		[ICSharpCode.SharpZipLib.Zip.FastZip+ConfirmOverwriteDelegate] $confirmOverwrite = $null
	)
	if ($overwrite -eq "Always") {
		$fastZip.ExtractZip($fileInfo.FullName, $targetFolder, $regexFilter)
	} else {
		$fastZip.ExtractZip($fileInfo.FullName, $targetFolder, $overwrite, $confirmOverwrite, $regexFilter, [string] $null, $true)
	}
}

function Compress-Zip([string] $zipFileName, [string] $sourceFolder, [string] $fileFilter=$null, [bool] $recurse = $true) {
	$fastZip.CreateEmptyDirectories = $true;
    #Write-Host "DEBUG: zipFileName = $zipFileName, sourceFolder = $sourceFolder\ filter = $filter recurse = $recurse" -ForegroundColor Blue
	$fastZip.CreateZip($zipFileName, $sourceFolder, $recurse, $fileFilter)
}

<# This works but it retains all of the path information for the file added.
function Add-FileToZip([string] $zipFilePath, [string] $sourceFilePath) {
    Write-Host "DEBUG: zipFilePath = $zipFilePath, sourceFilePath = $sourceFilePath " -ForegroundColor Blue
    $zipFile = New-Object "ICSharpCode.SharpZipLib.Zip.ZipFile" -ArgumentList $zipFilePath
    $zipFile.BeginUpdate()
    $zipFile.Add($sourceFilePath)
    $zipFile.CommitUpdate()
    $zipFile.Close()
}
#>
Export-ModuleMember Expand-Zip,Compress-Zip