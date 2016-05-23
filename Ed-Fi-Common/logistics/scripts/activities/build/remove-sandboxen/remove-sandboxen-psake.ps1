properties {
    . $folders.activities.invoke("build\$environment.vars.ps1")

    Import-Module $folders.modules.invoke('utility\build-utility.ps1') -force
    Import-Module $folders.modules.invoke('utility\credential-management.psm1') -force
    Import-Module $folders.modules.invoke('database\database-management.psm1') -force
    Import-Module $folders.modules.invoke('database\database-migrations.psm1') -force
    Import-Module $folders.scripts.Invoke('activities\common.psm1')

    . $folders.activities.invoke('build\remove-sandboxen\remove-sandboxen.credentials.ps1')

    Initialize-Credentials $folders.activities.invoke('build')
    $dbAdminCreds       = Get-NetworkCredential "DB Admin($sqlServerName)"
    $dbAdminSecureCreds = Get-NetworkCredential "DB Admin($sqlServerName)"

    $masterCSB = New-Object System.Data.Common.DbConnectionStringBuilder $true
    $masterCSB["Server"]              = $sqlServerName
    $masterCSB["Database"]            = "master"
    $masterCSB["Provider"]            = $defaultOleDbProvider
    if ($dbAdminCreds.UserName -and $dbAdminCreds.password) {
        $masterCSB["Uid"] = $dbAdminCreds.UserName
        $masterCSB["Pwd"] = $dbAdminCreds.Password
    }
    else {
        $masterCSB['Trusted_Connection'] = $true
    }
    $connectionString = Get-SqlConnectionString -dbCSB $masterCSB
}

task default -depends RemoveSandboxen

task RemoveSandboxen {
    $result = Invoke-SqlReader -connectionstring $connectionstring -sql 'select name from sys.databases;'
    $databases = [object[]]$result.values

    Write-Host "Removing Sandboxes from $sqlServerName"
    $sandboxes = @()
    foreach ($db in $databases) {
        if ($db.StartsWith("EdFi_Ods_Sandbox_")) {
            $sandboxes += @($db)
        }
    }
    write-host "Found $($sandboxes.count) sandboxes."
    foreach ($sandbox in $sandboxes) {
        Write-Host "DELETE database '$sandbox'"
        Remove-Database -sql_server $sqlServerName -database_name $sandbox -username $dbAdminCreds.username -password $dbAdminSecureCreds.SecurePassword
    }
}


