Feature: ClientApplications
	
Background: 
	Given The account "edfitestuser@gmail.com" does not exist
	Given There exists a regular user with name "Carl" password "123456" email "edfitestuser@gmail.com"
	Given I am logged in successfully with email "edfitestuser@gmail.com" and password "123456"
	Given I go to the Client Applications page

Scenario: Add a dedicated sandbox with sample data
	When I add an Application with "sample" data
	Then The application should appear on the Existing Applications Table
	
Scenario: Add a dedicated sandbox with minimal data
	When I add an Application with "minimal" data
	Then The application should appear on the Existing Applications Table

Scenario: Add and Delete a sandbox with sample data
	When I add an Application with "sample" data
	Then The application should appear on the Existing Applications Table
	When I delete the application
	Then The application should not appear on the Existing Applications Table

Scenario: Add and Update a sandbox with sample data
	When I add an Application with "sample" data
	Then The application should appear on the Existing Applications Table
	When I update the application
	Then The application should appear on the Existing Applications Table
	And The application's keys shouldn't be the same

	#TODO: Remove all the page refreshes once the bug is fixed.