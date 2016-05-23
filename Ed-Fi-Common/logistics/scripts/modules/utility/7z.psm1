if(-not $folders){
    $scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
    Import-Module $scriptDir\build-utility.ps1
}

# Load assembly needed for Zip file manipulation
$sevenZipSharpFolder = "$($folders.tools.invoke('SevenZipSharp'))"
$sevenZipSharpLib = "$sevenZipSharpFolder\SevenZipSharp.dll"

[System.Reflection.Assembly]::LoadFrom($sevenZipSharpLib) | Out-Null

if ([intptr]::size -eq 8) {
    [SevenZip.SevenZipBase]::SetLibraryPath("$sevenZipSharpFolder\7z64.dll")
} else {
    [SevenZip.SevenZipBase]::SetLibraryPath("$sevenZipSharpFolder\7z.dll")
}

function Expand-ArchiveWith7z { param ( [IO.FileInfo] $fileInfo, 
                              [string] $targetFolder, 
                              [string] $password )
    if ("$password" -eq "") {
        $extractor = new-object SevenZip.SevenZipExtractor($fileInfo)
    } else {
        $extractor = new-object SevenZip.SevenZipExtractor($fileInfo, $password)
    }
    
    Write-Host "Extracting $fileInfo to $targetFolder ..."
    try{ 
		$extractor.ExtractArchive($targetFolder)
	}
	catch {
		$extractor.Dispose()
		throw $_
	}
}

function Compress-DirectoryWith7z { param ( [string] $7zFileName, 
                             [string] $sourceFolder,
                             [string] $password=$null,
                             [string] $fileFilter=$null,
                             [bool]   $recurse = $true,
                             [switch] $requirePassword )
    
    $compressor = new-object SevenZip.SevenZipCompressor
    $compressor.CompressionMode = "Create"
    $compressor.PreserveDirectoryRoot = $false
    
    if ("$password" -eq "") {
        if ($requirePassword) { throw "No password was provided. However, one is required." }
        elseif ($fileFilter) { $compressor.CompressDirectory($sourceFolder, $7zFileName, $fileFilter, $recurse) }
        else { $compressor.CompressDirectory($sourceFolder, $7zFileName, $recurse) }
    } 
    else {
        $compressor.EncryptHeaders = $true
        $compressor.ZipEncryptionMethod = "Aes256"
        if ($fileFilter) { $compressor.CompressDirectory($sourceFolder, $7zFileName, $password, $fileFilter, $recurse) }
        else { $compressor.CompressDirectory($sourceFolder, $7zFileName, $recurse, $password) }
    } 
}
Export-ModuleMember Compress-DirectoryWith7z,Expand-ArchiveWith7z