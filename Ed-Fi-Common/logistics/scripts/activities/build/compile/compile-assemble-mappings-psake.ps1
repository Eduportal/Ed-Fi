properties { 
    . "$($folders.modules.invoke('utility\build-utility.ps1'))"
    . "$($folders.activities.invoke('build\compile\compile-assemble-mappings.vars.ps1'))"
}

task default -depends CompileAllMappingAssemblies

task CompileAllMappingAssemblies {
	# Compile mappings for known non-SIS-specific mappings
	foreach ($mappingName in $file_transformation_mappings) {
		$searchPath = [IO.Path]::Combine($mapping_search_directory, $mappingName)
		CompileMappingsUnderFolder "$searchPath"
	}
	
	# Compile mapping source code for SIS-specific mappings
	dir "$mapping_search_directory\$leaMappingFilter" | % { 
		CompileMappingsUnderFolder ([IO.Path]::Combine($_.FullName, "Mappings")) 
	}
}

Function CompileMappingsUnderFolder {
	Param (
		[Parameter(Mandatory=$True)]
		[string]$folderPath
	)
	
	$solutions = @(dir Mapping.sln -Path $folderPath -Recurse)
    if($solutions.Count -gt 0) {
        foreach ($solution in $solutions) {
            Write-Host "Building $($solution.FullName) ..."
            $build_directory = [IO.Path]::Combine($solution.DirectoryName, "bin\Release")
            
            &msbuild /verbosity:minimal /p:"Configuration=Release;Platform=Any CPU;OutDir=$build_directory\\" $solution.FullName | Out-Host
            
            if ($LASTEXITCODE -ne 0) {
                throw "Compilation failed."
            }
        
            Write-Host "Compilation completed successfully."
        }
    }
}

task CleanAssemblies {
    if (Test-Path $mappings_bin_directory) {
		Write-Host "Delete $mappings_bin_directory ..."
		remove-item $mappings_bin_directory -force -recurse #-ErrorAction Continue | Out-Null
	}
}

task AssembleMappingBinaries -depends CleanAssemblies {
	# Compile mapping source for non-SIS mappings
	foreach ($mappingName in $file_transformation_mappings) {
		$searchPath = [IO.Path]::Combine($mapping_search_directory, $mappingName)
		$destPath = [IO.Path]::Combine($mappings_bin_directory, $mappingName)
		AssembleBinaries "$searchPath" "$destPath"
	}
	
	# Compile mapping source code for SIS-specific mappings
	dir "$mapping_search_directory\$leaMappingFilter" | % { 
		$searchPath = [IO.Path]::Combine($_.FullName, "Mappings")
		$destPath = [IO.Path]::Combine($mappings_bin_directory, "$($_.Name)")
		Write-Host "Assembling mapping binaries for $($_.Name) ..."
		AssembleBinaries "$searchPath" "$destPath"
	}
}

Function AssembleBinaries([string] $folderPath,	[string] $destPath) {
	# Get all Altova mapping files under the specified mapping folder
	$mappingFiles = dir *.mfd -Path $folderPath -Recurse
	
	foreach ($mappingFile in $mappingFiles) {
		$mappingFolder = $mappingFile.DirectoryName
		$mappingFolderRelativePath = $mappingFolder.Replace("$folderPath\", "")
		
		# Find the compiled .NET executable mapping
		$mappingExe = dir Mapping.exe -Path $mappingFolder -Recurse | where {$_.FullName.Contains("\bin\Release\")} # Only exes found in "bin" subfolder (not "obj" folder)
		
		if ($mappingExe -eq $null) {
			Write-Host "Could not locate an executable for $($mappingFile.FullName)." -ForegroundColor Cyan
			continue;
		}
		
		if (-not ($mappingExe -is [IO.FileInfo])) {
			throw "Could not find distinct mapping executable for $($mappingFile.FullName)."
		}
		
		$mappingExeFolder = $mappingExe.DirectoryName

		$mappingBinFolder = [IO.Path]::Combine($destPath, $mappingFolderRelativePath)
				
		if (-not (Test-Path ($mappingBinFolder))) {
			Write-Host "Creating directory $mappingBinFolder ..."
			md $mappingBinFolder | Out-Null
		}
		
		Copy-Item $mappingExeFolder\* $mappingBinFolder # -whatif
	}
}
