#requires -version 3

function global:Initialize-DevelopmentEnvironment() {	
	param(
		[switch] $NoCompile,
		[switch] $NoResetDatabase,
		[switch] $NoDeploy
	)
	# Our psake scripts work throw if $error is not empty 
	# (this is a workaround for some TeamCity behavior). 
	Write-RunMessage "Clearing `$error..."
	$error.clear()

	Write-RunMessage "Updating solution scripts..."
	Update-SolutionScripts
	
	if (-not $NoResetDatabase) {
	
		Write-RunMessage "Resetting the empty database..."
		Reset-EmptyDatabase
		
	}
	
	if (-not $NoCompile) {
		Write-RunMessage "Rebuilding the solution..."
		Rebuild-Solution
	}

	Write-RunMessage "Initializing credentials..."
	Initialize-ProjectCredentials

	if (-not $NoResetDatabase) {
		Write-RunMessage "Removing all existings sandbox databases . . ."
		Remove-Sandboxes

		Write-RunMessage "Resetting the security metadata database..."
		Reset-SecurityDatabase -NoCompile
		
		Write-RunMessage "Resetting the admin database..."
		Reset-AdminDatabase -NoCompile

		#Write-RunMessage "Resetting the mapping test admin database..."
		#Reset-AdminDatabase -NoCompile -DatabaseName EdFi_AdminMappingTest

		Write-RunMessage "Resetting the empty rest api database..."
		Reset-RestApiWorkingDatabase -NoCompile

		Write-RunMessage "Resetting the populated databases..."
		Reset-PopulatedDatabases

		#Write-RunMessage "Resetting the empty template database..."
		#Reset-EmptyTemplateDatabase

		Write-RunMessage "Resetting the minimal template database..."
		Reset-MinimalTemplateDatabase -NoCompile

		#Write-RunMessage "Resetting the SSO integration database..."
		#Reset-SsoIntegrationDatabase

		#Write-RunMessage "Resetting the shared instance 2014 database..."
		#Reset-SharedInstanceDatabase -NoCompile -SchoolYear 2014

		#Write-RunMessage "Resetting the shared instance 2015 database..."
		#Reset-SharedInstanceDatabase -NoCompile -SchoolYear 2015
		Write-RunMessage "Resetting EduId Database project..." # Uses Populated Template to pre-populate mapping table
		Reset-EduIdDatabase
	}

	
	if (-not $NoDeploy) {
		#Write-RunMessage "Publishing all websites locally..."
		#Publish-Local

		#Write-RunMessage "Initializing local development configuration settings..."
		#Initialize-LocalTestSettings

		#Write-RunMessage "Initializing base queue directory..."
		#Initialize-BaseQueueDirectoryWithinRepository

		#Write-RunMessage "Deployment Locations For All Deployed Applications:"
		#Write-DeploymentInfo
	}
}
set-alias -scope Global initdev Initialize-DevelopmentEnvironment
