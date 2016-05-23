properties {
    $latestPackageModule = Select-EdFiFiles "logistics\scripts\modules\package\" "get-latest-package.psm1"
    if ($latestPackageModule) {Import-Module $latestPackageModule.FullName}
    Import-Module "$($folders.modules.invoke('utility\credential-management.psm1'))" -force
    . "$($folders.activities.invoke(`"deployment\$deploymentConfiguration.vars.ps1`"))"

    $localWebAppPackageDir = "$($folders.base.invoke(''))$deploymentSourceName\"

    # Initialize the primary named credentials needed by this deployment script
    $credmd = @(
        @{Name = "Application ($deploymentConfiguration)"; DefaultUsername = $null; Description = "Credentials used to access the $deploymentConfiguration database in support of the $deploymentConfiguration web application."; IntegratedSecuritySupported = $true},
        @{Name = "Remote Deployer"; DefaultUsername = $null; Description = "Credentials for DOGWOOD for the deploy folder."; IntegratedSecuritySupported = $true},
        @{Name = "Publish User ($webAppServerName)"; DefaultUsername = $null; Description = "Credentials for webapp server that is the deployment destination."; IntegratedSecuritySupported = $true}
        @{Name = "SMTP User ($deploymentConfiguration)"; DefaultUsername = $null; Description = "Credentials for SMTP server for logging."; IntegratedSecuritySupported = $true}
    )
    Add-CredentialMetadata $credmd
    #Do this here so that you don't have to be deploy to set credentials
    foreach($ldapEntry in $ldapEntries) {
        $normLdapEntry = $ldapEntry.Replace(" ", "")
        # Initialize the named credentials specified by this ldap ldapEntry
        Add-CredentialMetadata @( @{
            Name = "$normLdapEntry LDAP User ($deploymentConfiguration)"; 
            DefaultUsername = $null; 
            Description = "Credentials for LDAP user."; 
            IntegratedSecuritySupported = $true
        })
    }

    # The credentials file should be for the user this script will be executing under.
    Initialize-Credentials "$($folders.activities.invoke('deployment\'))"

    #deployer reads from the build server.
        $deployerUsername        = Get-Username("Remote Deployer")
        $deployerPassword        = Get-PlaintextPassword("Remote Deployer")
    #publisher is the user on the WebApp Server.    
        $publishUser             = Get-Username("Publish User ($webAppServerName)")
        $securePublishPassword   = Get-SecurePassword("Publish User ($webAppServerName)")
    #webapp sql server user identity
        $sqlUser                 = Get-Username("Application ($deploymentConfiguration)")
        $sqlPass                 = Get-PlaintextPassword("Application ($deploymentConfiguration)")
    #webapp loging smtp server user identity
        $smtpUser                 = Get-Username("SMTP User ($deploymentConfiguration)")
        $smtpPass                 = Get-PlaintextPassword("SMTP User ($deploymentConfiguration)")

    #indicator of whether a deployment should occur
        $global:shouldDeploy = $true
        
    #list of repos for remote deployment
        $global:remoteDeployRepos = @()
}

task default -depends Deploy

task GetLatestSetupDeploy -depends GetLatestPackage,RemoteSetup,Deploy

task GetLatestPackage {
    $packageType = $($deploymentSourceName + "." + $publishType)
    #Check for newer WebApp Code, should not deploy if there is not new code    
    $global:shouldDeploy = Get-LatestDeploymentPackage $deployerUsername $deployerPassword "" "$packageType" "$($folders.base.invoke(''))" -extractPackage
}

task RemoteSetup {
    if($shouldDeploy){
            try {
            #Some quasi Unique drive name
            $deployDriveName = "DeployServer"
            $deployDrive = "$deployDriveName`:"
            #Path to deploy the package to:
            $remoteDeploymentPath  = "\\$webAppServerName\$webAppShareName"
            
            # Use the deploy user's credentials to copy files by creating a DeployServer:\ PSDrive.
            # Frequently, this is the same as credentials of the build agent account, which this script will be running under. 
            # However, if the account is different, or the build agent machine is not on the same domain as the webservers, 
            # this is useful        

            #remove trailing slashes because the provider doesn't like them.
            $pathToLoad = [string]$remoteDeploymentPath.TrimEnd('\')
            write-host "Creating $deployDrive\ PSDrive, rooted at '$pathToLoad', using publish credentials"
            if (Test-Path "$deployDrive") { Remove-PSDrive -Name $deployDriveName }
            if ($publishCreds) {
                New-PSDrive -name $deployDriveName -psprovider FileSystem -root $pathToLoad -credential $publishCreds | Out-Null
            }
            else {
                New-PSDrive -name $deployDriveName -psprovider FileSystem -root $pathToLoad | Out-Null
            }

            Write-Host "Copying deployment artifacts... `n"
            
            $deploymentArtifacts = @()
            $deploymentArtifacts += (dir $folders.tools.invoke('SharpZipLib\ICSharpCode.SharpZipLib.dll'))
            $deploymentArtifacts += (dir $folders.modules.invoke('psake\psake.psm1'))
            $deploymentArtifacts += (dir $folders.modules.invoke('database\database-management.psm1'))
            $deploymentArtifacts += (dir $folders.modules.invoke('database\database-utility.psm1'))
            $deploymentArtifacts += (Select-RepositoryResolvedFiles 'logistics\scripts\modules\utility\' '*.ps*1')
            $deploymentArtifacts += (dir $folders.modules.invoke('path-resolver.psm1'))
            $deploymentArtifacts += (Select-RepositoryResolvedFiles 'logistics\scripts\activities\deployment\' "*.vars.ps1")
            $deploymentArtifacts += (Select-RepositoryResolvedFiles 'logistics\scripts\activities\deployment\web\' 'remote-deploy-*.ps*1')
            $deploymentArtifacts += (Select-RepositoryResolvedFiles 'logistics\scripts\activities\' '*common.ps*1')
            
            foreach ($file in $deploymentArtifacts) {
                if ($file.GetType().name -match '(File|Directory)Info$') {
                    $fullname = $file.fullname
                }
                else {
                    $fullname = get-item $file
                }
                $foundRepoRoot = $false
                foreach ($repoRoot in Get-RepositoryRoot) {
                    $repoName = split-path -leaf $repoRoot
                    $escRegex = [Regex]::Escape($repoRoot.tolower()) 
                    if ($fullname -match "^$escRegex")  { 
                        $relPath = $fullname -replace $escRegex,''
                        $destPath = "$deployDrive\$repoName\$relPath"
                        $destParentPath = Split-Path $destPath
                        if (-not $(Test-Path $destParentPath)){ md $destParentPath | out-null}
                        Copy-Item $fullname -Destination $destPath -Force
                        $global:remoteDeployRepos += $repoName
                        $foundRepoRoot = $true
                        break
                    }
                }
                if (-not $foundRepoRoot) {
                    throw "File '$($artifact.fullname)' does not appear to be in one of the repository roots."
                }
            }

            $destPath = "$deployDrive\WebApp"
            #Special Handling for Binaries (Move to Nuget when possible with the source as the deploying server's TeamCity/localdir?)
            if (-not $(Test-Path $destPath)){ md $destPath | out-null}
            Write-Host "Transfering web application binaries to: `"$destPath`"."
            Copy-Item "$localWebAppPackageDir\WebApp*.zip" -Destination $destPath -Force
            
            Write-Host "Completed transfer of deployment artifacts... `n"
        }
        finally {
            if (Test-Path "$deployDrive") { Remove-PSDrive -Name $deployDriveName }
        }
    }
    else {
        Write-Host "No web application changes, skipping transfer of deployment artifacts."
    }
}   

task Deploy {
    if ($shouldDeploy) {
        Write-Host "`nPerforming deployment..."

        #$credentialsList = @{}
        $credentialsList = @()
        write-host "Preparing SQL credentials..."
        $credentialsList += @(@{name="sql"; type="sql"; user=$sqlUser; pass=$sqlPass;})
        if ("$smtpUser" -ne "") {
            write-host "Preparing SMTP credentials..."
            $credentialsList += @(@{name = "smtp"; type = "smtp"; user = $smtpUser; pass = $smtpPass; })
        }
        foreach($ldapEntry in $ldapEntries) {
            $normLdapEntry = $ldapEntry.Replace(" ", "")
            Write-Host "Preparing Credentials for $ldapEntry..."
            $ldapu = Get-Username("$normLdapEntry LDAP User ($deploymentConfiguration)")
            $ldapp = Get-PlaintextPassword("$normLdapEntry LDAP User ($deploymentConfiguration)")
            $credentialsList += @(@{ name = $normLdapEntry; type = "ldap"; user = $ldapu; pass = $ldapp })
        }
        Write-Host "Begining $deploymentConfiguration deployment to $webAppServerName ..."
        # Execute a remote command to execute remote-deploy.ps1
        $remoteAccessCred = New-Object System.Management.Automation.PSCredential ($publishUser, $securePublishPassword)
        $rdpath = "$($folders.activities.invoke('deployment\web\remote-deploy.ps1'))"
        $rdargs = @(
            "$deploymentConfiguration",
            $specificEntityTypes,
            $webAppShareName,
            $credentialsList,
            $folders,
            "$legacyVersionNum",
            @("Application","STS")
        )
        #Sync the repo orders and only send repos that we need.
        $cannonicalRepos = Get-RepositoryNames
        $orderedDeploymentRepos = @()
        foreach ($repo in $cannonicalRepos) {
            if ($global:remoteDeployRepos -contains $repo) {
                $orderedDeploymentRepos += $repo
            }
        }
        
        $deploymentRepoEnvString = ($orderedDeploymentRepos -join ";")
        
        $webSession = New-PSSession -ComputerName $webAppServerName -Credential $remoteAccessCred
        $pathResolverOverrideSB = [Scriptblock]::Create("`$env:PathResolverRepositoryOverride='$deploymentRepoEnvString'")
        Invoke-Command -Session $webSession -ScriptBlock $pathResolverOverrideSB
        Invoke-Command -Session $webSession -FilePath $rdpath -ArgumentList $rdargs 
        #Removing the exit-PSSesion due to errors
        #Exit-PSSession $webSession
        #Hack to make production install if there is a new WebApp package. Need to think of a better way to do this.
        #It forces staging to remove the fact that a new package was already retrieved so that production will deploy it too.
        if ((Test-Path "$($folders.base.invoke(''))Hosting-WebApp-Version.txt") -and $deploymentConfiguration -eq "Staging") {
            Remove-Item "$($folders.base.invoke(''))Hosting-WebApp-Version.txt" -Force
        }
    }
    else {
        Write-Host "No web application changes, skipping deployment."
    }
}
