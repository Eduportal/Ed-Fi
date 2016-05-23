param([Parameter(Mandatory=$true)]
      [string]
      $districtName)

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
if (-not (get-module |? { $_.Name -eq "path-resolver" })) {
    Import-Module $PSScriptRoot\..\..\..\modules\path-resolver.psm1
}
Import-Module "$($folders.modules.invoke('utility\credential-management.psm1'))"
. "$($folders.activities.invoke('adhoc-processes\assessment-extraction\assessment-common.ps1'))"

$normalizedDistrictName = $districtName.Replace(" ","")
Add-CredentialMetadata `
	@(
        @{Name = "$normalizedDistrictName Eduphoria User"; DefaultUsername = $null; Description = "Username for the district to access the Eduphoria API"; IntegratedSecuritySupported = $false},
        @{Name = "Eduphoria API Key"; DefaultUsername = "Eduphoria API Key"; Description = "API Key to access Eduphoria data"; IntegratedSecuritySupported = $false},
        @{Name = "Eduphoria API Secret"; DefaultUsername = "Eduphoria API Secret"; Description = "API Secret to access Eduphoria Data"; IntegratedSecuritySupported = $false}
      )

Initialize-Credentials $folders.activities.invoke('adhoc-processes');

#Default is the list, otherwise it downloads data.
Function Get-EduphoriaWebRequestData ($eduphoriaRoot, $destination, $filename, $urlSuffix) {
    $uriBase = "https://$eduphoriaRoot/AwareAPI/EdFi"
    $creds = Get-NetworkCredential("$normalizedDistrictName Eduphoria User")
    #New-Object Net.NetworkCredential ($districtUser, "")
    $webrequest = [System.Net.WebRequest]::Create("$uriBase$urlSuffix")
    $webrequest.Headers.Add("APIKey", $(Get-PlaintextPassword "Eduphoria API Key"))
    $webrequest.Headers.Add("APISecret", $(Get-PlaintextPassword "Eduphoria API Secret"))
    $webrequest.Credentials = $creds
    $webrequest.Timeout = 14400000 #4hours
	$webrequest.ReadWriteTimeout = 14359999;
    $response = $webrequest.GetResponse()
    $sr = [Io.StreamReader]($response.GetResponseStream()) 
    [xml]$xmlout = $sr.ReadToEnd()
	$webrequest.Abort()
    $response.Close()    
	#If no destination is specified return XML
    if ("$destination" -eq ""){
        #ReturnXML
        return $xmlOut
    }
    else {
        $filePath = "$destination\$(Get-ClearFilename $filename)"
        if (-not (Test-Path $destination)) {
            md $destination | out-null
        }
        #null is UTF-8
        $b = New-object Xml.XmlTextWriter $filePath,$null
        $b.formatting = [Xml.Formatting]::Indented
        $xmlout.Save($b)
        $b.Dispose()
		
        Write-Host "Download finshed for file: $filePath"
    }
}

Export-ModuleMember Get-EduphoriaWebRequestData