# *************************************************************************
# ©2013 Ed-Fi Alliance, LLC. All Rights Reserved.
# *************************************************************************
param(
    [string]$certPrefix = $(if("$Env:certPrefix" -ne "") {$Env:certPrefix}),
    [switch]$silent
)
#This function is here so it can be used during the module loading.
Function Write-HostInternal{
    if (-not ($silent)) {
        #$myInvocation.OffSetInLine is broken using name.length for now
        #normalize newlines.
        $baseline = $myinvocation.line.TrimStart().Replace("`r", "")
        $multiLineIndex = $baseline.IndexOf("`n")
        if ($multiLineIndex -gt 0 -and $multiLineIndex -gt $myinvocation.InvocationName.length) {
            $lines = @()
            $lines += $baseline.Split("`n")
            Invoke-Expression "Write-Host $($lines[0].substring($myinvocation.InvocationName.length))"
            for ($i = 1; $i -le ($lines.Length - 1); $i++) {
                if ((-not $lines[$i].ToLower().Contains("-nonewline"))) {
                    Invoke-Expression "Write-Host $($lines[$i]) -NoNewline"
                }
                else {
                    Invoke-Expression "Write-Host $($lines[$i])"
                }
            }
        }
        else {
            Invoke-Expression "Write-Host $($myinvocation.line.TrimStart().substring($myinvocation.InvocationName.length))"
        }
    }
}


$script:allCredentialMetadata = [hashtable[]] @()
$script:allCredentials = [hashtable] @{}

#Check for cert prefix. If it is not provided, write a warning and use loopback.
if ([string]::IsNullOrWhiteSpace($certPrefix)) {
    Write-Warning "A certificate name prefix (`$certPrefix) was not provided as a parameter or set as an envrionment variable."
    Write-HostInternal "Using loopback mode. (All credential metadata is valid, but returns null credentials.)"
    $loopback = $true
} 
else {
    $loopback = $false
    #Load assembly needed to access EncryptedXml Class
    [System.Reflection.Assembly]::LoadWithPartialName("System.Security")
    #Create encrypted Xml Object
    $eXml = New-Object System.Security.Cryptography.Xml.EncryptedXml
    #Get the credential store.
    $store = New-Object System.Security.Cryptography.X509Certificates.X509Store("My", [System.Security.Cryptography.X509Certificates.StoreLocation]::LocalMachine)
    $store.Open([System.Security.Cryptography.X509Certificates.OpenFlags]::ReadOnly)
    $certCollection = $store.Certificates
    $certName = "CN=$($certPrefix)_Encryption"
    $certNameRegEx = "^.*$($certName)(?:,.+)?`$"
    $matchingCerts = @()
    $matchingCerts += $certCollection | Where {$_.Subject -match $certNameRegEx}
    $cert = $matchingCerts[0]
    $store.Close()
    if ($cert -eq $null){throw "A certificate with subject matching: $certName was not found in the the local machine store."}
}

Function Add-CredentialMetadata
{
	param (
		# TODO: Required
		[hashtable[]] $credentialMetadata
	)
	
	if ($credentialMetadata.Length -eq 0) {
		return
	}
	
	if (($credentialMetadata | where {-not($_.ContainsKey("Name"))}) -ne $null) {
		throw "Credential hashtables must have a 'Name' key."
	}

	$script:allCredentialMetadata += $credentialMetadata
}

Function Initialize-Credentials
{
	param (
		$credentialsFolder
	)
    #If we are in loopback mode do nothing.
	if ($loopback) { 
        Write-HostInternal "Loopback Mode: No Credentials are initialized. All Credential Metadata is valid."
        return
    }
	$currentUserFQN = [Environment]::UserDomainName + "\" + [Environment]::UserName
	$credentialsFileName = GetCredentialsFileNameForCurrentEnvironment($credentialsFolder)

	Write-HostInternal "`nLoading credentials (using cert: $certName ) from '$credentialsFileName'...`n"

	# Create empty credentials file, if necessary
	if ((Test-Path $credentialsFileName) -eq $false) {
		"<Credentials version='1.0'/>" | Set-Content $credentialsFileName -Force
	}

	$credentialTemplate = "<Username/><EncryptedPassword isElement='true' />"
	#$allCredentials = [hashtable]@{}

	$xml = New-Object XML
	$xml.Load($credentialsFileName)
    $credsDirty = $false

	# Trap cryptographic exceptions, and quit processing credential names due to the scope of the trap block being outside the foreach.
	trap [Exception] {
		if ($_.Exception.GetType().FullName -eq "System.Security.Cryptography.CryptographicException") {
			Write-HostInternal "An error occurred while trying to decrypt the password.  Are you sure the passwords file was created under the current identity?" -ForegroundColor Red
			continue # With next line in this scope, effectively exiting the foreach loop below.
		}
	}
    
	foreach ($entry in $allCredentialMetadata) {
		Write-HostInternal "Loading credentials for" $entry.Name "..."
		$name = $entry.Name
		$description = $entry.Description
		$defaultUsername = $entry.DefaultUsername
		
		# Find the credential, by name
		if ($xml.Credentials.Credential -eq $null) {
            $matchingCredential = $null
        }
        else {
            $matchingCredential = $xml.Credentials.Credential | Where {$_.Attributes.GetNamedItem("name").Value -eq $name}
		}
        
		if ($matchingCredential -ne $null) {
			if ($matchingCredential.EncryptedPassword -ne "") {
                $encryptedData = New-Object System.Security.Cryptography.Xml.EncryptedData 
                $encryptedData.LoadXml($matchingCredential.EncryptedPassword.EncryptedData); 

                $symetricalKey = $eXml.GetDecryptionKey($encryptedData, $encryptedData.EncryptionMethod.KeyAlgorithm)
                $credHolder = $xml.CreateElement("temp")
                $credHolder.InnerXml = ($eXml.Encoding.GetString($eXml.DecryptData($encryptedData,$symetricalKey)))
                $password = ConvertTo-SecureString -Force -AsPlainText -String $credHolder.EncryptedPassword.InnerText
                #clean up creds from memory
                $credHolder.RemoveAll()
                $credHolder = $null
			} else {
				$password = ""
			}
			$allCredentials[$name] = ($matchingCredential.Username, $password)
		} else {
			$newCredentialXml = $xml.CreateElement("Credential")
            $nameAttrib = $xml.CreateAttribute("name")
            $nameAttrib.Value = "$name"
            $newCredentialXml.Attributes.Prepend($nameAttrib) | Out-Null
            $newCredentialXml.InnerXml = $credentialTemplate
			
			Write-HostInternal "`nCredentials are required for '$name'." -ForegroundColor Yellow
			Write-HostInternal "$description`n" -ForegroundColor DarkYellow
			
			$username = ""
			
			while ($username.length -eq 0) {
				if ($defaultUsername -ne $null) {
					Write-HostInternal "Username (" -ForegroundColor Yellow -NoNewline
					Write-HostInternal $defaultUsername -ForegroundColor DarkGray -NoNewline
					Write-HostInternal "): " -ForegroundColor Yellow -NoNewline
					
					$username = Read-HostInternal
					
					if ($username -eq $null -or $username.length -eq 0) {
						$username = $defaultUsername
					}
				} else {
					Write-HostInternal "Username: " -ForegroundColor Yellow -NoNewline
					$username = Read-HostInternal

					if ($username.length -eq 0) {
						Write-HostInternal "Username is required. " -ForegroundColor Red
					}
				}
			}
			
			$passwordEntryCount = 0
			
			do {
				$passwordEntryCount++

				if ($passwordEntryCount -gt 1) {
					Write-HostInternal "Passwords don't match.  Please try again.`n" -ForegroundColor Red
				}
				
				Write-HostInternal "Password: " -ForegroundColor Yellow -NoNewline
				[System.Security.SecureString]$password = Read-HostInternal -AsSecureString

				Write-HostInternal "Confirm:  " -ForegroundColor Yellow -NoNewline
				[System.Security.SecureString]$passwordConfirm = Read-HostInternal -AsSecureString
				
				$t1 = DecryptSecureString($password)
				$t2 = DecryptSecureString($passwordConfirm)
			} while ($t1 -ne $t2)
			
			$allCredentials[$name] = ($username, $password)

			$newCredentialXml.Username = [string]$username
			#Set the password
			$newCredentialXml.EncryptedPassword.InnerText = DecryptSecureString($password)
			#encrypt the password
            $edElement = $eXml.Encrypt($newCredentialXml.EncryptedPassword, $Cert)
            [System.Security.Cryptography.Xml.EncryptedXml]::ReplaceElement($newCredentialXml.EncryptedPassword, $edElement, $true)
            #add the credentials
            $xml.Credentials.AppendChild($newCredentialXml) | Out-Null
			
			$credsDirty = $true
		}
	}
	
	if ($credsDirty) {
		$xml.Save($credentialsFileName)

		Write-HostInternal "New credentials have been saved.  Hit ENTER to continue script execution (or type Ctrl+C to quit)."
		Read-HostInternal | out-null
	}

	if ($allCredentials.Count -eq 0) {
		Write-HostInternal "`nNo credentials were loaded."
	} elseif ($allCredentials.Count -eq 1) {
		Write-HostInternal "`n1 set of credentials was loaded."
	} else {
		Write-HostInternal "`n" $allCredentials.Count "sets of credentials were loaded."
	}
}

Function Clear-Credentials ()
{
	$script:credentials = @()
}

Function Clear-CredentialMetadata ()
{
	$script:credentialMetadata = @()
}

Function Show-CredentialsFromFile([String]$credentialsFileName)
{
	if ($loopback) {
        Write-HostInternal "Loopback Mode: Credential files cannot be loaded."
        return
    }
    $fqUsername = GetFullyQualifiedUsername
	Write-HostInternal "`nDisplaying credentials for $fqUsername from '$credentialsFileName'..."

	Write-HostInternal "You are about to show all the credentials with the passwords in plain text.  Are you sure you want to do this?"
	if ((Read-HostInternal).ToLower() -ne "y") {
		return
	}
	
	$xml = New-Object XML
	$xml.Load($credentialsFileName)
	$xml.Credentials.Credential | select Name, Username, @{Name="Password"; Expression={DecryptPassword $_.EncryptedPassword}}
}

Function Test-CredentialExists([string] $credentialsName) {
	if ($loopback) { return $true }
    return $allCredentials.ContainsKey($credentialsName)
}

Function Get-Username([string] $credentialsName) {
	if (-not(Test-CredentialExists $credentialsName)) {
		Write-HostInternal "WARNING: Credentials '$credentialsName' not found." -fore Cyan
		$null
		return
	}
    if ($loopback) {return $null}
	return $allCredentials[$credentialsName][0]
}

Function Get-PlaintextPassword([string] $credentialsName) {
	if (-not(Test-CredentialExists $credentialsName)) {
		Write-HostInternal "WARNING: Credentials '$credentialsName' not found." -fore Cyan
		$null
		return
	}

	# Loopback or no password? Return null.
	if ($loopback -or $allCredentials[$credentialsName][1] -eq "") {
		return $null
	}

	return DecryptSecureString([Security.SecureString]$allCredentials[$credentialsName][1])
}

Function Get-SecurePassword([string] $credentialsName) {
	if (-not(Test-CredentialExists $credentialsName)) {
		Write-HostInternal "WARNING: Credentials '$credentialsName' not found." -fore Cyan
		return $null
	}

	# Loopback or No password? Return null.
	if ($loopback -or $allCredentials[$credentialsName][1] -eq "") {
		return $null
	}

	return [Security.SecureString]$allCredentials[$credentialsName][1]
}

Function Get-PSCredential([string] $credentialsName) {
	if (-not(Test-CredentialExists $credentialsName)) {
		Write-HostInternal "WARNING: Credentials '$credentialsName' not found." -fore Cyan
		return $null
	}

    #No User name = no credential. The password has to be something.
    #This also covers loopback as the user name will be null.
    $userName = Get-Username $credentialsName
    if ([String]::IsNullOrWhiteSpace($userName)) {
        return $null
    }
    
	# No password? Return null.
	if ($allCredentials[$credentialsName][1] -eq "") {
		return $null
	}
	return new-object System.Management.Automation.PSCredential($userName, [Security.SecureString]$allCredentials[$credentialsName][1])
}

Function Get-NetworkCredential([string] $credentialsName) {
	if (-not(Test-CredentialExists $credentialsName)) {
		Write-HostInternal "WARNING: Credentials '$credentialsName' not found." -fore Cyan
		return $null
	}
    #No User name = no credential. The password has to be something.
    #This also covers loopback as the user name will be null.
    $userName = Get-Username $credentialsName
    if ([String]::IsNullOrWhiteSpace($userName)) {
        return $null
    }
	return new-object Net.NetworkCredential -argumentlist ($userName),(Get-PlaintextPassword $credentialsName)
}

# ==============================================================================================
#                                       Support functions
# ----------------------------------------------------------------------------------------------
Function GetCredentialsFileNameForCurrentEnvironment($credentialFolder) {
	#Regardless of the path the credential file will need to be encrypted with the cert indicated by the cert prefix.
    $computerName = "$Env:ComputerName".Trim()
    $compSpecificCredPath = "$credentialFolder\credentials-$($certPrefix)-$computerName.xml"
	$certPrefixOnlyCredPath = "$credentialFolder\credentials-$($certPrefix).xml"
    If (Test-Path $compSpecificCredPath) { return $compSpecificCredPath }
    If (Test-Path $certPrefixOnlyCredPath) { return $certPrefixOnlyCredPath }
    #Default to more specifc to prevent unintentional overides.
    return $compSpecificCredPath
}

Function GetFullyQualifiedUsername() {
	[Environment]::UserDomainName + "\" + [Environment]::UserName
}

Function DecryptSecureString ([System.Security.SecureString] $encryptedText) {
	[System.Runtime.InteropServices.marshal]::PtrToStringAuto([System.Runtime.InteropServices.marshal]::SecureStringToBSTR($encryptedText))
}

Function DecryptPassword ([System.Xml.XmlElement]$EncryptedPassword) {

    if ($EncryptedPassword -ne "") {
        $encryptedData = New-Object System.Security.Cryptography.Xml.EncryptedData 
        $encryptedData.LoadXml($EncryptedPassword.EncryptedData); 

        $symetricalKey = $eXml.GetDecryptionKey($encryptedData, $encryptedData.EncryptionMethod.KeyAlgorithm)
        $credHolder = $xml.CreateElement("temp")
        $credHolder.InnerXml = ($eXml.Encoding.GetString($eXml.DecryptData($encryptedData,$symetricalKey)))
        $password = $credHolder.EncryptedPassword.InnerText
        #clean up creds from memory
        $credHolder.RemoveAll()
        $credHolder = $null
	} else {
		$password = ""
	}

    return $password
}

Function Register-CredentialsByName ([string[]]$credentialNames, $path = "."){
    $resolvedPath = Resolve-Path "$path"
    #Do this here so that you don't have to be deploying to set credentials
    foreach($credentialName in $credentialNames) {
        # Initialize the named credentials
        Add-CredentialMetadata @(@{Name = "$credentialName"; DefaultUsername = $null; Description = "Credentials for $credentialName"; IntegratedSecuritySupported = $true})
    }

    # The credentials file should be for the user this script will be executing under.
    Initialize-Credentials "$path"
}
Function Read-HostInternal(){
    if (-not ($silent)) {
        Invoke-Expression "Read-Host $args"
    }
}
# ==============================================================================================

# Declare exports
Export-ModuleMember Add-CredentialMetadata, Clear-CredentialMetadata, Initialize-Credentials, Test-CredentialExists, Show-CredentialsFromFile, Get-PlaintextCredentials, Get-SecureCredentials, Get-Username, Get-PlaintextPassword, Get-SecurePassword, Get-PSCredential, Get-NetworkCredential, Register-CredentialsByName
