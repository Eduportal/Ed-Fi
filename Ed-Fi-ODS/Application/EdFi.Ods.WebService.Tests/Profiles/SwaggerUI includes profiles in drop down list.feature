@SwaggerUI
Feature: SwaggerUI supports Ed-Fi ODS API Profiles

Scenario: SwaggerUI is first loaded
	Given I have loaded a fresh copy of the SwaggerUI web page in a web browser
	Then the first entry in the API section list should be "Resources"
    And the second entry in the API section list should be "Descriptors"
    And the third entry in the API section list should be "Types"
    And the fourth entry in the API section list should be "Other"
    And each profile entry in the API section list should use the profile name as the value
    And each profile entry in the API section list should be displayed with a prefix of "Profile: "
    And the number of entries in the API section list should correspond to the number of profiles plus the 4 standard sections
    And "Resources" should be selected in the API sections drop down list

Scenario: Descriptors section is selected
    Given I am on the SwaggerUI web page in a web browser
    When I select the API section "Descriptors" 
    Then the page title should contain "Ed-Fi Operational Data Store API"
    And the page title should not contain "Profile:"

# FireFox cannot render the metadata fast enough for this next test to pass
Scenario: Profile containing multiple resources is selected from the API Sections drop down list
	Given I am on the SwaggerUI web page in a web browser
    When I select the entry for the profile named "Test-Profile-Student-and-School-Include-All"
    And I expand all the resource sections
    Then the page title should contain "Ed-Fi Operational Data Store API"
    And the page title should contain "(Profile: Test-Profile-Student-and-School-Include-All)"
    And the page should only contain sections for the resources defined in the profile
    And the GET operations should only provide a single option for the Response Content Type that is the profile-specific content type for the resource
    And the PUT operations should only provide a single option for the Parameter Content Type that is the profile-specific content type for the resource
    And the POST operations should only provide a single option for the Parameter Content Type that is the profile-specific content type for the resource

Scenario: SwaggerUI is used for a GET operation
	Given I am on the SwaggerUI web page in a web browser
    When I select the entry for the profile named "Test-Profile-Resource-IncludeOnly"
    And I GetAll "schools"
    Then the response code should indicate success
    And the response headers should contain the profile-specific content type for the resource

Scenario: SwaggerUI is used for a POST operation
	Given I am on the SwaggerUI web page in a web browser
    When I select the entry for the profile named "Test-Profile-Resource-IncludeOnly"
    And I POST a school
    Then the response code should indicate success

Scenario: SwaggerUI is used for a PUT operation
	Given I am on the SwaggerUI web page in a web browser
    When I select the entry for the profile named "Test-Profile-Resource-IncludeOnly"
    And I PUT a school
    Then the response code should indicate success
