Add-CredentialMetadata `
	@(
        @{Name = "DB Admin ($sqlDataSourceName)";  DefaultUsername = "edfiAdmin";    Description = "Credentials for full control of the SQL Server instance."; IntegratedSecuritySupported = $true}
        @{Name = "Application ($deploymentConfiguration)"; DefaultUsername = "edfiApplication"; Description = "Credentials used to access the $deploymentConfiguration database in support of the $deploymentConfiguration web application."; IntegratedSecuritySupported = $true}
      )
