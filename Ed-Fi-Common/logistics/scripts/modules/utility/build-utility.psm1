
function Get-AncestorItem ([string]$path, [string]$itemName) {
    $pos = $path.LastIndexOf("\$itemName\")
    
    if ($pos -lt 0) {
        if ($path.EndsWith("\$itemName")) {
            $pos = $path.LastIndexOf("\$itemName")
        } else {
            throw "Unable to locate '$itemName' in path '$path'."
        }
    }
    
    $newPath = $path.Substring(0, $pos) + "\$itemName"
    
    Resolve-Path $newPath
}

function Test-XPath($targetFilePath, $xpath) {
    [xml] $fileXml = Get-Content $targetFilePath
    return $fileXml.SelectSingleNode($xpath) -ne $null
}

<#
.synopsis
Get the value of an Xpath from an XML document

.parameter TargetFile
The target XML file to modify

.parameter Xpath
The XPath to modify within the target XML file. 

Note that if there is a default namespace specified in the XML document, -TargetXmlNamespace must be provided and the Xpath must be modified in accordance with the namespaces passed to it. 

.parameter TargetXmlNamespace
In a document using an XML namespace (which would be specified as xmlns:"http://schema.example.com/whatever/"), a hashtable must be provided where the keys are short names for the namespace, and the value is the URL. 

This is true even for a document with just one, default namespace. 

See the help for Set-Xpath for examples on using XML namespaces. 
#>
function Get-Xpath {
    [cmdletbinding()] 
    param(
        $targetFilePath, 
        $xpath,
        [hashtable] $targetXmlNamespace
    )
    [xml] $fileXml = Get-Content $targetFilePath
    if ($targetXmlNamespace) {
        write-verbose "Getting $xpath using XML namespace(s): $($targetXmlNamespace.keys)"
        $nsManager = New-Object System.Xml.XmlNamespaceManager($fileXml.NameTable)
        foreach ($ns in $targetXmlNamespace.keys) {
            $nsManager.AddNamespace($ns, $targetXmlNamespace[$ns])
        }
        return $fileXml.SelectSingleNode($xpath, $nsManager).value
    }
    else {
        write-verbose "Getting $xpath without an XML namespace."
        return $fileXml.SelectNodes($xpath).value
    }
}
set-alias Peek-Xml Get-Xpath

<#
.synopsis
Make changes to an XML file

.parameter TargetFile
The target XML file to modify

.parameter Xpath
The XPath to modify within the target XML file. 

Note that if there is a default namespace specified in the XML document, -TargetXmlNamespace must be provided and the Xpath must be modified in accordance with the namespaces passed to it. 

.parameter Value
The new (string) value for the Xpath. 

If this parameter is a FileInfo object, or is a string path to a file ending in .config, assume it is an XML file, and use the contents of that file as the value for the Xpath. 

.parameter TargetXmlNamespace
In a document using an XML namespace (which would be specified as xmlns:"http://schema.example.com/whatever/"), a hashtable must be provided where the keys are short names for the namespace, and the value is the URL. 

This is true even for a document with just one, default namespace. 

.example 
Peek-Xml -TargetFile C:\file.xml -Xpath "/Asdf/Zxcv[@name='SubSomething']" -Value "NewValue"

This is the right way call this function if there is not a default XML namespace specified in the target file. For example: 

    <?xml version="1.0" encoding="utf-8"?>
    <Asdf name="Something" >
        <Zxcv name="SubSomething" value="OldValue" />
    </Asdf>

.example
Peek-Xml -TargetFile C:\file.xml -Value "NewValue" -TargetXmlNamespace @{ xxxx = "http://schema.example.com/whatever" } -Xpath "/xxxx:Asdf/xxxx:Zxcv[@name='SubSomething']"

This is the right way call this function if there is a default XML namespace defined in the target file. For example: 

    <?xml version="1.0" encoding="utf-8"?>
    <Asdf name="Something" xmlns:"http://schema.example.com/whatever">
        <Zxcv name="SubSomething" value="OldValue" />
    </Asdf>

Note that the -TargetXmlNamespace parameter must be specified, AND the -Xpath parameter must change to match it. The key for the default namespace ('xxxx' in the above example) can be anything, as long as it matches both arguments. 
#>
function Set-Xpath {
    [cmdletbinding()]
    param(
        [parameter(mandatory=$true)] $targetFile,
        [parameter(mandatory=$true)] $xpath,
        [parameter(mandatory=$true)] $value,
        [hashtable] $targetXmlNamespace
    )
    try {
        if ($targetFile -is [IO.FileInfo]) {
            $targetFilePath = $fileInfo.FullName
        }
        else {
            $targetFilePath = $targetFile
        }
	
        $targetFileName = [IO.Path]::GetFileName($targetFilePath)
	
        if ($value -is [IO.FileInfo]) {
            try {
                [xml]$valueDoc = Get-Content $value.FullName
            } 
            catch [Exception] {
                Write-Host "Unable to load XML document $($value.FullName): " $_.Exception.ToString()
                throw
            }
        
            Write-Host "Poking XML file $($value.Name) into $targetFilePath ..."
        } 
        elseif (([IO.File]::Exists($value)) -and ([IO.Path]::GetExtension($value).ToLower -eq ".config") ) {
            [xml]$valueDoc = Get-Content $value
            Write-Host "Poking XML file $([IO.Path]::GetFileName($value)) into $targetFilePath ..."
        } 
        else {
            Write-Host "Poking XML value at XPath '$xpath' into $targetFilePath ..."
        }

        [xml] $fileXml = Get-Content $targetFilePath
    
        if ($targetXmlNamespace) {
            write-verbose "Using XML namespace(s): $($targetXmlNamespace.keys)"
            $nsManager = New-Object System.Xml.XmlNamespaceManager($fileXml.NameTable)
            foreach ($ns in $targetXmlNamespace.keys) {
                $nsManager.AddNamespace($ns, $targetXmlNamespace[$ns])
            }
            $nodes = $fileXml.SelectSingleNode($xpath, $nsManager)
        }
        else {
            write-verbose "Using no XML namespaces"
            $nodes = $fileXml.SelectSingleNode($xpath)
        }
	
        $count = 0
        if($nodes) {
            foreach($node in $nodes) {
                if ($node) {
                    if ($valueDoc -ne $null) {
                        # Make sure element name matches contents of file containing snippet
                        if ($valueDoc.DocumentElement.Name -ne $node.Name) {
                            throw "XPath must refer to an XML element of the same name as the root element of the file referenced by $value."
                        }
    			
                        $node.InnerXml = $valueDoc.DocumentElement.InnerXml
                        $node.Attributes.RemoveAll()
                    
                        if ($valueDoc.DocumentElement.Attributes -ne $null){
                            foreach($attribute in $valueDoc.DocumentElement.Attributes){
                                $node.Attributes.SetNamedItem($attribute)
                            }
                        }
                    }
                    else {
                        $node.Value = $value
                    }
                    $count++
               }
            }
            $fileXml.Save($targetFilePath) 
            Write-Host "$count nodes updated in $targetFileName.`n"
        }
        else {
            throw "Poke-Xml could not locate $xpath in $targetFileName.  Update failed.`n"
        }

        if($count -eq 0) {
            throw "Poke-Xml could not update any nodes at $xpath in $targetFileName. Failed.`n"
        }
    } 
    catch {
        Write-Host "ERROR:`n $($_.Exception.ToString())"
        throw
    }
} 
set-alias Poke-Xml Set-Xpath

function Get-64BitStatus {
    return ([IntPtr]::Size -eq 8)
}
set-alias Is64Bit Get-64BitStatus

function Get-ProgramFilesX86Path() {
  if (Is64Bit -eq $true) {
    (Get-Item "Env:ProgramFiles(x86)").Value
  }
  else {
    (Get-Item "Env:ProgramFiles").Value
  }
}
set-alias Get-ProgramFiles-x86-Path Get-ProgramFilesX86Path

<#
.synopsis
Start a process and wait for it to exit
.description
This is a workaround for a stupid idiot bug in start-process that only triggers sometimes.
http://social.technet.microsoft.com/Forums/scriptcenter/en-US/37c1066e-b67f-4709-b195-aa2790216bd0
https://connect.microsoft.com/PowerShell/feedback/details/520554/
The bug has it return instantly even when -wait is passed to start-process, at least on Eric's local box. 
When that happens, $process.ExitCode hasn't been populated, and won't be, even when the process does actually exit.
System.Diagnostics.Process doesn't have that behavior, so that's what we're going to use instead
#>
function Invoke-ProcessAndWait {
    [cmdletbinding(DefaultParameterSetName="CheckExitCode")]
    param(
        [parameter(mandatory=$true)] [string] $command,
        [string[]] $argumentList,
        #[switch] $RedirectStandardError,
        [switch] $ShowStandardOutput,
        [parameter(ParameterSetName="Passthru")] [switch] $Passthru,
        [parameter(ParameterSetName="CheckExitCode")] [switch] $CheckExitCode
    )
    write-verbose "Running Invoke-ProcessAndWait in verbose mode. WARNING: this may show sensitive commandline arguments (passwords, connection strings) and should only be used in development!"
    write-verbose "Running '$command' with arguments '$argumentList'"
    $process = New-Object System.Diagnostics.Process
    $process.StartInfo = New-Object System.Diagnostics.ProcessStartInfo
    $process.StartInfo.FileName = $command
    #$process.StartInfo.RedirectStandardError = $RedirectStandardError
    if ($ShowStandardOutput) {
        $process.StartInfo.RedirectStandardOutput = $true
    }
    $process.StartInfo.UseShellExecute = $false # AKA don't run in a new window
    $process.StartInfo.Arguments = $argumentList
    $process.Start() | Out-Null
    if ($ShowStandardOutput) {
        $line = $process.StandardOutput.ReadLine()
        while ($line -ne $null) {
            Write-Host $line
            $line = $process.StandardOutput.ReadLine()
        }
    }
    $process.WaitForExit()
    write-verbose "Process exited with exit code $($process.ExitCode)"
    if ($PSCmdlet.ParameterSetName -eq "CheckExitCode") {
        if ($process.ExitCode -ne 0) {
            write-verbose "Command $command with arguments '$argumentList' exited with code $($process.ExitCode)"
            throw "Command '$command' with $($argumentList.count) arguments exited with code $($process.ExitCode)"
        }
    }
    else {
        return $process
    }
}

<#
.synopsis
Import a PFX certificate
.description 
Sets the PersistKeySet flag, which means that you can actually use the certificate later on
(Unlike, say, I dunno, the first-party Import-PfxCertificate function.)
#>
function Import-X509Certificate {
    [cmdletbinding()]
    # valid StoreLocation values: http://msdn.microsoft.com/en-us/library/system.security.cryptography.x509certificates.storelocation(v=vs.110).aspx
    # valid StoreName values: http://msdn.microsoft.com/en-us/library/system.security.cryptography.x509certificates.storename(v=vs.110).aspx
    param(
        [parameter(mandatory=$true)] [string] $Path,
        [ValidateSet("CurrentUser","LocalMachine")] [string] $StoreLocation = "CurrentUser",
        [ValidateSet("AddressBook","AuthRoot","CertificateAuthority",
            "Disallowed","My","Root","TrustedPeople","TrustedPublisher")] [string] $StoreName = "My",
        [string] $pfxPassword,
        [switch] $exportable,
        [switch] $protected
    )

    $flags = [System.Security.Cryptography.X509Certificates.X509KeyStorageFlags]::PersistKeySet
    if ($exportable) {
        $flags = $flags -bxor [System.Security.Cryptography.X509Certificates.X509KeyStorageFlags]::Exportable
    }
    if ($protected) {
        $flags = $flags -bxor [System.Security.Cryptography.X509Certificates.X509KeyStorageFlags]::UserProtected
    }

    if (-not $pfxPassword) {
        $securePfxPassword = read-host "Enter the certificate password" -AsSecureString
    }
    elseif ($pfxPassword.gettype() -ne "SecureString") {
        $securePfxPassword = ConvertTo-SecureString -string $pfxPassword -AsPlainText -force
    }
    else {
        $securePfxPassword = $pfxPassword
    }

    $pfx = new-object System.Security.Cryptography.X509Certificates.X509Certificate2
    $pfx.import($Path, $securePfxPassword, $flags)

    $store = new-object System.Security.Cryptography.X509Certificates.X509Store($StoreName, $StoreLocation)
    $store.open('MaxAllowed')
    $store.add($pfx)
    $store.close()
    write-verbose "Imported certificate from '$path' into 'cert:/$StoreLocation/$StoreName'."
}

Export-ModuleMember -function * -alias *