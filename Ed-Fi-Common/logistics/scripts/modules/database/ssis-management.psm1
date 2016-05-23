if (-not (get-module |? { $_.Name -eq "path-resolver" })) {
    Import-Module $PSScriptRoot\..\path-resolver.psm1
}
Import-Module $folders.modules.Invoke('database\database-management.psm1')
#choco install SqlServerDataTools.2012
[System.Reflection.Assembly]::LoadWithPartialName("Microsoft.SqlServer.Management.IntegrationServices") | Out-Null


# Creates SSIS catalog in SSIS database if doesn't already exist
Function New-SsisCatalog {
    [cmdletbinding()]
    param(
        [parameter(mandatory=$true)] [string] $sqlServer,
        [string] $username,
        [System.Security.SecureString] $password,
		[parameter(mandatory=$true)] [System.Security.SecureString] $ssisCatalogPassword
    )

    trap [Exception] {
        write-error $("ERROR: " + $_.Exception.ToString());
        break;
    }

	$server = Get-Server $sqlServer $username $password
	$ssisServer = New-Object Microsoft.SqlServer.Management.IntegrationServices.IntegrationServices $server
    
	# Check if catalog is already present, if not create one
    if($ssisServer.Catalogs.Count -lt 1) { 
	    (New-Object Microsoft.SqlServer.Management.IntegrationServices.Catalog $ssisServer,"SSISDB",$ssiscatalogpassword).Create()
    }
    return
}


#TODO: Add Help allow parameter set with just server object.
Function Get-SsisCatalog {
    [cmdletbinding()]
    param(
        [parameter(mandatory=$true)] [string] $sqlServer,
        [string] $username,
        [System.Security.SecureString] $password
    )
    $server = Get-Server $sqlServer $username $password
	$ssisServer = New-Object Microsoft.SqlServer.Management.IntegrationServices.IntegrationServices $server
    return $ssisServer.Catalogs["SSISDB"]
}

# Create folder in SSIS Catalog 
Function New-SsisFolder {
    [cmdletbinding()]
    param(
        [parameter(mandatory=$true)] [string] $sqlServer,
        [string] $username,
        [System.Security.SecureString] $password,
		[parameter(mandatory=$true)] [string] $ssisFolderName,
		[string] $ssisFolderComment
    )

    trap [Exception] {
        write-error $("ERROR: " + $_.Exception.ToString());
        break;
    }
	
	$server = Get-Server $sqlServer $username $password
    #Is this really necessary if we check for the catalog right after this?
	if (Test-DatabaseExists $server "SSISDB") {
		$ssisCatalog = Get-SsisCatalog $sqlServer $username $password

        #Verify that catalog is present
		if(!$ssisCatalog) {
			Write-Host "Catalog for SSISDB does not exist. Cannot create Folder."
			return $false
		}
		else {
			#Check if Folder is already present, if not create one
			if(!$ssisCatalog.Folders.Item($ssisFolderName)) {
				(New-Object Microsoft.SqlServer.Management.IntegrationServices.CatalogFolder $ssisCatalog,$ssisFolderName,$ssisFolderComment).Create()
			}
		}
	}
	else {
		Write-Host "SSISDB database does not exist. Cannot create Folder."
		return $false
	}
}

Function Sync-SsisEnvironment {
    [cmdletbinding()]
    param(
        [parameter(mandatory=$true)] [string] $sqlServer,
        [string] $username,
        [System.Security.SecureString] $password,
		[parameter(mandatory=$true)] [string] $ssisFolderName,
        [parameter(mandatory=$true)] [string] $ssisEnvironmentName
    )
    $ssisCatalog = Get-SsisCatalog $sqlServer $username $password
    $ssisFolder = $ssisCatalog.Folders.Item($ssisFolderName)
    if($ssisFolder) {
        if ($ssisFolder.Environments.Contains($ssisEnvironmentName) ) {
            $ssisEnvironment = $ssisFolder.Environments.Item($ssisEnvironmentName)
            $ssisEnvironment.Drop()
            $ssisEnvironment = New-Object "Microsoft.SqlServer.Management.IntegrationServices.EnvironmentInfo"($ssisFolder, $ssisEnvironmentName, "")
            $ssisEnvironment.Create()
        } 
        else {
            $ssisEnvironment = New-Object "Microsoft.SqlServer.Management.IntegrationServices.EnvironmentInfo"($ssisFolder, $ssisEnvironmentName, "")
            $ssisEnvironment.Create()
        }    
    }
    else {
        Write-Warning "SSIS folder does not exist, could not sync SSIS environment."
    }

}

Function Set-SsisEnvironmentVariables {
    [cmdletbinding()]
    param(
        [parameter(mandatory=$true)] [string] $sqlServer,
        [string] $username,
        [System.Security.SecureString] $password,
		[parameter(mandatory=$true)] [string] $ssisFolderName,
        [parameter(mandatory=$true)] [string] $ssisEnvironmentName,
        #TODO: Add validation of hashtable sub structure or convert to a custom object.
        [parameter(mandatory=$true)] [Hashtable] $variables
    )
    <# Example of variables hash structure:
    $variables = @{
        "EdFiDW_ConnectionString" = @{
            Type       = 
            Value      =
            Sensitive  =
            Description =
        }
    
    }
    #>
    
    $ssisCatalog = Get-SsisCatalog $sqlServer $username $password
    $ssisFolder = $ssisCatalog.Folders.Item($ssisFolderName)
    if(!$ssisFolder) {
        throw "SSIS folder: $ssisFolderName does not exist."
    }
    if (!$ssisFolder.Environments.Contains($ssisEnvironmentName)) {
        throw "SSIS environment: $ssisEnvironmentName does not exist."
    }
    $ssisEnvironment = $ssisFolder.Environments.Item($ssisEnvironmentName)
    foreach ($varName in $variables.Keys) {
        if (!$ssisEnvironment.Variables.Contains($varName)) {
            $varProps = $variables[$varName]
            #Set defaults:
            #If type is not specified, derive it from .NET Type.
            if(!$varProps.Type) {
                $varProps.Type = [System.Type]::GetTypeCode($varProps.Value.GetType())
            }
            #Assume not sensitive if it is not provided
            if(!$varProps.Sensitive) {
                $varProps.Sensitive = $false
            }
            $ssisEnvironment.Variables.Add($varName,$varProps.Type,$varProps.Value,$varProps.Sensitive,$varProps.Description)
        }
        else {
            Write-Verbose "For SSIS Environment: $ssisEnvironmentName in SSIS Folder: $ssisFolderName Variable: $varName already exists."
        }
    }
    $ssisEnvironment.Alter()
}

Function Set-SsisProjectEnvironment {
    [cmdletbinding()]
    param(
        [parameter(mandatory=$true)] [string] $sqlServer,
        [string] $username,
        [System.Security.SecureString] $password,
		[parameter(mandatory=$true)] [string] $ssisFolderName,
		[parameter(mandatory=$true)] [string] $ssisProjectName,
        [parameter(mandatory=$true)] [string] $ssisEnvironmentName
    )

    $ssisCatalog = Get-SsisCatalog $sqlServer $username $password
    $ssisFolder = $ssisCatalog.Folders.Item($ssisFolderName)
    if(!$ssisFolder) {
        throw "SSIS folder: $ssisFolderName does not exist."
    }
    $ssisProject = $ssisFolder.Projects.Item($ssisProjectName)
    $ssisProject.References.Add($ssisEnvironmentName, $ssisFolderName)
    $ssisProject.Alter()
}

Function Sync-SsisProjectParmatersToEnvironmentVariables {
    [cmdletbinding()]
    param(
        [parameter(mandatory=$true)] [string] $sqlServer,
        [string] $username,
        [System.Security.SecureString] $password,
		[parameter(mandatory=$true)] [string] $ssisFolderName,
		[parameter(mandatory=$true)] [string] $ssisProjectName,
        [parameter(mandatory=$true)] [string] $ssisEnvironmentName
    )

    $ssisCatalog = Get-SsisCatalog $sqlServer $username $password
    $ssisFolder = $ssisCatalog.Folders.Item($ssisFolderName)
    if(!$ssisFolder) {
        throw "SSIS folder: $ssisFolderName does not exist."
    }
    $ssisEnvironment = $ssisFolder.Environments.Item($ssisEnvironmentName)
    $ssisProject = $ssisFolder.Projects.Item($ssisProjectName)
    foreach ($parameter in $ssisProject.Parameters) {
        #Make sure we have a variable that matches the name reference.
        if ($ssisEnvironment.Variables.Contains($parameter.Name)) {
            $parameter.Set([Microsoft.SqlServer.Management.IntegrationServices.ParameterInfo+ParameterValueType]::Referenced,$parameter.Name)
        }
    }
    $ssisProject.Alter()
}

# If project already exists drop and re-deploy
Function Publish-SsisProject {
    [cmdletbinding()]
    param(
        [parameter(mandatory=$true)] [string] $sqlServer,
        [string] $username,
        [System.Security.SecureString] $password,
		[parameter(mandatory=$true)] [string] $ssisFolderName,
		[Alias("projectFilePath")]
        [parameter(mandatory=$true)] [string] $packageFilePath,  # This is the ispac path
		[parameter(mandatory=$true)] [string] $ssisProjectName
    )

    trap [Exception] {
        write-error $("ERROR: " + $_.Exception.ToString());
        break;
    }
	
	$server = Get-Server $sqlServer $username $password

	if (Test-DatabaseExists $server "SSISDB") {
		$ssisServer = New-Object Microsoft.SqlServer.Management.IntegrationServices.IntegrationServices $server

		#Verify that catalog is present
		if(!$ssisServer.Catalogs["SSISDB"]) {
			Write-Host "Catalog for SSISDB does not exist."
			return $false
		}
		else {
			$ssisCatalog = $ssisServer.Catalogs["SSISDB"]
					
			#Check if project is already deployed or not, if deployed drop it and deploy again

			if(!$ssisCatalog.Folders.Item($ssisFolderName)) {
				Write-Host "$ssisFolderName does not exist in Catalog for SSISDB."
				return $false
			}
			else {
				$ssisFolder = $ssisCatalog.Folders.Item($ssisFolderName)
				if($ssisFolder.Projects.Item($ssisProjectName)) {
					$ssisFolder.Projects.Item($ssisProjectName).Drop()
				}
				if(!$ssisFolder.Projects.Item($ssisProjectName)) {
					$ssisFolder.DeployProject($ssisProjectName,[System.IO.File]::ReadAllBytes($packageFilePath))
				}
			}
		}
	}
	else {
		Write-Host "SSISDB database does not exist."
		return $false
	}
}

Function Get-SsisCompilerPath { 
    [cmdletbinding()]
    param(
        [Parameter(Position=0, Mandatory=$true, ParameterSetName='project')]
        [string]$dtprojPath,
        [Parameter(Position=0, Mandatory=$true, ParameterSetName='version')]
        [int]$maxMajorVersion
    )
    
    if ($PsCmdlet.ParameterSetName -eq "project") {
        #Getting the version from the proj file.
        [xml]$x = Get-Content $dtprojPath
        [int]$maxMajorVersion = $x.Project.ProductVersion.Split('.')[0]
    }
    
    #Going to go see what versions of SSDT are installed that are compatible with this project.
    $results = @()
    $pathBases =  @("\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", "\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall")
    #Check for local machine and user installs. If the path to check needs to change for a user install, that could be added later.
    #Currently I've never seen it installed that way and I'm not sure its possible in current versions.
    $driveBases = @("HKLM","HKCU")
    foreach ($path in $pathBases) {   
        foreach ($drive in $driveBases) {
            $regPath = "$($drive):$path"
            if (Test-Path $regPath) { 
                $results += Get-ChildItem -Path $regPath |
                Get-ItemProperty |
                Where-Object -Property DisplayName -Match "(?i)^Microsoft SQL Server Data Tools [-–] .+$" |
                #This logic is a little weird since you would think the newer version would be MORE compatible
                #However in this case, b/c VS tries to run project upgrades in newer versions it blocks the build.
                Where-Object { $_.DisplayVersion.Split('.')[0] -LE $maxMajorVersion } |
                #We want the highest compatible version first.
                Sort-Object -Property DisplayVersion -Desc |
                Select-Object -Property DisplayName, DisplayVersion
            }
        }
    }
    #check each of the SSDT entries for the presence of devenv.com
    foreach ($ssdt in $results) {
        [int]$ssdtMajorVer = $ssdt.DisplayVersion.Split('.')[0]
        $compilerPath = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio $ssdtMajorVer.0\Common7\IDE\devenv.com"
        #Return if we get a valid match
        If (Test-Path $compilerPath) { return $compilerPath }
    }
    throw "Sql Server Data Tools for is required. devenv.com was not found."
}

Function New-Ispac {
    [cmdletbinding(
    DefaultParameterSetName = 'lookup'
    )]
    param(
        [Parameter(Position=0, Mandatory=$true, ParameterSetName='compiler')]
        [string]$compilerPath,
        [Parameter(Position=0, Mandatory=$true, ParameterSetName='lookup')]
        [Parameter(Position=1, Mandatory=$true, ParameterSetName='compiler')]
        [string]$solutionPath,
        [Parameter(Position=1, Mandatory=$false, ParameterSetName='lookup')]
        [Parameter(Position=2, Mandatory=$false, ParameterSetName='compiler')]
        [string]$solutionConfigurationName
    )
    #Set default here if "" "  " or $null is passed.
    if ([string]::IsNullOrWhitespace($solutionConfigurationName)) {
        $solutionConfigurationName = 'Development|Default'
    }
    
    if ($PsCmdlet.ParameterSetName -eq "lookup") {
        $solutionName = dir $solutionPath | select -ExpandProperty basename
        #For this parameter set the assumption is that there will be a project in the solution folder with the same name as the solution.
        $dtprojPath = Join-Path (Split-Path -Parent $solutionPath) "$solutionName.dtproj" 
        $compilerPath = Get-SsisCompilerPath $dtprojPath
    }
    Write-Host "Starting SSIS ispac compilation on $env:ComputerName"
    & $compilerPath "$solutionPath" /ReBuild "$solutionConfigurationName"
    Write-Host "Exit Code: $LASTEXITCODE"
    if ($LASTEXITCODE -ne 0) {throw "SSIS ispac compilation failed."}
}

<# This needs to finish being parameterized. Commenting out until that is done.
# If Create SQL Server jobstep for running of packages on DW server
Function New-SSISJobStep {
    [cmdletbinding()]
    param(
        [parameter(mandatory=$true)] [string] $sqlServer,
		[parameter(mandatory=$true)] [string] $jobName,
		[parameter(mandatory=$true)] [string] $jobStepName,
		[parameter(mandatory=$true)] [string] $ssisProjectName,
		[parameter(mandatory=$true)] [string] $ssisFolderName,
		[parameter(mandatory=$true)] [string] $environmentName,
		[parameter(mandatory=$true)] [string] $ssisPackageName
    )

    trap [Exception] {
        write-error $("ERROR: " + $_.Exception.ToString());
        break;
    }

	[string] $username = "";
	[System.Security.SecureString] $password = ConvertTo-SecureString 'password' -AsPlainText -Force;
	[string] $ssisUsername = "";
	[System.Security.SecureString] $ssisPassword = ConvertTo-SecureString 'password' -AsPlainText -Force;
	
	$server = Get-Server $sqlServer $username $password
	
	$job = $server.JobServer.Jobs[$jobName]
	if (-not ($job.JobSteps.Contains($jobStepName) ) ) {
		$jobStep=New-Object Microsoft.SqlServer.Management.Smo.Agent.JobStep($job, $jobStepName)

		$jobStep.OnSuccessAction = [Microsoft.SqlServer.Management.Smo.Agent.StepCompletionAction]::QuitWithSuccess
		$jobStep.OnFailAction = [Microsoft.SqlServer.Management.Smo.Agent.StepCompletionAction]::QuitWithFailure
		$jobStep.Subsystem=[Microsoft.SqlServer.Management.Smo.Agent.AgentSubSystem]::SSIS
		$jobStep.Server=$sqlServer
		
		# Connect to SQL server which has SSIS Package

		$server = Get-Server $sqlServer $ssisUsername $ssisPassword 
		
		if (Test-DatabaseExists $server "SSISDB") {
			if ($ssisPassword -eq $null) {
				$netCreds = new-object Net.NetworkCredential($ssisUsername, $null)
			}
			else {
				$netCreds = new-object Net.NetworkCredential($ssisUsername, ([System.Runtime.InteropServices.marshal]::PtrToStringAuto([System.Runtime.InteropServices.marshal]::SecureStringToBSTR($ssisPassword))))
			}
			$connectionString = Get-SqlConnectionString $sqlServer "SSISDB" $netCreds
			
			#Connect to Integration Service Catalog and load project
			$sqlCn = New-Object System.Data.SqlClient.SqlConnection $connectionString;
			$ssisServer = New-Object Microsoft.SqlServer.Management.IntegrationServices.IntegrationServices $sqlCn;

			$ssisCatalog = $ssisServer.Catalogs["SSISDB"]
			$ssisFolder = $ssisCatalog.Folders.Item($ssisFolderName)
			$ssisProject = $ssisFolder.Projects.Item($ssisProjectName)

			 
			#Provide SSIS command which need to be executed, Environment reference will decide which environment we are using

			$environmentReference = $ssisProject.References.Item($environmentName, $ssisFolder.Name)
			$environmentReference.Refresh()
			$environmentID = $environmentReference.ReferenceId

			$jobStepCmd=@"
/ISSERVER "\"\SSISDB\$ssisFolderName\$ssisProjectName\$ssisPackageName\"" /SERVER "$sqlServer" /ENVREFERENCE $environmentId /Par 
"@
			$jobStepCmd = $jobStepCmd + '"\"\$ServerOption::LOGGING_LEVEL(Int16)\"";1 /Par "\"\$ServerOption::SYNCHRONIZED(Boolean)\"";True /CALLERINFO SQLAGENT /REPORTING E'
			# DEBUG Write-Host $jobStepCmd
			$jobStep.Command=$jobStepCmd
			$jobStep.Create()

			$jobScheduleName = "$jobName EOD Weeknights"
			$jobSchedule=New-Object Microsoft.SqlServer.Management.Smo.Agent.JobSchedule($job, $jobScheduleName)
			$jobSchedule.IsEnabled = 0;
			$jobSchedule.FrequencyTypes = 8 #[Microsoft.SqlServer.Management.Smo.Agent.FrequencyType]::Weekly
			$jobSchedule.FrequencyInterval = 124 #TuWThFSa
			$jobSchedule.FrequencyRecurrenceFactor = 1
			$jobSchedule.FrequencySubDayTypes = 1
			$jobSchedule.FrequencySubDayInterval = 0
			$jobSchedule.FrequencyRelativeIntervals = 0
			
			$today = Get-Date
			$start = $today.AddDays(1)
			[System.TimeSpan] $startTS = new-object System.TimeSpan(0,1,0);
			[System.TimeSpan] $endTS = new-object System.TimeSpan(23,59,59);
			
			$jobSchedule.ActiveStartDate = $start
			$jobSchedule.ActiveStartTimeOfDay = $startTS #one minute after midnight
			$jobSchedule.ActiveEndTimeOfDay = $endTS #just before midnight
			# DEBUG write-host $jobSchedule.Script()
			$jobSchedule.Create()

		}
		else {
			Write-Host "SSISDB database does not exist."
			return $false
		}
	} else {
		Write-Host "Job Step $jobStepName Already Exists."
		return $false;
	}
}
#>

Export-ModuleMember New-SsisCatalog, Get-SsisCatalog, New-SsisFolder, Sync-SsisEnvironment, Publish-SsisProject, Set-SsisProjectEnvironment, Set-SsisEnvironmentVariables, Sync-SsisProjectParmatersToEnvironmentVariables, Get-SSISCompilerPath, New-Ispac