function global:Initialize-LocalTestSettings() {
	Set-DeveloperSetting "AdminServiceBaseAddress" "$(Get-ApplicationWebAddress 'Admin')"
	Set-DeveloperSetting "AdminUiTests.ServerBaseUrl" "$(Get-ApplicationWebAddress 'AdminUiTest')"
	Set-DeveloperSetting "TestServerBase" "$(Get-ApplicationWebAddress 'WebApi-Empty')"
	Set-DeveloperSetting "RepositoryRoot" $nDevConfig.repositoryRoot
}