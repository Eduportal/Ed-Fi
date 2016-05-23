#param([parameter(Mandatory = $true)][String]$districtName, [parameter(Mandatory = $true)]$anonymizerUser, [parameter(Mandatory = $true)]$anonymizerPass, [parameter(Mandatory = $true)]$publishType)

properties{
. "$($folders.activities.invoke('build\anonymize\Anonymize.vars.ps1'))"
. "$($folders.modules.invoke('utility\build-utility.ps1'))"
$configXpathPokeValues = @{ "/configuration/appSettings/add[@key='DestinationDirRoot']/@value" = $xmlAnonDataDirectoryRoot; 
                            "/configuration/connectionStrings/add[@name='$anonConnStringName']/@connectionString" = "Data Source=$dbServerName;Persist Security Info=True; User Id=$anonymizerUser;Password=$anonymizerPass;database=$anonDbName" }
$anonymizerConfigPath = "$($anonymizerPath).config"
$normalizedDistrictName = $districtName.Replace(" ", "")
$districtId = $districtNormalizedNameIds["$normalizedDistrictName"]
if (-not ($districtId)){
#If a district Id was not found halt processing. It is imperative to the anonymizing process.
throw "District $normalizedDistrictName does not have a coresponding district id configured."
}
$xmlSourceDataDirectory = "$xmlSourceDataDirectoryRoot\$normalizedDistrictName"
$xmlAnonDataDirectory = "$xmlAnonDataDirectoryRoot\$normalizedDistrictName"
#Initialize
$xmlDataFileCollectionHash = @{assessmentFiles  = @();
                               anonymizeFiles  = @();
                               directCopyFiles = @(); }
}

task default -depends ExecuteFileActions

task Initialize {
    foreach ($xPath in $configXpathPokeValues.Keys) {
        if(Test-XPath $anonymizerConfigPath $xPath) {
            Write-Host "Updating Anonymizer Config at XPath: $xPath"
            Poke-Xml $anonymizerConfigPath $xPath $configXpathPokeValues["$xPath"]
        }
        else {
            throw "Anonymizer Config not updated, could not find element to update using xpath: $xPath"
        }
    }
}

task CleanUp {
    #Clean up anon dir before we get started
    if (Test-Path $xmlAnonDataDirectory) {
        Remove-Item $xmlAnonDataDirectory -Recurse -Force
    }
    if(-not(Test-Path $xmlAnonDataDirectory)) {
        Write-Host "Creating Anonymous Data Destination Directory: $xmlAnonDataDirectory"
        md $xmlAnonDataDirectory | Out-Null
    }
}

task ProcessInputFiles {
    $xmlDataFiles = Get-ChildItem $xmlSourceDataDirectory\*.xml
    foreach ($xmlDataFile in $xmlDataFiles) {
        foreach ($fileType in $fileTypesToAnonymize) {
            $regEx = "^(?i)$fileType([-]{1}[\w\s]+)*\.xml$"
            $match = [regex]::Match($xmlDataFile.Name, $regEx)
            #If we find a match, stop.
            if ($match.Success) { break }
        }
        foreach ($assessmentFileType in $assessmentFileTypesToAnonymize) {
            $regEx = "^(?i)$assessmentfileType([-]{1}.*)*\.xml$"
            $assessmentMatch = [regex]::Match($xmlDataFile.Name, $regEx)
            #If we find a match, stop.
            if ($assessmentMatch.Success) { break }
        }
        if ($match.Success) {
            $xmlDataFileCollectionHash.anonymizeFiles += $xmlDataFile 
        }
        elseif ($assessmentMatch.Success) {
            $xmlDataFileCollectionHash.assessmentFiles += $xmlDataFile
        }
        else {
            $xmlDataFileCollectionHash.directCopyFiles += $xmlDataFile
        }
    }
}

task ProcessAnonymizeFiles -depends Initialize,CleanUp,ProcessInputFiles {
    Write-Host "Beginning Data File Anonymization:"
    try {    
        if (Test-Path $xmlAnonDataIsolation\$normalizedDistrictName) {
            Remove-Item $xmlAnonDataIsolation\$normalizedDistrictName -Recurse -Force
        }
        md $xmlAnonDataIsolation\$normalizedDistrictName | Out-Null
        foreach ($xmlDataFile in $xmlDataFileCollectionHash.anonymizeFiles) {
            #This is to address a weird issue with the anonymizer that only happens in R10 and I can't get a debugger on.
            Write-Host "`r`nIsolating file $xmlDataFile"
            Copy-Item $xmlDataFile -Destination $xmlAnonDataIsolation\$normalizedDistrictName -Force
            
            $isolatedXmlDataFile = Get-ChildItem "$xmlAnonDataIsolation\$normalizedDistrictName\$($xmlDataFile.Name)"
            Write-Host "Anonymizing $($isolatedXmlDataFile.Name):`r`n"
            # Anonymize
            Invoke-Expression "$anonymizerPath $anonymizerConfigDirectory $districtId '$isolatedXmlDataFile'" | Write-Output
            $exitCode = $LASTEXITCODE
            if ($exitCode -ne 0) {
                throw "Anonymization failed for $($xmlDataFile.Name)(exit code $exitCode)."
            }
        }
        Write-Host "`r`nData file anonymization completed.`r`n"
    }
    catch { throw $_ }
    finally {
        if (Test-Path $xmlAnonDataIsolation\$normalizedDistrictName) {
            Remove-Item $xmlAnonDataIsolation\$normalizedDistrictName -Recurse -Force
                #cleanup values when done.
            foreach ($xPath in $configXpathPokeValues.Keys) {
                if(Test-XPath $anonymizerConfigPath $xPath) {
                    Write-Host "Blanking Anonymizer Config at XPath: $xPath"
                    Poke-Xml $anonymizerConfigPath $xPath "#######"
                }
            }
        }
    }
}

task ProcessDirectCopyFiles -depends CleanUp,ProcessInputFiles {
    Write-host "Beginning Data File Direct Copies:"
    foreach ($xmlDataFile in $xmlDataFileCollectionHash.directCopyFiles) {        
        Write-Host "`r`nBegining direct copy of $($xmlDataFile.Name) to $xmlAnonDataDirectory ..."
        Copy-Item $xmlDataFile -Destination $xmlAnonDataDirectory\ -Force
        Write-Host "Copy Complete."
    }
    Write-Host "`r`nData file direct copies completed."
}

task ProcessAssessmentFiles -depends ProcessInputFiles,ProcessAnonymizeFiles {
    #Preform Assessment post processing
    foreach ($xmlAssessmentDataFile in $xmlDataFileCollectionHash.assessmentFiles) {
        #This has to run after all other anonymization is complete.
        Write-Host "TODO : Anonymizing Assessment File: $($xmlAssessmentDataFile.Name)"
        #Insert Assessment Annonymizer call here. 
    }   
}

task ExecuteFileActions -depends ProcessAnonymizeFiles,ProcessDirectCopyFiles 
#,ProcessAssessmentFiles