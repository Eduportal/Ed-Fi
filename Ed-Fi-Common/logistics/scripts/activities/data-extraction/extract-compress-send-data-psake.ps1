# *************************************************************************
# Copyright (C) 2010, Michael & Susan Dell Foundation. All Rights Reserved.
# *************************************************************************
properties { 
	$normalizedLocalEducationAgencyName = $localEducationAgencyName.Replace(" ", "")    
    . "$($folders.modules.invoke('utility\build-utility.ps1'))"
    Import-Module "$($folders.modules.invoke('utility\credential-management.psm1'))" -force
    Import-Module "$($folders.modules.invoke('package\get-latest-package.psm1'))"
    . "$($folders.activities.invoke('data-extraction\host.vars.ps1'))"
    . "$($folders.activities.invoke('data-extraction\host.credentials.ps1'))"    
    . "$($folders.activities.invoke('data-extraction\dlp.credentials.ps1'))"

    #If this district has variables, load them.
    if (Test-Path "$($folders.activities.invoke('data-extraction\'))$normalizedLocalEducationAgencyName\vars.ps1") {
        . "$($folders.activities.invoke('data-extraction\'))$normalizedLocalEducationAgencyName\vars.ps1"
    }
	#If this district has credentials, load them.
    if (Test-Path "$($folders.activities.invoke('data-extraction\'))$normalizedLocalEducationAgencyName\credentials.ps1") {
        . "$($folders.activities.invoke('data-extraction\'))$normalizedLocalEducationAgencyName\credentials.ps1"
    }
    #Load scriptwide variables.
    . "$($folders.activities.invoke('data-extraction\'))extract-compress-send-data.vars.ps1"
    
    #If the connection string has not been specified (typically in the vars.ps1 file), default it.
    if ("$connStringFormat" -eq "") {
		$connStringFormat = "DSN={0};SERVER={1};UID={2};Pwd={3};"
	}
    
    $dataDirectory          = [IO.Path]::Combine("$($folders.base.invoke('..\'))", "Data")
    $winScpPath             = "$($folders.tools.invoke('WinSCP\WinSCP.com'))"    

    $sisMappingsDirectory = [IO.Path]::Combine($mappingsDirectory, "$normalizedLocalEducationAgencyName")

    $runMappingX32Path      = dir -path $folders.tools.invoke("RunMapping\").Path -recurse -filter RunMapping.exe | where { (Split-path -leaf $_.Directory) -eq "build" } | % {$_.FullName}
    $runMappingX64Path      = dir -path $folders.tools.invoke("RunMapping\").Path -recurse -filter RunMapping.exe | where { (Split-path -leaf $_.Directory) -eq "x64" } | % {$_.FullName}
    $runMappingPath         = if (Is64Bit) {$runMappingX64Path} else {$runMappingX32Path}
        
    Initialize-Credentials "$($folders.activities.invoke('data-extraction\'))"
    
    # Credentials used in this script
    $dbUser                  = Get-Username("$normalizedLocalEducationAgencyName Database")
    $dbPassword              = Get-PlaintextPassword("$normalizedLocalEducationAgencyName Database")
	
    $scpUser                 = Get-Username("Destination SCP")
    $scpPassword             = Get-PlaintextPassword("Destination SCP")
    #Legacy Support
    if ($scpUser -eq $null) {
        $scpUser                 = Get-Username("Region 10 SCP")
        $scpPassword             = Get-PlaintextPassword("Region 10 SCP")
	}
    
	$deployerUsername        = Get-Username("Remote Deployer")
	$deployerPassword        = Get-PlaintextPassword("Remote Deployer")
}

Function GetMappingMetadata ($altovaAssembly, $mappingAssembly) {
    Write-Host "Acquiring Mapping MetaData..."
    $scriptBlock = {
        $vars = $null
        if ($Input.'<>4__this' -ne $null) {$Input.'<>4__this'.read() | % {$vars = $_};}
        foreach ($var in $vars.Keys) {if ("$var" -ne ""){Set-Variable $var -Value $($vars[$var]) -Scope Script -Force}};
        $altova = [Reflection.Assembly]::LoadFrom("$altovaAssembly")
        $mapping = [Reflection.Assembly]::LoadFrom("$mappingAssembly")
        $traceProviderType = $altova.GetType("Altova.TraceProvider")
        $mappingType = $mapping.GetTypes() | where {$traceProviderType.IsAssignableFrom($_) -and $_.Name.StartsWith("MappingMap")} | select -first 1
        #$dbIsRequired = $false
        #$parmCount = 0
        #Get all of the Run methods in the mapping where all the parameters are strings. If one of
        #those parameters has the phrase ConnectionString in it's name then a DBconnection is expected.
        #$mappingType.GetMethods() | where { $_.Name -eq "Run" -and ($_.GetParameters() | % {if($_.ParameterType -ne [System.String]) {break} else {$_}} | % { if ((-not $dbIsRequired) -and $_.Name -match "ConnectionString") { $dbIsRequired = $true; $parmCount++; $true } else { $parmCount++; $true }})} | Out-Null
        $mappingMethods = $mappingType.GetMethods()
        foreach ($mappingMethod in $mappingMethods)
        {
            $parameters = $mappingMethod.GetParameters()
            $parmCount = 0
            $dbIsRequired = $false
            foreach ($parameter in $parameters) {
                if($parameter.ParameterType -eq [System.String]) {
                    if ((-not $dbIsRequired) -and $parameter.Name -match "ConnectionString") {
                        $dbIsRequired = $true
                        $parmCount++ 
                    } 
                    else { $parmCount++ }
                }
                else { 
                    $exit = $true
                    break
                }
                if ($exit) {break}
            }
            #If we found one that works, go with it.
            if (-not $exit) {break}
        }
        Write-Host "MetaData acquired!"
        return @($dbIsRequired,$parmCount)
    }
    try {
        $session = New-PSSession
        Invoke-Command -Scriptblock $scriptBlock -Session $session -InputObject @{altovaAssembly = $altovaAssembly; mappingAssembly = $mappingAssembly}
    }
    finally {
        Remove-PSSession $session
    }
}

task default -depends ExtractSisData

#task ExtractAndTransformAllData -depends ExtractSisData, TransformFiles
#task ExtractSisData -depends EstablishVpnConnection, ExtractSisDataCore, DisconnectVpn
#task GenerateSourceCodeForMappings -depends EstablishVpnConnection, GenerateSourceCodeForMappingsCore, DisconnectVpn

task ExecutePreExtractScript {
    if ("$preExtractScriptName" -ne "") {
        Write-Host "Before executing pre-extract script: $preExtractScriptName"
        Invoke-Expression "$($folders.activities.invoke(`"data-extraction\$normalizedLocalEducationAgencyName\$preExtractScriptName`"))"
        Write-Host "Pre-extract script execution completed."
    }
    else {
        Write-Host "No pre-extract script specifed. Continuing..."
    }
}

task GetLatestDeploymentPackage {
	# Get the latest deployment package
	Get-LatestDeploymentPackage $deployerUsername $deployerPassword $normalizedLocalEducationAgencyName "DataExtraction.$publishType" $folders.base.invoke('..\') -extractPackage
}

task InitializeDataFolder {
	if (-not(Test-Path $dataDirectory)) {
		Write-Host "Creating data directory to hold data."
		md $dataDirectory | out-null
	} else {
		Write-Host "Clearing data directory."
		del "$dataDirectory\*.xml"
	}
}

task ExtractSisData -depends InitializeDataFolder {
	# Get all mapping names to execute (based on sub-directories of the school district-specific Mappings folder)
	# For executing out of source tree => $mappingNames = Get-ChildItem $district_mapping_directory | where {$_ -is [IO.DirectoryInfo]} | % {$_.Name}
	
	# Execute mappings out of stage assembly folder
	$mappingNames = Get-ChildItem $sisMappingsDirectory -Exclude $excludes | where {$_ -is [IO.DirectoryInfo]} | % {$_.Name}

	if ($excludes -ne $null) {
		Write-Host "=========================================================================================================================" -Foreground Cyan
		Write-Host "WARNING: The following mappings have been excluded from execution: $excludes" -Foreground Cyan
		Write-Host "=========================================================================================================================" -Foreground Cyan
	}

	$connString = $connStringFormat -f $dsnName,$dbServerName,$dbUser,$dbPassword

	# Run each mapping...
	foreach ($mappingName in $mappingNames) {
		Write-Host "Running mapping $mappingName ..."
        if ("$inputPath" -ne "" -and "$fileNamePattern" -ne "") {
            #Use File source
            if ($multiSourceFileInput) {
                #Run all the files that are pattern matched as a single input into the mapping.
                #Files are currently name matched to the inputnames in the mapping.
                &$runMappingPath $mappingName /replace /sourcePath "$inputPath" /pattern "$fileNamePattern" /destPath "$dataDirectory" /searchPath "$sisMappingsDirectory" /multiSource
            }
            else {
                &$runMappingPath $mappingName /replace /sourcePath "$inputPath" /pattern "$fileNamePattern" /destPath "$dataDirectory" /searchPath "$sisMappingsDirectory"
            }
        }
        else {
            #Use Db Connection
            &$runMappingPath $mappingName /replace /connection $connString /destPath "$dataDirectory" /searchPath "$sisMappingsDirectory"
        }
    }
}

task ExtractTertiaryInterchangeData -depends InitializeDataFolder {
    $connString = $tertiaryConnStringFormat -f $dsnName,$dbServerName,$dbUser,$dbPassword
    
    foreach ($interchange in $tertiaryInterchanges) {
        Write-Host "Begin processing for Interchange: $interchange"
        $mappingTypes = Get-ChildItem $mappingsDirectory\$interchange -Exclude $excludes | where {$_ -is [IO.DirectoryInfo]} | % {$_.Name}

        if ($excludes -ne $null) {
            Write-Host "=========================================================================================================================" -Foreground Cyan
            Write-Host "WARNING: The following mappings have been excluded from execution: $excludes" -Foreground Cyan
            Write-Host "=========================================================================================================================" -Foreground Cyan
        }
        
        # Run each mapping...
        foreach ($mappingType in $mappingTypes) {
            Write-Host "Begin processing for mapping type $mappingType ..."
            $subMappings = @()
            $subMappings += @(Get-ChildItem $mappingsDirectory\$interchange\$mappingType -recurse -include "Mapping.exe" | where { (Split-Path -Leaf (Split-Path -Parent $_.FullName)) -ne $mappingType})
            if ($subMappings.Count -gt 0) { throw new-object NotImplementedException "Sub-mappings for year/file types are not implemented." }
            $mappingPath = Get-ChildItem $mappingsDirectory\$interchange\$mappingType -recurse -include "Mapping.exe" | Select -ExpandProperty FullName -First 1
            $altovaPath = "$(Split-Path -Parent $mappingPath)\Altova.dll"
            $metaData = GetMappingMetadata $altovaPath $mappingPath
            Write-Host "Mapping MetaData:"
            Write-Host "Uses Database: $($metaData[0])"
            Write-Host "Parameter Count: $($metaData[1])"
            if ($metaData[0]) {
                Write-Host "Before executing mapping using database connection."
                #Use Db Connection
                &$runMappingPath $interchange /type "$mappingType" /replace /connection "$connString" /destPath "$dataDirectory" /searchPath "$mappingsDirectory" 
            }
            elseif (Test-Path "$inputPath\$interchange\$mappingType") {
                Write-Host "Before executing mapping using file source."
                &$runMappingPath $interchange /type "$mappingType" /replace /sourcePath "$inputPath\$interchange\$mappingType" /pattern ".*" /destPath "$dataDirectory" /searchPath "$mappingsDirectory"
            }
            else { Write-Host "WARNING: No Mapping execution occured" }
        }
    
    }

    
}

task CompressAndSendData {
    try{
        Import-Module "$($folders.modules.invoke('utility\7z.psm1'))"
        $zipFileSuffix = [DateTime]::Now.ToString("yyyy-MM-dd.HH-mm-ss") + ".$publishType"
        $dataInfix = if($isPartial) {"PartialData"} else {"Data"}
        $zipFileName = "$($folders.base.invoke('..\'))$normalizedLocalEducationAgencyName-$dataInfix-$zipFileSuffix.7z"
        Compress-DirectoryWith7z $zipFileName "$($folders.base.invoke('..\Data\'))" $deployerPassword -requirePassword
        
        $ppkPath = "$($folders.base.invoke('..\..\'))Keys\$normalizedLocalEducationAgencyName.ppk"
        
        #Note: Run the WinSCP.exe in the \tools\winscp folder with ADMIN rights, and connect to the server to cache the hostkey.
        $winScpCommands += "```"option batch ```"abort```"```" "#Stop on all errors
		$winScpCommands += "```"option confirm ```"off```"```" " #Requesting action confirmation is off
		if(Test-Path $ppkPath) {
            $winScpCommands += "```"open ```"scp://$($scpUser)@$($scpHostAddress)```" -privatekey=$ppkPath```" "
        }
        else {
            $winScpCommands += "```"open ```"scp://$($scpUser):$($scpPassword)@$($scpHostAddress)```"```" "
        }
		$winScpCommands += "```"cd ```"$normalizedLocalEducationAgencyName```"```" "
		$winScpCommands += "```"put ```"$zipFileName```"```" "
		$winScpCommands += "```"exit ```" "
        Write-Host "Before copying package: $(Get-Date)"
        Invoke-Expression "$winScpPath /command $winScpCommands" | Out-Host
        Write-Host "After copying package: $(Get-Date)"
        if($LASTEXITCODE -gt 0) {
            throw "An Error occured during package transfer!"
        }
    }
    finally{
        #Clean data dir
        Write-Host "Clearing data directory."
        del "$dataDirectory\*.xml"
    }
}