@API
Feature: Profiles can exclude optional references	

Scenario: The Read content type can exclude optional references
    Given the caller is using the "Academic-Week-Readable-Excludes-Optional-References" profile
    When a GET (by id) request is submitted using raw JSON to academicWeeks with an accept header content type of the appropriate value for the profile in use
    Then the response should only contain the included references

Scenario: The Write content type can exclude optional references
    Given the caller is using the "Academic-Week-Writable-Excludes-Optional-References" profile
    When a PUT request with a completely updated resource is submitted using raw JSON to academicWeeks with a request body content type of the appropriate value for the profile in use    
    Then the persisted entity model should have unmodified values for the excluded reference