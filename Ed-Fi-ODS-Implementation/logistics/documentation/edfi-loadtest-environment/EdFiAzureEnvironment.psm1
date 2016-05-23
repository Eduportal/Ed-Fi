$script:AzureOpsLog = @()
function Out-AzureOpsLog {
    [cmdletbinding()] param(
        [parameter(mandatory=$true, ValueFromPipeline=$true)] $azureOperation
    )
    $azureOperation = Add-Member -InputObject $azureOperation -NotePropertyName "LogDate" -NotePropertyValue (get-date) -passthru
    $script:AzureOpsLog += @($azureOperation)
    if ($verbosePreference -ne "SilentlyContinue") {
        write-host "$($azureOperation.OperationDescription) - $($azureOperation.OperationId) - $($azureOperation.OperationStatus)"
    }
}

function Get-AzureOpsLog {
    [cmdletbinding()] param()
    return $script:AzureOpsLog
}

<#
.synopsis
Install the WinRM certificate to the local certificate store for a given Azure VM
.parameter ServiceName
The name of the Cloud Service that the VM belongs to
.parameter Name
The name of the VM
#>
Function Install-WinRMCertificateForVM {
    [cmdletbinding()] param(
        [string] $ServiceName, 
        [string] $Name
    )

    $Me = [Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()
    $IsAdmin= $Me.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
    if (-not $IsAdmin) {
        Write-Error "Must run PowerShell elevated to install WinRM certificates."
        return
    }
    
    Write-Verbose "Installing WinRM Certificate for remote access: $ServiceName $Name"
    $WinRMCert = (Get-AzureVM -ServiceName $ServiceName -Name $Name | select -ExpandProperty vm).DefaultWinRMCertificateThumbprint
    $AzureX509cert = Get-AzureCertificate -ServiceName $ServiceName -Thumbprint $WinRMCert -ThumbprintAlgorithm sha1

    $certTempFile = [IO.Path]::GetTempFileName()
    $AzureX509cert.Data | Out-File $certTempFile

    # Target The Cert That Needs To Be Imported
    $CertToImport = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2 $certTempFile

    $store = New-Object System.Security.Cryptography.X509Certificates.X509Store "Root", "LocalMachine"
    $store.Open([System.Security.Cryptography.X509Certificates.OpenFlags]::ReadWrite)
    $store.Add($CertToImport)
    $store.Close()
    
    Remove-Item $certTempFile
}

<#
.synopsis
Add a DNS server entry to the net.cfg XML for an Azure VNet
.parameter netCfgXml
An object representing the XML for the net.cfg file
.parameter dnsServerName
The name of the DNS server
.parameter dnsServerIp
The IP address of the DNS server
.parameter vnetName
The name of the Azure VNet
#>
Function Add-DnsServerToAzureNetCfg {
    [cmdletbinding()] param(
        [parameter(mandatory=$true,ValueFromPipeline=$true)] [System.Xml.XmlDocument] $netCfgXml,
        [parameter(mandatory=$true)] [string] $dnsServerName,
        [parameter(mandatory=$true)] [string] $dnsServerIP,
        [parameter(mandatory=$true)] [string] $vnetName
    )

    $nsManager = New-Object System.Xml.XmlNamespaceManager($netCfgXml.NameTable)
    $nsManager.AddNamespace("default","http://schemas.microsoft.com/ServiceHosting/2011/07/NetworkConfiguration")

    # The <Dns> element appears to exist by default, even if it is empty
    $dnsElem = $netCfgXml.SelectSingleNode('/default:NetworkConfiguration/default:VirtualNetworkConfiguration/default:Dns',$nsManager)

    # There should only be ONE <DnsServers> element; check to see if it exists before creating
    $dnsServersXpath = '/default:NetworkConfiguration/default:VirtualNetworkConfiguration/default:Dns/default:DnsServers'
    $dnsServersElem = $netCfgXml.SelectSingleNode($dnsServersXpath, $nsManager)
    if (-not $dnsServersElem) {
        $dnsServersElem = $netCfgXml.CreateElement('DnsServers', $netCfgXml.DocumentElement.NamespaceURI)
        $dnsElem.AppendChild($dnsServersElem) | out-null
    }
        
    # There should only be ONE <DnsServer> element for a given DNS server name; check to see if it exists before creating
    $newDnsServerXpath = "/default:NetworkConfiguration/default:VirtualNetworkConfiguration/default:Dns/default:DnsServers/default:DnsServer[@name='$dnsServerName']"
    $newDnsServerElem = $netCfgXml.SelectSingleNode($newDnsServerXpath, $nsManager)
    if (-not $newDnsServerElem) {
        $newDnsServerElem = $netCfgXml.CreateElement('DnsServer', $netCfgXml.DocumentElement.NamespaceURI)
        $newDnsServerElem.SetAttribute("name", $dnsServerName)
        $newDnsServerElem.SetAttribute("IPAddress", $dnsServerIp)
        $dnsServersElem.AppendChild($newDnsServerElem) | out-null
    }
    else {
        write-verbose "There is already a DNS server entry present for '$dnsServerName' under the <Dns> element, skipping..."
    }
    
    # There should by only ONE <DnsServersRef> node under a given <VirtualNetworkSite> node; check to see if it exists before creating
    $dsrXpath = "/default:NetworkConfiguration/default:VirtualNetworkConfiguration/default:VirtualNetworkSites/default:VirtualNetworkSite[@name='$vnetName']/default:DnsServersRef"
    $dsrElem = $netCfgXml.SelectSingleNode($dsrXpath, $nsManager)
    if (-not $dsrElem) {
        $dsrElem = $netCfgXml.CreateElement('DnsServersRef', $netCfgXml.DocumentElement.NamespaceURI)
        $vnsXpath = "/default:NetworkConfiguration/default:VirtualNetworkConfiguration/default:VirtualNetworkSites/default:VirtualNetworkSite[@name='$vnetName']"
        $vnsElem = $netCfgXml.SelectSingleNode($vnsXpath, $nsManager)
        $vnsElem.AppendChild($dsrElem) | out-null 
   }

    # There should be only ONE <DnsServerRef> element for a given DNS server name; check to see if it exists before creating
    $newDsrXpath = "/default:NetworkConfiguration/default:VirtualNetworkConfiguration/default:VirtualNetworkSites/default:VirtualNetworkSite[@name='$vnetName']/default:DnsServersRef/default:DnsServerRef[@name='$dnsServerName']"
    $newDsrElem = $netCfgXml.SelectSingleNode($newDsrXpath, $nsManager)
    if (-not $newDsrElem) {
        $newDsrElem = $netCfgXml.CreateElement('DnsServerRef', $netCfgXml.DocumentElement.NamespaceURI)
        $newDsrElem.SetAttribute("name", $dnsServerName)
        $dsrElem.AppendChild($newDsrElem) | out-null
    }
    else {
        write-verbose "There is already a DNS server entry present for '$dnsServerName' under the '$vnetName' VNet, skipping..."
    }

    return $netCfgXml
}

function Get-EdFiAzureWinRMUri {
    [cmdletbinding()] param(
        [parameter(mandatory=$true)] [Microsoft.WindowsAzure.Commands.ServiceManagement.Model.PersistentVMRoleContext] $VM
    )

    # It doesn't hurt to do this multiple times
    Install-WinRMCertificateForVM -Name $VM.Name -ServiceName $VM.ServiceName

    $vmWinRMUri = Get-AzureWinRMUri -Name $VM.Name -ServiceName $VM.ServiceName
    return $vmWinRMUri
}

function Wait-UntilAzureVMReady {
    [cmdletbinding()] param(
        [parameter(mandatory=$true)] [Microsoft.WindowsAzure.Commands.ServiceManagement.Model.PersistentVMRoleContext] $VM,
        $sleepSeconds = 10
    )
    while ($VM.Status -ne "ReadyRole") {
        sleep $sleepSeconds
        try {
            $VM = Get-AzureVM -Name $VM.Name -ServiceName $VM.ServiceName
            write-verbose "VM status for '$($VM.Name)' is '$($VM.Status)'"
            if ($VM.status -match "^Stop") {
                throw "Virtual machine $($VM.Name) is stoppped."
            }
        }
        catch [Microsoft.WindowsAzure.Commands.ServiceManagement.IaaS.GetAzureVMCommand] {
            # Sometimes Azure cmdlets just fail! This is fun!
            write-verbose "Got an AzureVMCommand failure; continuing..."
            continue
        }
    }
    return $VM
}

<#
.synopsis
Create a new Azure VM in an Ed-Fi promotion environment
.parameter BaseName
The base name for the VM. This is used directly for the cloud service name, and is also the first part of the VM name.
.parameter Count
The number of VMs to create. VMs will be named $BaseName-$VMNumber - for instance, passing -BaseName edfi-test and -Count 2, you'll end up with two VMs created in the edfi-test cloud service, named edfi-test-1 and edfi-test-2. 
.parameter BaseImageFamilyName
The family name of the image to use as the base. See available images with Get-AzureVMIage and note the .ImageFamily property of the images that come back. New-EdFiEnvironmentVM always uses the most recently updated vesion of the image family. 
.parameter Size
The size of the VM(s) to create. See sizes in the documentation for New-AzureVMConfig's -InstanceSize parameter
.parameter AffinityGroupName
The name of the affinity group in which to create the VM
.parameter VNetName
The name of the VNet in which to create the VM
.parameter AdminCredential
A PSCredential object that contains the desired administrator username and password for the new VM
.parameter CustomNetworkConfigurationSb
A scriptblock that is invoked to customize the network configuration of the VM before it is created. For instance, you could use this to set a VM to have a static IP address. The scriptblock MUST accept a [Microsoft.WindowsAzure.Commands.ServiceManagement.Model.PersistentVMRoleContext] object as its first argument, and it MUST return that object after it is modified. It may optionally accept other arguments (see the -CustomNetworkConfigurationArgs parameter).
.parameter CustomNetworkConfigurationArgs
If you pass a CustomNetworkConfigurationSb parameter, you may wish to pass in arguments to it when it is invoked. Provide those arguments here. Note that the CustomNetworkConfigurationSb MUST accept a VM object as its first argument, but you do NOT need to specify that argument here - it is prepended automatically to this list. 
.parameter WaitUntilReady
Do not return until the VM's status is "ReadyRole"
#>
Function New-EdFiEnvironmentVM {
    [cmdletbinding()] param(
        [parameter(mandatory=$true)] [string] $BaseName,
        [parameter(mandatory=$true)] [int] [ValidateRange(1,8)] $Count,
        [parameter(mandatory=$true)] [string] $BaseImageFamilyName,
        [parameter(mandatory=$true)] [string] $Size,
        [parameter(mandatory=$true)] [string] $AffinityGroupName,
        [parameter(mandatory=$true)] [string] $VNetName,
        [parameter(mandatory=$true)] [System.Management.Automation.PSCredential] $AdminCredential,
        [ScriptBlock] $CustomNetworkConfigurationSb,
        [array] $CustomNetworkConfigurationArgs,
        [switch] $WaitUntilReady
    )

    if ($PSCmdlet.ParameterSetName -eq "DomainController" -and $Count -gt 1) {
        throw "Creation of redundant domain controllers is not currently supported."
    }

    $baseImage = Get-AzureVMImage |? { $_.ImageFamily -like $BaseImageFamilyName } | sort-object PublishedDate | select-object -last 1
    write-verbose "Using base image named '$($baseImage.ImageFamily)' updated on '$($baseImage.PublishedDate)'"

    $finishedVMs = @()
    foreach ($c in 1..$Count) {
        $vmName = "$BaseName-$c"

        if (Get-AzureVM -Name $vmName -ServiceName $BaseName) {
            write-verbose "Found existing server '$vmName'"
        }
        else {
            $vmConfig = New-AzureVMConfig -ImageName $baseImage.ImageName -Name $vmName -InstanceSize $Size `
                -HostCaching "ReadWrite" -DiskLabel "System" -AvailabilitySetName $BaseName |
                Add-AzureProvisioningConfig -Windows -AdminUsername $AdminCredential.Username -Password $AdminCredential.GetNetworkCredential().Password |
                Set-AzureSubnet -SubnetNames $SubnetName

            if ($CustomNetworkConfigurationSb) {
                $CustomNetworkConfigurationArgs = @($vmConfig) + $CustomNetworkConfigurationArgs
                $vmConfig = invoke-command $CustomNetworkConfigurationSb -argumentList $CustomNetworkConfigurationArgs
            }

            $service = Get-AzureService -ServiceName $BaseName -ErrorAction SilentlyContinue
            write-verbose "Deploying VM '$vmName'..."

            New-AzureVM -VMs $vmConfig -ServiceName $BaseName -VNetName $VNetName -AffinityGroup $AffinityGroupName | Out-AzureOpsLog
        }

        $deployedVM = Get-AzureVM -Name $vmName -ServiceName $BaseName
        if ($deployedVM.Status -match "^Stopped") {
            write-verbose "VM '$vmName' was stopped. Starting..."
            Start-AzureVM -name $vmName -ServiceName $BaseName | Out-AzureOpsLog
        }
        else {
            write-verbose "Waiting for '$vmName' to start..."
        }

        if ($WaitUntilReady) {
            $deployedVM = Wait-UntilAzureVMReady -VM $deployedVM
        }
        $finishedVMs += @($deployedVM)
    }
    return $finishedVMs
}

function Invoke-EdFiEnvironmentCommand {
    [cmdletbinding()] param(
        [parameter(mandatory=$true)] [Microsoft.WindowsAzure.Commands.ServiceManagement.Model.PersistentVMRoleContext] $VM,
        [parameter(mandatory=$true)] [System.Management.Automation.PSCredential] $Credential,
        [parameter(mandatory=$true)] [ScriptBlock] $scriptBlock,
        [Array] $argumentList,
        [switch] $WaitUntilReady
    )
    $vmWinRMUri = Get-EdFiAzureWinRMUri $VM

    $tryCount = 0
    $success = $false
    while (-not $success) {
        try {
            [string]$output = Invoke-Command -ConnectionUri $vmWinRMUri -credential $Credential -ScriptBlock $scriptBlock -ArgumentList $argumentList
            write-verbose $output
            $success = $true
        }
        catch [System.Management.Automation.Remoting.PSRemotingTransportException] {
            write-verbose "Failed to connect to server"
            $tryCount += 1
            if ($tryCount -gt 1) {
                # Give up
                throw $_
            }
            sleep 15
        }
    }

    if ($WaitUntilReady) {
        Wait-UntilAzureVMReady -VM $VM | out-null
    }
}

function Install-EdFiEnvironmentDomain {
    [cmdletbinding()] param(
        [parameter(mandatory=$true)] [Microsoft.WindowsAzure.Commands.ServiceManagement.Model.PersistentVMRoleContext] $VM,
        [parameter(mandatory=$true)] [System.Management.Automation.PSCredential] $LocalAdminCredential,
        [parameter(mandatory=$true)] [string] $ADDomainName,
        [parameter(mandatory=$true)] [System.Management.Automation.PSCredential] $DSRMCredential,
        [switch] $WaitUntilReady
    )
    ## This is the section that would need work to support automatically provisioning more than one domain controller
    write-verbose "Installing Active Directory and setting '$($VM.Name)' as the domain controller..."
    $setupAdSb = {
        param($ADDomainName,$DSRMPassword)
        Install-WindowsFeature "AD-Domain-Services" | out-null
        import-module ADDSDeployment
        $cs = gwmi win32_computersystem 
        if ($cs.PartOfDomain) {
            write-output "Computer already part of domain '$($cs.Domain)'"
        }
        else {
            write-output "Installing current VM as domain controller for new domain '$ADDomainName'..."
            Install-ADDSForest -DomainName $ADDomainName -SafeModeAdministratorPassword $DSRMPassword -InstallDns -Force -NoRebootOnCompletion
            Set-DnsServerForwarder -IPAddress '8.8.8.8'
            Restart-Computer -Force
            # The server will reboot at this point
        }
    }
    $setupAdArgs = @($ADDomainName,$DSRMCredential.Password)
    Invoke-EdFiEnvironmentCommand -VM $VM -Credential $LocalAdminCredential -scriptBlock $setupAdSb -argumentList $setupAdArgs -WaitUntilReady:$WaitUntilReady
}

function Join-EdFiEnvironmentDomain {
    [cmdletbinding()] param(
        [parameter(mandatory=$true)] [Microsoft.WindowsAzure.Commands.ServiceManagement.Model.PersistentVMRoleContext] $VM,
        [parameter(mandatory=$true)] [System.Management.Automation.PSCredential] $LocalAdminCredential,
        [parameter(mandatory=$true)] [string] $ADDomainName,
        [parameter(mandatory=$true)] [System.Management.Automation.PSCredential] $DomainAdminCredential,
        [switch] $WaitUntilReady
    )
    write-verbose "Joining '$($VM.name)' to the domain..."
    $joinDomainSb = {
        param($ADDomainName,$DomainAdminCredential)
        $cs =gwmi win32_computersystem 
        if ($cs.PartOfDomain) {
            write-output "Computer already part of domain '$($cs.Domain)'"
        }
        else {
            write-output "Joining domain '$ADDomainName'..."
            Add-Computer -Domain $ADDomainName -Credential $DomainAdminCredential -Restart
        }
    }
    $joinDomainArgs = @($ADDomainName, $DomainAdminCredential)
    Invoke-EdFiEnvironmentCommand -VM $VM -Credential $LocalAdminCredential -scriptBlock $joinDomainSb -argumentList $joinDomainArgs -WaitUntilReady:$WaitUntilReady
}

function Install-ChocolateyOnVM {
    [cmdletbinding()] param(
        [parameter(mandatory=$true)] [Microsoft.WindowsAzure.Commands.ServiceManagement.Model.PersistentVMRoleContext] $VM,
        [parameter(mandatory=$true)] [System.Management.Automation.PSCredential] $AdminCredential
    )
    write-verbose "Installing chocolatey "
    $chocoSb = {
        if (test-path env:ChocolateyInstall) {
            write-output "Chocolatey is already installed"
        }
        else {
            iex ((new-object net.webclient).DownloadString('https://chocolatey.org/install.ps1'))
        }
    }
    Invoke-EdFiEnvironmentCommand -VM $VM -Credential $AdminCredential -scriptBlock $chocoSb
}

Export-ModuleMember -function *