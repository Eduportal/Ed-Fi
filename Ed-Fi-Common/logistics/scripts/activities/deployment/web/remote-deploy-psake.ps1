$ErrorActionPreference = "Stop"
properties {
    Import-Module WebAdministration
    $configPath = "deployment\$($deploymentConfiguration).vars.ps1"
    . "$($folders.activities.invoke($configPath))"
    Import-Module "$($folders.activities.invoke('common.psm1'))"
    Import-Module "$($folders.modules.invoke('database\database-management.psm1'))"
    . "$($folders.modules.invoke('utility\build-utility.ps1'))" 
    Import-Module "$($folders.modules.invoke('utility\permissions.psm1'))" -force
    #If applicable, the name of the webApp we are deploying.
    #$webAppName = "EdFiTest" Should be set in the build configuration file.
    #indicator of whether this is deployment is at root or in a webApplication subfolder.
    $isWebAppDeployment = if("$webAppName" -eq "") {$false} else {$true}
    . "$($folders.activities.invoke(`"deployment\web\remote-deploy-$($websiteType).vars.ps1`"))"
    
    #The environment type of configs to use.
    #$configEnvironmentType - located in deploymentConfiguration vars file. 
    
    #The type of website to be deployed.
    #$websiteType - Passed on psake invocation.
    
	#The legacy zip package name. (From website types vars)
	#$deploymentPackageName
	
	#The project name of the Web project to be deployed.
	#$deploymentProjectName
	
    #Name of the cert to be used for SSL
    #$sslFriendlyCertName - located in deploymentConfiguration vars file. 
    
    #Configuration type for this deployment
    #$deploymentConfiguration  - Passed on psake invocation.

    #Indicates whether the appPool should load a user profile.
    #$loadUserProfile - Located in the websiteType vars file

    #SET names.
    #$specificEntityTypes - Passed on psake invocation.

    #The credentials for district specific configurations
    #$credentialsList

    #The port the website is being deployed to.
    $webSitePort = if($siteIsSSL) {443} else {80}

    #The name of the website we are deploying:
    #$websiteName  - Located in the websiteType vars file

    #The ipAddress we are deploying for. '*' is all unassigned.
    $webSiteIP = '*'

    #Website Object. Value is set during "CreateWebsite" and utilized in subsequent steps
    $Script:deploymentWebsite = $null
    #Object that is operated on during deployment. Typically WebApplication or Website as properties are utilized.
    $Script:deploymentArtifact = $null
    
    $Script:appPools = @()
}
#The order here is VERY important.
task default -depends InitializeAppPool, RemoveWebsite, CreateWebsite, EnsureBindings, CreateWebApplication, ApplyArtifactIpFilters, DeployBinaries, PerformCustomConfiguration, StartWebsiteAndAppPool
#task webAppDeployment -depends InitializeAppPool, CreateWebsite, EnsureBindings, CreateWebApplication, ApplyArtifactIpFilters, DeployBinaries, PerformCustomConfiguration, StartWebsiteAndAppPool
#task websiteDeployment -depends InitializeAppPool, RemoveWebsite, CreateWebsite, EnsureBindings, ApplyArtifactIpFilters, DeployBinaries, PerformCustomConfiguration, StartWebsiteAndAppPool

task CreateAppPool {
    #If the apppool doesn't exist, create it 		
    #It should be named after the websiteName
    if (-not (Test-Path "IIS:\AppPools\$websiteName")) {
        # Create new .NET 4.0 AppPool for website
        Write-Host "Creating .NET v4.0 AppPool $websiteName..."
        $Script:appPools += New-WebAppPool $websiteName
    }
    elseif (-not $isWebAppDeployment) {
        #Only do these changes to the website appPool if this is not a subdir deployment
        $Script:appPools += (dir IIS:\AppPools\ | where {$_.Name -eq $websiteName})
    }
    if ($isWebAppDeployment) {
        if (-not (Test-Path "IIS:\AppPools\$webAppName")) {
            # Create new .NET 4.0 AppPool for website
            Write-Host "Creating .NET v4.0 AppPool $webAppName..."
            $Script:appPools += New-WebAppPool $webAppName
        }
        else {
            $Script:appPools += (dir IIS:\AppPools\ | where {$_.Name -eq $webAppName})
        }
    }
}

task InitializeAppPool -depends CreateAppPool {
    #set the appropriate properties on the appPools
    foreach ($appPool in $appPools) {
        #Configure AppPool
        Set-ItemProperty "IIS:\AppPools\$($appPool.name)" managedRuntimeVersion v4.0
        #set idle timeout to 1 day.
        Write-Host "Setting appPool $($appPool.name) IdleTimeout..."     
        Set-ItemProperty "IIS:\AppPools\$($appPool.name)" -Name "ProcessModel.Idletimeout" -value "1.00:00:00"            
        if ($loadUserProfile) {
            Write-Host "Setting appPool $($appPool.name) to Load User Profile..."
            Set-ItemProperty "IIS:\AppPools\$($appPool.name)" -name "ProcessModel.loadUserProfile" -value $true

        }
    }
}

task RemoveWebsite -precondition {return (-not $isWebAppDeployment)} {
    if (Test-Path IIS:\Sites\$websiteName) {
        Remove-Item IIS:\Sites\$websiteName -recurse -force
    }  
}

task CreateWebsite {
    #If the website doesn't exist, create it.
    if (-not (Test-Path IIS:\Sites\$websiteName)) {

        #Get the web path from Default Web Site else use default
        if (Test-Path 'IIS:\Sites\Default Web Site') {
           $defaultPath = Split-Path -Parent $(Get-Item 'IIS:\Sites\Default Web Site').PhysicalPath
           $websitePath = "$defaultPath\$websiteName"
        }
        else {
           $websitePath = "%systemdrive%\inetpub\$websiteName"
        }
        
        #-SSL sets https binding however the certificate used is based on the previously configured SSL bindings for the website port.
        #New-WebSite -Name Integration -Port 444 -HostHeader Integration -ApplicationPool Integration -Ssl -Force -PhysicalPath "$env:systemdrive\inetpub\Integration"
        if($siteIsSSL) {
            if ($applyHostHeader) {
                $Script:deploymentWebsite = New-WebSite -IPAddress $webSiteIP -Name $websiteName -HostHeader $websiteName -Port $websitePort -ApplicationPool $websiteName -Ssl -Force -PhysicalPath $websitePath
            }
            else {
                $Script:deploymentWebsite = New-WebSite -IPAddress $webSiteIP -Name $websiteName -Port $websitePort -ApplicationPool $websiteName -Ssl -Force -PhysicalPath $websitePath        
            }
        }
        else {
            #Production
            if ($applyHostHeader) {
                $Script:deploymentWebsite = New-WebSite -IPAddress $webSiteIP -Name $websiteName -HostHeader $websiteName -Port $websitePort -ApplicationPool $websiteName -Force -PhysicalPath $websitePath
            }
            else {
                $Script:deploymentWebsite = New-WebSite -IPAddress $webSiteIP -Name $websiteName -Port $websitePort -ApplicationPool $websiteName -Force -PhysicalPath $websitePath    
            }
        }
    }
    else {
        $Script:deploymentWebsite = Get-website | where { $_.Name -eq $websiteName}
    }
    $Script:deploymentArtifact = $Script:deploymentWebsite
}

<#
Using wild card cert instead.
task CreateSSLBinding {
    #remove existing binding
    If (Test-Path IIS:\SslBindings\0.0.0.0!$websitePort) {
        Remove-Item IIS:\SslBindings\0.0.0.0!$websitePort
    }
    #Create SSL binding using the sslFriendlyCertName on the websitePort
    Get-ChildItem cert:\LocalMachine\Root | where { $_.FriendlyName -like "$sslFriendlyCertName" } | select -First 1 | New-Item IIS:\SslBindings\0.0.0.0!$websitePort | Out-Null
} #>

task EnsureBindings {
    #May should add certificate lookup by common name from deployment configuration vars file: dir cert:\localmachine\my | where {$_.Subject -eq "CN=*.dlpdemo.com"}
    #and registration with 0.0.0.0 on 443. Problems may include broken bindings for existing sites.
    #http://learn.iis.net/page.aspx/491/powershell-snap-in-configuring-ssl-with-the-iis-powershell-snap-in/

    $protocol = if($siteIsSSL) {'https'} else {'http'}
    #Make sure that only this website has https bindings on this port.
    #Iterate all websites, with https protool bindings on the sasme port with the same host header that is being used for the website being deployed
    #Remove conflicting bindings for any site that isn't the one being deployed.
    foreach ($site in (Get-ChildItem IIS:\Sites)) {
        #these get-webbinding calls have a problem with websites with no/bad bindings.
        if ($applyHostHeader) {
            Get-WebBinding -name $site.name -IPAddress $webSiteIP -protocol $protocol -HostHeader $websiteName -port $websitePort | % { if($site.name -ne $websiteName){ $_ | Remove-WebBinding}}
        }
        else {
            Get-WebBinding -name $site.name -IPAddress $webSiteIP -protocol $protocol -port $websitePort | % { if($site.name -ne $websiteName -and $_.bindingInformation.EndsWith(":")){ $_ | Remove-WebBinding}}
        }
    }
}

task RemoveWebApplication {
    $existingWebApp = dir "IIS:\Sites\$($deploymentWebsite.name)\" | where { $_.name -eq $webAppName} #$_.NodeType -eq "application" -and
    if($existingWebApp){
        Remove-WebApplication -name $webAppName -site $deploymentWebsite.name
    }
}

task CreateWebApplication -precondition {return $isWebAppDeployment} -depends RemoveWebApplication {

    $webAppPath = "$($deploymentWebsite.PhysicalPath)\$webAppName"
    if (-not (Test-Path $webAppPath)) { md $webAppPath -force }
    New-WebApplication -Site $deploymentWebsite.Name -Name $webAppName -PhysicalPath $webAppPath -ApplicationPool $webAppName -Force

    $Script:deploymentArtifact =  dir "IIS:\Sites\$($deploymentWebsite.name)\" | where { $_.name -eq $webAppName} #$_.NodeType -eq "application" -and 
}

#From this point on the deployment for WebApp and the Website are identical.
#$Script:deploymentArtifact houses the item being deployed.

task StopArtifactAppPool {
    Stop-DeploymentArtifactAppPool
}

task CleanBinaries -depends StopArtifactAppPool {
    $artifactPath = [Environment]::ExpandEnvironmentVariables($deploymentArtifact.physicalPath)
    #Check if there is already a Web.Config
    if(Test-Path "$artifactPath\Web.Config") {
        #If so back it up:
        $backUpFolderName = if($isWebAppDeployment) {"$($deploymentWebsite.name)-$($deploymentArtifact.name)"} else {$deploymentArtifact.name}
        Write-Host "Backing up existing Web.Config ... `n"
        #Make sure the destination folder exists or it throws an error since we're trying to modify the file name in flight.
        if(-not (Test-Path "$localDeploymentPath\Backup\$backUpFolderName\")) {
            md "$localDeploymentPath\Backup\$backUpFolderName\" | Out-Null
        }
        Copy-Item "$artifactPath\Web.Config" -Destination "$localDeploymentPath\Backup\$backUpFolderName\Web.Config.bak" -Force
    }
    
    if (($artifactPath -ne $null) -and (Test-Path $artifactPath)) {
    	Write-Host "Clean up: Removing existing $artifactPath ... `n"
        Remove-Item $artifactPath -force -Recurse
    }
    
    #Put the folder back
    if (-not (Test-Path $artifactPath)) {
        Write-Host "Creating $artifactPath ... `n"
        md $artifactPath | Out-Null
        Add-DirectoryPermissions "$artifactPath" "IIS_IUSRS" "Read"
    }
}

task DeployBinaries -depends CleanBinaries {
    $artifactPath = [Environment]::ExpandEnvironmentVariables($deploymentArtifact.physicalPath)
    #Make sure the folder is there
    if (-not (Test-Path $artifactPath)) {
        Write-Host "Creating $artifactPath ... `n"
        md $artifactPath | Out-Null
        Add-DirectoryPermissions "$artifactPath" "IIS_IUSRS" "Read"
    }
    
    # Unzip the file if zip packages are being used.
	#Old package type:
	$legacyZipPackage = "$localDeploymentPath\WebApp\$deploymentPackageName"
	#New project based deploy
	$projectLocation = "$localDeploymentPath\$deploymentProjectName"
	if (Test-Path $legacyZipPackage) {
		#This module is only needed if zip files are used.
		Import-Module "$($folders.modules.invoke('Utility\zip.psm1'))"
		Write-Host "Extracting $legacyZipPackage to $artifactPath ... `n"
		Expand-Zip -fileInfo $legacyZipPackage -targetFolder "$artifactPath"
	}
	elseif (Test-Path $projectLocation) {
		Write-Output "Transferring $deploymentProjectName binaries to $artifactPath ... `n"
		Copy-Item -Path "$projectLocation\*" -Destination $artifactPath -force -recurse
	}
	else {
		throw "Web deployment artifact source could not be found."
	}
}

task PerformConfigurationSectionReplacement -depends DeployBinaries {
    $artifactPath = [Environment]::ExpandEnvironmentVariables($deploymentArtifact.physicalPath)
    #These are the config file names that will have sections replaced along with the coresponding section XPath.
    $baseConfigFileNamesXqueries = @{
        Web = "`"/configuration/`$sectionName`"";
        "log4net-appender" = "`"/log4net/appender[@name='`$sectionName']`""; 
        "log4net-logger" = "`"/log4net/logger[@name='`$sectionName']`""; 
    }
    
    foreach ($baseFileSectionXqueryKey in $($baseConfigFileNamesXqueries.Keys)) {
        # Perform config file section replacements
        $sectionFiles = Get-ChildItem $artifactPath\$baseFileSectionXqueryKey.*.config
        $sectionNamesFound = @("")

        #order is important, most restrictive RegEx first
        $regExpressions = @(#"(?i)$baseFileSectionXqueryKey\.$configEnvironmentType\.$normalizedDistrictName\.(?<SectionName>(?(Development|Integration|Demo|Staging|Production|([a-z_\.]+ISD))Ignore|([a-z_]+)(\.[a-z_]+)*))\.config", #deploy-district-config
        "(?i)$baseFileSectionXqueryKey\.$configEnvironmentType\.(?<SectionName>(?(Development|Integration|Demo|Staging|Production|PublicDemo|([a-z_\.]+ISD))Ignore|([a-z_]+)(\.[a-z_]+)*))\.config", #deploy-config
        "(?i)$baseFileSectionXqueryKey\.(?<SectionName>(?(Development|Integration|Demo|Staging|Production|PublicDemo|([a-z_\.]+ISD))Ignore|([a-z_]+)(\.[a-z_]+)*))\.config") #common config	
        
        #Using the most restrictive regex first, iterate all files in the list.
        #Then use the less restrictive ones. Any sections that have been found in a
        #higher order regEx will not be replaced again.
        foreach ($regEx in $regExpressions) {
        Write-Host "Pattern: $regEx in use"
            foreach ($sectionFile in $sectionFiles) {
                $match = [regex]::Match($sectionFile.Name, $regEx)
                if ($match.Success) {
                    $sectionName = $match.Groups["SectionName"].Value
                    if (-not ($sectionNamesFound -contains $sectionName)) {
                        $sectionNamesFound = $sectionNamesFound + $sectionName
                        #Process variables in context
                        $xQuery = Invoke-Expression $($baseConfigFileNamesXqueries[$baseFileSectionXqueryKey])
                        Write-Host "Using $xQuery"
                        $baseFilename = if($baseFileSectionXqueryKey -like "log4net*"){"log4net"} else{"$baseFileSectionXqueryKey"}
                        Poke-Xml $artifactPath\$baseFilename.config "$xQuery" $sectionFile
                    }
                }
            }
        }
        # Clean up unnecessary config files
        Get-ChildItem $artifactPath\$baseFileSectionXqueryKey.*.config | Remove-Item
    }
}

task PerformCustomConfiguration -depends PerformConfigurationSectionReplacement {
    $artifactPath = [Environment]::ExpandEnvironmentVariables($deploymentArtifact.physicalPath)
    
    # The following variables are used elsewhere in this script, in Set-WebsiteSETSpecificCustomConfigurations, and maybe other places too:
    #             $ldapUser, $ldapPass, $sqlUser, $sqlPass, $smtpUser, $smtpPass
    #
    # Note that below, we are using the first matching credentials we find, so if there's more than one set of SQL or SMTP creds, 
    # only one set will be used. 

    $matchingSqlCreds = @($credentialsList | where-object {$_.type -eq "sql"})
    if ($matchingSqlCreds.count -gt 0) {
        write-host  "Found $($matchingSqlCreds.count) SQL creds (using only first set)"
        $sqlUser = $matchingSqlCreds[0].user
        $sqlPass = $matchingSqlCreds[0].pass
    }
    else {
        write-host  "Found no SQL creds."
    }

    #region log4net.Config Setup
    #SMTP information is currently environment specific, pass in the variables if needed.
    $matchingSmtpCreds = @($credentialsList | where-object {$_.type -eq "smtp"})
    if ($matchingSmtpCreds.count -gt 0) {
        write-host "Found $($matchingSmtpCreds.count) SMTP creds (using only first set)"
        $smtpUser = $matchingSmtpCreds[0].user
        $smtpPass = $matchingSmtpCreds[0].pass
        if (($smtpUser -ne $null) -and ($smtpPass -ne $null)) {
            Poke-Xml $artifactPath\log4net.config "/log4net/appender[@type='EdFi.Dashboards.Infrastructure.log4net.SmtpCustomAppender, EdFi.Dashboards.Infrastructure']/username/@value" "$smtpUser"
            Poke-Xml $artifactPath\log4net.config "/log4net/appender[@type='EdFi.Dashboards.Infrastructure.log4net.SmtpCustomAppender, EdFi.Dashboards.Infrastructure']/password/@value" "$smtpPass"
        }
        else {
            write-host "SMTP creds missing username or password, skipping..."
        }
    }
    else {
        write-host "Found no SMTP creds"
    }

    write-host "Processing $($specificEntityTypes.count) specific entities..."
    foreach ($spEnType in $specificEntityTypes) {
        $normSpecificEntityType = $spEnType -replace ' ',''

        $matchingLdapCreds = @($credentialsList | where-object {$_.type -eq "ldap" -and $_.name -eq $normSpecificEntityType})
        write-host "$normSpecificEntityType remote credential varible count = $($matchingLdapCreds.count)"
        if ($matchingLdapCreds.count -gt 0) {
            $ldapUser = $matchingLdapCreds[0].user
            $ldapPass = $matchingLdapCreds[0].pass
        }

        #Set DB Connection:
        $schoolYear = $null
        #TODO: Add Schoolyear support
        $parms = @{
            projecttype = $appTypePrefix;
            specificEntityTYpeName = $normSpecificEntityType;
            databaseType = $dashboardDbNameSeed;
            buildConfigurationTypeName = $deploymentConfiguration;
            schoolYear = $schoolyear;
            versionSuffix = $legacyVersionNum;
        }
        $sqlDataBaseName = Get-ProjectSpecificDatabaseName @parms

        
        
        #Original xpathvalue before I added if statement
        #$xpathvalue = "Data Source=$sqlDataSourceName;Database=$sqlDatabaseName;User Id=$sqlUser;Password=$sqlPass;"
        
        if ($environmentCSBsByDbType -and $environmentCSBsByDbType.$dashboardDbNameSeed)
        {
        #$xpathvalue = $environmentCSBsByDbType.$dashboardDbNameSeed.connectionstring
        $environmentCSBsByDbType.$dashboardDbNameSeed.add("Uid", "$sqlUser")
        $environmentCSBsByDbType.$dashboardDbNameSeed.add("Pwd", "$sqlPass")
        $environmentCSBsByDbType.$dashboardDbNameSeed.add("ApplicationIntent", "ReadOnly")
        $xpathvalue = Get-SqlConnectionString $environmentCSBsByDbType.$dashboardDbNameSeed
        }
        else
        {
        $xpathvalue = "Data Source=$sqlDataSourceName;Database=$sqlDatabaseName;User Id=$sqlUser;Password=$sqlPass;"
        }


        if ($specificEntityTypes.Count -eq 1 -and [string]::IsNullOrWhiteSpace($specificEntityTypes[0])) {
            $xpath = "/configuration/connectionStrings/add[@name='Multi_LEA']/@connectionString" 
        }
        else {
            $xpath = "/configuration/connectionStrings/add[@name='$normSpecificEntityType']/@connectionString" 
        }
        poke-xml $artifactPath\Web.config $xpath $xpathvalue

        # If present, this runs any website specifc configurations that have a specificEntityType paramater
        # (The LEA version is present for backwards compatibility.)
        if(Test-Function "Set-WebsiteSETSpecificCustomConfigurations") {
            Set-WebsiteSETSpecificCustomConfigurations
        }
        elseif(Test-Function "Set-WebsiteLeaSpecificCustomConfigurations") {
            Set-WebsiteLeaSpecificCustomConfigurations
        }
    }

    #Add ApplicationDB connectionstring creds
    #Poke-Xml $artifactPath\Web.config "/configuration/connectionStrings/add[@name='Application_Db']/@connectionString" "Data Source=$sqlDataSourceName;Database=$applicationDbName;User Id=$sqlUser;Password=$sqlPass;"
    
    
    if ($environmentCSBsByDbType -and $environmentCSBsByDbType.$applicationDbNameSeed)
    
    {
    $environmentCSBsByDbType.$applicationDbNameSeed.add("Uid", "$sqlUser")
    $environmentCSBsByDbType.$applicationDbNameSeed.add("Pwd", "$sqlPass")
    $ApplicationNewCSB = Get-SqlConnectionString $environmentCSBsByDbType.$applicationDbNameSeed
    Poke-Xml $artifactPath\Web.config "/configuration/connectionStrings/add[@name='Application_Db']/@connectionString" $ApplicationNewCSB
    }
    else
    {
    Poke-Xml $artifactPath\Web.config "/configuration/connectionStrings/add[@name='Application_Db']/@connectionString" "Data Source=$sqlDataSourceName;Database=$applicationDbName;User Id=$sqlUser;Password=$sqlPass;"
    }
    
    # Poke deployment configuration name into the location of the existing configuration name in the Configuration-Specific installer
    $installerTypeName = Peek-Xml $artifactPath\Web.config "/configuration/inversionOfControl/installers/add[@name='Configuration-Specific Installer']/@typeName"
    $installerTypeName = [Text.RegularExpressions.Regex]::Replace($installerTypeName, "\.(Development|Integration|Demo|Staging|Production)", ".$configurationSpecificInstaller")
    Poke-Xml $artifactPath\Web.config "/configuration/inversionOfControl/installers/add[@name='Configuration-Specific Installer']/@typeName"  $installerTypeName

    #Using LocalApplicationData path for logs.
    $logFilePath = [Environment]::GetFolderPath("CommonApplicationData") + "\$appTypePrefix\Logs\$($deploymentWebsite.name)"
    if ($isWebAppDeployment) { $logFilePath += "\$webAppName" }
    if(-not (Test-Path $logFilePath)) {
        md $logFilePath | Out-Null
    }
    
    #$logFileName is from the environment config so it can vary between environments (so if two environments for the same district are on the same server they don't append the same file)
    #Poke in the logfile path and the logfile name for the environment.
    Poke-Xml $artifactPath\log4net.config "/log4net/appender[@name='ErrorRollingFile']/file/@value" "$logFilePath\Error$logFileName"
    Poke-Xml $artifactPath\log4net.config "/log4net/appender[@name='FeedbackRollingFile']/file/@value" "$logFilePath\Feedback$logFileName"
    Poke-Xml $artifactPath\log4net.config "/log4net/appender[@name='RollingFile']/file/@value" "$logFilePath\$logFileName"

    #Add for log4net logging itself:
    Poke-Xml $artifactPath\Web.config "/configuration/system.diagnostics/trace/listeners/add[@name='textWriterTraceListener']/@initializeData" "$logFilePath\log4net.log"

    #If present, this runs any website specific configurations
    if (Test-Function "Set-WebsiteCustomConfigurations") {
        Set-WebsiteCustomConfigurations
    }
}

task ApplyArtifactIpFilters -precondition {return Test-Function "ApplyIPFilters"} -depends CleanBinaries {   
    #add ip restrictions.
    try{
        ApplyIPFilters
    }
    catch{
        #if the filters failed try and just stop the website
        Write-Host "An error occurred while applying IP filters!"
        try{
            Stop-DeploymentArtifactAppPool
        }
        catch{
            #bring out the big guns and try to kill IIS if the previous step had an error.
            Write-Host "Error stopping AppPool!`r`nStopping IIS!"
            Stop-Service "W3SVC" -Force
        }
        finally{
            throw "IP filter error. The site may be publicly exposed. Verify the site is down!"
        }
    }
}

task StartWebsiteAndAppPool {
    $appPoolName = $deploymentArtifact.applicationPool
    Write-Host "Starting up AppPool: $appPoolName"
    $tries = 0
    while ($appPoolInstance = (ls IIS:\AppPools | where {$_.Name -eq "$appPoolName" -and $_.State -ne "Started"})) {
        if ($appPoolInstance.State -ne "Starting") {
            Start-WebItem IIS:\AppPools\$appPoolName
        }
        $tries += 1
        if ($tries -ge 50) {
            throw "Unable to start appPool: $appPoolName"
        }
        Sleep 1
    }
    if ($tries -lt 50) {
        Write-Host "AppPool started successfully!"
    }
    
    Write-Host "Starting Website: $($deploymentWebsite.name)"
    $tries = 0
    while ($deploymentWebsite.State -ne "Started") {
        if ($deploymentWebsite.State -ne "Starting") {
            $deploymentWebsite.Start()
        }
        $tries += 1
        if ($tries -ge 50) {
            throw "Unable to start website: $($deploymentWebsite.name)"
        }
        Sleep 1
    }
    if ($tries -lt 50) {
        Write-Host "WebSite started successfully!"
    }
}

<#
Function Stop-WebStack {
    Write-Host "Stopping Website: $($deploymentWebsite.name)"
    #Bring down the website
    $tries = 0
    while ($deploymentWebsite.State -ne "Stopped") {
        if ($deploymentWebsite.State -ne "Stopping") {
            $deploymentWebsite.Stop()
        }
        $tries += 1
        if ($tries -ge 50) {
            Write-Host "Unable to stop Website: $($deploymentWebsite.name)"
            break
        }
        Sleep 1
    }
    if ($tries -lt 50) {
        Write-Host "WebSite stopped successfully!"
    }
}
#>
Function Stop-DeploymentArtifactAppPool {
    $appPoolName = $deploymentArtifact.applicationPool
    Write-Host "Bringing down AppPool: $appPoolName"
    #Bring down the apppool to prevent locking.
    $tries = 0
    while ($appPoolInstance = (ls IIS:\AppPools | where {$_.Name -eq "$appPoolName" -and $_.State -ne "Stopped"})) {
        if ($appPoolInstance.State -ne "Stopping" -and $appPoolInstance.State -ne "Stopped") {
            Stop-WebItem IIS:\AppPools\$appPoolName
        }
        $tries += 1
        if ($tries -ge 50) { 
            throw "Unable to stop appPool: $appPoolName"
            break
        }
        Sleep 1
    }
    if ($tries -lt 50) {
        Write-Host "AppPool stopped successfully!"
    }
}
