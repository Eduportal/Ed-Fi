<# 
.synopsis
Deploy databases from this package. 
.description
Deploy databases from this package. 

This file is intended to be placed in the root of a chocolatey package and called by a chocolateyInstall.ps1 file or an Octopus PostDeploy.ps1 file. 
#>
param(
    [parameter(mandatory=$true)] [string] $PromotionEnvironment,
    [parameter(mandatory=$true)] [string] $PathResolverRepositoryOverride,
    [parameter(mandatory=$true)] [string] $CertPrefix,
	[parameter(mandatory=$true)] [string] $InstallType,
    [switch] $WhatIf
)

$env:CertPrefix = $CertPrefix
$env:PathResolverRepositoryOverride = $PathResolverRepositoryOverride

# Import the path resolver from the most specific repository passed to us
$appsRepo = $PathResolverRepositoryOverride.Split(";")[0]
Import-Module $PSScriptRoot\$appsRepo\logistics\scripts\modules\path-resolver.psm1

$databaseDeploymentTasks = @(
    @{
        Name = "Remove Sandboxen"
        Script = $folders.activities.invoke('build/remove-sandboxen/remove-sandboxen.ps1')
        ScriptParameters = @{
            Environment = $PromotionEnvironment
        }
        OdsTypes = "Sandbox"
    }
    @{
        Name = "Deploy EdFi_Admin"
        Script = $folders.activities.invoke('build/initialize-database/initialize-database.ps1')
        ScriptParameters = @{
            Environment = $PromotionEnvironment
            DbTypeNames = "EdfiAdmin"
            Tasks = @("default","ApplyEntityFrameworkMigrations")
            BuildConfigurationName = "AcquiredFromCSB"
        }
        OdsTypes = 'Sandbox','SharedInstance'
    }
    @{
        Name = "Deploy EduId"
        Script = $folders.activities.invoke('build/initialize-database/initialize-database.ps1')
        ScriptParameters = @{
            Environment = $PromotionEnvironment
            DbTypeNames = "EduId.Database"
            BuildConfigurationName = "AcquiredFromCSB"
        }
        OdsTypes = 'Sandbox','SharedInstance'
    }        
    @{
        Name = "Deploy EdFi_Bulk"
		Script = $folders.activities.invoke('build/initialize-database/initialize-database.ps1')
        ScriptParameters = @{
            Environment = $PromotionEnvironment
            # [ODS-433] Required to be Rest_Api due to client dependencies on folder name, rename later to EdFi_Bulk
            DbTypeNames = "Rest_Api"
            BuildConfigurationName = "AcquiredFromCSB"
            Tasks = @("default","ApplyEntityFrameworkMigrations")
        }
        OdsTypes = 'Sandbox','SharedInstance'
    }
    @{
        Name = "Deploy EdFi_Ods_Populated_Template"
        Script = $folders.activities.invoke('build/initialize-database/initialize-database.ps1')
        ScriptParameters = @{
            Environment = $PromotionEnvironment
            DbTypeNames = "EdFi"
            DatabaseName = "EdFi_Ods_Populated_Template"
            Tasks = @("RestorePopulatedDbTemplate")
        }
        OdsTypes = 'Sandbox'
    }
    @{
        Name = "Deploy EdFi_Ods from populated template"
        Script = $folders.activities.invoke('build/initialize-database/initialize-database.ps1')
        ScriptParameters = @{
            Environment = $PromotionEnvironment
            DbTypeNames = "EdFi"
            BuildConfigurationName = "AcquiredFromCSB"
            Tasks = @("RestorePopulatedDbTemplate")
        }
        OdsTypes = 'Sandbox'
    }
    @{
        Name = "Deploy EdFi_Ods from SQL scripts"
        Script = $folders.activities.invoke('build/initialize-database/initialize-database.ps1')
        ScriptParameters = @{
            Environment = $PromotionEnvironment
            DbTypeNames = "EdFi"
            BuildConfigurationName = "AcquiredFromCSB"
            Tasks = @("default","ApplyDescriptors") 
        }
		OdsTypes = 'SharedInstance','HybridMinimal'
    }
    @{
        Name = "Deploy EdFi_Ods_Empty_Template"
        Script = $folders.activities.invoke('build/initialize-database/initialize-database.ps1')
        ScriptParameters = @{
            Environment = $PromotionEnvironment
            DbTypeNames = "EdFi"
            DatabaseName = "EdFi_Ods_Empty_Template"
        }
        OdsTypes = 'Sandbox'
    }
    @{
        Name = "Deploy EdFi_Ods_Minimal_Template"
        Script = $folders.activities.invoke('build/initialize-database/initialize-database.ps1')
        ScriptParameters = @{
            Environment = $PromotionEnvironment
            DbTypeNames = "EdFi"
            DatabaseName = "EdFi_Ods_Minimal_Template"
            Tasks = @("default","ApplyDescriptors") 
        }
        OdsTypes = 'Sandbox'
    }
    @{
        Name = "Deploy SSO_Integration"
        Script = $folders.activities.invoke('build/initialize-database/initialize-database.ps1')
        ScriptParameters = @{
            Environment = $PromotionEnvironment
            DbTypeNames = "SSO_Integration"
            BuildConfigurationName = "AcquiredFromCSB"
        }
        OdsTypes = @()
    }
)

foreach ($task in ($databaseDeploymentTasks |? { $_.OdsTypes -contains $InstallType })) {
    write-host "Running database deployment task: $($task.Name)"
    write-host "Calling script: $($task.Script)"
    write-host "With parameters: "
    $task.ScriptParameters | out-host

    if (-not $whatif) {
        $pp = $task.ScriptParameters
        . $task.Script @pp
    }

    if ($error) {
        $error | Format-List * -Force  #write out error details
        throw $error
        exit 1 
    }
}

