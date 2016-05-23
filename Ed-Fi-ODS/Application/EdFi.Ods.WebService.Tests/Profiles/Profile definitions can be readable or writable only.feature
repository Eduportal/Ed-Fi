@API
Feature: Profile definitions can be readable or writable only	

# Readable only
Scenario: A GET request is made with a read only profile 
    Given the caller is using the "Test-Profile-Resource-ReadOnly" profile	
    When a GET (by id) request is submitted to schools with an accept header content type of the appropriate value for the profile in use
    Then the response should indicate success
    
Scenario: A PUT request is made with a read only profile
    Given the caller is using the "Test-Profile-Resource-ReadOnly" profile
    When a PUT request with a completely updated resource is submitted using raw JSON to schools with a request body content type of the appropriate value for the profile in use
    Then the response should contain a 405 Method Not Allowed failure indicating that "The allowed methods for this resource with the '{profile}' profile are GET, DELETE and OPTIONS."

Scenario: A POST request is made with a read only profile
    Given the caller is using the "Test-Profile-Resource-ReadOnly" profile	
    When a POST request with a resource is submitted to schools with a request body content type of the appropriate value for the profile in use
    Then the response should contain a 405 Method Not Allowed failure indicating that "The allowed methods for this resource with the '{profile}' profile are GET, DELETE and OPTIONS."

# Writable only
Scenario: A GET request is made with a write only profile
    Given the caller is using the "Test-Profile-Resource-WriteOnly" profile	
    When a GET (by id) request is submitted to schools with an accept header content type of the appropriate value for the profile in use
    Then the response should contain a 405 Method Not Allowed failure indicating that "The allowed methods for this resource with the '{profile}' profile are PUT, POST, DELETE and OPTIONS."

Scenario: A PUT request is made with a write only profile 
    Given the caller is using the "Test-Profile-Resource-WriteOnly" profile	
    When a PUT request with a completely updated resource is submitted using raw JSON to schools with a request body content type of the appropriate value for the profile in use
    Then the response should indicate success

Scenario: A POST request is made with a write only profile 
    Given the caller is using the "Test-Profile-Resource-WriteOnly" profile	
    When a POST request with a resource is submitted to schools with a request body content type of the appropriate value for the profile in use
    Then the response should indicate success

