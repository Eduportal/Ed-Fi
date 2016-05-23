@API
Feature: Resources not covered by an assigned profile are accessible using either the standard JSON content type or a valid profile content type

Scenario: The caller requests a resource not covered by any of their assigned profiles using the standard content type
    Given the caller is assigned the "Test-Profile-Resource-IncludeAll" profile
    And the caller is assigned the "Test-Profile-StaffOnly-Resource-IncludeAll" profile
    When a GET (by id) request is submitted to students with an accept header content type of "application/json"
    Then the response should indicate success

# Callers can always use unassigned profiles to access data
Scenario: The caller requests a resource not covered by their assigned profiles using a valid but unassigned profile-specific content type
    Given the caller is assigned the "Test-Profile-Resource-IncludeAll" profile
    And the caller is assigned the "Test-Profile-StaffOnly-Resource-IncludeAll" profile
    And the caller is using the "Test-Profile-StudentOnly-Resource-IncludeAll" profile
    When a GET (by id) request is submitted to students with an accept header content type of the appropriate value for the profile in use
    Then the response should indicate success

# Callers can always use standard content type to update data
Scenario: The caller updates a resource not covered by any of their assigned profiles using the standard content type
    Given the caller is assigned the "Test-Profile-Resource-IncludeAll" profile
    And the caller is assigned the "Test-Profile-StaffOnly-Resource-IncludeAll" profile
    When a POST request with a resource is submitted to students with a request body content type of "application/json"
    Then the response should indicate success

# Callers can always use unassigned profiles to update data
Scenario: The caller updates a resource not covered by any of their assigned profiles using a valid but unassigned profile-specific content type
    Given the caller is assigned the "Test-Profile-Resource-IncludeAll" profile
    And the caller is assigned the "Test-Profile-StaffOnly-Resource-IncludeAll" profile
    And the caller is using the "Test-Profile-StudentOnly-Resource-IncludeAll" profile
    When a POST request with a resource is submitted to students with a request body content type of the appropriate value for the profile in use
    Then the response should indicate success
