Add-CredentialMetadata `
	@(
        @{Name = "DB Admin";  DefaultUsername = "edfiAdmin";    Description = "Credentials for full control of the SQL Server instance."; IntegratedSecuritySupported = $true},
		@{Name = "DB Loader";  DefaultUsername = "edfiLoader";  Description = "Credentials for loading EdFi and Dashboard databases."}
    )
# TODO: Need to handle build-environment-specific creds?
if ($environment.ToLower() -eq "cloud-integration") {
    Add-CredentialMetadata `
        @(
            # Credentials used by load-database when running at Amazon EC2
            @{Name = "TeamCity Deployer";  DefaultUsername = "BuildAgent"; Description = "Credentials used for accessing anonymized data sets over HTTPS."},
            @{Name = "Amazon S3 Key/Secret Key";  DefaultUsername = $null; Description = "Amazon AWS user key and secret key for accessing S3 buckets."}
    )
}