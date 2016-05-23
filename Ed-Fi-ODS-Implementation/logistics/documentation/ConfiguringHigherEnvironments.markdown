# Configuring higher promotion environments

Instructions for configuring higher promotion environments (such as Staging and Production). 

Assumes a working Development promotion environment & existing orchestrator server.

## Document version: pre-alpha work in progress

This document should *not* (yet) be considered final or authoritative. 

Many aspects may change in the short term. In particular, sections of this document that deal with worker roles (EdFi.Workers.BulkLoad and EdFi.Workers.UploadCommit) or application servers (which will run these workers) are expected to change. 

## Assumptions

**Prerequisites**: This document assumes that TeamCity is automatically building and packaging code, and Octopus is deploying that code, in a Development or Continuous Integration environment. Setting up this software is beyond the scope of this document. See the [TeamCity Documentation](https://www.jetbrains.com/teamcity/documentation/) and [Octopus Documentation](http://docs.octopusdeploy.com/display/OD/Home) for more information. 

**Access**: This document assumes a sysadmin who has access to the orchestrator server in the development environment, all servers in the new promotion environment, and the repositories holding the Ed-Fi source code.

**Resources**: This document assumes a sysadmin who is familiar with git, and has a local clone of all relevant Ed-Fi repositories.

## Pre-steps

Before continuing, you must make some decisions and gather information and credentials for the new environment. 

- Determine name of new environment. 
    - The name should be short and indicative of the purpose of the new environment
    - The name should contain only letters and numbers, with no spaces or other characters.
    - You may wish for the name to reflect the ODS type backing that environment. See the next section for a discussion of ODS types.
    - Example environment names: "Staging", "SanboxProduction", "TexasProd", etc

- Create Windows users

- Create database logins

    The database must have the following logins created: 

    - A user that we will refer to later as `DB Admin`. This user must have sysadmin rights to the database server. 
    - A user referred to as `DB Loader`. This user needs no special permissions - the admin user will grant it the permissions it needs.
    - A user referred to as `EdFi Admin App`. This user needs no special permissions - the admin user will grant it the permissions it needs.

- Determine hostname & version of the SQL server

This document walks you through creating a new environment called `SampleProduction`. 

### The ODS type: Sandbox or SharedInstance?

One of the important considerations for a new environment is the type of ODS that will back it. 

SharedInstance ODSes are intended for normal use. There is a single ("shared") database acting as the ODS, which means that one database is authoritative for the environment. 

Sandbox ODSes are intended for development. When vendors create new API keys, a completely seperate database is created for their key. This can be useful if, for example, a vendor wants to clean out its sandbox in a way that does not affect other users. 

Typically, Sandbox environments will install the SwaggerUI documentation site, while SharedInstance environments may not (although it does not hurt to do so). 

Most organizations have an environment stack that looks like this: 

                 Development
              _______/\_______
             /                \
      SandboxStaging     SharedStaging
            |                  |
    SandboxProduction   SharedProduction

The SandboxProduction environment is for vendors to test their code against, and typically have a best-effort SLA, while the SharedProduction environment is the canonical ODS for student data. 

## Creating a certificate for the new promotion environment

A certificate and private key are used to encrypt and decrypt sensitive data. It is best practice to create a new certificate and private key for each promotion environment. 

This certificate is referred to as the *credentials certificate* for your new environment. It is not the same thing as an SSL certificate for IIS. 

Each certificate is actually used for two different types of credential encryption: configuration transform encryption (such as connection strings), and custom credentials files created and managed by the credential-management module (found in the `Ed-Fi-Common` repository). For the latter, note that it is mandatory that the name of the certificate ends with `_Encryption`, and that the first part of the name is referred to as a "CertPrefix". A CertPrefix can be any text, however, it is good practice to use the name of the promotion environment that it will be used in. 

Create a certificate with [OpenSSL](http://slproweb.com/products/Win32OpenSSL.html) from a Powershell prompt. See this example, where a certificate with a certPrefix of "SampleProduction" is created:

    # First, make sure Powershell can find OpenSSL, and set a required environment variable
    # If the set-alias or OPENSSL_CONF lines fail, find the path to openssl.exe and a valid OpenSSL config file before continuing.
    $OpenSSLdir = resolve-path C:\OpenSSL-Win64\bin 
    set-alias openssl $OpenSSLdir\openssl.exe
    $env:OPENSSL_CONF = resolve-path "$OpenSSLdir\openssl.cfg"

    $certName = "SampleProduction_Encryption"
    $daysValid = 7300
    openssl req -x509 -nodes -days $daysValid -subj "/CN=$certName" -newkey rsa:2048 -keyout "$certName.pem" -out "$certName.pem"

Now convert the certificate to a pfx file for easy import and secure storage. It will prompt for a password, which is used to encrypt the private key of the certificate - choose a strong password when prompted.

    openssl pkcs12 -export -out "$certName.pfx" -in "$certName.pem" -name "$certName"

This certificate must then be installed into the Personal store of your local machine so that you can encrypt credentials with it. The following must be run from an administrator Powershell console: 

    Import-PfxCertificate -filepath "$certName.pfx" -CertStoreLocation Cert:\LocalMachine\My -Exportable -Password (read-host -AsSecureString "PFX password")

Note the thumbprint this command returns. It is the thumbprint of the new certificate, and it is used later, when creating encrypted configuration transforms. 

Make sure to back up the .pfx file. The certificate contained in this file must be installed to every server in the new promotion environment. 

Also, seurely delete the .pem file. This file contains an unencrypted version of the private key, and is no longer necessary because of the backed up, encrypted .pfx version. 

## Writing configuration transforms

Several config transformation files need to be written, have sensitive sections (such as connectionStrings) encrypted, and then included in the project that requires them. 

### Creating new configuration transforms

There are 6 configuation transforms that must be written for each environment. 

Project name                   | Project type  | Transformation filename
-------------------------------|---------------|-------------------------
EdFi.Ods.Admin.Web             | Web           | Web.SampleProduction.config
EdFi.Ods.WebApi                | Web           | Web.SampleProduction.config
EdFi.Ods.SwaggerUI             | Web           | Web.SampleProduction.config
EdFi.Ods.BulkLoad.Console      | Application   | EdFi.Ods.BulkLoad.Console.exe.SampleProduction.config
EdFi.Ods.Workers.BulkLoad      | Application   | EdFi.Ods.Workers.BulkLoad.exe.SampleProduction.config
EdFi.Ods.Workers.UploadCommit  | Application   | EdFi.Ods.Workers.UploadCommit.exe.SampleProduction.config

It's best to copy the `*.Example.config` files in each of these projects into new files, named after your new environment, and then make modifications. 

Important note: there are two naming conventions for the config transformations - one for web projects, and one for applications. Web projects have a `Web.config` file in the project. This file is copied to the output directory as `Web.config`, and sometimes transformed. Transformation may be done by Visual Studio, which will automatically apply a transformation named after the build configuration name; e.g. if the build configuration name is "SampleProduction", it will automatically apply the `Web.SampleProduction.config` transformation file, if it exists. Transformation files named after the deployment environment may be applied by Octopus, at deployment time, as well; e.g. if the deployment environment in Octopus is called "SampleProduction", it will automatically apply a `Web.SampleProduction.config` transformation file, if it exists. 

Applications are similar in some regards, but have important differences. At build time, the `App.config` file is copied to the output directory as `AppName.exe.config`. Visual Studio will not apply transformations to non-web projects; however, the [SlowCheetah](https://visualstudiogallery.msdn.microsoft.com/69023d00-a4f9-4a34-a6cd-7e854ba318b5) extension, available as NuGet package, can enable this behavior, and the NuGet package has been added to the relevant non-web projects in the ODS solution. SlowCheetah will apply an `App.SampleProduction.config` transform file, analogous to the one in the web project above. However, for Octopus to apply the transformation, it must be called `AppName.exe.SampleProduction.config` instead, reflecting the post-compilation name of the app config file. 

When creating the config transform for application projects, make sure to give it the correct name. 

### Sensitive config sections

There are a few different pieces of sensitive information that may go in to config files

- The `<connectionStrings>` element usually contains plaintext passwords. 
    - (Exception: Environments where Windows authentication is used.)
- The `<DlpProtectedSettings>` element, only present in the EdFi.Ods.Admin.Web project, may need to be encrypted. 
    - This section was created because, while you can encrypt the `<mailSettings>` element in a `Web.config` file, you cannot encrypt that element in a config overlay (due to issues with encrypted XML and attributes).
    - Environments which send mail via an authenticated SMTP connection should place the password in this section and encrypt it. 
- Possibly there will be service bus connection strings for the WebApi and both workers that are sensitive 

Some environments, such as development or CI environments, may use Windows authentication (where "Trusted_Connection=True" is set in the connection string)

### Encrypting sensitive config sections

The officially supported way to produce encrypted sections is to use `aspnet_regiis.exe`. The advantages of this approach is that it uses a tool that comes with Windows, and administrators may already be familiar with its use. Its disadvantages are that it is a brittle tool and asks you to do a lot manually. 

**Note:** *You may find it easier to use [Cloud Configuration Cryptography](https://github.com/danludwig/CloudConfigCrypto) to encrypt the sections. The advantage of this approach is that it is more user-friendly after it's been set up properly. The disadvantages are that it takes time to set up initially, and it is easy to set it up insecurely (if you allow remote access). Installation and configuration of this software is outside the scope of this document, however, and the rest of this section assumes you are using `aspnet_regiis`.*

`aspnet_regiis.exe` works only on files named Web.config - does not work on App.config files or any transform file. To use it, you must create a specially crafted temporary config file, call the tool on the temporary file, and copy the newly-encrypted section out to the real config transformation file, and then add some xdt attributes.

Four of the projects require connection strings. If you examine the `*.Example.config` files for all the projects, you'll see that those projects require different sets of connection strings. However, there is no harm in providing extras. Because the process with `aspnet_regiis` is so cumbersome, it is simpler to compile a master list of all the connection strings required, then do the following procedure only once, and copy the encrypted connection string block to all of the transforms that require it. 

1. Get a copy of [Pkcs12ProtectedConfiguationProvider](https://www.nuget.org/packages/Pkcs12ProtectedConfigurationProvider). (This DLL is a NuGet dependency for several projects in the solution, so if you have a local copy of the solution, you should be able to find this file in the NuGet packages folder once you've done a NuGet Package Restore within Visual Studio.) Install the DLL in a place where `aspnet_regiis` can find it on your local workstation.
    - You can install it into the GAC of your local workstation if you like
    - Alternatively, you can copy it to the folder where `aspnet_regiis.exe` lives: `C:\Windows\Microsoft.NET\Framework\v4.0.30319`

2. Create a file named `Web.config` anywhere on your local filesystem. Edit it to contain just the section you wish to encrypt, plus a `<configProtectedData>` section. This is just a temporary file, whose purpose is to be in a format that `aspnet_regiis` can process. Later, the encrypted sections will be copied out into your transformation file(s). Here's how you would create the temporary file for encrypting a `<connectionStrings>` section. 

        <?xml version="1.0" encoding="utf-8"?>
          <connectionStrings>
            <clear />
            <add name="connString1" ... />
            <add name="connString2" ... />
            <add name="connStringN" ... />
          </connectionStrings>
          <configProtectedData>
            <providers>
              <add name="CustomProvider" thumbprint="THUMBPRINT" type="Pkcs12ProtectedConfigurationProvider.Pkcs12ProtectedConfigurationProvider, PKCS12ProtectedConfigurationProvider, Version=1.0.0.0, Culture=neutral, PublicKeyToken=34da007ac91f901d" />
            </providers>
          </configProtectedData>
        </configuration>

    - Note that the `<connectionStrings>` element contains a `<clear/>` *element* - this is part of doing the transformations. 
    - Note that, by contrast, transformation-related *attributes*, such as `xdt:Transform` attributes, are not present. These cannot be added until the encryption process is complete.
    - Make sure to include all the connection strings required, and to replace `THUMBPRINT` with the thumbprint of the credentials certificate for the new environment. (This requires that the credentials certificate has been installed to the Personal store on the LocalMachine, as described previously in this document.)

3. Change directory into the location of your Web.config file and run `aspnet_regiis` to encrypt it. The following is an example, as would be run from within a Powershell prompt:

        set-alias aspnet_regiis C:\Windows\Microsoft.NET\Framework\v4.0.30319\aspnet_regiis.exe
        aspnet_regiis -pef connectionStrings . -prov CustomProvider

    - The connectionStrings section has now been encrypted in place in the Web.config file
    - If you wish to encrypt other sections, you must re-run `aspnet_regiis` for each section name
    - It can be decrypted in place with `aspnet_regiis -pdf connectionStrings .`.
    - The file's `<connectionStrings>` element will now look something like this (note: the text within the `<CipherValue>` elements has been trimmed in this example; it will be much longer): 

            <connectionStrings configProtectionProvider="CustomProvider">
              <EncryptedData Type="http://www.w3.org/2001/04/xmlenc#Element"
                xmlns="http://www.w3.org/2001/04/xmlenc#">
                <EncryptionMethod Algorithm="http://www.w3.org/2001/04/xmlenc#aes192-cbc" />
                <KeyInfo xmlns="http://www.w3.org/2000/09/xmldsig#">
                  <EncryptedKey xmlns="http://www.w3.org/2001/04/xmlenc#">
                    <EncryptionMethod Algorithm="http://www.w3.org/2001/04/xmlenc#rsa-1_5" />
                    <KeyInfo xmlns="http://www.w3.org/2000/09/xmldsig#">
                      <KeyName>rsaKey</KeyName>
                    </KeyInfo>
                    <CipherData>
                      <CipherValue>...</CipherValue>
                    </CipherData>
                  </EncryptedKey>
                </KeyInfo>
                <CipherData>
                  <CipherValue>...</CipherValue>
                </CipherData>
              </EncryptedData>
            </connectionStrings>

4. Now the XDT attributes can be added. Add `xdt:Transform="Replace"` attributes to the XML elements you've been encrypting (e.g. `connectionStrings`) as well as to the `configProtectedData` elements in the transform file. The elements should now look like this:

        ...
        <connectionStrings configProtectionProvider="CustomProvider" xdt:Transform="Replace">
        ...
        <configProtectedData xdt:Transform="Replace">
        ...

5. Copy the connectionStrings and configProtectedData sections into the transform file.

6. Repeat for each configuration section in each transformation file that needs encryption.

The final version of your configuration transforms should look something like this example, which has redacted CipherValue elements: 

    <?xml version="1.0" encoding="utf-8"?>
    <!-- For more information on using web.config transformation visit http://go.microsoft.com/fwlink/?LinkId=125889 -->
    <configuration xmlns:xdt="http://schemas.microsoft.com/XML-Document-Transform">
    <configProtectedData xdt:Transform="Replace">
      <providers>
        <add name="CustomProvider" thumbprint="THUMBPRINT" type="Pkcs12ProtectedConfigurationProvider.Pkcs12ProtectedConfigurationProvider, PKCS12ProtectedConfigurationProvider, Version=1.0.0.0, Culture=neutral, PublicKeyToken=34da007ac91f901d" />
      </providers>
    </configProtectedData>
    <connectionStrings configProtectionProvider="CustomProvider" xdt:Transform="Replace">
      <EncryptedData Type="http://www.w3.org/2001/04/xmlenc#Element" xmlns="http://www.w3.org/2001/04/xmlenc#">
        <EncryptionMethod Algorithm="http://www.w3.org/2001/04/xmlenc#aes192-cbc" />
        <KeyInfo xmlns="http://www.w3.org/2000/09/xmldsig#">
          <EncryptedKey xmlns="http://www.w3.org/2001/04/xmlenc#">
            <EncryptionMethod Algorithm="http://www.w3.org/2001/04/xmlenc#rsa-1_5" />
            <KeyInfo xmlns="http://www.w3.org/2000/09/xmldsig#">
              <KeyName>rsaKey</KeyName>
            </KeyInfo>
            <CipherData>
              <CipherValue>...</CipherValue>
            </CipherData>
          </EncryptedKey>
        </KeyInfo>
        <CipherData>
          <CipherValue>...</CipherValue>
        </CipherData>
      </EncryptedData>
    </connectionStrings>
    </configuration>

### Including the transform in the project

Now the transformation files must be added to each project. 

This step is required for Octopus to run the transforms. It also means that the new transform is visible in Visual Studio as dependent upon the base `.config` file. 

Close the ODS solution in Visual Studio, if it is open. For each project for which you have created a new config transform, open its `.csproj` file in a text editor, such as Notepad. There will be an XML element for an Example config transformation. Find it and copy it, changing the filename to refer to your new transform. Here's an example for a web project - the existing Example transform is first, and then the new copy: 

    <Content Include="Web.Example.config">
      <DependentUpon>Web.config</DependentUpon>
    </Content>
    <Content Include="Web.SampleProduction.config">
      <DependentUpon>Web.config</DependentUpon>
    </Content>

Remember that `App.config` transformation files will be named a bit differently. Here's an example for an application project: 

    <Content Include="EdFi.Ods.BulkLoad.Console.exe.Example.config">
      <DependentUpon>App.config</DependentUpon>
      <CopyToOutputDirectory>Always</CopyToOutputDirectory>
    </Content>
    <Content Include="EdFi.Ods.BulkLoad.Console.exe.SampleProduction.config">
      <DependentUpon>App.config</DependentUpon>
      <CopyToOutputDirectory>Always</CopyToOutputDirectory>
    </Content>

Save the `.csproj` files, and then reopen the ODS solution in Visual Studio. You should be able to click the disclosure triangle next to the `Web.config` or `App.config` files for the projects you modified, and see your newly-created configuration transform underneath it. 

## Writing vars files

Enter the `logistics/scripts/activities/build` in the Ed-Fi-ODS-Implementation repository. Copy the `Example.vars.ps1` to match the new environment, such as `SampleProduction.vars.ps1`.

Make changes to at least the following values (more may be required for other environments): 

- `$odsType`
- `$sqlServerName`
- `$defaultOleDbProvider`

You may also wish to consult the `Example-Complete.vars.ps1` file, which has more comments and some less commonly used variables that you may wish to customize for the new environment. 

## Creating an encrypted credentials file

Encrypted credentials files are used by the Powershell deployment scripts to talk to databases. They are encrypted with the credentials certificate.

To create the encrypted credentials file, run the following from a Powershell prompt, first changing the values on the first three lines to match your new environment.

    $certPrefix = "SampleProduction"
    $environmentName = "SampleProduction"
    $odsImplPath = resolve-path "X:\EdFi\Ed-Fi-ODS-Implementation"

    $errorActionPreference = "stop"
    cd $odsImplPath
    import-module logistics/scripts/modules/path-resolver.psm1
    cd logistics/scripts/activities/build

    # Dot-source the vars file created in the previous step
    # If this gives an error, be sure to figure out why before continuing!
    . $environmentName.vars.ps1

    $env:CertPrefix = "$certPrefix"
    import-module $folders.modules.invoke('utility/credential-management.psm1')
    $credmd = @(
        @{Name="DB Admin($sqlServerName)"}
        @{Name="Ed-Fi Admin App($sqlServerName)"}
        @{Name="DB Loader($sqlServerName)"}
    )
    Add-CredentialMetadata -credentialMetadata $credmd
    Initialize-Credentials (resolve-path .)

It will now prompt you for a few usernames and passwords. Once you have entered them, a new credentials file with a name like `credentials-SampleProduction-MACHINENAME.xml`, where MACHINENAME is the name of your local workstation, will be in your working directory. This must be renamed to `credentials-SampleProduction.xml` (that is, the MACHINENAME part of it must be removed).

## Checking changes into git 

You have now made a number of changes to the Ed-Fi-ODS-Implementation repository that must be checked in to source control. 

- `Application\EdFi.Ods.Admin.Web\EdFi.Ods.Admin.Web.csproj`
- `Application\EdFi.Ods.Admin.Web\Web.SampleProduction.config`
- `Application\EdFi.Ods.WebApi\EdFi.Ods.WebApi.csproj`
- `Application\EdFi.Ods.WebApi\Web.SampleProduction.config`
- `Application\EdFi.Ods.SwaggerUI\EdFi.Ods.SwaggerUI.csproj`
- `Application\EdFi.Ods.SwaggerUI\Web.SampleProduction.config`
- `Application\EdFi.Ods.BulkLoad.Console\EdFi.Ods.BulkLoad.Console.csproj`
- `Application\EdFi.Ods.BulkLoad.Console\EdFi.Ods.BulkLoad.Console.Exe.SampleProduction.config`
- `Application\EdFi.Ods.Workers.BulkLoad\EdFi.Ods.Workers.BulkLoad.csproj`
- `Application\EdFi.Ods.Workers.BulkLoad\EdFi.Ods.Workers.BulkLoad.Exe.SampleProduction.config`
- `Application\EdFi.Ods.Workers.UploadCommit\EdFi.Ods.Workers.UploadCommit.csproj`
- `Application\EdFi.Ods.Workers.UploadCommit\EdFi.Ods.Workers.UploadCommit.Exe.SampleProduction.config`
- `logistics\scripts\activities\build\SampleProduction.vars.ps1`
- `logistics\scripts\activities\build\credentials-SampleProduction.xml`

Check these into Git and push the change. 

Note that the encrypted credentials file is *ignored* by Git, by default. To add it to the repository, you must use the `--force` option, as in `git add --force credentials-SampleProduction.xml; git commit`. 

## Configuring the individual servers

A handful of tasks must be performed on each server directly. 

### Required for all servers

1. Every server in the environment except the orchestrator must have an Octopus tentacle installed so that Octopus can deploy code or database migrations to it. 

    - [Download the tentacle](http://octopusdeploy.com/downloads)
    - Install it on each server in the environment
    - It will ask for the certificate thumbprint of the Octopus server. Find this value under Configuration -> Certificates in the Octopus web administrative UI. 
    - Open the chosen port (10933 by default) on any firewalls between the Orchestrator server and each other server in the environment.

2. Every server must also install the credentials certificate for the new promotion environment. Run the following from an elevated Powershell prompt:

        Import-PfxCertificate -filepath "$certName.pfx" -CertStoreLocation Cert:\LocalMachine\My -Exportable -Password (read-host -AsSecureString "PFX password")

    After the credentials certificate has been imported, you can control access to the private key - which is what allows decryption - in the Certificates MMC.

    - Launch `mmc.exe`
    - File -> Add/Remove Snap-in...
    - Add the "Certificates" snap-in. 
    - It will ask what to manage certificates for; select "Computer account" and then "Local computer"
    - Click OK
    - "Certificates (Local Computer)" -> "Personal" -> "Certificates"
    - Right click on the credential certificate -> All tasks -> Manage private keys
    - Modify the list of users who should have access to the private key and click OK.

### Configuring web servers

1. Add the required Windows features. Open an elevated powershell windows and execute the following:

        Import-Module Servermanager

        Add-WindowsFeature Web-Server,Web-WebServer,Web-Common-Http,Web-Static-Content,Web-Default-Doc,Web-Http-Errors,Web-Http-Redirect,Web-App-Dev,Web-Asp-Net,Web-Net-Ext,Web-Asp-Net45,Web-Net-Ext45,Web-ISAPI-Ext,Web-ISAPI-Filter,Web-Health,Web-Http-Logging,Web-Log-Libraries,Web-Request-Monitor,Web-Custom-Logging,Web-Security,Web-Basic-Auth,Web-Windows-Auth,Web-IP-Security,Web-Url-Auth,Web-Filtering,Web-Performance,Web-Stat-Compression,Web-Dyn-Compression,Web-Performance,Web-Mgmt-Tools,Web-Mgmt-Console,Web-Scripting-Tools

        Add-WindowsFeature File-Services,FS-FileServer

2. Install Chocolatey. Open an elevated command prompt and execute the following:

        @powershell -NoProfile -ExecutionPolicy unrestricted -Command "iex ((new-object net.webclient).DownloadString('https://chocolatey.org/install.ps1'))" && SET PATH=%PATH%;%systemdrive%\chocolatey\bin

3. Install required libraries. Open an elevated Powershell window and execute the following: 

        cinst WIF -source webpi
        cinst MVC3 -source webpi
        cinst UrlRewrite2 -source webpi

4. If necessary, install SSL certificates to the Windows certificate store. 

5. Make sure the IIS user has access to the private key for the credentials cert


### Configuring database servers

1. Make sure the user running Octopus has access to the credential certificate's private key. (By default, Octopus runs as the LOCAL SYSTEM user that will have access.)

### Configuring application servers

1. Make sure the service user has access to the encryption certificate's private key. 

## Creating the promotion environment in Octopus Deploy

Create the new promotion environment: 

- Log on to the Octopus server on the Orchestrator
- Click on the Environments tab at the top
    - Click the "Add environment" button at the top right
    - Make sure to use the exact name used in your configuration transforms and variables file. 
    - It will return you to the page with a list of environments. 
- Add each machine in the new environment by clicking the "Add machine" button next to your new environment. 
    - Enter the hostname and port of the machine
    - Assign it the required role - WebServer, AppServer, or DatabaseServer

Now you must configure the new environment: 

- Click Projects at the top, and select the ODS deployment project from the drop down
- Click the Variables tab on the left
- You will see an array of configuration parameters, their definitions, and the environments they apply to. 
- Create a value for each of these variable names that is valid in the new environment
    - `CertPrefix` is the certPrefix for this environment
    - `InstallType` will be either "Sandbox" or "SharedInstance"
    - `PathResolverRepositoryOverride` is a semicolon-delimited list of all the repositories required for the new environment, from most general to most specific. 
    - `PromotionEnvironment` is the name of the environment itself

## Finish

The promotion environment is now complete. Build code in TeamCity and then deploy to your new environment using Octopus.