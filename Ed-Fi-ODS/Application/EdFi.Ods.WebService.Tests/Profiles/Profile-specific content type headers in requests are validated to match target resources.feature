@API
Feature: Profile-specific content type headers in requests are validated to match target resources

Scenario: GET Request contains an accept header with a content type using a profile that does not include the targeted resource
    Given the caller is using the "Test-Profile-StaffOnly-Resource-IncludeAll" profile
    When a GET (by id) request is submitted to students with an accept header content type of the appropriate value for the profile in use
    Then the response should contain a 400 Bad Request failure indicating that "the resource is not accessible through the profile specified by the content type"

Scenario: GET Request contains an accept header with a content type using a profile that includes the targeted resource
    Given the caller is using the "Test-Profile-StudentOnly-Resource-IncludeAll" profile
    When a GET (by id) request is submitted to students with an accept header content type of the appropriate value for the profile in use
    Then the response should indicate success

Scenario: GET Request contains a content type header with a resource that does not match the requested resource
    Given the caller is using the "Test-Profile-StudentOnly-Resource-IncludeAll" profile
    When a GET (by id) request is submitted to students with an accept header content type of "application/vnd.ed-fi.school.test-profile-studentonly-resource-includeall.readable+json"
    Then the response should contain a 400 Bad Request failure indicating that "the resource is not accessible through the profile specified by the content type"

Scenario: GET Request contains a content type header with a read/write content that does not match the requested resource
    Given the caller is using the "Test-Profile-StudentOnly-Resource-IncludeAll" profile
    When a GET (by id) request is submitted to students with an accept header content type of "application/vnd.ed-fi.student.test-profile-studentonly-resource-includeall.writable+json"
    Then the response should contain a 400 Bad Request failure indicating that "the resource is not accessible through the profile specified by the content type"

Scenario: POST Request contains a content type header using a profile that does not include the targeted resource
    Given the caller is using the "Test-Profile-StaffOnly-Resource-IncludeAll" profile
    When a POST request with a resource is submitted to students with a request body content type of the appropriate value for the profile in use
    Then the response should contain a 400 Bad Request failure indicating that "the resource is not accessible through the profile specified by the content type"

Scenario: POST Request contains a content type header using a profile that includes the targeted resource
    Given the caller is using the "Test-Profile-StudentOnly-Resource-IncludeAll" profile
    When a POST request with a resource is submitted to students with a request body content type of the appropriate value for the profile in use
    Then the response should indicate success

Scenario: POST Request contains a content type header with a resource that does not match the requested resource
	Given the caller is using the "Test-Profile-StudentOnly-Resource-IncludeAll" profile
    When a POST request with a resource is submitted to students with a request body content type of "application/vnd.ed-fi.school.test-profile-studentonly-resource-includeall.writable+json"
    Then the response should contain a 400 Bad Request failure indicating that "the resource is not accessible through the profile specified by the content type"

Scenario: POST	 Request contains a content type header with a read/write content that does not match the requested resource
	Given the caller is using the "Test-Profile-StudentOnly-Resource-IncludeAll" profile
    When a POST request with a resource is submitted to students with a request body content type of "application/vnd.ed-fi.student.test-profile-studentonly-resource-includeall.readable+json"
    Then the response should contain a 400 Bad Request failure indicating that "the resource is not accessible through the profile specified by the content type"
