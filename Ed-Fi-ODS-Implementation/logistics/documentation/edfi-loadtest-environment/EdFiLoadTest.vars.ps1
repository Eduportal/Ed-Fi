#### New environment configuration file

# Must be a valid datacenter location
# You can find these by poking around in the Azure admin UI, or making a List Locations call: 
# http://msdn.microsoft.com/en-us/library/azure/gg441293.aspx
$DatacenterLocation = "South Central US"

# The name of your subscription. (The default subscription name is "Pay-As-You-Go".)
$SubscriptionName = "Ed-Fi Alliance"

# The name of your storage account. If this does not exist, it will be created.
$StorageAccountName = "edfiloadteststore"

# The base image family name for all VMs. Be careful what you put here. 
# If you change this value, consider running
#     Get-AzureVMImage |? { $_.ImageFamily -like $BaseImageFamilyName } | sort-object PublishedDate
# ... and make sure that it returns a list of updates to your image, with the most recent update last.
$BaseImageFamilyName = "Windows Server 2012 R2 Datacenter"

# The name and description of your affinity group. If this does not exist, it will be created.
$AffinityGroupName = "edfiloadtest"
$AffinityGroupDescription = "Ed-Fi Load Test AG"

# Set this to true once the manual network configuration has been done 
# This is a manual step that cannot be done automatically because of a bug in the Powershell cmdlets
# (See the documentation for more information)
$ManualNetworkCreationComplete = $true

# The names of your VNet and the subnet it contains.
# These must already be created.
$VNetName = 'edfiloadtest'
$SubnetName = 'edfiloadtest-sub1'

# The first *usable* address on your subnet.
# The Azure web UI tells you the usable address range when you create the subnet
# Your first DC will be assigned this address. (Subsequent DCs will be given subsequent addresses.)
$SubnetFirstUsableAddress = [System.Net.IPAddress]"10.0.0.10" 

# The name for your new Active Directory domain
# This must be the DNS name of the domain ("contoso.com"), not the short name ("CONTOSO")
$ADDomainName = "edfiloadtest.internal"

# Server configuration settings
#
# The *BaseName variables will be used as the name for the Azure Cloud Services that the VMs will run in.
# Individual VMs will be named $baseName-$count. 
# If you have a WebServerBaseName of 'test-web', you'll end up with one Cloud Service with that name, 
# an then test-web-1, test-web-2, ... test-web-N up to the number specified in WebServerCount. 
#
# As for the Size settings, see the help page for the New-AzureVMConfig cmdlet's -InstanceSize parameter to see a list of valid options
# Information about each size is available here: http://azure.microsoft.com/en-us/pricing/details/virtual-machines/

# Domain controller information
$DomainControllerBaseName = 'edfi-lt-dc'
$DomainControllerCount = 1
$DomainControllerSize = "Small"

# Web server information
$WebServerBaseName = 'edfi-lt-web'
$WebServerCount = 2
$WebServerSize = "Medium"

# Application server information
$AppServerBaseName = 'edfi-lt-app'
$AppServerCount = 1
$AppServerSize = "Standard_D12"

# Database server information
$DbServerBaseName = 'edfi-lt-ods'
$DbServerCount = 1
$DbServerSize = "Standard_D13"
$SqlServerIsoPath = "\\checkmate.doubleline.us\Public\Dev ISOs\Microsoft Installers\SQL Server Developer 2012\SQL_Svr_Developer_Edition_2012_w_SP1_English_Download_Only_64bit_-2_X18-78070.ISO"

# Usernames
# Note that you will be prompted for passwords when the script is run
# None of these usernames can be "Administrator"!
$DomainAdminUsername = "EdFiAdministrator"
$DomainMemberLocalAdminUsername = "EdFiAdministrator"

# Octopus stuff
$OctopusServerUrl = "http://tabiya.doubleline.us:88"
$OctopusEnvironmentName = "EFALoadTest"

# Set to $true to force verbose mode, even when -Verbose is not passed on the command line
$ForceVerbose = $true
