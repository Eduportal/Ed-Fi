@API
Feature: Profile definitions can filter child collections to include or exclude items based on Type and Descriptor values

# Read, Include Types
Scenario: The Read content type filters a child collection to only include certain Type values
    Given the caller is using the "Test-Profile-Resource-Child-Collection-Filtered-To-IncludeOnly-Specific-Types-and-Descriptors" profile
    When a GET (by id) request is submitted to schools with an accept header content type of the appropriate value for the profile in use
    Then the response model's collection items should only contain items matching the included Type values

# Read, Exclude Types
Scenario: The Read content type filters a child collection to only exclude certain Type values
    Given the caller is using the "Test-Profile-Resource-Child-Collection-Filtered-To-ExcludeOnly-Specific-Types-and-Descriptors" profile
    When a GET (by id) request is submitted to schools with an accept header content type of the appropriate value for the profile in use
    Then the response model's collection items should not contain items matching the excluded Type values

# Read, Include Descriptors
Scenario: The Read content type filters a child collection to only include certain Descriptor values
    Given the caller is using the "Test-Profile-Resource-Child-Collection-Filtered-To-IncludeOnly-Specific-Types-and-Descriptors" profile
    When a GET (by id) request is submitted to schools with an accept header content type of the appropriate value for the profile in use
    Then the response model's collection items should only contain items matching the included Descriptor values

# Read, Exclude Descriptors
Scenario: The Read content type filters a child collection to only exclude certain Descriptor values
    Given the caller is using the "Test-Profile-Resource-Child-Collection-Filtered-To-ExcludeOnly-Specific-Types-and-Descriptors" profile
    When a GET (by id) request is submitted to schools with an accept header content type of the appropriate value for the profile in use
    Then the response model's collection items should not contain items matching the excluded Descriptor values

# Write, Include Types, Conforming
Scenario: The Write content type filters a child collection to only include certain Type values and only conforming values are supplied
    Given the caller is using the "Test-Profile-Resource-Child-Collection-Filtered-To-IncludeOnly-Specific-Types-and-Descriptors" profile
    When a PUT request with a collection containing only conforming included Type values is submitted to schools with a request body content type of the appropriate value for the profile in use
    Then the response should indicate success
    And the submitted Type values should be persisted to the School
    And the pre-existing Type values should be intact on the School

# Write, Include Types, Non-conforming
Scenario: The Write content type filters a child collection to only include certain Type values and non-conforming values are supplied
    Given the caller is using the "Test-Profile-Resource-Child-Collection-Filtered-To-IncludeOnly-Specific-Types-and-Descriptors" profile
    When a PUT request with a collection containing only non-conforming included Type values is submitted to schools with a request body content type of the appropriate value for the profile in use
    Then the response should contain a 400 Bad Request failure indicating that "the value of {suppliedValue} supplied for the {property} of the {entity} does not conform with the filter values defined by profile {profileName}"

# Write, Exclude Types, Conforming
Scenario: The Write content type filters a child collection to only exclude certain Type values and only conforming values are supplied
    Given the caller is using the "Test-Profile-Resource-Child-Collection-Filtered-To-ExcludeOnly-Specific-Types-and-Descriptors" profile
    When a PUT request with a collection containing only conforming excluded Type values is submitted to schools with a request body content type of the appropriate value for the profile in use
    Then the response should indicate success
    And the submitted Type values should be persisted to the School
    And the pre-existing Type values should be intact on the School

# Write, Exclude Types, Non-conforming
Scenario: The Write content type filters a child collection to only exclude certain Type values and non-conforming values are supplied
    Given the caller is using the "Test-Profile-Resource-Child-Collection-Filtered-To-ExcludeOnly-Specific-Types-and-Descriptors" profile
    When a PUT request with a collection containing only non-conforming excluded Type values is submitted to schools with a request body content type of the appropriate value for the profile in use
    Then the response should contain a 400 Bad Request failure indicating that "the value of {suppliedValue} supplied for the {property} of the {entity} does not conform with the filter values defined by profile {profileName}"

# Write, Include Descriptors, Conforming
Scenario: The Write content Descriptor filters a child collection to only include certain Descriptor values and only conforming values are supplied
    Given the caller is using the "Test-Profile-Resource-Child-Collection-Filtered-To-IncludeOnly-Specific-Types-and-Descriptors" profile
    When a PUT request with a collection containing only conforming included Descriptor values is submitted to schools with a request body content type of the appropriate value for the profile in use
    Then the response should indicate success
    And the submitted Descriptor values should be persisted to the School
    And the pre-existing Descriptor values should be intact on the School

# Write, Include Descriptors, Non-conforming
Scenario: The Write content Descriptor filters a child collection to only include certain Descriptor values and non-conforming values are supplied
    Given the caller is using the "Test-Profile-Resource-Child-Collection-Filtered-To-IncludeOnly-Specific-Types-and-Descriptors" profile
    When a PUT request with a collection containing only non-conforming included Descriptor values is submitted to schools with a request body content type of the appropriate value for the profile in use
    Then the response should contain a 400 Bad Request failure indicating that "the value of {suppliedValue} supplied for the {property} of the {entity} does not conform with the filter values defined by profile {profileName}"

# Write, Exclude Descriptors, Conforming
Scenario: The Write content Descriptor filters a child collection to only exclude certain Descriptor values and only conforming values are supplied
    Given the caller is using the "Test-Profile-Resource-Child-Collection-Filtered-To-ExcludeOnly-Specific-Types-and-Descriptors" profile
    When a PUT request with a collection containing only conforming excluded Descriptor values is submitted to schools with a request body content type of the appropriate value for the profile in use
    Then the response should indicate success
    And the submitted Descriptor values should be persisted to the School
    And the pre-existing Descriptor values should be intact on the School

# Write, Exclude Descriptors, Non-conforming
Scenario: The Write content Descriptor filters a child collection to only exclude certain Descriptor values and non-conforming values are supplied
    Given the caller is using the "Test-Profile-Resource-Child-Collection-Filtered-To-ExcludeOnly-Specific-Types-and-Descriptors" profile
    When a PUT request with a collection containing only non-conforming excluded Descriptor values is submitted to schools with a request body content type of the appropriate value for the profile in use
    Then the response should contain a 400 Bad Request failure indicating that "the value of {suppliedValue} supplied for the {property} of the {entity} does not conform with the filter values defined by profile {profileName}"
