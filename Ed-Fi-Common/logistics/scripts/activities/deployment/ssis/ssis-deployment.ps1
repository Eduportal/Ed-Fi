<#
    .Synopsis
    This script deploys SSIS ispac packages to a target server utilizing
    the specified properties.
    .Description
    This script deploys SSIS ispac packages to a target server utilizing
    the specified properties. These properties are either directly passed to
    this script or are specified in an environment variable file that matches
    the environment specified. There parameters are then used to orchestrate
    the deployment of the ispac utilizing the ssis-management module.
    
    The default task execution performs the following tasks:
    PrepareServer- Establishes the SSIS package database and enables CLI
    Deploy - Deploys the ispac
    SyncronizeSSISEnvironmentVariables - Syncronizes the SSIS environment variables to the
        SSIS project.
    
    If utilized, the environment variable file must specify 
    the variable [hashtable]$ssisDeploymentVarsByDbType it should contain
    the database type seed name as the key and the value should be a hashtable
    containing keys that match the parameters of this script and the values
    desired.
    Additionally the $environmentCSBsByDbType must contain a CSB for the specified
    databaseType.
    
    .Parameter Environment
    The environment to resolve the desired parameters from to execute this
    process.
    
    .Parameter DatabaseType
    When specifying an environment the databaseType that lives on the target 
    server must be specified. It is utilized to determine the 
    
    .Parameter SsisProjectName
    The name of the SSIS project being deployed.
    
    .Parameter SsisEnvironment
    The name of the SSIS environment to be associated with the project
    being deployed.
    
    .Parameter SsisCatalogFolderName
    The SSIS catalog folder in the SSIS database to  
    
    .Parameter SsisEnvironmentVariables
    This is hashtable of all the environment variables. It utilizes
    the format required by Set-SsisEnvironmentVariables.
        Please see: Get-Help Set-SsisEnvironmentVariables
        
    .Parameter SsisPackageFilePath
    The path to the compiled ispac to be deployed.
#>

#Define ALL parameters required for execution.
[cmdletbinding(DefaultParameterSetName = 'environment')]
param(
    [Parameter(Position=0, Mandatory=$true, ParameterSetName='environment')]
    [string]$environment = "ConfirmedPostDeploymentLock",
    [Parameter(Position=1, Mandatory=$true, ParameterSetName='environment')]
    [string]$databaseType,
    [Parameter(Position=0, Mandatory=$true, ParameterSetName='complete')]
    [string]$ssisProjectName,
    [Parameter(Position=1, Mandatory=$true, ParameterSetName='complete')]
    [string]$ssisEnvironment,
    [Parameter(Position=2, Mandatory=$true, ParameterSetName='complete')]
    [string]$ssisCatalogFolderName,
    [Parameter(Position=3, Mandatory=$true, ParameterSetName='complete')]
    [hashtable]$ssisEnvironmentVariables,
    [Parameter(Position=4, Mandatory=$true, ParameterSetName='complete')]
    [string]$ssisPackageFilePath,
    [Parameter(Position=5, Mandatory=$true, ParameterSetName='complete')]
    [ValidateScript({
        #Must have server
        (![string]::IsNullOrWhitespace($_.DataSource)) -and
        #Must have creds
        ($_.IntegratedSecurity -or (![string]::IsNullOrEmpty($_.UserID)))
    })]
    [System.Data.SqlClient.SqlConnectionStringBuilder]$ssisDeploymentCSB,
    [Parameter(Position=2, Mandatory=$false, ParameterSetName='environment')]
    [Parameter(Position=6, Mandatory=$false, ParameterSetName='complete')]
    [string[]]$tasks = @()
)

#If an environment was specified, load the variables then call this script recursively.
if ($PsCmdlet.ParameterSetName -eq "environment") {
    Import-Module $folders.modules.invoke("database\database-management.psm1")
    . $folders.activities.invoke("deployment\$environment.vars.ps1") 
    $parms = @{
        ssisProjectName          = $ssisDeploymentVarsByDbType.$databaseType.ssisProjectName 
        ssisEnvironment          = $ssisDeploymentVarsByDbType.$databaseType.ssisEnvironment
        ssisCatalogFolderName    = $ssisDeploymentVarsByDbType.$databaseType.ssisCatalogFolderName
        ssisEnvironmentVariables = $ssisDeploymentVarsByDbType.$databaseType.ssisEnvironmentVariables
        ssisPackageFilePath      = $ssisDeploymentVarsByDbType.$databaseType.ssisPackageFilePath
        ssisDeploymentCSB        = (Convert-CommonDbCSBtoSqlCSB $environmentCSBsByDbType.$databaseType)
        tasks                    = $tasks
    }
    & $PSCommandPath @parms
}
else {
    Import-Module "$($folders.modules.invoke('psake\psake.psm1'))"
    Invoke-Psake "$($folders.activities.invoke('deployment\ssis\ssis-deployment-psake.ps1'))" $tasks -parameters $PSBoundParameters
}

if ($error.Count -gt 0) {
	write-host "ERROR: $error" -fore RED
	exit $error.Count
}




