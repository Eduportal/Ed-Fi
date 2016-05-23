# *************************************************************************
# ©2013 Ed-Fi Alliance, LLC. All Rights Reserved.
# *************************************************************************
$ErrorActionPreference = "Stop"
properties { 
	
    if (-not (get-module |? { $_.Name -eq "path-resolver" })) {
        Import-Module $PSScriptRoot\..\..\..\modules\path-resolver.psm1
    }
    
    . "$($folders.modules.invoke('utility\build-utility.ps1'))"

    Assert-Variables "environment"
	$etlUnitExePath = Resolve-Path "$($folders.tools.Invoke('EtlUnit\EtlUnit.exe'))"
    
    Import-Module "$($folders.activities.invoke('common.psm1'))"
    Import-Module "$($folders.modules.invoke('utility\credential-management.psm1'))"
    Import-Module "$($folders.modules.invoke('database\database-management.psm1'))"
    . "$($folders.activities.invoke(`"build\$environment.vars.ps1`"))"
    . "$($folders.activities.invoke('build\unit-test-etl\unit-test-etl-vars.ps1'))"
    . "$($folders.activities.invoke('build\unit-test-etl\unit-test-etl-credentials.ps1'))"    

   	# Initialize credentials
    Initialize-Credentials $folders.activities.invoke('build\')

   	# Credentials
	$testDbTestRunnerCreds      = Get-NetworkCredential "Test Database Test Runner"
	$executionDbTestRunnerCreds = Get-NetworkCredential "Test Runner"

    #Paramaters for ETLUNIT execution
    $testDatabaseArgs = "-tdsvr=$testDataSqlServerName -tddb=$testDataDatabaseName -tuser=$($testDbTestRunnerCreds.UserName) -tpwd=$($testDbTestRunnerCreds.Password)"
    
    # This build is running on the sql server box we added this check to work arround sudden problems with resolving DOGWOOD in queries when the ETL test run
	if ($env:COMPUTERNAME -eq $sqlServerName) {
		$executionSqlServerName = ""
	} 
    else {
		$executionSqlServerName = $sqlServerName
	}
    $versionArg = if ($edfiStandardVersion) {"-version $edfiStandardVersion"}
    $executionDatabaseArgs = "-xsvr=$executionSqlServerName -xdb=$testExecutionDatabaseName -xuser=$($executionDbTestRunnerCreds.Username) -xpwd=$($executionDbTestRunnerCreds.Password)"
}

task default -depends CleanUp, RunUnitTests, CreateExecutionGraph

task CleanUp {
    #This just makes sure we don't publish an old graph somehow.
    if (Test-Path "$($folders.base.invoke(''))Etl\ExecutionGraph.xml") {
        Remove-Item "$($folders.base.invoke(''))Etl\ExecutionGraph.xml" -force
    }
}

task CreateAcceptanceDatabase {
    $createParms = @{ dbTypeNames            = $dbTypeNames;
                      databaseName           = $testExecutionDatabaseName;
					  edfiStandardVersion	 = $edfiStandardVersion;
                      environment            = $environment
	}
    . "$($folders.activities.Invoke('build\initialize-database\initialize-database.ps1'))" @createParms
    if ($error) { exit 1 }
}

task RunUnitTests -depends CreateAcceptanceDatabase {
    $saveResultOption = $etlUnitSaveResultOption
	$commandLineArgs = "RunTests -pid=$etlUnitProjectId $testDatabaseArgs $executionDatabaseArgs -path=$etlUnitPath $saveResultOption -diagnostics $versionArg" 
	#Write-Host "DEBUG: $commandLine" -Fore Blue
    #It appears the packages here are executed from context, which needs to be the base dir.
    
    $etlProcess = Start-Process "$etlUnitExePath" -NoNewWindow -PassThru -Wait -WorkingDirectory "$(Resolve-Path $folders.base.invoke('..\'))" -ArgumentList "$commandLineArgs" 
    #Set-Location $origLocation

	# Check to see if EtlUnit returned a failure code
	if (($etlProcess.ExitCode) -ne 0) {
		throw "EtlUnit indicated testing failed."
	}
}

task CreateExecutionGraph -depends RunUnitTests {
    # Create an updated execution graph
	$commandLine = "$etlUnitExePath CreateExecutionPlan -pid=$etlUnitProjectId -xplan=$($folders.base.invoke(''))Etl\ExecutionGraph.xml $testDatabaseArgs $executionDatabaseArgs $versionArg" # Execution creds are required for schema queries for primary key fields during dependency graph creation
    
    $origLocation = Get-Location
    Set-Location $folders.base.invoke('..\')
    
    Invoke-Expression $commandLine
	
    Set-Location $origLocation
	# Check to see if EtlUnit returned a failure code
	if ($LASTEXITCODE -ne 0) {
		throw "EtlUnit indicated failure while attempting to build a new execution graph."
	}
}
