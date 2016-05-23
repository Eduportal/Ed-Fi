properties {
Import-Module "$($folders.modules.invoke('utility\7z.psm1'))"
. "$($folders.activities.invoke('build\' + $environment + '.vars.ps1'))"
Import-Module "$($folders.modules.invoke('utility\credential-management.psm1'))" -force
. "$($folders.activities.invoke('build\process-files\process-files.credentials.ps1'))"
#In environment config:
#$teamCityUrl
#$teamCityProjectName
#$leaLoadBuildFormat

#ensure no spaces
$localEducationAgencyName = $localEducationAgencyName.Replace(" ","")

$isNonInteractive = (gwmi -Class Win32_Process -Filter "ProcessID=$PID" | Select -Expand CommandLine | Where{$_ -like "*-nonInteractive*"}) -ne $null

Initialize-Credentials $folders.activities.invoke('build')

$destinationPath = "$sourceDataBasePath\$localEducationAgencyName"
}

task default -depends ExtractArchive,Archive,BeginBuild,AnonymizePackagePublish
task ProcessPartial -depends ExtractArchive,Archive

task ExtractArchive {
    Write-Host "Preparing to process archive $fileArchive for matched Local Education Agency: $localEducationAgencyName"
    Add-CredentialMetadata `
    @(
        @{Name = "$localEducationAgencyName Package User";  DefaultUsername = $null;    Description = "Credential (Password) to access archive file for LEA"}
    )
    if($isNonInteractive) {
        try {
            Initialize-Credentials $folders.activities.invoke('build')
            $archivePassword = Get-PlaintextPassword "$localEducationAgencyName Package User"
        }
        catch {
            Write-Host "Archive Password not found for `"$localEducationAgencyName Package User`""
            #$error.RemoveAt(0)
        }
    }
    else {
        Initialize-Credentials $folders.activities.invoke('build')
        $archivePassword = Get-PlaintextPassword "$localEducationAgencyName Package User"
    }
    $done = $false
    #Expected multiple password form is [Security],[IsSuper],[Fun]
    $passwordCaptures = [Regex]::Match($archivePassword, "(?:(?:\[)([^\]]*)(?:\]\,?))+")
    If ($passwordCaptures.Success) {
        $errorCache = $error.ToArray()
        #iterate 
        foreach ($password in $passwordCaptures.Groups[1].Captures) {
            try {
                #Write-Host "Debug- Trying password: $password"
                $done = $true
                Expand-ArchiveWith7z $fileArchive $destinationPath $password
                if ($done) {break;}
            }
            catch {
                $done = $false
            }
        }
        #Remove password tries from errors unless we didn't hit
        if ($done) {
            $error.Clear()
            $error.AddRange($errorCache)
        }
        else {
            if ($error.Count -gt 0) {
                #prevent double listing of the error.
                $errorToThrow = $error[0]
                $error[0] = $null
                #throw the error so that feedback is given regarding the failure.
                throw $errorToThrow 
			}
			else {
				$error.Clear()
				$error.AddRange($errorCache)
			}
        }
    }
    else {
        Expand-ArchiveWith7z $fileArchive $destinationPath $archivePassword
    }
}

task Archive {
    $archivePath = "$(Split-Path $fileArchive)\Archive"
    if(-not (Test-Path $archivePath)) { md $archivePath | out-null}
    if (Test-Path "$archivePath\$(Split-Path $fileArchive -Leaf)") {
        rm "$archivePath\$(Split-Path $fileArchive -Leaf)" -force
    }
    Write-Host "Moving $fileArchive to Archive folder."
    mv $fileArchive $archivePath -force
    #Possible need for this? clean up any left overs.
    #mv "$(Split-Path $fileArchive)\*$($localEducationAgencyName)*.*" -include @("*.zip","*.7z") -destination $archivePath -force
}

task AnonymizePackagePublish -precondition {return $shouldAnonymize} -depends ExtractArchive {
    &"$((gwmi -Class Win32_Process -Filter "ProcessID=$PID").ExecutablePath)" -NonInteractive -ExecutionPolicy UnRestricted -Command "& {$($folders.activities.invoke('build\Publish\AnonymizePackagePublish.ps1')) '$localEducationAgencyName' '$publishType'}"
}

task BeginBuild -depends ExtractArchive {
    #REST URLs
    $projectsUrl = $teamCityUrl + "/httpAuth/app/rest/projects"
    $projectUrl  = $teamCityUrl + "/httpAuth/app/rest/projects/id:" # project @id
    $addToQueue = $teamCityUrl + "/httpAuth/action.html?add2Queue=" # buildType @id
    
    $teamCityProjectName = $teamCityProjectNameByPublishType["$publishType"]
    
    [xml]$xml = GetWebContent($projectsUrl)
    $node = $xml.SelectSingleNode("//project[@name='$teamCityProjectName']/@id")
    if ($node -eq $null) {
        Write-Host "TeamCity Project: $teamCityProjectName was not found!"
        return
    }
    
    [xml]$xml = GetWebContent("$projectUrl$($node.Value)")
    $resolvedLeaLoadBuild = $leaLoadBuildFormat -f $localEducationAgencyName
    $node = $xml.SelectSingleNode("//buildType[@name='$resolvedLeaLoadBuild']/@id")
    
    if ($node -eq $null) {
        Write-Host "TeamCity Build: $resolvedLeaLoadBuild`r`nWas not found in project: $teamCityProjectName"
        return
    }
    
    Write-Host "Adding build for local education agency '$localEducationAgencyName' to the TeamCity build queue."
    GetWebContent("$addToQueue$($node.Value)") | Out-Null
}

function GetWebContent([string] $requestUrl) {
    $creds = Get-NetworkCredential "Build Server User"
    $webrequest = [System.Net.WebRequest]::Create($requestUrl)
    $webrequest.Credentials = $creds
    $response = $webrequest.GetResponse()
    $sr = [Io.StreamReader]($response.GetResponseStream())
    $out = $sr.ReadToEnd()
    $response.Close()
    return $out
}