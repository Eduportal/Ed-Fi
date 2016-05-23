

- New on-prem environment in Azure VMs 
- Want Azure so they can use "Azure Insight" for monitoring or something
- env for load testing
- Server & network specs
    - Based on the way SharedStaging in TN is set up
    - Probably w/ AlwaysOn but we have to ask EdFi
    - Can we put the workers on a separate server? Need to resolve this question.
    - Get a monthly cost for them if possible

# Load testing environment

See the [Ed-Fi Solution Integration](https://docs.google.com/a/doublelinepartners.com/document/d/1N4ABmZwOyYUFEHn7vMPgjqfpZOwf0YRNBx1VCIe8LvI) document for recommended server specs. 

## Network and server layout

Application servers: 

- REST API & REST Admin web servers: 2x Azure A2 Standard @~$135/mo/ea
- Ed-Fi ODS database server: 2x Azure D13 Standard @~$1080/mo/ea
- Application server for bulk load: 1x (non-redundant) Azure D12 Standard @~$600/mo/ea

Other infrastructure

- AD server: 1x Azure A2 Standard @~$135/mo
- Cloud service for web applications & round-robin load balancer
- Internal Load Balancer for SQL Server
- Single storage account
- Single affinity group
- Single virtual network
- Single subnet (b/c I'm lazy)

## Creating the new environment on Azure

The creation of the load test environment was scripted as much as possible. 

Pre-steps: 

- Install the Azure cmdlets from WebPI
- Add your Azure account to your local machine by typing `Add-AzureAccount` and logging in with your Microsoft account.

### Workarounds: 

You'd want the script to call the `New-WAPackVNet` cmdlet to create a new vnet for the test environment. However, it looks like the WAPack cmdlets have [a bug](https://github.com/Azure/azure-sdk-tools/issues/2935) which prevents us from using them. So, you'll have to go create a vnet from the web UI. You can do this first - it doesn't affect anything else. Just make sure that the location is the same as what is set in your new environment config file. 

- Go to the management portal -> Networks -> New. 
- Set the name to be the value of the `$VNetName` variable in the config file
- Set the location to be the value of the `$DatacenterLocation` variable in the config file
- Do not enter DNS servers at this time - we will set the DNS server to the AD server later on. 
- Use an address space of 10.0.0.0/8, and add a single subnet for 10.0.0.0/24, with a name that matches the value of `$SubnetName` in the config file

### Running the script

First, modify the create an `ENVIRONMENT.vars.ps1` file, (named after your environment). 

Now run the `New-Environment.ps1` file from an elevated prompt. 

When it completes, you'll have a private VNet in the Azure cloud, an Active Directory domain, and the web, app, and database servers you requested

### Further admin tasks

- For the time being, SQL Server install cannot be scripted. Connect to the database server(s) and install it manually.
- Open the ports required for MSMQ on the application server(s): http://support.microsoft.com/kb/178517
- Depending on your configuration, it may be necessary to create the sites and application pools in IIS Manager on all your web servers before Octopus will deploy to them.

### Configuring Octopus

- Install the Octopus tentacle on the database, application, and web servers in the new environment
- Create an endpoint in Azure for the listening port on the Octopus tentacles. (Note that an Internet-wide DNS name is assigned to the cloud service, not the VM; if you have `example-web-1` and `example-web-2` VMs in the `example-web` cloud service, you'll only get one DNS name, `example-web.cloudapp.net`; you'll therefore need to use different ports for the Octopus tentacles on the two web servers.)
- Create a promotion environment on the Octopus server
- Add the tentacles to the promotion environment

### Writing config files and transforms

- Create configuration transforms, vars files, and credentials files, as required.

