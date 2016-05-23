# *************************************************************************
# ©2013 Ed-Fi Alliance, LLC. All Rights Reserved.
# *************************************************************************
Add-CredentialMetadata `
	@(
        @{Name = "DB Admin($($initializeCSB["Server"]))";  DefaultUsername = $null;    Description = "Credentials for full control of the SQL Server instance."; IntegratedSecuritySupported = $true}
     )
