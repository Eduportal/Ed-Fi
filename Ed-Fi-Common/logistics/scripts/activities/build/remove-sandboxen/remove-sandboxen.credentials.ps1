Add-CredentialMetadata @(
    @{
        Name = "DB Admin($sqlServerName)"; 
        DefaultUsername = $null;    
        Description = "Credentials for full control of the SQL Server."; 
        IntegratedSecuritySupported = $true;
    },
    
    @{
        Name = "DB Loader($sqlServerName)"; 
        DefaultUsername = $null;    
        Description = "Credentials for the db loader."; 
        IntegratedSecuritySupported = $true;
    }
)
