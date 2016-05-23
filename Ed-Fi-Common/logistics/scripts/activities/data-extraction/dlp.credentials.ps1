#Variables for the DLP location hosting and publishing the most recent versions...

Add-CredentialMetadata `
    @(
        @{Name = "Remote Deployer";  DefaultUsername = $null; Description = "Credentials to be used to connect to get latest scripts and deployment artifacts."}
    )