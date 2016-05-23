Param($webAppServerName, $deploymentConfiguration)
if (-not (get-module |? { $_.Name -eq "path-resolver" })) {
    Import-Module $PSScriptRoot\..\..\..\modules\path-resolver.psm1
}
Import-Module "$($folders.modules.invoke('utility\credential-management.psm1'))" -force
. "$($folders.activities.invoke('deployment\' + $deploymentConfiguration + '.vars.ps1'))"

if ("$webAppName" -eq "") {
    $websiteNames = @{"$deploymentNamePrefix$deploymentNameSuffix" = $null;
                      "$($deploymentNamePrefix)STS.$deploymentNameSuffix" = $null;}
}
else {
    $websiteNames = @{"$deploymentNamePrefix$deploymentNameSuffix" = @("$webAppName", "$($webAppName)_STS");}
}
Add-CredentialMetadata `
@(
    @{Name = "Publish User ($webAppServerName)"; DefaultUsername = $null; Description = "Credentials for webapp server that is the deployment destination."; IntegratedSecuritySupported = $true}
)

# The credentials file should be for the user this script will be executing under.
Initialize-Credentials "$($folders.activities.invoke('deployment'))"
    
#publisher is the user on the WebApp Server.    
$publishUser             = Get-Username("Publish User ($webAppServerName)")
$securePublishPassword   = Get-SecurePassword("Publish User ($webAppServerName)")

foreach($websiteName in $websiteNames.Keys) {

    #Script to run remotely
    $remoteScript = {
        Set-ExecutionPolicy Unrestricted -scope process
        Import-Module WebAdministration
        #Setup Variables from input object
        $Input.'<>4__this'.read() | %{$vars = $_}
        foreach ($var in $vars.Keys) {if ("$var" -ne ""){Set-Variable $var -Value $($vars[$var]) -Scope Script -Force}}
        Write-Host "Starting Application Pool Recycle for: $websiteName on $env:computername"
        if (Test-Path "IIS:\Sites\$websiteName") {
            $site = Get-Item "IIS:\Sites\$websiteName"
        } else {
            throw "Website: $websiteName Could not be found."
        }
        
        if ($webApps) {
            #watch out for the dynamic variable typing here.
            foreach ($webApp in $webApps) {
                $webAppInstance = dir "IIS:\Sites\$($websiteName)\" | where { $_.name -eq $webApp}
                if (-not ($webAppInstance)) {
                    #Can't Throw here because we have more than one app pool to deal with.
                    Write-host "WebApp: $webApp Could not be found."
                }
                else {
                    Write-Host "Verifying AppPool: $($webAppInstance.applicationPool) is up..."
                    $appPool = Get-Item "IIS:\APPPOOLS\$($webAppInstance.applicationPool)"
                    #Make sure appPool is up to prevent error.
                    if($appPool.state -ne "Started") {
                        Write-Host "AppPool was down! Starting..."
                        $appPool.Start()
                    }
                    Write-Host "Done."
                    Write-Host "Preparing to Recycle AppPool: $($webAppInstance.applicationPool) ..."
                    #Recycle AppPool
                    $appPool.Recycle()
                    Write-Host "Recycle Completed."
                    #Make sure the apppool is up.
                    Write-Host "Verifying AppPool: $($webAppInstance.applicationPool) is up..."
                    $tries = 0
                    while (ls IIS:\AppPools | where {$_.Name -eq "$($webAppInstance.applicationPool)" -and $_.State -ne "Started"}) {
                        Start-WebItem IIS:\AppPools\$($webAppInstance.applicationPool)
                        $tries += 1
                        if ($tries -ge 50) {
                            throw "Unable to start appPool: $($webAppInstance.applicationPool)"
                        }
                        Sleep 1
                    }
                    Write-Host "Verified."
                }
            }
        }
        else {
            Write-Host "Verifying AppPool: $($site.applicationPool) is up..."
            $appPool = Get-Item "IIS:\APPPOOLS\$($site.applicationPool)"
            #Make sure appPool is up to prevent error.
            if($appPool.state -ne "Started") {
                Write-Host "AppPool was down! Starting..."
                $appPool.Start()
            }
            Write-Host "Done."
            Write-Host "Preparing to Recycle AppPool: $($site.applicationPool) ..."
            #Recycle AppPool
            $appPool.Recycle()
            Write-Host "Recycle Completed."
            #Make sure the apppool is up.
            Write-Host "Verifying AppPool: $($site.applicationPool) is up..."
            $tries = 0
            while (ls IIS:\AppPools | where {$_.Name -eq "$($site.applicationPool)" -and $_.State -ne "Started"}) {
                Start-WebItem IIS:\AppPools\$($site.applicationPool)
                $tries += 1
                if ($tries -ge 50) {
                    throw "Unable to start appPool: $($site.applicationPool)"
                }
                Sleep 1
            }
            Write-Host "Verified."
        }
    }
    Write-Host "Starting Remote Execution on server: $webAppServerName"
    $remoteAccessCred = New-Object System.Management.Automation.PSCredential ($publishUser, $securePublishPassword)
    Invoke-Command $remoteScript -Credential $remoteAccessCred -ComputerName $webAppServerName -InputObject @{websiteName = $websiteName; webApps = $websiteNames[$websiteName]}
}