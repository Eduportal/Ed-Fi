@API
Feature: Requests using content types referencing undefined profiles will fail

Scenario: The caller attempts to read a resource using a non-existing profile
    Given the caller is using the "Non-Existing" profile
    When a GET (by id) request is submitted to schools with an accept header content type of the appropriate value for the profile in use
    Then the response should contain a 406 Not Acceptable failure indicating that "the profile specified by the content type is not supported by this host"

Scenario: The caller attempts to update a resource using a non-existing profile
    Given the caller is using the "Non-Existing" profile
    When a POST request with a resource is submitted to schools with a request body content type of the appropriate value for the profile in use
    Then the response should contain a 415 Unsupported Media Type failure indicating that "the profile specified by the content type is not supported by this host"
