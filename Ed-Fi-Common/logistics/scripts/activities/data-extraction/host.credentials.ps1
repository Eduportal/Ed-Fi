#This file is used to specify the credential information about the host (data destination) location.
Add-CredentialMetadata `
    @(
        @{Name = "Destination SCP";  DefaultUsername = $null; Description = "Credentials to be used to connect and securely copy extracted data to destination."}
    )