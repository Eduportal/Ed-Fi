@API
 Feature: Profiles assigned to callers must be used for covered resources

# Read a covered resource with correct content type
Scenario: The caller is assigned a profile and requests a covered resource using the correct content type header
    Given the caller is assigned the "Test-Profile-Resource-IncludeOnly" profile
    And the caller is using the "Test-Profile-Resource-IncludeOnly" profile
    When a GET (by id) request is submitted to schools with an accept header content type of the appropriate value for the profile in use
    Then the response should indicate success

# Read a covered resource with a different profile's content type
Scenario: The caller is assigned a profile and requests a covered resource using a different profile's content type
    Given the caller is assigned the "Test-Profile-StudentOnly-Resource-IncludeAll" profile
    And the caller is using the "Test-Profile-StudentOnly2-Resource-IncludeAll" profile
    When a GET (by id) request is submitted to students with an accept header content type of the appropriate value for the profile in use
    Then the response should contain a 403 Forbidden failure indicating that "one of the following profile-specific content types is required when requesting this resource"

# Read a covered resource with standard content type
Scenario: The caller is assigned a profile and requests a covered resource using the standard content type header
    Given the caller is assigned the "Test-Profile-Resource-IncludeAll" profile
    When a GET (by id) request is submitted to schools with an accept header content type of "application/json"
    Then the response should contain a 403 Forbidden failure indicating that "one of the following profile-specific content types is required when requesting this resource"

# Write a covered resource with correct content type
Scenario: The caller is assigned a profile and attempts to update a covered resource using the correct content type header
    Given the caller is assigned the "Test-Profile-Resource-IncludeAll" profile
    And the caller is using the "Test-Profile-Resource-IncludeAll" profile
    When a POST request with a resource is submitted to schools with a request body content type of the appropriate value for the profile in use
    Then the response should indicate success

# Write a covered resource with a different profile's content type
Scenario: The caller is assigned a profile and attempts to update a covered resource using a different profile's content type
    Given the caller is assigned the "Test-Profile-StudentOnly-Resource-IncludeAll" profile
    And the caller is using the "Test-Profile-StudentOnly2-Resource-IncludeAll" profile
    When a POST request with a resource is submitted to students with a request body content type of the appropriate value for the profile in use
    Then the response should contain a 403 Forbidden failure indicating that "based on the assigned profiles, one of the following profile-specific content types is required when updating this resource"

# Write a covered resource with standard content type
Scenario: The caller is assigned a profile and attempts to update a covered resource using the standard content type header
    Given the caller is assigned the "Test-Profile-Resource-IncludeAll" profile
    When a POST request with a resource is submitted to schools with a request body content type of "application/json"
    Then the response should contain a 403 Forbidden failure indicating that "based on the assigned profiles, one of the following profile-specific content types is required when updating this resource"
