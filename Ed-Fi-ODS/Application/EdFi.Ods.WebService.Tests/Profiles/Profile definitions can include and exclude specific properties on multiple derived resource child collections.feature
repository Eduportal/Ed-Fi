@API @SDK
Feature: Profile definitions can include and exclude specific properties of base class child collections independently on multiple derived resources

Scenario Outline: The Read content type only includes a distinct set of properties from a base class child collection for each derived resource
    Given the caller is using the "Test-Profile-EdOrgs-Resources-Child-Collection-IncludeOnly" profile
    When a GET (by id) request is submitted using <call mechanism> to <resource> with an accept header content type of the appropriate value for the profile in use
    Then the response model's base class <collection_property> collection items should contain the contextual primary key properties of [<contextual_pk_properties>]
    And the response model's base class <collection_property> collection items should contain the explicitly included properties of [<included_properties>]
    And the number of properties on the response model's base class <collection_property> collection items should reflect the number of included properties plus the contextual primary key properties

Examples:
| call mechanism  | resource                | collection_property | contextual_pk_properties | included_properties                     |
| the SDK         | schools                 | addresses           | addressType              | city, stateAbbreviationType, postalCode |
| raw JSON        | localEducationAgencies  | addresses           | addressType              | latitude, longitude                     |

Scenario Outline: The Read content type only excludes a distinct set of properties from a base class child collection for each derived resource
    Given the caller is using the "Test-Profile-EdOrgs-Resources-Child-Collection-ExcludeOnly" profile
    When a GET (by id) request is submitted using <call mechanism> to <resource> with an accept header content type of the appropriate value for the profile in use
    Then the response model's base class <collection_property> collection items should contain the contextual primary key properties of [<contextual_pk_properties>]
    And the response model's base class <collection_property> collection items should not contain the explicitly excluded properties of [<excluded_properties>]
    And the number of properties on the response model's base class <collection_property> collection items should reflect the number of properties on the full <child_resource_model_name> resource model less the explicitly excluded ones

Examples:
| call mechanism  | resource               | collection_property | contextual_pk_properties | excluded_properties                     | child_resource_model_name    |
| the SDK         | schools                | addresses           | addressType              | city, stateAbbreviationType, postalCode | EducationOrganizationAddress |
| raw JSON        | localEducationAgencies | addresses           | addressType              | latitude, longitude                     | EducationOrganizationAddress |

Scenario Outline: The Write content type only includes certain properties from a base class child collection
    Given the caller is using the "Test-Profile-EdOrgs-Resources-Child-Collection-IncludeOnly" profile
    When a PUT request with a completely updated resource with preserved child collections is submitted using <call mechanism> to <resource> with a request body content type of the appropriate value for the profile in use
    Then the only values changed on the entity model's base class <collection_property> collection items should be the explicitly included properties of <included_properties>

Examples:
| call mechanism  | resource               | collection_property            | included_properties                                              |
| the SDK         | schools                | EducationOrganizationAddresses | Latitude, Longitude                                              |
| raw JSON        | localEducationAgencies | EducationOrganizationAddresses | City, StateAbbreviationType, StateAbbreviationTypeId, PostalCode |

Scenario Outline: The Write content type only excludes certain properties from a base class child collection
    Given the caller is using the "Test-Profile-EdOrgs-Resources-Child-Collection-ExcludeOnly" profile
    When a PUT request with a completely updated resource with preserved child collections is submitted using <call mechanism> to <resource> with a request body content type of the appropriate value for the profile in use
    Then the only values not changed on the entity model's base class <collection_property> collection items should be the contextual primary key values of [<contextual_pk_properties>], and the explicitly excluded properties of [<excluded_properties>]

Examples:
| call mechanism  | resource               | collection_property            | excluded_properties                                              | contextual_pk_properties   |
| the SDK         | schools                | EducationOrganizationAddresses | Latitude, Longitude                                              | AddressType, AddressTypeId |
| raw JSON        | localEducationAgencies | EducationOrganizationAddresses | City, StateAbbreviationType, StateAbbreviationTypeId, PostalCode | AddressType, AddressTypeId |
