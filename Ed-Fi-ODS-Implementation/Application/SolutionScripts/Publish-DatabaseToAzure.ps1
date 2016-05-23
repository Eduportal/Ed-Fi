Function global:Publish-DatabaseToAzure([string] $databaseName = "EdFi_Ods_Populated_Template", [string] $targetEnvironment = "development", [string] $outputDatabaseName) {
	if (-not $outputDatabaseName) {
		$outputDatabaseName = $databaseName
	}

	$isTemplate = $databaseName -imatch '^EdFi_Ods.*_Template$'
	
	function ApplyScript($file, $connectionString) {
		# Get-Content returns a string array of all the lines. We join that into a single string
		$fileContents = Get-Content "$($file.FullName)"

		if ($fileContents -ne $null -and $fileContents.Length -gt 0) {
			$sql = [string]::Join([Environment]::NewLine, $fileContents);

			<# TODO: GKM - Not needed here?
			if ($stripMetadataCommands) {
				$sql = $sql -replace $versionRegex, '$1'
				$sql = [regex]::Replace($sql, "(if exists \(select \* from \:\:.*?BEGIN.*?END\r\nexec sys.sp_addextendedproperty .*?'\r\ngo\r\n)", [string]::Empty, [System.Text.RegularExpressions.RegexOptions]::Singleline, [Regex]::InfiniteMatchTimeout)
				$sql = $sql.Replace("EXECUTE sp_addextendedproperty", "-- EXECUTE sp_addextendedproperty")
			}
			#>

			Invoke-SqlScript $connectionString $sql 30 #$executionTimeOut
		}
	}

	Write-CommandMessage "Publish-DatabaseToAzure $databaseName $targetEnvironment $outputDatabaseName"

	if (-not (Test-DatabaseExists localhost $databaseName)) {	
		throw "Database '$databaseName' does not exist locally, and cannot be published."
	}

	if (($targetEnvironment -ne "development") -and ($targetEnvironment -ne "production")) {
		throw "Parameter 'targetEnvironment' must be either 'development' or 'production'"
	}

	if ($targetEnvironment -eq "production") {
		Write-Host 
		Write-Host "============== WARNING ==============" -ForegroundColor Yellow
		Write-Host "Using PRODUCTION database settings" -ForegroundColor Yellow
		Write-Host "=====================================" -ForegroundColor Yellow
		Write-Host 

		$databaseSettings = $remoteDeploymentSettings.databaseServers.AzureProduction
	} else {
		Write-Host "Using Development database settings"
		$databaseSettings = $remoteDeploymentSettings.databaseServers.SQLAzureDev
	}

	$serverName = "$($databaseSettings.ServerName).database.windows.net"
	$userName = "$($databaseSettings.UserName)@$($databaseSettings.ServerName)"
	$password = "$($databaseSettings.Password)"

	$outputPath = [IO.Path]::Combine([IO.Path]::GetTempPath(), "SQLAzureMigrations")

	# Locate the latest version of the migration wizard
	$latestMigrationPackage = Get-LatestNugetPackagePath "Dlp.SQLAzureMW.Batch"

	# Extract migration artifacts
	#cd "$($solutionPaths.src)\packages\Dlp.SQLAzureMW.Batch.3.9.13.1\Backup"
	Write-RunMessage "Extracting migration artifacts for database '$databaseName'..."

	# Save the original location so we can restore it when the process completes
	$originalLocation = Get-Location

	# Azure Migration utility must have current directory set to executable location
	cd "$latestMigrationPackage\Backup"
	.\SQLAzureMWBatchBackup.exe -S localhost -T -D $databaseName -O $outputPath

	# Locate most recently generated artifacts
	$migrationScriptFileName = dir $outputPath -filter "$databaseName.sql" -recurse |
		sort LastWriteTime -descending |
		select -first 1 | 
		select -expand FullName

	if (-not $migrationScriptFileName) {
		throw "Unable to find migration script file under '$outputPath'."
	}

	$resultsFileName = [IO.Path]::Combine([IO.Path]::GetDirectoryName($migrationScriptFileName), "UploadResults.txt")

	# Publish artifacts to SQL Azure
	Write-RunMessage "Migrating artifacts for database '$databaseName' to SQL Azure server '$serverName'..."

	# Azure Migration utility must have current directory set to executable location
	cd "$latestMigrationPackage\Upload"

	.\SQLAzureMWBatchUpload.exe `
		-S "$serverName" `
		-U "$userName" `
		-P "$password" `
		-D $outputDatabaseName -d true -s 1 -e web -i $migrationScriptFileName -o $resultsFileName

	if ($isTemplate) {
		Write-Host "Applying EdFiAzure scripts to database..."
		$creds = new-object Net.NetworkCredential($userName, $password)
		$connectionString = Get-SqlConnectionString $serverName $outputDatabaseName $creds

		$azureScriptsPath = [IO.Path]::Combine($solutionPaths.repositoryRoot, "Database\Structure\EdFiAzure")
		$files = dir *.sql -Path $azureScriptsPath | sort Name

		foreach ($file in $files) {
			ApplyScript $file $connectionString
		}
	}

	# Restore original location
	cd $originalLocation
}