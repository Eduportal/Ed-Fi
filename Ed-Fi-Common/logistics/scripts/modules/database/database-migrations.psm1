# *************************************************************************
# �2013 Ed-Fi Alliance, LLC. All Rights Reserved.
# *************************************************************************

if (-not (get-module |? { $_.Name -eq "path-resolver" })) {
    Import-Module $PSScriptRoot\..\modules\path-resolver.psm1
}
Import-module "$($folders.modules.invoke('database\database-management.psm1'))"
Import-Module "$($folders.modules.invoke('database\database-utility.psm1'))"
Import-Module "$($folders.modules.invoke('utility\common-objects.psm1'))"
###
# Script for database change management
# Author: Mikael Lundin
# Website: http://mint.litemedia.se
# E-mail: mikael.lundin@litemedia.se
#

#Add type would be nice here, but it is impractical without having someway to query the GAC
[System.Reflection.Assembly]::LoadWithPartialName("Microsoft.SqlServer.SMO") | out-null

# Assemblies needed for database backup operation
[System.Reflection.Assembly]::LoadWithPartialName("Microsoft.SqlServer.SmoExtended") | Out-Null
[System.Reflection.Assembly]::LoadWithPartialName("Microsoft.SqlServer.ConnectionInfo") | Out-Null
[System.Reflection.Assembly]::LoadWithPartialName("Microsoft.SqlServer.SmoEnum") | Out-Null
[System.Reflection.Assembly]::LoadWithPartialName("Microsoft.SqlServer.SqlEnum") | Out-Null # For recovery model

# Will create a new database and add table to manage versions
Function New-DatabaseWithMigrations ([string]$sqlServer, [string]$databaseName, [string]$username=$null, [System.Security.SecureString]$password=$null, $recoveryModel = "Simple") {
	# http://sqlblog.com/blogs/allen_white/archive/2008/04/28/create-database-from-powershell.aspx
	[System.Reflection.Assembly]::LoadWithPartialName('Microsoft.SqlServer.SMO')  | out-null
	$s = Get-Server $sqlServer $username $password
	
	# Instantiate the database object and create database
	$db = new-object ('Microsoft.SqlServer.Management.Smo.Database') ($s, $databaseName)
	
    if ($recoveryModel -eq "Full") {
        Write-Host "Creating database with 'Full' recovery model..."
        $db.RecoveryModel = [Microsoft.SqlServer.Management.Smo.RecoveryModel]::Full
    } elseif ($recoveryModel -eq "BulkLogged") {
        Write-Host "Creating database with 'BulkLogged' recovery model..."
        $db.RecoveryModel = [Microsoft.SqlServer.Management.Smo.RecoveryModel]::BulkLogged
    } elseif ($recoveryModel -eq "Simple") {
        Write-Host "Creating database with 'Simple' recovery model..."
        $db.RecoveryModel = [Microsoft.SqlServer.Management.Smo.RecoveryModel]::Simple
    }
    
    $db.Create()
	# Create table and column for handling database version
    # ScriptSource - The source of the script being run.
    # ScriptType - The type of script being run. (Structure, Data, PostLoad)   
    # DatabaseType - The type of database being run for (Application, Dashboard, EdFi, etc.)
    # SubType - Any subtype divisions for the script versions. (LocalEducationAgencies, etc.)
    # VersionLevel - The highest script version that has been successfully executed against this database.
    
	$db.ExecuteNonQuery("CREATE TABLE [$databaseName].[dbo].[VersionLevel] ([ScriptSource] VARCHAR(256) NOT NULL, [ScriptType] VARCHAR(256) NOT NULL, [DatabaseType] VARCHAR(256) NOT NULL, [SubType] VARCHAR(1024), [VersionLevel] int NOT NULL)");
    
    return $db
}

# Helper functions
Function Get-VersionLevel ([Parameter(Mandatory=$true)][string][ValidateNotNullOrEmpty()] $connectionString,
 [Parameter(Mandatory=$true)][string][ValidateNotNullOrEmpty()] $scriptSource,
 [Parameter(Mandatory=$true)][string][ValidateNotNullOrEmpty()] $scriptType,
 [Parameter(Mandatory=$true)][string][ValidateNotNullOrEmpty()] $databaseType,
 [string] $subType = [String]::Empty
 ) {
    #Please use splatting to supply paramaters to this function via a hastable.

	$sql = "SELECT TOP 1 [VersionLevel] FROM [dbo].[VersionLevel] Where [ScriptSource] = '$scriptSource' and [ScriptType] = '$scriptType' and [DatabaseType] = '$databaseType'"
	if (-not [String]::IsNullOrWhiteSpace($subType)) {
        $sql += " and [SubType] = '$subType'"
    }

	$dataSet = Invoke-SqlScript $connectionString $sql -returnDataSet
    foreach ($row in $dataSet.Tables[0].Rows) {
        $version = $row["VersionLevel"]
    }
	return $version
}

Function Update-VersionLevel ([Parameter(Mandatory=$true)][string][ValidateNotNullOrEmpty()] $connectionString,
 [Parameter(Mandatory=$true)][string][ValidateNotNullOrEmpty()] $scriptSource,
 [Parameter(Mandatory=$true)][string][ValidateNotNullOrEmpty()] $scriptType,
 [Parameter(Mandatory=$true)][string][ValidateNotNullOrEmpty()] $databaseType,
 [string] $subType = [String]::Empty,
 [Parameter(Mandatory=$true)][int] $version
 ) {
    $sql = "UPDATE [dbo].[VersionLevel] SET [VersionLevel] = $version WHERE [ScriptSource] = '$scriptSource' and [ScriptType] = '$scriptType' and [DatabaseType] = '$databaseType'"
    if (-not [String]::IsNullOrWhiteSpace($subType)) {
        $sql += " and [SubType] = '$subType'"
    }

    Invoke-SqlScript $connectionString $sql
}

Function Insert-VersionLevel ([Parameter(Mandatory=$true)][string][ValidateNotNullOrEmpty()] $connectionString,
 [Parameter(Mandatory=$true)][string][ValidateNotNullOrEmpty()] $scriptSource,
 [Parameter(Mandatory=$true)][string][ValidateNotNullOrEmpty()] $scriptType,
 [Parameter(Mandatory=$true)][string][ValidateNotNullOrEmpty()] $databaseType,
 [string] $subType = [String]::Empty,
 [Parameter(Mandatory=$true)][int] $version
 ) {
    $sql = "INSERT INTO [dbo].[VersionLevel] ([ScriptSource], [VersionLevel], [ScriptType], [DatabaseType], [SubType])`r`n"
    $sql += "Values('$scriptSource', $version, '$scriptType', '$databaseType', '$($subType.Trim())')"
    Invoke-SqlScript $connectionString $sql
}

Function Get-ScriptMigrationMetadata ($scriptPath) {
    $newMetadata = @{}    
    $scriptFolders = (Split-Path -Parent $scriptPath).ToUpper().Replace("/", "\")
    #Should make the ancestor folder an externally setable variable.
    $dbAncestorFolder = "\DATABASE\"
    if (-not $scriptFolders.Contains($dbAncestorFolder)) { throw "$dbAncestorFolder was not found in the path: $scriptPath" }
    
    $ancestorIndex = $scriptFolders.IndexOf($dbAncestorFolder)
    $newMetadata.ScriptSource = Split-Path -Leaf $scriptFolders.Substring(0, $ancestorIndex)
    $metaElements = $scriptFolders.Substring($ancestorIndex + $dbAncestorFolder.Length).Trim('\').Split('\')

    if ($metaElements.Length -lt 2) {
        throw "Unable to derive script metadata information"
    }
    if ($metaElements.Length -ge 2) {
        $newMetadata.ScriptType = $metaElements[0]
        $newMetadata.DatabaseType = $metaElements[1]
    }
    if ($metaElements.Length -ge 3) {
        $newMetadata.SubType = $metaElements[2]
    }
    if ($metaElements.Length -ge 4) {
        #TODO: Make this clearer and return both core and additional properties.
        throw "Too many metadata elements derived!"
    }

    return $newMetadata
}

Function Get-ScriptsToRun ([System.IO.FileSystemInfo[]] $fileSystemInfoCollection, [string]$connectionString, [string]$databaseName, [int]$executionTimeOut = 30) {
	# Get ordered creation scripts
	#$files = $fileSystemInfoCollection | sort Name | where { $_.Name -match "^([\w\d\s\-]*\.sql|[\w\d\s\-]*\." + $databaseName + "\.sql)$" }
	
	# Get ordered creation scripts
	# 	* Get all sql files
	# 	* Sort by name
	#	* Filter to include general SQL files or database-instance specific ones
	#	* Group by file "base name" so if we have both a generic version and a database specific version, they'll be group by the name of the file w/o the extension
	#	* Grab the last entry in each group, which will be the database-specific script, if it is present
    
    # This check is to allow versioned databases to pick up the appropriate DB specific scripts
    $versionRegExBase = "^([\w\s\d\-]+\D)"
    #If the version suffix is present use it, otherwise use the numerical matcher (used for Ed-Fi standard version).
    $versionRegex = if ($Env:versionSuffix) {"$($versionRegExBase)(?:_$Env:versionSuffix)`$"} else {"$($versionRegExBase)(?:_\d{2,})`$"}
    if ($databaseName -match $versionRegex) {
        $matchDbName = $databaseName -replace $versionRegex, '$1'
    }
    else {
        $matchDbName = $databaseName
    }
    #Write-Debug $fileSystemInfoCollection
    
    #Get the list of all the scripts to run grouped by the first part of the name
    $fileGroups = $fileSystemInfoCollection | where { $_.Attributes -notcontains "Directory" } | 
        where { $_.Name -match "^(\d+\-[\w\d\s\-]*\.sql|\d+\-[\w\d\s\-]*\." + $matchDbName + "\.sql)`$" } | 
        group DirectoryName,{$_.Name.Split(".")[0]} 

    $files = @()
    foreach ($groupListing in $fileGroups) {
        $pathToAdd = $null
        #If there is only one path use it.
        if($groupListing.Count -eq 1){
            $pathToAdd = $groupListing.Group[0]
        }
        else {
            #First look for the first script that is for this database specifically
            $sortedGroups = $groupListing.Group | sort Name
            foreach ($filePath in $sortedGroups) {
                if ($filePath.Name -match "^([\w\d\s\-]*\." + $matchDbName + "\.sql)`$") {
                    $pathToAdd = $filePath
                    break
                }
            }
            #if one wasn't found fall back to the root script.
            if($pathToAdd -eq $null) {
                foreach ($filePath in $groupListing.Group) {
                    if ($filePath.Name -match "^([\w\d\s\-]*\.sql)`$") {
                        $pathToAdd = $filePath
                        break
                    }
                }
            }
        }
        $files += $pathToAdd
    }
    
	<# For Debug purposes only
	Write-Host "Executing script with connection string: " -NoNewline -ForegroundColor Cyan
	Write-Host $connectionString
	#>
    #This holds cached metadata during a given execution so that scripts with the same metadata values don't all hit the DB.
    $cachedMetadata = New-HashtableKeyedHashtable
	$scriptsToRun = New-HashtableKeyedHashtable
    if ($files -ne $null -and $files.Count -gt 0) {
        # For each of those files, run query on database
        foreach ($file in $files) {
            $scriptMetaData = Get-ScriptMigrationMetadata $file.fullname
            $version = $null
            #Look to see if we've already looked up the version for this set of KVP's before
            foreach ($metadata in $cachedMetadata.PSBase.Keys) {
                $matchFailure = $false
                foreach ($key in $metadata.Keys)
                {
                    if ($metadata[$key] -ne $scriptMetaData[$key]) {$matchFailure = $true}
                    #Stop looking through the keys if we already had a match failure (all key's values must match)
                    if ($matchFailure) { break }
                }
                #if all of the values for the keys matched then this is the cached version we're looking for.
                if (-not $matchFailure) {
                    $version = $cachedMetadata[$metadata]
                    #leave loop
                    break
                }
            }
            
            #If we didn't find a cached version, go look up the version in the DB
            if ($version -eq $null) {
                $version = Get-VersionLevel -connectionString $connectionString @scriptMetaData
                if ($version -eq $null) {
                    #No version was found in DB, default to 0.
                    $version = 0
                    #Go add it to the DB
                    Insert-VersionLevel -connectionString $connectionString @scriptMetaData -version $version
                }
                
                #Cache the version for this metadata key
                $cachedMetadata[$scriptMetaData] = $version
            }

            # Get this version number
            $fileVersion = [int]::Parse($file.Name.Substring(0, 4))
            if ($version -lt $fileVersion) {
                $scriptsToRun[$scriptMetaData] += @($file)
				#update the metadata too.
				$cachedMetadata[$scriptMetaData] = $fileVersion
            }
        }
    }
	return $scriptsToRun
}

<#Function Initialize-Database([string]$databaseDirectory, [string]$connectionString, [string]$databaseName) {
    Sync-Database (dir "$databaseDirectory\*.sql") $connectionString $databaseName
}#>
# Will execute creation scripts
#TODO: allow metadataScripts to be passed.
Function Sync-Database([System.IO.FileSystemInfo[]] $fileSystemInfoCollection, [string]$connectionString, [string]$databaseName, [int]$executionTimeOut = 30, [hashtable]$sqlVars) {
	$metaHashScript = Get-ScriptsToRun $fileSystemInfoCollection $connectionString $databaseName $executionTimeOut
	if ($metaHashScript.PSBase.Count -gt 0) {
		foreach ($metaData in $metaHashScript.PSBase.Keys)
        {
            foreach ($file in $metaHashScript[$metaData]) {
			    $fileVersion = [int]::Parse($file.Name.Substring(0, 4))
			    Write-Host "Apply script: $($file.FullName)"
			
			    $syncScript = New-SqlScript -scriptPath $file.FullName -sqlVariables $sqlVars
                if ($syncScript.timeOut -eq $null) {
                    $syncScript.timeOut = $executionTimeOut
                }
			    $syncScript.connectionString = $connectionString
                Invoke-SqlScript $syncScript
			    # Update the database with current version number
                #This could be an issue for any models where a db version is updated outside of this execution (e.g. in a script that is run)
			    Update-VersionLevel -connectionString $connectionString -version $fileVersion @metaData
            }
		}
	}
}

#see if there's scripts to execute.
Function Test-SyncDatabase ([System.IO.FileSystemInfo[]] $fileSystemInfoCollection, [string]$connectionString, [string]$databaseName, [int]$executionTimeOut = 30) {
	$metaHashScript = Get-ScriptsToRun $fileSystemInfoCollection $connectionString $databaseName $executionTimeOut
	if ($scriptMetaHash.PSBase.Count -gt 0) { return $true }
	else { return $false }
}

Export-ModuleMember  New-DatabaseWithMigrations, Get-VersionLevel, Sync-Database, Test-SyncDatabase, Get-ScriptMigrationMetadata
