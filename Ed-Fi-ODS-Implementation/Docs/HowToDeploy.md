Production Deployment
==
Follow these steps to push the latest version of the software to production.  Right now, this requires a developer workstation along with developer access to the deployment environment.

If you need access to Azure, [follow the directions here] (https://github.com/TennesseeDOE/TDOE-RestApiSpike/wiki/Get-Admin-Access-to-the-Azure-Environment)

Getting Everything Ready
==
1. Make sure the build is green for the branch you wish to deploy -- this should be development
1. Ask the rest of the team to hold pushes to the development branch.
1. Pull the latest changes into your local development and production branches.
1. Merge the release notes from the Pending folder into the ReleaseNotes.md file.
  * Running the script found at ReleaseNotes/Select-PendingMigrations.ps1 will gather all the pending release notes into a single pending.md file.
  * Edit the ReleaseNotes.md with the merged pending notes.  You may need to condense or alter the notes for clarity
  * Delete all notes in the pending folder
  * Make sure these changes are made on the *development* branch.
  * Commit your changes to the development branch and push to the main repository.
1. Tag the current revision of development and push the tag to the main repository. (`git tag YYYY_MM_DD` followed by `git push --tags`)
1. Create a release for the tag in github.  Include the release notes.
1. Open the current development branch in Visual Studio
1. Rebuild the solution.
1. Initialize-DevelopmentEnvironment
1. **If your deployment will change the ODS schema**
  1. Make sure you have administrator access to the the Admin console

Deploying
==
1. Deploy the **Admin Console** using the EdFi.Ods.Admin.Azure project
  1. Right-click the EdFi.Ods.Admin.Azure project and select "Publish..."
  1. Select the "tn-rest-admin-production"
  1. Make certain there are no valdation failures for the target profile (The would show up as red x's)
  1. Push the "Publish" button
  1. ... wait a few seconds ...
  1. Push the "Replace" button to replace the current deployment
  1. Monitor the output in the Windows Azure Activity Log window in VS
  1. Verify the deployment by hitting the landing page (URL shown in output).
    * Note that as of now, the HTTPS certificates are still bad, so you will have to accept the bad certificate in your browser to view the site.
1. Deploy the **REST API** project using the same steps as the Admin project
  * NOTE: Use the *EdFi.Ods.WebApi.Azure* project to deploy the *AzureProd* target profile.
1. Deploy the **Swagger** project using the same steps as the Admin project
  * NOTE: Use the *EdFi.Ods.SwaggerUI.Azure* project to deploy the *tn-rest-apidocs-production* target profile.
1. Deploy the **UploadCommit Worker** project
  1. Right-click the EdFi.Workers.UploadCommit.Azure project and select "Publish..."
  1. Select the "AzureProd"
  1. Make certain there are no valdation failures for the target profile (The would show up as red x's)
  1. Push the "Publish" button
  1. ... wait a few seconds ...
  1. Push the "Replace" button to replace the current deployment
  1. Monitor the output in the Windows Azure Activity Log window in VS
  1. Verify the deployment
    1. Log into manage.windowsazure.com
	1. Click on "Cloud Services" in the left panel
	1. Select the cloud service you are deploying to (in this case, tn-services-prod-uploadcommit)
	1. Click "Instances"
	1. Verify that the deployed instance status is "Running."
1. Deploy the **BulkLoad Worker** project following the same steps as the **UploadCommit Worker** project
  * NOTE: Use the *EdFi.Workers.BulkLoad.Azure* project to deploy to the *tn-services-prod-bulkupload* cloud service.
1. If the ODS Schema has changed or the sandbox templates need to be updated, be sure to follow the process for updating the sandbox templates detailed below in, "Dealing With Changed ODS Schema"
1. Verify the deployment by logging into the Admin site, creating a sandbox, then executing a query against that sandbox through Swagger.
1. Move the production branch to the same commit as the release tag.

Dealing With Changed ODS Schema
==
If the ODS schema has changed, then all existing sandboxes will now have an invalid schema.  Existing sandboxes should be removed.  Additionally, the new sandbox templates should be pushed to the production database server.

Removing Existing Sandboxes
--
1. Log into the production database server using SQL Management Studio
1. Execute the following queries to remove all current access tokens and sandbox records in the database:
        
        DELETE from [EdFi_Admin].[dbo].[ClientAccessTokens]
	
	DELETE from [EdFi_Admin].[dbo].[ClientAuthorizationCodes]
	
	DELETE from [EdFi_Admin].[dbo].[ApiClients]

1. Log into the admin site with an administrator account
1. Remove all orphan sandboxes
  * NOTE: Deleting the sandboxes can take up to 5 seconds per sandbox to remove.  So, deleting 12 sandboxes could take a minute if SQL Azure is being slow that day.

Publishing Database Templates
--
Database templates are published by first creating the template locally, then using an Azure utility to publish the local database up to the Azure SQL server.

**NOTE:** This process produces a TON of console output when running.  Also, it takes a few minutes since the process is dumping all schema to SQL scripts along with exporting all the data, then executing the SQL scripts and pushing the data over the network.

Using the *Package Manager Console*, execute the following commands to publish the database templates to production:

        Publish-DatabaseToAzure EdFi_Ods_Populated_Template production
        
        Publish-DatabaseToAzure EdFi_Ods_Minimal_Template production

Publishing EdFi_Bulk Database
==
Using the *Package Manager Console*, execute the following commands to publish the database to production:
        Publish-DatabaseToAzure EdFi_Bulk production


Migrating Admin Database
==
If the admin schema has changed, you will need to migrate the database.  Otherwise you will see EntityFramework complaining when trying to request
pages from the deployed admin site.

1. *Make sure to backup the admin database first*
1. Migrate the admin database with the following Solution Script

		Migrate-EntityFrameworkDatabase DatabaseName

1. Valid values for DatabaseName include: DevAdmin, ProductionAdmin.  If you want to migrate any database with "production" in the name, you will have to use the following switch:

		-IPromiseIHaveAlreadyBackedUpTheDatabase
		
