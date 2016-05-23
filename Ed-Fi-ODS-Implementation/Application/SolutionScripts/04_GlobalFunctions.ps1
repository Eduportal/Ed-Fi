# Emulate functionality from Nuget Package Manager for use outside of NPM
if (-not (get-command |? { $_.name -eq "Get-Project" -and $_.ModuleName -eq "NuGet" })) {
	Write-Host "Defining the function 'Get-Project'"
    function global:Get-Project {
        [cmdletbinding(DefaultParameterSetName="Specific")]
        param(
            [Parameter(Position=0, ParameterSetName="Specific", Mandatory=$true)] [string[]] $projectName,
            [Parameter(Position=0, ParameterSetName="All", Mandatory=$true)] [switch] $all
        )
        if ($all) {
            $projectName = (gci $folders.base.invoke('Application') -Directory).Name
        }
        $projects = @()
        foreach ($proj in $projectName) {
            $projDir = gci $folders.base.invoke('Application') -filter $proj
            $projFile = gci $projDir.FullName -filter "$proj.*proj"
            if ($projFile) {
                $projects += @(@{ ProjectName = $proj; FullName = $projFile.fullname })
<#
                $projObj = New-Object PSObject -property @{
                    ProjectName = $proj
                    FullName = $projFile.fullname
                }
                add-member -inputObject $projObj -force -membertype ScriptMethod -name ToString -value { 
                    $this.ProjectName
                }
                $projects += @($projObj)
#>
            }
        }
        return $projects
    }
}

function global:ParseConnectionString([string]$cs) {
    $connectionStringTokens = $cs.split(";")
    $tokens = @{};

    foreach($token in $connectionStringTokens)
    {
        $nameValue = $token.split("=")
        $name = $nameValue[0]

        switch -regex ($name.ToLower())
        {
            "data source"
            {
                $tokens.serverName = $nameValue[1]
            }
            "initial catalog"
            {
                $tokens.databaseName = $nameValue[1]
            }
            "database"
            {
                $tokens.databaseName = $nameValue[1]
            }
            "user id"
            {
                $tokens.databaseLogin = $nameValue[1]
            }
            "password"
            {
                $tokens.databasePassword = $nameValue[1]
            }
        }
    }
    return $tokens;
}

Function global:Load-ConnectionString([string]$connectionStringName, [string]$configFilePath) {
    # initialize the xml object
    $appConfig = New-Object XML
    # load the config file as an xml object
    $appConfig.Load($configFilePath)
    # iterate over the settings
    # TODO:  Separate out a function to find the connection string
    foreach($connectionString in $appConfig.configuration.connectionStrings.add)
    {
        if ($connectionString.name -eq $connectionStringName)
        {
            return $connectionString.connectionString
        }
    }

	throw "Could not find connection string [$connectionStringName] in file [$configFilePath]"
}

Function global:Get-ConnectionStringTokens($databaseSettingsName) {
	Assert-DatabaseSettingsExist $databaseSettingsName
    $connectionString = Get-ConnectionString $databaseSettingsName

	if (($connectionString.Length) -eq 0) {
		throw "Connection string for database [$databaseSettingsName] is empty"
	}

    $connectionStringTokens = ParseConnectionString $connectionString
	return $connectionStringTokens
}

Function global:Get-ConnectionStringTokensByProject([string]$projectName, [string]$configName, [string]$connectionStringName) {
	$appConfigPath = Get-FileInProject -ProjectName $projectName -FilePath $configName
    $connectionString = Load-ConnectionString $connectionStringName $appConfigPath
    $connectionStringTokens = ParseConnectionString $connectionString
	return $connectionStringTokens
}

Function global:Get-FileInProject() {
	Param(
		[Parameter(Mandatory=$True)]
		[string]$ProjectName,
		[Parameter(Mandatory=$True)]
		[string]$FilePath
	)

	$projectPath = (Get-Project $ProjectName).FullName
	$projectDirectory = Split-Path -Path $projectPath -Parent
	$path = Join-Path -Path $projectDirectory -ChildPath $FilePath
	return $path
}

Function global:Get-ConnectionString($databaseSettingsName) {
	Function Get-ConfigurationFile($projectName) {
		$appConfigPath = Get-FileInProject -ProjectName $databaseSettings.AppConfigProject -FilePath App.config

		if (Test-Path $appConfigPath) {
			return $appConfigPath
		}

		$webConfigPath = Get-FileInProject -ProjectName $databaseSettings.AppConfigProject -FilePath Web.config

		if (Test-Path $webConfigPath) {
			return $webConfigPath
		}

		throw "Could not find configuration file in project '$projectName'."
	}
	
	Assert-DatabaseSettingsExist $databaseSettingsName
	$databaseSettings = Get-DatabaseSettings $databaseSettingsName

	$appConfigPath = Get-ConfigurationFile $databaseSettings.AppConfigProject
    $connectionString = Load-ConnectionString $databaseSettings.ConnectionStringName $appConfigPath

    return $connectionString
}

Function global:GetMigrationScripts() {
	return dir "$($solutionPaths.repositoryRoot)\Database\Structure\EdFi" -filter *.sql | Sort-Object Name
}

Function global:GetPostLoadMigrationScripts() {
	return dir "$($solutionPaths.repositoryRoot)\Database\Structure\EdFi\PostLoad" -filter *.sql | Sort-Object Name
}

Function global:Get-PathForProject([string]$projectName) {
	return (Get-Project $projectName).FullName
}

Function global:Write-RunMessage([string]$message) {
	Write-Host "$([environment]::NewLine)$message" -ForegroundColor Cyan
}

Function global:Write-CommandMessage([string]$message) {
	Write-Host "$([environment]::NewLine)Console Command: $message" -Fore Cyan -Back DarkCyan
	# Write-Host "$message$([environment]::NewLine)" -Fore Cyan -Back DarkCyan
}

Function global:Ping-Site([string]$applicationName) {
	$url = Get-ApplicationWebAddress $applicationName
	Write-Host "Pinging $applicationName site at [$url]..." -ForegroundColor Cyan

	Initialize-Url -url $url -timeoutseconds 120
}

Function global:Build-Project() {
    Param(
        [Parameter(mandatory=$true)] [string]$projectName,
        [Parameter(mandatory=$true)] [string]$buildConfiguration,
        [Parameter(mandatory=$false)] [string]$exeFile
    )
    $projectFile = Get-PathForProject $projectName
    $buildSuccess = Invoke-MsBuild "`"$projectFile`" `"/verbosity:minimal`" `"/target:rebuild`" `"/nr:false`" `"/property:Configuration=$buildConfiguration`""
    $projectDirectory = Split-Path -Path $projectFile -Parent
	if ($exeFile) {
		return "$projectDirectory\bin\$buildConfiguration\$exeFile"
	}
}

function global:Replace-InFile([string] $filePath, [string] $find, [string] $replace) {
    Write-Host "Replacing '$find' with '$replace' in $filePath"
    (Get-Content $filePath) | Foreach-Object {$_ -replace $find, $replace} | Set-Content $filePath
}

function global:Invoke-MsBuild {
    param(
        [string] $commandlineOptions, 
        [int] $retryCount = 0
    )
    $msbuildCommand = "& `"$msbuildEXE`" $commandlineOptions `"/nr:false`""
    $tryCounter = 0
    do {
        $tryCounter += 1
        if ($tryCounter -gt 1) {
            Write-Host "Retrying msbuild." -Fore Black -Back Yellow
        }
        Write-Host "`nStarting MSBUILD [$msbuildCommand]`n" -ForegroundColor Cyan
        Invoke-Expression $msbuildCommand | Write-Host
        $success = $LASTEXITCODE -eq 0
    } while (-not $success -and $tryCounter -le $retryCount)
    if (-not $success) {
        throw "MSBuild Failed."
    }
}

function global:Invoke-TextTransform {
    param(
        [string]$commandlineOptions
    )
    $t4Command = "& `"$t4EXE`" $commandlineOptions"
    Write-Host "`nStarting TextTransform [$t4Command]`n" -Fore DarkCyan
    Invoke-Expression $t4Command | Write-Host
    Write-Host
    $success = $LASTEXITCODE -eq 0

    if (-not $success) {
        throw "TextTransform Failed."
    }
}

function global:Initialize-ProjectCredentials {
    if (-not (dir cert:\LocalMachine\my | where {$_.Subject -eq "CN=$($Env:certPrefix)_Encryption"})) {
        write-host "Credentials certificate required."
        $certPath = $folders.base.Invoke("logistics\Certificates\$($env:certPrefix).pfx")
        Import-X509Certificate -path $certPath -storeLocation "Localmachine" -storeName "My" -Exportable
    }
    else {
        write-host "Credentials certificate already imported."
    }
}

function global:Add-Pkcs12ConfigProviderToGac {
    $pkcs12Package = Get-LatestNugetPackagePath Pkcs12ProtectedConfigurationProvider
    $pkcs12Dll = get-item "$pkcs12Package\lib\NET40\PKCS12ProtectedConfigurationProvider.dll"

    Add-Type -AssemblyName "System.EnterpriseServices"
    $publish = new-object System.EnterpriseServices.Internal.Publish
    $publish.GacInstall($pkcs12Dll.fullname)
}

$global:aspnetRegiisPath = "${env:WinDir}\Microsoft.NET\Framework\v4.0.30319\aspnet_regiis.exe"
set-alias -scope global aspnet_regiis $aspnetRegiisPath


<#
.synopsis 
Encrypt or decrypt a section in a configuration file
.description
This is a wrapper to make it easier to encrypt and decrypt sections of config files, to work around inconvenient behavior of aspnet_regiis.exe.
.parameter ConfigFile
The file which contains the section you wish to encrypt
.parameter BaseConfigFile
If the config file you wish to encrypt is actually a config TRANSFORM file, you must provide the base config file as well. This file will not be modified.
.parameter SectionXpath
The Xpath to the section you wish to encrypt. This must start with '/configuration' because we rely on aspnet_regiis.exe, and that tool assumes you are working somewhere under that node. 
For example: /configuration/connectionStrings
.parameter CustomProvider
The name of the custom encryption provider. It must be already specified in a <configProtectedData> section of the ConfigFile.
.parameter Action
The action to perform: either encrypt or decrypt the section specified by -SectionXpath
.example
Encode-ConfigFileSection -configFile web.config -sectionXpath '/configuration/connectionStrings' -action Encrypt
#>
function global:Encode-ConfigFileSection {     
    [cmdletbinding()]
    param(
        [parameter(mandatory=$true)] $configFile,
        [parameter(mandatory=$true)] $sectionXpath, 
        [parameter(mandatory=$true)] [ValidateSet("Encrypt","Decrypt")] $action,
        $baseConfigFile,
        $customProvider = "Pkcs12CustomProvider"
    )
    $configFile = resolve-path $configFile
    if ($baseConfigFile) { 
        $baseConfigFile = resolve-path $baseConfigFile 
    }

    $splitSectionXpath = $sectionXpath -split '/'
    if ($splitSectionXpath[1] -ne "configuration") {
        throw "-sectionXpath was '$sectionXpath', which did not start with '/configuration'. aspnet_regiis requires an Xpath starting with /configuration."
    }
    $regiisSection = ($splitSectionXpath)[2..($splitSectionXpath.count-1)] -join '/'
    $sectionParentXpath = $splitSectionXpath[1..($splitsectionXpath.count-2)] -join '/'

    $workDir = mkdir "${env:temp}\$([System.Guid]::NewGuid().toString())"
    write-verbose "Using '$workdir' as working directory"
    $workFile = "$workDir\Web.config"

    if ($baseConfigFile) {
        # Run a transform with the UNMODIFIED transformation file
        WebConfigTransformRunner $baseConfigFile $configFile $workFile
    }
    else {
        copy-item $configFile $workFile
    }

    # Encode the desired section of the whole, transformed file. (File must be called "Web.config" in $workDir b/c of aspnet_regiis.)
    $splitSectionXpath = $sectionXpath -split '/'
    $regiisSection = ($splitSectionXpath)[2..($splitSectionXpath.count-1)] -join '/'
    try {
        if ($action -eq "encrypt") {
            Invoke-ProcessAndWait -command $aspnetRegiisPath -argumentlist @("-pef",$regiisSection,$workDir,"-prov",$customProvider)
        }
        elseif ($action -eq "decrypt") {
            Invoke-ProcessAndWait -command $aspnetRegiisPath -argumentlist @("-pdf",$regiisSection,$workDir)
        }
    }
    catch {
        write-host -foreground Red "aspnet_regiis failed to process the file."
        write-host -foreground Red "Read its output (above) for more information."
        write-host -foreground Red "Possible causes (NOT exhaustive): "
        write-host -foreground Red "  File might not have <configProtectedData> section for the '$customProvider' custom encryption provider."
        write-host -foreground Red "  You might be trying to encrypt an already-encrypted section."
        throw
    }

    copy-item $workFile "${configFile}.${action}ed"

    # Cleanup
    rm -recurse -force $workDir
}

<#
.synopsis
Encrypt or decrypt all the sections that we know about from all the config files that we know about
#>
function global:Encode-ConfigFiles {
    [cmdletbinding()]
    param(
        [parameter(mandatory=$true)] [ValidateSet("Encrypt","Decrypt")] $action,
        $EncryptConfigSections = $AllEncryptConfigSections,
        [switch] $WhatIf
    )
    foreach ($fileEntry in $EncryptConfigSections) {
        write-verbose "Processing $($fileENtry.relpath)"
        $file = get-item $folders.base.invoke("Application\$($fileEntry.relpath)")

        $splitFileBaseName = $file.name -split '\.' # backslash required b/c regex
        if ($splitFileBaseName.count -eq 3) {
            write-verbose "My incredibly advanced heuristic has detected that this must be a transformation file."
            $configFileDir = split-path $file
            $baseFilename = $splitFileBaseName[0,2] -join '.' # no backslash here on purpose
            $baseConfigFile = "$configFileDir\$baseFilename"
        }
        elseif ($splitFileBaseName.count -eq 2) {
            write-verbose "My incredibly advanced heuristic has detected that this must be a normal config file (i.e. not a transformation)."
        }
        else {
            throw "My incredibly advanced heuristic could not detect what type of file this is."
        }

        foreach ($section in $fileEntry.sections) {
            $dacfArgs = @{}
            $dacfArgs.configFile = $file.fullname
            $dacfArgs.action = $action
            $dacfArgs.sectionXpath = "/configuration/$section"
            if ($baseConfigFile) { $dacfArgs.baseConfigFile = $baseConfigFile }

            $verboseMessage = "Running Encode-ConfigFileSection with arguments: "
            foreach ($k in $dacfArgs.keys) { $verboseMessage += "`r`n         ${k} = $($dacfArgs[$k]); "}

            if ($WhatIf) {
                write-host $verboseMessage
            }
            else {
                write-verbose $verboseMessage
                Encode-ConfigFileSection @dacfArgs 
            }
        }
    }
}

