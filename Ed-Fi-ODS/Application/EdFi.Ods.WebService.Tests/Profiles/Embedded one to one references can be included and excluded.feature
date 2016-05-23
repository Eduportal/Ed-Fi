@API
Feature: Embedded one to one references can be included and excluded

Scenario: The Read content type can include embedded one to one references
    Given the caller is using the "Student-Roster-Readable-Includes-Embedded-Object" profile
    When a GET (by id) request is submitted using raw JSON to students with an accept header content type of the appropriate value for the profile in use	
    Then the response should contain the embedded object learningStyle

Scenario: The Read content type can exclude embedded one to one references
    Given the caller is using the "Student-Roster-Readable-Excludes-Embedded-Object" profile
    When a GET (by id) request is submitted using raw JSON to students with an accept header content type of the appropriate value for the profile in use
    Then the response should not contain the embedded object learningStyle

Scenario: The Write content type can include embedded one to one references
    Given the caller is using the "Student-Roster-Writable-Includes-Embedded-Object" profile
    When a PUT request with a completely updated resource is submitted using raw JSON to students with a request body content type of the appropriate value for the profile in use
    Then the persisted entity model embedded object StudentLearningStyle should be changed

Scenario: The Write content type can exclude embedded one to one references
    Given the caller is using the "Student-Roster-Writable-Excludes-Embedded-Object" profile
    When a PUT request with a completely updated resource is submitted using raw JSON to students with a request body content type of the appropriate value for the profile in use
    Then the persisted entity model embedded object StudentLearningStyle should not be changed