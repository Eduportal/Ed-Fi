@API @SDK
Feature: Profile definitions can include and exclude specific properties on child collections

Scenario Outline: The Read content type only includes certain properties from a child collection
    Given the caller is using the "Test-Profile-AssessmentFamily-Resource-Child-Collection-IncludeOnly" profile
    When a GET (by id) request is submitted using <call mechanism> to assessmentFamilies with an accept header content type of the appropriate value for the profile in use
    Then the response model's identificationCodes collection items should contain the contextual primary key properties of [assessmentIdentificationSystemDescriptor]
    And the response model's identificationCodes collection items should contain the explicitly included properties of [identificationCode]
    And the number of properties on the response model's base class identificationCodes collection items should reflect the number of included properties plus the contextual primary key properties
    
Examples:
    | call mechanism |
    | the SDK        |
    | raw JSON       |

Scenario Outline: The Read content type only excludes certain properties from a child collection
    Given the caller is using the "Test-Profile-AssessmentFamily-Resource-Child-Collection-ExcludeOnly" profile
    When a GET (by id) request is submitted using <call mechanism> to assessmentFamilies with an accept header content type of the appropriate value for the profile in use
    Then the response model's identificationCodes collection items should contain the contextual primary key properties of [assessmentIdentificationSystemDescriptor]
    And the response model's identificationCodes collection items should not contain the explicitly excluded properties of [identificationCode]
    And the number of properties on the response model's base class identificationCodes collection items should reflect the number of properties on the full AssessmentFamilyIdentificationCode resource model less the explicitly excluded ones

Examples:
    | call mechanism |
    | the SDK        |
    | raw JSON       |

Scenario Outline: The Write content type only includes certain properties from a child collection
    Given the caller is using the "Test-Profile-AssessmentFamily-Resource-Child-Collection-IncludeOnly" profile
    When a PUT request with a completely updated resource with preserved child collections is submitted using <call mechanism> to assessmentFamilies with a request body content type of the appropriate value for the profile in use
    Then the only values changed on the entity model's AssessmentFamilyIdentificationCodes collection items should be the explicitly included properties of AssigningOrganizationIdentificationCode
    
Examples:
    | call mechanism |
    | the SDK        |
    | raw JSON       |

Scenario Outline: The Write content type only excludes certain properties from a child collection
    Given the caller is using the "Test-Profile-AssessmentFamily-Resource-Child-Collection-ExcludeOnly" profile
    When a PUT request with a completely updated resource with preserved child collections is submitted using <call mechanism> to assessmentFamilies with a request body content type of the appropriate value for the profile in use
    Then the only values not changed on the entity model's AssessmentFamilyIdentificationCodes collection items should be the contextual primary key values of [AssessmentIdentificationSystemDescriptor, AssessmentIdentificationSystemDescriptorId], and the explicitly excluded properties of [AssigningOrganizationIdentificationCode]
    
Examples:
    | call mechanism |
    | the SDK        |
    | raw JSON       |