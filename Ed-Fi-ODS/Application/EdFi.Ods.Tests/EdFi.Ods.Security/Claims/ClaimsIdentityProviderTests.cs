// ****************************************************************************
//  Copyright (C) 2015 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using EdFi.Common.Security;
using EdFi.Common.Security.Claims;
using EdFi.Ods.Common.Security;
using EdFi.Ods.Security.Claims;
using EdFi.Ods.Security.Metadata.Models;
using EdFi.Ods.Security.Metadata.Repositories;
using EdFi.Ods.Tests._Bases;
using EdFi.Ods.Tests._Extensions;
using Rhino.Mocks;
using Should;
using Action = EdFi.Ods.Security.Metadata.Models.Action;

namespace EdFi.Ods.Tests.EdFi.Ods.Security.Claims
{
    public class ClaimsIdentityProviderTests
    {
        public class When_creating_a_ClaimsIdentity_with_no_API_key_context_available : TestFixtureBase
        {
            // Supplied values

            // Actual values

            // Dependencies

            /// <summary>
            /// Executes the code to be tested.
            /// </summary>
            protected override void Act()
            {
                var apiKeyContextProvider = Stub<IApiKeyContextProvider>();
                apiKeyContextProvider.Stub(x => x.GetApiKeyContext()).Return(null);

                var provider = new ClaimsIdentityProvider(apiKeyContextProvider, null);
                var unused = provider.GetClaimsIdentity();
            }

            [Assert]
            public void Should_throw_an_EdFiSecurityException_related_to_the_missing_API_key_context()
            {
                ActualException.ShouldBeExceptionType<EdFiSecurityException>();
                ActualException.MessageShouldContain("API key");
            }
        }

        public class When_creating_a_ClaimsIdentity_for_a_caller_with_associated_education_organizations : TestFixtureBase
        {
            // Supplied values
            private List<int> _suppliedEducationOrganizationIds = new List<int> {1, 2};
            private string _suppliedNamespacePrefix = "namespacePrefix";
            private List<string> _suppliedProfiles = new List<string> { "supplied-assigned-profile" }; 

            // Actual values
            private ClaimsIdentity _actualIdentity;

            // Dependencies
            private IApiKeyContextProvider _apiKeyContextProvider;
            private ISecurityRepository _securityRepository;

            protected override void Arrange()
            {
                // Initialize dependencies

                const string suppliedClaimSetName = "claimSetName";

                var apiKeyContext = new ApiKeyContext(
                    "apiKey",
                    suppliedClaimSetName,
                    _suppliedEducationOrganizationIds,
                    _suppliedNamespacePrefix,
                    _suppliedProfiles);

                _apiKeyContextProvider = Stub<IApiKeyContextProvider>();
                _apiKeyContextProvider.Stub(x => x.GetApiKeyContext())
                                      .Return(apiKeyContext);

                var suppliedResourceClaims = new List<ClaimSetResourceClaim>
                {
                    new ClaimSetResourceClaim
                    {
                        Action = new Action {ActionUri = "actionUri-1a"},
                        ResourceClaim = new ResourceClaim {ClaimName = "resourceClaimName1"},
                    },
                    new ClaimSetResourceClaim
                    {
                        Action = new Action {ActionUri = "actionUri-1b"},
                        ResourceClaim = new ResourceClaim {ClaimName = "resourceClaimName1"},
                    },
                    new ClaimSetResourceClaim
                    {
                        Action = new Action {ActionUri = "actionUri-2"},
                        ResourceClaim = new ResourceClaim {ClaimName = "resourceClaimName2"},
                    },
                };

                _securityRepository = Stub<ISecurityRepository>();
                _securityRepository.Stub(x => x.GetClaimsForClaimSet(suppliedClaimSetName))
                                   .Return(suppliedResourceClaims);
            }

            protected override void Act()
            {
                // Execute code under test
                var provider = new ClaimsIdentityProvider(
                    _apiKeyContextProvider,
                    _securityRepository);

                _actualIdentity = provider.GetClaimsIdentity();
            }

            [Assert]
            public void Should_issue_a_namespace_prefix_claim_using_the_supplied_value()
            {
                _actualIdentity.Claims.SingleOrDefault(c => 
                    c.Type == EdFiOdsApiClaimTypes.NamespacePrefix
                    && c.Value == _suppliedNamespacePrefix)
                    .ShouldNotBeNull();
            }

            [Assert]
            public void Should_issue_one_claim_for_each_distinct_resource_claim_defined_in_the_callers_claim_set()
            {
                // Count the "resource" claims issued
                _actualIdentity.Claims.Count(c => c.Type.StartsWith("resource")).ShouldEqual(2);

                _actualIdentity.Claims.Count(c => c.Type == "resourceClaimName1")
                    .ShouldEqual(1, "Multiple actions associated with the same claim set should not result in multiple claims being issued.");

                _actualIdentity.Claims.Count(c => c.Type == "resourceClaimName2")
                    .ShouldEqual(1);
            }

            [Assert]
            public void Should_combine_multiple_actions_for_the_same_resource_claim_into_an_array_under_a_single_issued_claim()
            {
                var claim1 = _actualIdentity.Claims.SingleOrDefault(c => c.Type == "resourceClaimName1");
                var edFiResourceClaim1 = claim1.ToEdFiResourceClaimValue();

                edFiResourceClaim1.Actions.Count().ShouldEqual(2);
                edFiResourceClaim1.Actions.ShouldContain("actionUri-1a");
                edFiResourceClaim1.Actions.ShouldContain("actionUri-1b");
            }

            [Assert]
            public void Should_associate_the_callers_education_organizations_with_each_resource_claim()
            {
                var resourceClaim1 = _actualIdentity.Claims.SingleOrDefault(c => c.Type == "resourceClaimName1");
                var resourceClaim1Value = resourceClaim1.ToEdFiResourceClaimValue();

                resourceClaim1Value.EducationOrganizationIds.ShouldEqual(_suppliedEducationOrganizationIds);
                

                var resourceClaim2 = _actualIdentity.Claims.SingleOrDefault(c => c.Type == "resourceClaimName2");
                var resourceClaim2Value = resourceClaim2.ToEdFiResourceClaimValue();

                resourceClaim2Value.EducationOrganizationIds.ShouldEqual(_suppliedEducationOrganizationIds);
            }
        }
    }
}