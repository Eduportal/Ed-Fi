Deploying Admin Database
==

The primary concern standing up the Admin database is getting the initial user credentials setup in the database.  For testing environments, the admin application bootstraps its own users.  Because the application uses the machine key for one-way encryption of passwords, those passwords need to be established from the running web application. The steps provided below detail how to make this happen.

The basic process for establishing the correct database on the deployed environment is:

1. Create the database in the development environment. This is typically done by running **Initialize-DevelopmentEnvironment** on a developer workstation.
1. Backup the database, then restore it into the target environment.
1. Remove any existing user credentials
1. Make a GET request to the deployed Admin application.  This request will cause the test user to be created.
1. Use the newly created test user to send invitations to create other admin accounts.
1. Change the password to the original test user and forget it.


Creating The Empty Database
--
To create the empty database, simply run **Initialize-DevelopmentEnvironment**.  This will create the **EdFi_Admin** database.


Move the Database To The Target Environment
--
Use your SQL management tools of choice to backup the EdFi_Admin database then restore that database into the target environment.  Make sure to reconnect login credentials for the **edfiAdminApp** user.

Remove Existing User Credentials
--
Because the user credentials are encrypted with the machine key, the password set for the test user when the database was created will not work in the new environment.

The following script will remove all user credentials and associated data:

    -- This database isn't migrated with EdFi migrations.  This table is a distraction
    drop table dbo.VersionLevel
    
    delete from dbo.webpages_UsersInRoles;
    delete from dbo.webpages_OAuthMembership;
    delete from dbo.webpages_Membership;
    
    delete from dbo.ClientAccessTokens;
    delete from dbo.ClientAuthorizationCodes;
    delete from dbo.ApiClients;
    delete from dbo.Users;
    delete from dbo.ApplicationLocalEducationAgencies;
    delete from dbo.ApplicationLocalEducationAgencyApiClients;
    delete from dbo.Applications;
    delete from dbo.Vendors;


Re-establish login credentials
--
The application can automatically create the initial admin user.  Here are the required steps:

1. Make sure the empty database is deployed to the target environment and that user credentials have been cleared (as described above)
1. Recycle the application pool for the admin application
1. Execute a GET request to the initialization URL (using a web browser is fine).
 
Here is the initialization URL
    
    http://<Base URL>/TestUsers/Initialize
    
If the request is successful, you will see an empty page.  If the request fails, you should be redirected to an error screen.

After the request is complete, verify that you can log in to the application using the following credentials:

    Username: test@edfi.org
    Password: ***REMOVED***

Create Additional Admin Users
--
Use the built-in functionality of the application to create new users.  Once the user accounts have been created, you will need to execute a database query to make the new accounts adimistrators (where desired).

Use this query, run against the admin database, to add administrator privileges to existing users (Make sure to set the desired value for &lt;Email Address&gt;):
    
    INSERT INTO webpages_UsersInRoles 
    	([UserId]
        ,[RoleId])
    VALUES
    	((SELECT [UserId] FROM Users WHERE email='<Email Address>'),
    	1)
    	

Verify New Admin Users
--
Log into the application using one of the new admin user accounts and verify that the "Manage Accounts" option is displayed in the masthead.

Change Built-In Password
--
Any admin operations can be handled by other admin users now.

Log into the application using the "test@edfi.org" user.  Change the password to a highly secure random password, then forget it.  Something like KeePass is a good password generator to use.

