# *************************************************************************
# ©2012 Ed-Fi Alliance, LLC. All Rights Reserved.
# *************************************************************************

. (Get-CorePath "logistics\scripts\activities\build\initialize-database\initialize-database-psake.ps1")
$currentContext = $psake.context.Peek()
$currentContext.tasks.Remove('EnsureBindings')
task EnsureBindings {
    Write-Host "Binding deployment override"
}


task RestorePopulatedDbTemplate -depends DropDatabase,CreateDatabaseByRestoringPopDbTemplate,InitializeEncryption,ApplyEntityFrameworkMigrations,SyncDbTypes

task CreateDatabaseByRestoringPopDbTemplate -precondition {(-not (Test-DatabaseExists $sqlServer $sqlDatabaseName))} -depends InitializeSqlLogins { 
    . $folders.activities.invoke('build/copy-populatedtemplate/copy-populatedtemplate.vars.ps1')
    if ($msSqlBackupPath) {
        $restorableBackup = cp $SamplesOdsFullPath $msSqlBackupPath -passthru
    }
    else {
        $restorableBackup = cp $SamplesOdsFullPath $sqlServer.BackupDirectory -passthru
    }
    Write-Host "Restoring the Ed-Fi sample database [$databaseName] from '$SamplesOdsFullPath'..."
    $rdbargs = @{
        sqlserver = $sqlServerName
        databasename = $initializeCSB["Database"]
        username = $dbAdminSecureCreds.username
        password = $dbAdminSecureCreds.password
        backupfile = $restorableBackup
    }
    $dbLoaderSecureCreds = Get-PSCredential      "DB Loader($sqlServerName)"
    if ($dbLoaderSecureCreds) {
        $rdbargs += @{ usersToReassociate = @{ $dbLoaderSecureCreds.username = $dbLoaderSecureCreds.password } }
    }
    Restore-Database @rdbargs

    $script:dbCreated = $true
}
