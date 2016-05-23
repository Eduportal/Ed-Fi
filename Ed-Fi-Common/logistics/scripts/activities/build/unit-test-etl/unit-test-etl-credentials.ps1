Add-CredentialMetadata `
	@(
        @{Name = "Test Runner";  DefaultUsername = "edfiTestRunner"; Description = "Credentials for running tests against the test execution SQL Server instance."; },
        @{Name = "Test Database Test Runner"; DefaultUsername = "edfiTestRunner"; Description = "Credentials for retrieving test data from the SQL Server instance where the Test Database exists."; }
      )
