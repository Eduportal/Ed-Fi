#choco Install Unofficial.Microsoft.SQLServer.SMO -source https://www.nuget.org/api/v2/
#Trying Add-Type $env:chocolatyInstall\lib\Unofficial.Microsoft.SQLServer.SMO.11.0.3000.0\lib
$assembliesToLoad = @("Microsoft.SqlServer.SMO","Microsoft.SqlServer.SmoExtended","Microsoft.SqlServer.ConnectionInfo","Microsoft.SqlServer.SqlEnum")
foreach ($assembly in $assembliesToLoad) {
    $dllPath = "$env:ChocolateyInstall\lib\Unofficial.Microsoft.SQLServer.SMO.11.0.3000.0\lib\$assembly.dll"
    if (Test-Path $dllPath) {
        #Use the file if it is there
        Add-Type -Path $dllPath | Out-Null
        Write-Verbose "Loaded $assembly from $dllPath"
    }
    else {
        #Otherwise use the assembly from the GAC (Can't use add type here without specifying the full assembly version).
        [System.Reflection.Assembly]::LoadWithPartialName($assembly) | Out-Null
    }
}

$script:modulePath = $MyInvocation.MyCommand.Path
if (-not (get-module |? { $_.Name -eq "path-resolver" })) {
    Import-Module $PSScriptRoot\..\path-resolver.psm1
}
Import-Module "$($folders.modules.invoke('database\database-utility.psm1'))"

Function Copy-DbConnectionStringBuilder {
    [cmdletbinding()]
    Param(
        [Parameter(Mandatory=$true)]
            [System.Data.Common.DbConnectionStringBuilder] $dbCSB
    )
    $newDbCSB = New-Object System.Data.Common.DbConnectionStringBuilder $true
    #Copy object
    foreach($key in $dbCSB.Keys) {
        $newDbCSB[$key] = [String]::Copy($dbCSB[$key])
    }
    return $newDbCSB
}

Function Backup-Database ([string]$sqlServer, [string]$databaseName, [string] $username = $null, [System.Security.SecureString] $password = $null, [string] $backupDirectory) { #, [bool]$createZip) {
	$backupFilePath = Backup-DbCore $sqlServer $databaseName $username $password $backupDirectory "database" #$true
	return $backupFilePath
}

Function Backup-TransactionLog ([string]$sqlServer, [string]$databaseName, [string] $username = $null, [System.Security.SecureString] $password = $null, [string]$backupDirectory) { #, [bool]$createZip) {
	$backupFilePath = Backup-DbCore $sqlServer $databaseName $username $password $backupDirectory "log" #$true
	return $backupFilePath
}

Function Backup-DbCore ([string]$sqlServer, [string]$databaseName, [string] $username = $null, [System.Security.SecureString] $password = $null, [string]$backupDirectory, [string] $action) { #, [bool]$createZip) {
 trap [Exception] { 
        write-error $("ERROR: " + $_.Exception.ToString()); 
        break; 
    }

    $server = Get-Server $sqlServer $username $password
    $local = @("localhost","127.0.0.1",$env:ComputerName)
    if (Test-DatabaseExists $server $databaseName) {
        # Construct file names -- $timestamp = Get-Date -format yyyyMMddHHmmss
        if ($action -eq "log") {$bakFilePath = "$backupDirectory${databaseName}_log.bak"}
		else {$bakFilePath = "$backupDirectory$databaseName.bak"}
        $zipFilePath = "$backupDirectory$databaseName.zip"
        
        #Run setup/clean up if the sql server isthe machine runing the script, or if it is a remote path.
        if($local -contains $sqlServer -or ($backupDirectory.StartsWith("\\") -and (Test-Path "$([Io.Path]::GetPathRoot($backupDirectory))"))) {
            # Make sure the backup folder exists
            [IO.Directory]::CreateDirectory($backupDirectory) | Out-Null

            # Delete the existing backup file
            if (Test-Path $bakFilePath) {
                ri $bakFilePath
            }
        }
        else { 
            Write-Host "The machine executing this script is not the the SQL Server and the backup path is not remote.`r`nLocal backup paths are local to the SQL server. Skipping preliminary backup path validations."
        }
        
        # Create a backup object
        $smoBackup = New-Object ("Microsoft.SqlServer.Management.Smo.Backup")
        
        $smoBackup.Action = $action
        $smoBackup.BackupSetDescription = "Full " + $action + " Backup for " + $databaseName
        $smoBackup.BackupSetName = $databaseName + " " + $action + " Backup"
        $smoBackup.CompressionOption = "On"
        $smoBackup.Database = $databaseName
        $smoBackup.MediaDescription = "Disk"
        $smoBackup.Devices.AddDevice($bakFilePath, "File")
        
        $smoBackup.SqlBackup($server)

        return $bakFilePath
    }
    else {
		if ($action -eq "log") {Write-Host "Cannot backup log, database: $databaseName`nIt was not found to exist on server: $sqlServer"}
        else {Write-Host "Cannot backup database: $databaseName`nIt was not found to exist on server: $sqlServer"}
    }
}

#region ConnectionStrings
    #OLEDB
    Function Get-OleDbConnectionString {
        [cmdletbinding()]
        Param(
        [Parameter(Position=0, Mandatory=$true, ParameterSetName='legacy')]
            [string] $serverName,
        [Parameter(Position=1, Mandatory=$true, ParameterSetName='legacy')]
            [string] $databaseName,
        [Parameter(Position=2, Mandatory=$false, ParameterSetName='legacy')]
            [Net.NetworkCredential] $creds,
        [Parameter(Position=0, Mandatory=$true, ParameterSetName='CSB')]
            [System.Data.Common.DbConnectionStringBuilder] $dbCSB
        )
        if ($PsCmdlet.ParameterSetName -eq "legacy") {
            if (($creds -eq $null) -or ($creds.UserName -eq "")) {
                return "Provider=SQLNCLI10;Server=$serverName;Database=$databaseName;Trusted_Connection=yes;"
            } else {
                return "Provider=SQLNCLI10;Server=$serverName;Database=$databaseName;Uid=$($creds.UserName);Pwd=$($creds.Password);"
            }
        }
        else {
            $localCSB = Copy-DbConnectionStringBuilder $dbCSB
            #Multisubnet failover does not work with OLEDB : http://www.mattmasson.com/index.php/2012/03/alwayson-multi-subnet-failover-and-ssis/
            $localCSB.Remove("MultiSubnetFailover") | Out-Null
            #Not doing userID checking logic. The creator of the dbCSB object in the vars file should know if integrated security is configured for the environment.
            return (New-Object System.Data.OleDb.OleDbConnectionStringBuilder $localCSB.ConnectionString).ConnectionString
        }
    }

    Function Get-OracleOleDbConnectionString ([string] $dataSourceName, [Net.NetworkCredential] $creds) {
        #FOR CSB the regular OLEDB connection string provider should be used and the additional parameters added to the dbCSB in the vars file.
        if (($creds -eq $null) -or ($creds.UserName -eq "")) {
            "Provider=OraOLEDB.Oracle;Data Source=$dataSourceName;OSAuthent=1;"
        } else {
            "Provider=OraOLEDB.Oracle;Data Source=$dataSourceName;User Id=$($creds.UserName);Password=$($creds.Password);"
        }
    }

    #SQL   
    Function Get-SqlConnectionString {
        [cmdletbinding()]
        Param(
        [Parameter(Position=0, Mandatory=$true, ParameterSetName='legacy')]
            [string] $serverName,
        [Parameter(Position=1, Mandatory=$true, ParameterSetName='legacy')]
            [string] $databaseName,
        [Parameter(Position=2, Mandatory=$false, ParameterSetName='legacy')]
            [Net.NetworkCredential] $creds,
        [Parameter(Position=0, Mandatory=$true, ParameterSetName='CSB')]
            [System.Data.Common.DbConnectionStringBuilder] $dbCSB
        )
        if ($PsCmdlet.ParameterSetName -eq "legacy") {
            if (($creds -eq $null) -or ($creds.UserName -eq "")) {
                "Server=$serverName;Database=$databaseName;Trusted_Connection=True;"
            } else {
                "Server=$serverName;Database=$databaseName;User ID=$($creds.UserName);Password=$($creds.Password);"
            }
        }
        else {
                (Convert-CommonDbCSBtoSqlCSB $dbCSB).ConnectionString
        }
    }
#endregion

function Convert-CommonDbCSBtoSqlCSB {
    [cmdletbinding()]
    Param(
    [Parameter(Position=0, Mandatory=$true)]
        [System.Data.Common.DbConnectionStringBuilder] $dbCSB
    )
    $localCSB = Copy-DbConnectionStringBuilder $dbCSB
    
    #Remove problematic kpv's
    $localCSB.Remove("Provider") | Out-Null
    $localCSB.Remove("AutoTranslate") | Out-Null
    #Not doing userID checking logic. The creator of the dbCSB object in the vars file should know if integrated security is configured for the environment.
    return New-Object System.Data.SqlClient.SqlConnectionStringBuilder $localCSB.ConnectionString
}

Function Enable-DatabaseEncryption([Microsoft.SqlServer.Management.Smo.Database] $db, $encryptionAlgorithm, $encryptionType, $encryptorName) {
    if ($db.EncryptionEnabled -eq $true) {
        Write-Host "Encryption already enabled."
    }
    else {
        Write-Host "Enabling $encryptionAlgorithm database encryption using certificate $encryptorName ..."
        $db.DatabaseEncryptionKey.EncryptionAlgorithm = $encryptionAlgorithm
        $db.DatabaseEncryptionKey.EncryptionType = $encryptionType
        $db.DatabaseEncryptionKey.EncryptorName = $encryptorName
        $db.EncryptionEnabled = $true
        $db.Alter()
    }
}

Function Get-Server([string]$sql_server, [string]$username = $null, [System.Security.SecureString]$password = $null) {
    if ($username) {
        # Initialize Server instance using standard security
        # Write-Host "Connecting to SQL Server using standard security for user '$username'..."

        $s = New-Object ('Microsoft.SqlServer.Management.Smo.Server') $sql_server
        $s.ConnectionContext.LoginSecure = $false
        $s.ConnectionContext.Login = $username
        $s.ConnectionContext.SecurePassword = $password
    } else {
        # Initialize Server instance using integrated security
        # Write-Host "Connecting to SQL Server using integrated security..."

        $s = new-object ('Microsoft.SqlServer.Management.Smo.Server') $sql_server
    }
    
    # Don't let operations performed on this Server time out
    $s.ConnectionContext.StatementTimeout = 0
    
    $s
}

Function Initialize-SqlLogin ([string] $serverName, [string] $loginName, [Security.SecureString] $securePassword, [string] $adminUser, [Security.SecureString] $adminSecurePassword, [Microsoft.SqlServer.Management.Smo.Server] $serverInstance) {
    if (-not $serverInstance) {
        $serverInstance = Get-Server $serverName $adminUser $adminSecurePassword 
    }
    $login = $serverInstance.Logins | where {$_.Name -eq $loginName}
    # Before creating, check if login already exists
    if (-not($login)) {
        $login = New-Object 'Microsoft.SqlServer.Management.SMO.Login' $serverInstance, $loginName
        #$login.DefaultDatabase = $defaultDatabaseName
        $login.LoginType = [Microsoft.SqlServer.Management.Smo.LoginType]::SqlLogin
        #$login.Enable() 
        $login.Create($securePassword)
        Write-Host "Login '$loginName' created."
    }
    return $login
}

Function Invoke-SqlQuery ([string]$connectionString, [string]$sql) {
    Invoke-SqlScript $connectionString $sql
}

Function Invoke-SqlReader {
    [cmdletbinding()]
    Param(
    [Parameter(Position=0, Mandatory=$true, ParameterSetName='object')]
    [ValidateScript({
        #Must have connection string
        (-not [string]::IsNullOrWhitespace($_.connectionString)) -and
        #Must have timeout
        ($_.timeOut -ne $null) -and
        #Must have a batch
        ($_.scriptBatches.Count -gt 0) 
    })]
        [PSCustomObject]$sqlscript,
    [Parameter(Position=0, Mandatory=$true, ParameterSetName='legacy')]
        [string] $connectionString,
    [Parameter(Position=1, Mandatory=$true, ParameterSetName='legacy')]
        [string]$sql,
    [Parameter(Position=2, Mandatory=$false, ParameterSetName='legacy')]
        [Int32]$commandTimeout
    )
    #This function is here for legacy use. New uses should call Invoke-SqlScript with the -returnDataSet flag.
    if ($PsCmdlet.ParameterSetName -eq "object") {
        $dataSets = Invoke-SqlScript $sqlscript -returnDataSet
    }
    else {
        if ($PSBoundParameters.ContainsKey('commandTimeout')){
            $dataSets = Invoke-SqlScript $connectionString $sql $commandTimeout -returnDataSet
        }
        else {
            $dataSets = Invoke-SqlScript $connectionString $sql -returnDataSet
        }
    }
    $result = @{}
    $index = 0
    foreach ($dataSet in $dataSets) {
        foreach ($table in $dataSet.Tables) {
            foreach ($row in $table.Rows) {
                $result.add($index,$row.ItemArray)
                $index++ | Out-Null
            }
        }
    }
    return $result
}

Function Invoke-SqlForXmlQuery ([string]$connectionString, [string]$sqlForXml) {
    $connection = New-Object System.Data.SqlClient.SqlConnection $connectionString
    $connection.Open()

    $command = New-Object System.Data.SqlClient.SqlCommand $sqlForXml,$connection
    $reader = $command.ExecuteXmlReader()
    $reader.MoveToContent() | out-null
    $reader.ReadOuterXml()

    $connection.Close()
}

Function Invoke-SqlScript {
    [cmdletbinding()]
    Param(
    [Parameter(Position=0, Mandatory=$true, ParameterSetName='object')]
    [ValidateScript({
        #Must have connection string
        (-not [string]::IsNullOrWhitespace($_.connectionString)) -and
        #Must have timeout
        ($_.timeOut -ne $null) -and
        #Must have a batch
        ($_.scriptBatches.Count -gt 0) 
    })]
        [PSCustomObject] $sqlscript,
    [Parameter(Position=0, Mandatory=$true, ParameterSetName='legacy')]
        [string]$connectionString,
    [Parameter(Position=1, Mandatory=$true, ParameterSetName='legacy')]
        [string]$sql,
    [Parameter(Position=2, Mandatory=$false, ParameterSetName='legacy')]
        [Int32]$commandTimeout,
    [Parameter(Position=3, Mandatory=$false, ParameterSetName='legacy')]
    [Parameter(Position=1, Mandatory=$false, ParameterSetName='object')]
        [switch]$returnDataSet
    ) 
    if ($PsCmdlet.ParameterSetName -eq "legacy") {
        #We don't have the variables in context here, ignore metadata.
        $newSqlScript = New-SqlScript -scriptSql $sql -ignoreMetadata
        $newsqlScript.connectionString = $connectionString
        #Only override the timeout if it is passed
        if ($PSBoundParameters.ContainsKey('commandTimeout')) {
            $newsqlScript.timeout = $commandTimeout
        }
        #Set default timeout if we haven't been given one.
        if ($newsqlScript.timeOut -eq $null) {
            $newsqlScript.timeOut = 30
        }
        #Callback with object to enforce validation.
        return Invoke-SqlScript $newsqlscript -returnDataSet:$returnDataSet.IsPresent
    } 

    #setup connection.
    $conn = new-object Microsoft.SqlServer.Management.Common.ServerConnection
    $conn.ConnectionString = $sqlscript.connectionString
    $conn.StatementTimeout = $sqlScript.timeOut
    $srv = new-object Microsoft.SqlServer.Management.Smo.Server($conn)
    try {    
        #Initialize Variables
        $resultDataSets = @()
        $variableBlock = $sqlscript.scriptVariables -join [Environment]::NewLine
        foreach ($batch in $sqlScript.scriptBatches) {
                # Skip empty commands
                if ([string]::IsNullOrWhitespace($batch.script)) {
                    continue
                }
                $sqlToExecute = $variableBlock + [Environment]::NewLine + $batch.script
                try {
				    trap [Exception] {
						#Enhance error output to include full stack from inside DLL call.
						write-error $("ERROR: " + $_.Exception.ToString());
						break;
					}
                    for ($i=0; $i -lt $batch.executionCount; $i++) {
                        if ($returnDataSet.IsPresent) {
                            $resultDataSets += $srv.ConnectionContext.ExecuteWithResults($sqlToExecute)
                        } 
                        else {
                            $srv.ConnectionContext.ExecuteNonQuery($sqlToExecute) | Out-Null
                        }
                    }
                }
                catch {
                    Write-Output "Script Stacktrace: " $_.ScriptStackTrace
                    Write-Output "Failing script: " $sqlToExecute
                    Write-Output "Last successful command:" $previousBatch
                    throw $_
                }
                
                # Make note of last successful command to aid in any troubleshooting
                $previousBatch = $sqlToExecute
        }
    }
    finally {    
        # Clean up   
        $srv.ConnectionContext.Disconnect()
    }
    if ($returnDataSet.IsPresent) {
        return $resultDataSets
    }
    else { return }
}

Function Clear-DatabaseUsers ([string]$sql_server, [string]$database_name, [string] $username = $null, [System.Security.SecureString] $password = $null, [switch]$safe) {
    $s = Get-Server $sql_server $username $password
    if (Test-DatabaseExists $s $database_name) {
        if ($password -eq $null) {
            $netCreds = new-object Net.NetworkCredential($username, $null)
        }
        else {
            $netCreds = new-object Net.NetworkCredential($username, ([System.Runtime.InteropServices.marshal]::PtrToStringAuto([System.Runtime.InteropServices.marshal]::SecureStringToBSTR($password))))
        }
        $connectionString = Get-SqlConnectionString $sql_server "master" $netCreds
        #Try to kill db connections ahead of time.
        $killUsersSQL = @"
                        -- Create the sql to kill the active database connections 
                        declare @execSql varchar(1000), @databaseName varchar(100) 
                        -- Set the database name for which to kill the connections 
                        set @databaseName = '$($database_name)' 
                        set @execSql = '' 
                        select @execSql = @execSql + 'kill ' + convert(varchar(10), spid) + ' ' 
                       from master..sysprocesses 
                       where db_name(dbid) = '$database_name'
                        and DBID <> 0 
                        and spid <> @@spid 
                        and status <> 'background' 
                        -- to get the user process
                        and status in ('runnable','sleeping') 
                        exec (@execSql) 
                        GO
"@
        Invoke-SqlScript $connectionString $killUsersSQL
        #This is an additional check that takes the database offline.
        if ($safe) {
            #$db = $s.Databases.Item("$database_name")
            #Try to make sure there is nothing pending. This didn't work.
            #$s.KillAllProcesses("$database_name") $db.SetOffline(); $db.SetOnline();
            #This is very fast if nothing is pending, otherwise it will cause a wait, but prevent an error.
            
            #Allow local path determination if running without path resolver.
            $this = if ($folders) {"$($folders.modules.invoke('\database\database-management.psm1'))"} else {"$script:modulePath"}
            $this = Resolve-Path $this
            $scriptCommand = {$vars = $null
            $Input.'<>4__this'.read() | %{$vars = $_};
            foreach ($var in $vars.Keys) {if ("$var" -ne ""){Set-Variable $var -Value $($vars[$var]) -Scope Script -Force}};
            Import-Module $this;
            $s = Get-Server $sql_server $username $password;
            $db = $s.Databases.Item("$database_name");
            $db.SetOffline();
            $db.SetOnline();}
            $inputHash = @{sql_server = $sql_server; username = "$username"; database_name = $database_name; this = $this}
            if ($password -ne $null){ $inputHash.Add("password", $password) }
            
            $task = "$scriptCommand $inputHash"
            $anotherThread = [Powershell]::Create()
            $anotherThread.AddScript($scriptCommand)
            $anotherThread.AddParameters($inputHash)
            $job1 = $anotherThread.BeginInvoke()
            $timeout = 0
            do
            {
                if($job1.AsyncState -eq "Failed") { throw "Stopping $database_name failed!"; break}
                    if($timeout % 10 -eq 0) { Write-Host "Status: $($job1.AsyncState)" }
                    Start-Sleep 1
                    $timeout++
            }
            while ($timeout -lt 600 -and -not($job1.IsCompleted))
            
            if ($timeout -ge 599) {
                throw "Failed to stop $database_name after 10 min. Please check if there are any active connection to the database."
            }
            
        }
        return $true
    }
    else {
        Write-Host "$database_name does not exist."
        return $false
    }
}

# Will delete the database if it exists
Function Remove-Database ([string]$sql_server, [string]$database_name, [string] $username = $null, [System.Security.SecureString] $password = $null, [switch]$safe) {
    try {
        Write-Host "Starting removal of database $database_name ..."
        if (Clear-DatabaseUsers $sql_server $database_name $username $password -safe:$safe) {
            $s = Get-Server $sql_server $username $password
            $s.KillDatabase($database_name)
            Write-Host "Removed database $database_name"
        }
        else {
            Write-Host "Database was not removed."
        }
    }
    catch {
        Write-Host "Attempt to remove database $database_name failed: `n`t$_`n" -ForegroundColor Red
    }
}

Function Rename-Database ([string]$sql_server, [string]$database_name, [string]$newDatabaseName, [string] $username = $null, [System.Security.SecureString] $password = $null, [switch]$clobber) {
    try {
        Write-Host "Starting rename of database '$database_name' to '$newDatabaseName' ..."
        if (Clear-DatabaseUsers $sql_server $database_name $username $password) {
            $s = Get-Server $sql_server $username $password
            #See if we have a database with the destination name already.
            if (Test-DatabaseExists $s $newDatabaseName) {
                if ($clobber) { 
                    Remove-Database $sqlDataSourceName $newDatabaseName $username $password
                }
                else {
                    throw "A database already exists with the target name: '$newDatabaseName'!" 
                }
            }
            $db = New-Object Microsoft.SqlServer.Management.Smo.Database
            $db = $s.Databases.Item($database_name)
            $db.Rename($newDatabaseName)
            $db.Refresh()
            Write-Host "Renamed database '$database_name' to '$newDatabaseName'"
            return $true
        }
        else { 
            Write-Host "Database was not renamed."
            return $false
        }
    }
    catch {
        Write-Host "Attempt to rename database '$database_name' to '$newDatabaseName' failed: `n`t$_`n" -ForegroundColor Red
        return $false
    }
}

<#
    .Synopsis
    Get a valid log and/or data file location for e.g. database restores. 
    .Description
    Query a db server SMO and return a valid log and/or data file location for 
    database restores. If the server has set a DefaultFile property, return it.
    If not, use the MasterDBPath property. 
#>
function Get-SmoFileLocation {
    [cmdletbinding()]
    param(
        [Parameter(mandatory=$true)] [Microsoft.SqlServer.Management.Smo.Server] $server,
        [Parameter(mandatory=$true)] [ValidateSet("Data","Log")] [string] $fileType
    )
    $SmoProps = @{
        Data = @{
            Default = "DefaultFile"
            Master = "MasterDBPath"
        }
        Log = @{
            Default = "DefaultLog"
            Master = "MasterDBLogPath"
        }
    }
    foreach ($ft in $fileType) {
        $default = $server.$($SmoProps.$ft.Default)
        $master = $server.$($SmoProps.$ft.Master)
        if ($default) {
            write-verbose "Using Default at '$default' for the $ft files"
            return $default
        }
        else {
            write-verbose "Using Master at '$master' for the $ft files"
            return $master
        }
    }
}

Function Restore-TransactionLog {
	[cmdletbinding()]
	param(
		[parameter(mandatory=$true)] [string] $sqlServer, 
		[parameter(mandatory=$true)] [string] $databaseName, 
		[string] $username, 
		[System.Security.SecureString] $password,
		[parameter(mandatory=$true)] [string] $backupFile, 
		[Hashtable] $usersToReassociate, 
		[string] $dataPath, 
		[string] $logPath, 
		[switch] $norecovery
	) 
	
	Restore-DbCore $sqlServer $databaseName $username $password  $backupFile $usersToReassociate $dataPath $logPath "log" $norecovery $false
}

Function Restore-Database {
	[cmdletbinding()]
	param(
		[parameter(mandatory=$true)] [string] $sqlServer, 
		[parameter(mandatory=$true)] [string] $databaseName, 
		[string] $username, 
		[System.Security.SecureString] $password,
		[parameter(mandatory=$true)] [string] $backupFile, 
		[Hashtable] $usersToReassociate, 
		[string] $dataPath, 
		[string] $logPath, 
		[switch] $norecovery
	) 
	
	Restore-DbCore $sqlServer $databaseName $username $password  $backupFile $usersToReassociate $dataPath $logPath "database" $norecovery $true
}

Function Restore-DbCore {
    [cmdletbinding()]
    param(
        [parameter(mandatory=$true)] [string] $sqlServer,
        [parameter(mandatory=$true)] [string] $databaseName,
        [string] $username,
        [System.Security.SecureString] $password,
        [parameter(mandatory=$true)] [string] $backupFile,
        [Hashtable] $usersToReassociate,
        [string] $dataPath,
        [string] $logPath,
		[parameter(mandatory=$true)] [string] $action, 
		[boolean] $norecovery, 
		[parameter(mandatory=$true)] [boolean] $replace
    )

    trap [Exception] {
        write-error $("ERROR: " + $_.Exception.ToString());
        break;
    }

    $server = Get-Server $sqlServer $username $password
    $smoRestore = New-Object("Microsoft.SqlServer.Management.Smo.Restore")

    # Add the bak file as a device
    $backupDevice = New-Object("Microsoft.SqlServer.Management.Smo.BackupDeviceItem") ($backupFile, "File")
    $smoRestore.Devices.Add($backupDevice)

    if (-not $dataPath) { $dataPath = Get-SmoFileLocation -server $server -filetype data }
    if (-not $logPath)  { $logPath  = Get-SmoFileLocation -server $server -filetype log }
   
   foreach ($row in $smoRestore.ReadFileList($server).Rows) {
       if ([IO.Path]::GetExtension($row[1]) -eq ".ldf") {
            $designatedPath = $logPath
       }
       else {
            $designatedPath = $dataPath
       }
       $dbFilePath = $row[1]
       $dbFile = New-Object("Microsoft.SqlServer.Management.Smo.RelocateFile")
       $dbFile.LogicalFileName = $row[0]
       $dbFile.PhysicalFileName = [IO.Path]::Combine($dataPath, $databaseName + [IO.Path]::GetExtension($dbFilePath))
       Write-Host "dbFile.LogicalFileName = $($dbFile.LogicalFileName)"
       Write-Host "dbFile.PhysicalFileName = $($dbFile.PhysicalFileName )"
       $smoRestore.RelocateFiles.Add($dbFile) | out-null
    }   

   # Set options
   $smoRestore.Database = $databaseName
   $smoRestore.NoRecovery = $norecovery
   $smoRestore.ReplaceDatabase = $replace
   $smoRestore.Action = $action

   $backupSets = $smoRestore.ReadBackupHeader($server)
   $smoRestore.FileNumber = $backupSets.Rows.Count # Most recent backup in the set

   # Write-Host "DEBUG: backup sets in file: $($smoRestore.FileNumber)"

   # Show notifications
   $smoRestore.PercentCompleteNotification = 10

   #$smoRestoreDetails = $smoRestore.ReadBackupHeader($server)
   #"Database Name from Backup Header : " + $smoRestoreDetails.Rows[0]["DatabaseName"]

   # Forcibly close all connections on the target database
   $server.KillAllProcesses($databaseName)

    #create server users for db users to be re-assoicated
    if ($usersToReassociate -ne $null) {
        foreach ($user in $usersToReassociate.Keys) {
            write-host "Reassociating user: $user"
            Initialize-SqlLogin $null "$user" $usersToReassociate["$user"] -serverInstance $server
        }
    }

   # Restore the database
   # Trap { Write-Host -Fore Red -back White $_.Exception.ToString(); Break; };
   $smoRestore.SqlRestore($server)

   # Reassociate user accounts with logins on server
   # (Useful in scenarios where database is being restored on a different server)
   if ($usersToReassociate -ne $null) {
       $db = $server.Databases[$databaseName]

       foreach ($user in $usersToReassociate.Keys) {
          Write-Host "Reassociating user account '$user' ..."
          $query = "IF EXISTS (SELECT name FROM sys.database_principals WHERE name = `'$user`')"
          $query += "BEGIN `nALTER USER $user with LOGIN=$user `nEND"
          $db.ExecuteNonQuery($query)
       }
   }
}

Function Test-ConnectionString ([string]$connectionString) {
    [System.Data.SqlClient.SqlConnection]::ClearAllPools()
    
    try    {
        ## Connect to the data source and open it
        $connection = New-Object System.Data.SqlClient.SqlConnection $connectionString
        $connection.Open()
        $connection.Close()
        $true
    }
    catch {
        $false
    }
}

Function Test-DatabaseExists([Microsoft.SqlServer.Management.Smo.Server]$server, $databaseName) {
    $found = $false
    #Makesure we have the latest database information.
    $server.Databases.Refresh($true)
    foreach ($db in $server.Databases) {
        if ($db.Name -eq $databaseName) {
            $found = $true
            break
        }
    }
    return $found
}

Function Add-DbAvailabilityGroup ([string]$listener, [string]$databaseName, [string]$AGName,  [string] $username = $null, [System.Security.SecureString] $password = $null, [switch] $force, $dbDomainSuffix) { 
 trap [Exception] { 
        write-error $("ERROR: " + $_.Exception.ToString()); 
        break; 
    }
	#Use the listener to get the primary database server
	$server = Get-Server $listener $username $password

	#Get the Availability Group 
	$AvailabilityGroup = $server.AvailabilityGroups | where { $_.Name -eq $AGName}
	
	$AvailabilityDb = New-Object Microsoft.SqlServer.Management.Smo.AvailabilityDatabase($AvailabilityGroup,$databaseName)

	#Get list of secondaries that are online in the availability group
	$operationalSecondaries = Get-AvailabilityGroupSecondaries $AGName $server
	
	#Get list of secondaries that are in a state other than online in the availability group
	$defunctSecondaries = Get-AvailabilityGroupSecondaries $AGName $server -notOnline
	
	#if not a force, then fail if any secondaries are defunct, otherwise add db to availability group, and add to all active secondaries
	if ($defunctSecondaries.Count -ge 1) {
		if (-not $force) {
			throw "[String]::Join(', ', $defunctSecondaries) Secondary Replicas are not Online. Database $databaseName was not added to the Availability Group $AGName"
		}
		else {
			Write-Warning "[String]::Join(', ', $defunctSecondaries) Secondary Replicas are not Online. Database $databaseName could not be added to these Secondary Replicas for Availability Group $AGName.  Availability Group $AGName will be in a compromised state."
			
			$AvailabilityDb.Create()
		
			foreach ($replica in $operationalSecondaries) {
				$fqdnReplica = if($dbDomainSuffix){"${replica}.$dbDomainSuffix"} else{"$replica"}
				$serverSecondary = Get-Server $fqdnReplica $username $password
				$serverSecondary.AvailabilityGroups[$AGName].AvailabilityDatabases[$databaseName].JoinAvailablityGroup() 
			}
		}
	}
	else {
		$AvailabilityDb.Create()
		
		foreach ($replica in $operationalSecondaries) {
			$fqdnReplica = if($dbDomainSuffix){"${replica}.$dbDomainSuffix"} else{"$replica"}
			$serverSecondary = Get-Server $fqdnReplica $username $password
			$serverSecondary.AvailabilityGroups[$AGName].AvailabilityDatabases[$databaseName].JoinAvailablityGroup() 
		}
	}
}

Function Remove-DbAvailabilityGroup ([string]$listener, [string]$databaseName, [string]$agName,  [string] $username = $null, [System.Security.SecureString] $password = $null, [switch] $force, $dbDomainSuffix) { 
 trap [Exception] { 
        write-error $("ERROR: " + $_.Exception.ToString()); 
        break; 
    }

	$server = Get-Server $listener $username $password
	
	#Get list of secondaries that are online in the availability group
	$operationalSecondaries = Get-AvailabilityGroupSecondaries $AGName $server
	
	#Get list of secondaries that are in a state other than online in the availability group
	$defunctSecondaries = Get-AvailabilityGroupSecondaries $AGName $server -notOnline

	#if not a force, then fail if any secondaries are defunct, otherwise remove db from availability group, drop the database on the primary and all secondaries
	if ($defunctSecondaries.Count -ge 1) {
		if (-not $force) {
			throw "[String]::Join(', ', $defunctSecondaries) Secondary Replicas are not Online. Database $databaseName was not removed from the Availability Group $AGName and the database was not dropped"
		}
		else {
			Write-Warning "[String]::Join(', ', $defunctSecondaries) Secondary Replicas are not Online. Database $databaseName will not be dropped from those replicas"	
			
			$server.AvailabilityGroups[$AGName].AvailabilityDatabases[$databaseName].Drop() 
			
			Remove-Database $listener $databaseName $username $password 
			Start-Sleep 10
			if ($operationalSecondaries.Count -ge 1) {
				foreach ($replica in $operationalSecondaries) {
					$fqdnReplica = if($dbDomainSuffix){"${replica}.$dbDomainSuffix"} else{"$replica"}
					Write-Host "Dropping database $databaseName from Secondary replica $fqdnReplica ..."
					$replicaserver = Get-Server $fqdnReplica $username $password
					$replicadb = $replicaserver.Databases.Item($databaseName)
					$replicadb.Drop()
				}
			}
		}
	}
	else {
		$server.AvailabilityGroups[$AGName].AvailabilityDatabases[$databaseName].Drop() 
		
		Remove-Database $listener $databaseName $username $password 
		Start-Sleep 10
		foreach ($replica in $operationalSecondaries) {
			$fqdnReplica = if($dbDomainSuffix){"${replica}.$dbDomainSuffix"} else{"$replica"}
			Write-Host "Dropping database $databaseName from Secondary replica $fqdnReplica ..."
			$replicaserver = Get-Server $fqdnReplica $username $password
			$replicadb = $replicaserver.Databases.Item($databaseName)
			$replicadb.Drop()
		}
	}
}


Function Get-AvailabilityGroupSecondaries { 
<#
    .Synopsis
    Returns the secondary servers in the Availability Group

    .Description 
    Get the secondary replica server names for the specified availability group. By default returns only the online servers.

    .Parameter listenerName
    The server name of the listener to be queried regarding the availability group.
    
    .Parameter listenerUsername
    The user name to use when establishing a connection with the server specified by listenerName. Omit for integrated security.
    
    .Parameter listenerPassword
    The password to use when establishing a connection with the server specified by listenerName. Omit for integrated security.
    
    .Parameter server
    The server object to use when querying the availability group.
    
    .Parameter agName
    The name of the availability group to retrieve the secondary servers from.
    
    .Parameter notOnline
    Switch to indicate to return only the secondary replicas that are not online.
    
    .Parameter all
    Switch to indicate to return all the secondary replicas.
#>
[cmdletbinding()]
Param(
    [Parameter(Position=0, Mandatory=$true)]
        [string]$agName,
    [Parameter(Position=1, Mandatory=$true, ParameterSetName='noServer')]
    [Parameter(Position=1, Mandatory=$true, ParameterSetName='noServerNotOnline')]
    [Parameter(Position=1, Mandatory=$true, ParameterSetName='noServerAll')]
        [string] $listenerName,
    [Parameter(Position=2, Mandatory=$false, ParameterSetName='noServer')]
    [Parameter(Position=2, Mandatory=$false, ParameterSetName='noServerNotOnline')]
    [Parameter(Position=2, Mandatory=$false, ParameterSetName='noServerAll')]
        [string] $listenerUsername = $null,
    [Parameter(Position=3, Mandatory=$false, ParameterSetName='noServer')]
    [Parameter(Position=3, Mandatory=$false, ParameterSetName='noServerNotOnline')]
    [Parameter(Position=3, Mandatory=$false, ParameterSetName='noServerAll')]
        [System.Security.SecureString] $listenerPassword = $null,
    [Parameter(Position=1, Mandatory=$true, ParameterSetName='server')]
    [Parameter(Position=1, Mandatory=$true, ParameterSetName='serverNotOnline')]
    [Parameter(Position=1, Mandatory=$true, ParameterSetName='serverAll')]
        [Microsoft.SqlServer.Management.Smo.Server] $server,
    [Parameter(Position=4, Mandatory=$true, ParameterSetName='noServerNotOnline')]
    [Parameter(Position=2, Mandatory=$true, ParameterSetName='serverNotOnline')]
        [switch]$notOnline,
    [Parameter(Position=4, Mandatory=$true, ParameterSetName='noServerAll')]    
    [Parameter(Position=2, Mandatory=$true, ParameterSetName='serverAll')]
        [switch]$all
    )  
    trap [Exception] { 
        write-error $("ERROR: " + $_.Exception.ToString()); 
        break; 
    }
    if ($PsCmdlet.ParameterSetName -like "noServer*") {
        $server = Get-Server $listenerName $listenerUsername $listenerPassword
	}
	#Get list of secondaries in the availability group
	$secondaries = @()
	$secondaries += $server.AvailabilityGroups[$agName].AvailabilityReplicas | Where-Object {$_.Role -eq "Secondary"} 
    $resultSecondaries = @()
    if ($notOnline) {
        $resultSecondaries += $secondaries | Where {$_.MemberState -ne "Online"} | Select -ExpandProperty name
	}
    elseif ($all) {
        $resultSecondaries = $secondaries | Select -ExpandProperty name
    }
    else {
        $resultSecondaries += $secondaries | Where {$_.MemberState -eq "Online"} | Select -ExpandProperty name
    }
	return $resultSecondaries
}

Function Enable-CLR {
    [cmdletbinding()]
    param(
        [parameter(mandatory=$true)] [string] $sqlServer,
        [string] $username,
        [System.Security.SecureString] $password
    )

    trap [Exception] {
        write-error $("ERROR: " + $_.Exception.ToString());
        break;
    }

	$server = Get-Server $sqlServer $username $password
    $server.Configuration.IsSQLClrEnabled.ConfigValue = 1
	$server.Configuration.Alter()
}

Function New-SqlServerJob {
    [cmdletbinding()]
    param(
        [parameter(mandatory=$true)] [string] $sqlServer,
        [string] $username,
        [System.Security.SecureString] $password,
		[parameter(mandatory=$true)] [string] $jobName
    )

    trap [Exception] {
        write-error $("ERROR: " + $_.Exception.ToString());
        break;
    }
	
	$server = Get-Server $sqlServer $username $password
	
	if (-not ($server.JobServer.Jobs.Contains($jobName) ) ) {
		$job = New-Object Microsoft.SqlServer.Management.SMO.Agent.Job($server.JobServer, $jobName)
		$job.Create()
		$job.ApplyToTargetServer($sqlServer)
	}
}

Export-ModuleMember Backup-Database, Enable-DatabaseEncryption, Remove-Database, Initialize-SqlLogin, Invoke-SqlQuery, Invoke-SqlForXmlQuery, Invoke-SqlScript, Get-OleDbConnectionString, Get-Server, Get-SqlConnectionString, Restore-Database, Test-ConnectionString, Test-DatabaseExists, Invoke-SqlReader, Get-OracleOleDbConnectionString, Rename-Database, Copy-DbConnectionStringBuilder, Backup-TransactionLog, Restore-TransactionLog, Add-DbAvailabilityGroup, Remove-DbAvailabilityGroup, Get-AvailabilityGroupSecondaries, Enable-CLR, New-SqlServerJob, Convert-CommonDbCSBtoSqlCSB