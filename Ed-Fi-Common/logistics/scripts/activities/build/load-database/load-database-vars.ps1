#Set Thread Limits
if ($environment -eq "cloud-integration") {
    $threadCount = [Math]::Max(4, [int] ([Environment]::ProcessorCount * 2))
} else {
    # Throttle script threads during the workday, if needed.
    if (Test-CurrentlyBusinessHours) {
        $threadCount = if ([int]$peakLoadThreads -eq 0) {2} else {[int]$peakLoadThreads}
    } else {
        #$threadCount = [Math]::Min(6, [int] ([Environment]::ProcessorCount * 1.5))
        $threadCount = if ([int]$offPeakLoadThreads -eq 0) {2} else {[int]$offPeakLoadThreads}
    }
}
#This is currently capped here because of issues with the SSIS packages using more than 4 threads to load.
#The belief is this is possibly related to some form of latent locking. The behavior is such that when some SSIS
#packages execute simultaneously, all of the SQL Server connections related to the package execution become suspended,
#the packages then hang indefinitely.
$threadCount = [Math]::Min(3,$threadCount)

# If you want the database backups to be *published* to another server at the end of the load step, 
# put the destination direct here as a UNC path (it is probably most appropriate to do this in the .vars file specific
# to the environment e.g. integration/testing/deployment/whatever):
#$publishDatabaseBackupDestination = ""

$connectionStringBuilders = @{}
$transientCSBs = @{}
$adminCSBs = @{}
$sqlDbName = ""
if ("$defaultOleDbProvider" -eq "") { $defaultOleDbProvider = "SQLNCLI11"}
foreach ($dbTypeName in $dbTypeNames) {
    $sqlDbName = Get-ProjectSpecificDatabaseName $appTypePrefix $normSpecificEntityType $dbTypeName $buildConfigurationName $schoolyear $legacyVersionNum
    Write-Host "Adding connection string for $sqlDbName on server $sqlServerName ..."
    $dbCSB = New-Object System.Data.Common.DbConnectionStringBuilder $true
    $dbCSB["Server"]              = $sqlServerName
    $dbCSB["Database"]            = $sqlDbName
    $dbCSB["Provider"]            = $defaultOleDbProvider
    if ($dbLoaderCreds -ne $null) {
        $dbCSB["Uid"]             = $dbLoaderCreds.UserName
        $dbCSB["Pwd"]             = $dbLoaderCreds.Password
    }
    else {
        $dbCSB["Trusted_Connection"] = "yes"
    }
    $transientCSBs.Add("$dbTypeName", $dbCSB)
    
    $adminDbCSB = Copy-DbConnectionStringBuilder $dbCSB
    #Reset credentialing from object copy
    $adminDbCSB.Remove("Trusted_Connection") | Out-Null
    $adminDbCSB.Remove("Uid") | Out-Null
    $adminDbCSB.Remove("Pwd") | Out-Null
    
    if ($dbAdminCreds -ne $null) {
        $adminDbCSB["Uid"] = $dbAdminCreds.UserName
        $adminDbCSB["Pwd"] = $dbAdminCreds.Password
    }
    else {
        $adminDbCSB["Trusted_Connection"] = "yes"
    }
    $adminCSBs.Add("$dbTypeName", $adminDbCSB)
}
$connectionStringBuilders = $connectionStringBuilders + $transientCSBs 

#Get SqlServer Object for db existence checks
$sqlServer = Get-Server $sqlServerName $dbAdminSecureCreds.UserName $dbAdminSecureCreds.Password

#Add packages variables hashtable
$packageVariables = @{}

#Vars for postloads and QA.
$sqlVars = @{}
#Commenting out this line. It needs to be in the implementation repos for anyone not on v.Next, but should no longer be in core.
#$sqlVars.EdFiSanityScript = if($edfiStandardVersion) {($folders.base.invoke("Etl\EdFi.Etl.EdFi\$edfiStandardVersion\QA Scripts\EdFiSanity.txt")).Path} else {($folders.base.invoke('Etl\EdFi.Etl.EdFi\QA Scripts\EdFiSanity.txt')).Path}
$sqlVars.IntegrationTempKeyword = $buildConfigurationName
$sqlVars.StagingKeyword = $environment

if (-not (test-path variable:leaSubTypeRegex)) {
    $leaSubTypeRegex = $specificEntitySubTypeRegex
}

function Publish-DatabaseBackup {
    param(
        [parameter(mandatory=$true)] [string] $backupFile
    )

    if ($loadedDatabaseBackupPaths -eq $null) {
        $loadedDatabaseBackupPaths = @()
    }
    if ("$publishDatabaseBackupDestination" -ne "") {
        $loadedDatabaseBackupPaths += $publishDatabaseBackupDestination
    }
    #only do this logic if we have destinations.
    if($loadedDatabaseBackupPaths.Count -gt 0) {
        $oneMustSucceed = @()
        if ($error.Count -gt 0) {
            $errorCache = $error.ToArray()
            $error.Clear()
        }
        else { $errorCache = $null }
        foreach ($path in $loadedDatabaseBackupPaths) {
            try {
                #Some quasi Unique drive name
                $driveName = "LDBP113"
                #remove trailing slashes because the provider doesn't like them.
                $pathToLoad = [string]$path.TrimEnd('\')
                if (Test-Path "$driveName`:") { Remove-PSDrive -Name $driveName }
                if ($loadedDatabaseBackupPSCredentials) {
                    New-PSDrive -name $driveName -psprovider FileSystem -root $pathToLoad -credential $loadedDatabaseBackupPSCredentials | Out-Null
                }
                else {
                    New-PSDrive -name $driveName -psprovider FileSystem -root $pathToLoad | Out-Null
                }
            
                $serverIsUnreachable = $true
                $timeOut = (Get-Date) + (New-TimeSpan 0:00:00 0:01:00)
                while ($serverIsUnreachable -and (Get-Date) -lt $timeOut) {
                    try {
                        $serverIsUnreachable = $false
                        copy-item $backupFile "$driveName`:"
                    }
                    catch {
                        $serverIsUnreachable = $true
                        Sleep 5
                    }
                }
                $oneMustSucceed += (-not $serverIsUnreachable)
            }
            finally {
                if (Test-Path "$driveName`:") { Remove-PSDrive -Name $driveName }
            }
        }
        #There should be a bitwise way to do this but this is faster to remember right now.
        if($oneMustSucceed -notcontains $true) {
            #Stop processing if all copies fail
            throw $error
        }
        elseif ($oneMustSucceed -contains $false) {
            Write-Warning "Errors occured during backup transfer, however at least one succeeded. Errors were as follows:`r`n" + ($error -join "`r`n")
            $error.Clear()
        }
        if ($errorCache -ne $null) {
            #Restore previous errors if present.
            $error.AddRange($errorCache)
        }
    }
}


#---------------------------------------- For Legacy --------------------------remove later!:
$connectionStrings = @{}
$transientConnectionStrings = @{}
#for postloads (won't accept OLE Conn string):
$transientSqlConnectionStrings = @{}

foreach ($connectionName in $transientCSBs.Keys){
    $transientConnectionStrings[$connectionName] = (Get-OleDbConnectionString $transientCSBs[$connectionName])
    $transientSqlConnectionStrings[$connectionName] = (Get-SqlConnectionString $transientCSBs[$connectionName])
}
$connectionStrings = $connectionStrings + $transientConnectionStrings

$adminConnectionStrings = @{}
foreach ($connectionName in $adminCSBs.Keys){
    $adminConnectionStrings[$connectionName] = (Get-SqlConnectionString $adminCSBs[$connectionName])
}
#----------------------------------------------------------------------------------------------
