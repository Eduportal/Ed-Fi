[cmdletbinding()] param(
    [parameter(mandatory=$true)] [string] $PromotionEnvironmentName,
    [string[]] $taskList
)

$errorActionPreference = "Stop"
$VerbosePreference = "SilentlyContinue"

#### Initial Setup

. "$PSScriptRoot\$PromotionEnvironmentName.vars.ps1"

## MODULES
# -   You must import the storage module before importing the Azure module. 
#     If you import Azure in a session before Storage, you must end the session and restart; you cannot simply remove Azure and import Storage
#     See the bug: https://connect.microsoft.com/PowerShell/feedback/details/888873/azure-module-conflict-with-storage-module
# -   The Hyper-V module is required for New-VHD cmdletf
# -   PSCX Mount-VHD conflicts with Hyper-V Mount-VHD
# -   OctopusDSC from https://github.com/OctopusDeploy/OctopusDSC
if ((Get-WindowsOptionalFeature -Online -FeatureName "Microsoft-Hyper-V-Management-Powershell").State -ne "Enabled") {
    Enable-WindowsOptionalFeature -FeatureName "Microsoft-Hyper-V-All","Microsoft-Hyper-V-Tools-All","Microsoft-Hyper-V-Management-Clients","Microsoft-Hyper-V-Management-Powershell" -Online
}
@("EdFiAzureEnvironment","PSCX") |% { if (get-module $_) { remove-module $_ } }
#@("Storage","Azure","Hyper-V","PSDesiredStateConfiguration","OctopusDSC","$PSScriptRoot\EdFiAzureEnvironment.psm1") |% { import-module $_ }
@("Storage","Azure","Hyper-V","PSDesiredStateConfiguration","$PSScriptRoot\EdFiAzureEnvironment.psm1") |% { import-module $_ }

if ($ForceVerbose) {
    $VerbosePreference = "Continue"
}

# It is best to read these in like this, rather than setting them in the file
# Setting them to global here means you'll only enter them once per session, rather than every time you run the script
if (-not $global:DomainAdminCred) {
    $global:DomainAdminCred = Get-Credential -Username "${DomainAdminUsername}@${ADDomainName}" -Message "Enter the Domain Admin password"
}
if (-not $global:DSRMCred) {
    $global:DSRMCred = Get-Credential -Username "Administrator" -Message "Enter the Safe Mode/DSRM password for the DC(s)"
}
if (-not $global:MemberLocalAdminCred) {
    $global:MemberLocalAdminCred = Get-Credential -Username $DomainMemberLocalAdminUsername -Message "Enter the local admin password for *all* domain memebers"
}
# This is used to connect to the DCs before they are joined to the domain
$PreDomainAdminCred = new-object System.Management.Automation.PSCredential("${DomainAdminUsername}", $global:DomainAdminCred.Password)

if (-not $global:OctopusApiKey) {
    read-host -AsSecureString "API Key for Octopus server"
}
$PlaintextOctopusApiKey = (new-object PSCredential ("api", $global:OctopusApiKey)).GetNetworkCredential().Password

## DSC Stuff
<#
if (test-path .\OctopusTentacle) {
    rm -recurse -force .\OctopusTentacle
}
Configuration OctopusTentacle {
    param(
        $APIKey,
        $OctopusServerUrl,
        $Environments,
        $Roles,
        $ListenPort = 9090,
        $NodeName
    )
    Import-DscResource -Module OctopusDSC
    Node $NodeName {
        cTentacleAgent OctopusTentale {
            Ensure = "Present"
            State = "Started"
            Name = "Tentacle"
            APIKey = $APIKey
            OctopusServerUrl = $OctopusServerUrl
            Environments = $Environments
            Roles = $Roles
            ListenPort = $ListenPort
        }
    }
}
#>

write-verbose "Looking for an Azure subscription named '$SubscriptionName'..."
$AzureSubscription = Get-AzureSubscription |? { $_.SubscriptionName -eq $SubscriptionName }
if (-not $AzureSubscription) {
    throw "Cannot find an Azure subscription called '$SubscriptionName'"
}
Select-AzureSubscription -Id $AzureSubscription.SubscriptionId | out-null

try {
    $affinityGroups = Get-AzureAffinityGroup
}
catch {
    Add-AzureAccount
    $affinityGroups = Get-AzureAffinityGroup
}

if ($affinityGroups |? { $_.Name -eq $affinityGroupName }) {
    write-verbose "Found affinity group named '$affinityGroupName'"
}
else {
    write-verbose "Creating affinity group named '$affinityGroupName'"
    New-AzureAffinityGroup -Name $affinityGroupName -Description $affinityGroupDescription -Location $DatacenterLocation | Out-AzureOpsLog
}

try {
    Get-AzureStorageAccount -StorageAccountName $StorageAccountName | out-null
    write-verbose "Found storage account named '$StorageAccountName'"
}
catch {
    write-verbose "Creating storage account named '$StorageAccountName"
    New-AzureStorageAccount -StorageAccountName $StorageAccountName -AffinityGroup $AffinityGroupName | Out-AzureOpsLog
}
Set-AzureSubscription -SubscriptionId $AzureSubscription.SubscriptionId -CurrentStorageAccountName $StorageAccountName

# Get the Azure image to use for all our VMs
# Deprecated after everything is entirely moved over to the module
$baseImage = Get-AzureVMImage |? { $_.ImageFamily -like $BaseImageFamilyName } | sort-object PublishedDate | select-object -last 1
write-verbose "Using base image named '$($baseImage.ImageFamily)' updated on '$($baseImage.PublishedDate)'"

#### Tasks
## If the -taskList argument was passed, only those tasks will run in the order they appear here
## If no tasks were passed via -taskList, all of the tasks in this section will run in the order they appear here

if (-not $taskList -or $taskList -contains 'SetupNetwork') {
    if (-not $ManualNetworkCreationComplete) {
        while ((read-host "Has the VNet been manualy configured in the Azure web UI? If so, type 'yes' to continue.") -ne "yes") {}
    }
}


if (-not $taskList -or $taskList -contains 'CreateDc') {
    $efevParam = @{
        BaseName = $DomainControllerBaseName
        Count = $DomainControllerCount
        Size = $DomainControllerSize
        BaseImageFamilyName = $BaseImageFamilyName
        AffinityGroupName = $affinityGroupName
        VNetName = $vnetName
        AdminCredential = $PreDomainAdminCred
        CustomNetworkConfigurationSb = { param($vm, $ipaddr); Set-AzureStaticVNetIP -VM $vm -IPAddress $ipaddr }
        CustomNetworkConfigurationArgs = @($SubnetFirstUsableAddress.ToString())
        WaitUntilReady = $true
    }
    $vms = New-EdFiEnvironmentVM @efevParam
    $vms |% {
        Install-EdFiEnvironmentDomain -VM $_ -LocalAdminCredential $PreDomainAdminCred -ADDomainName $ADDomainName -DSRMCredential $DSRMCred -WaitUntilReady
        sleep 60 # this could probably be shorter but, even passing -WaitUntilReady, I keep getting failures b/c the next steps won't connect via WinRM.
        Install-ChocolateyOnVM -VM $_ -AdminCredential $global:DomainAdminCred
    }
}

if (-not $taskList -or $taskList -contains 'SetupVnetDns') {
    write-verbose "Configuring the VNet to use the AD servers for DNS..."
    $netCfgXml = [xml](Get-AzureVNetConfig).XMLConfiguration
    foreach ($vm in (get-AzureVM -ServiceName $DomainControllerBaseName)) {
        $netCfgXml = Add-DnsServerToAzureNetCfg -netCfgXml $netCfgXml -dnsServerName $vm.name -dnsServerIP $vm.IpAddress -vnetName $vnetName
    }
    $netCfgPath = [System.IO.Path]::GetTempFileName()
    $netCfgXml.Save($netCfgPath)
    Set-AzureVNetConfig -ConfigurationPath $netCfgPath | Out-AzureOpsLog
    rm $netCfgPath
}

if (-not $taskList -or $taskList -contains 'CreateWeb') {
    $efevParam = @{
        BaseName = $WebServerBaseName
        Count = $WebServerCount
        Size = $WebServerSize
        BaseImageFamilyName = $BaseImageFamilyName
        AffinityGroupName = $affinityGroupName
        VNetName = $vnetName
        AdminCredential = $global:MemberLocalAdminCred
        WaitUntilReady = $true
    }
    $vms = New-EdFiEnvironmentVM @efevParam
    $octoPort = 9090
    $vms |% {
        Join-EdFiEnvironmentDomain -VM $_ -LocalAdminCredential $global:MemberLocalAdminCred -ADDomainName $ADDomainName -DomainAdminCredential $DomainAdminCred -WaitUntilReady
        sleep 60 # this could probably be shorter but, even passing -WaitUntilReady, I keep getting failures b/c the next steps won't connect via WinRM.
        Install-ChocolateyOnVM -VM $_ -AdminCredential $global:DomainAdminCred

        # Octopus & DSC: 
        # The NodeName must be the *pulic DNS name* 
        <#
        $configOut = OctopusTentacle -ApiKey $PlaintextOctopusApiKey -OctopusServerUrl $OctopusServerUrl -Environments $OctopusEnvironmentName -Roles "WebServer" -NodeName "$($_.ServiceName).cloudapp.net" -port $octoPort
        $cimSessionOption = New-CimSessionOption -UseSsl
        $winRmUri = Get-EdFiAzureWinRMUri $_
        $cimSession = New-CimSession -SessionOption $cimSessionOption -ComputerName $winRmUri.Host -Port $winRmUri.Port -Authentication Negotiate -Credential $global:DomainAdminCred
        Start-DscConfiguration -Path .\OctopusTentacle -Verbose -wait -CimSession $cimSession
        Test-DscConfiguration
        $octoPort += 1
        #>
    }
}

if (-not $taskList -or $taskList -contains 'CreateApp') {
    $efevParam = @{
        BaseName = $AppServerBaseName
        Count = $AppServerCount
        Size = $AppServerSize
        BaseImageFamilyName = $BaseImageFamilyName
        AffinityGroupName = $affinityGroupName
        VNetName = $vnetName
        AdminCredential = $global:MemberLocalAdminCred
        WaitUntilReady = $true
    }
    $vms = New-EdFiEnvironmentVM @efevParam
    $vms |% {
        Join-EdFiEnvironmentDomain -VM $_ -LocalAdminCredential $global:MemberLocalAdminCred -ADDomainName $ADDomainName -DomainAdminCredential $DomainAdminCred -WaitUntilReady
        sleep 60 # this could probably be shorter but, even passing -WaitUntilReady, I keep getting failures b/c the next steps won't connect via WinRM.
        Install-ChocolateyOnVM -VM $_ -AdminCredential $global:DomainAdminCred
    }
}

if (-not $taskList -or $taskList -contains 'CreateDb') {
    $efevParam = @{
        BaseName = $DbServerBaseName
        Count = $DbServerCount
        Size = $DbServerSize
        BaseImageFamilyName = $BaseImageFamilyName
        AffinityGroupName = $affinityGroupName
        VNetName = $vnetName
        AdminCredential = $global:MemberLocalAdminCred
        WaitUntilReady = $true
    }
    $vms = New-EdFiEnvironmentVM @efevParam
    $vms |% {
        Join-EdFiEnvironmentDomain -VM $_ -LocalAdminCredential $global:MemberLocalAdminCred -ADDomainName $ADDomainName -DomainAdminCredential $DomainAdminCred -WaitUntilReady
    }
}


if (-not $taskList -or $taskList -contains 'UploadDbIso') {
    $fileServer = Get-AzureVM -ServiceName $DomainControllerBaseName | select -first 1
    $containerUrl = "http://$StorageAccountName.blob.core.windows.net/vhdstore/SqlServerIsoContainer.vhd"

    if (Get-AzureDataDisk -VM $fileServer |? {$_.SourceMediaLink -eq "$containerUrl"}) {
        write-verbose "Fileserver $($fileserver.Name) already has the disk attached, skipping..."
    }
    else {
        write-verbose "Creating a temporary VHD to transfer the SQL Server ISO to Azure (the disk will be formatted automatically)..."

        $containerPath = "$PSScriptRoot\SqlServerIsoContainer.vhd"
        if (test-path $containerPath) {
            write-verbose "Found existing VHD container at $containerPath; using that VHD..."
        }
        else {
            # Azure VMs throw a fit if you try to add data disks that aren't even MB lmao
            $containerSize = (get-item $SqlServerIsoPath).Length + 200MB 
            $padding = 1024*1024 - ($containerSize % (1024*1024))
            $containerSize += $padding

            $volume = new-vhd -Path $containerPath -SizeBytes $containerSize |
                Mount-VHD -PassThru | 
                Initialize-Disk -PartitionStyle mbr -Confirm:$false -PassThru | 
                New-Partition -UseMaximumSize -AssignDriveLetter -MbrType IFS | 
                Format-Volume -NewFileSystemLabel "MSSQLISO" -Confirm:$false

            copy-item $SqlServerIsoPath "$($volume.DriveLetter):\" 
            Dismount-VHD $containerPath
        }

        write-verbose "Uploading temporary VHD to Azure"
        $containerUrl = "http://$StorageAccountName.blob.core.windows.net/tempvhdstore/SqlServerIsoContainer.vhd"
        Add-AzureVhd -Destination $containerUrl -LocalFilePath $containerPath

        write-verbose "Copying the SQL Server ISO to the domain controller..."
        $lun = (Get-AzureDataDisk -VM $fileServer | sort-object LUN | select -last 1).LUN +1
        Add-AzureDataDisk -VM $fileServer -ImportFrom -MediaLocation $containerUrl -DiskLabel "SQL Server ISO Container" -LUN $lun | Update-AzureVM | Out-AzureOpsLog
    }
}

if (-not $taskList -or $taskList -contains 'ConfigureOctopus') {
    # Requires OctopusDSC module: https://github.com/OctopusDeploy/OctopusDSC
}

#### Special tasks
## These tasks do not run unless explicitly passed to -taskList
if ($taskList -contains 'StopVMs') {
    Get-AzureVM |% { Stop-AzureVM -Name $_.Name -ServiceName $_.ServiceName -Force  | Out-AzureOpsLog }
}
if ($taskList -contains 'StartVMs') {
    Get-AzureVM |% { Start-AzureVM -Name $_.Name -ServiceName $_.ServiceName | Out-AzureOpsLog }
}
if ($taskList -contains 'RestartVMs') {
    Get-AzureVM |% { Restart-AzureVM -Name $_.Name -ServiceName $_.ServiceName | Out-AzureOpsLog }
}
if ($taskList -contains 'GetVMs') {
    # Can be useful for debugging
    Get-AzureVM
}

## Cleanup 
$VerbosePreference = "Continue"
$global:ShowAzureOps = $false