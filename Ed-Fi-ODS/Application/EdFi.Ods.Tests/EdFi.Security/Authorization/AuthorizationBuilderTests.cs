// ****************************************************************************
//  Copyright (C) 2015 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using EdFi.Common.Extensions;
using EdFi.Common.Security;
using EdFi.Common.Security.Authorization;
using EdFi.Common.Security.Claims;
using EdFi.Ods.Security.AuthorizationStrategies.Relationships;
using EdFi.Ods.Tests._Bases;
using EdFi.Ods.Tests._Extensions;
using NUnit.Framework;
using Rhino.Mocks;
using Should;

namespace EdFi.Ods.Tests.EdFi.Security.Authorization
{
    public class Feature_building_claims_authorization_segments
    {
        #region Givens

        private static IEducationOrganizationCache Given_a_cache_that_indicates_no_organizations_exist()
        {
            return MockRepository.GenerateStub<IEducationOrganizationCache>();
        }

        private static IEducationOrganizationCache Given_a_cache_that_indicates_all_education_organizations_exist_and_are_schools()
        {
            var educationOrganizationCache = MockRepository.GenerateStub<IEducationOrganizationCache>();
            educationOrganizationCache.Stub(x => x.GetEducationOrganizationIdentifiers(Arg<int>.Is.Anything))
                .Return(new EducationOrganizationIdentifiers(0, "School", null, null, null, null));

            return educationOrganizationCache;
        }

        private static IEnumerable<Claim> Given_a_claimset_with_a_claim_for_some_LocalEducationAgency()
        {
            return new[]
            {
                JsonClaimHelper.CreateClaim(
                    "someResource",
                    new EdFiResourceClaimValue("something", new List<int> {999}))
            };
        }

        private static IEducationOrganizationCache Given_a_cache_that_indicates_the_only_EducationOrganizationId_that_exists_is(int educationOrganizationId)
        {
            var educationOrganizationCache = MockRepository.GenerateStub<IEducationOrganizationCache>();
            educationOrganizationCache.Stub(x => x.GetEducationOrganizationIdentifiers(educationOrganizationId))
                .Return(
                    new EducationOrganizationIdentifiers(
                        educationOrganizationId,
                        "LocalEducationAgency",
                        null,
                        null,
                        null,
                        null));

            return educationOrganizationCache;
        }

        private static IEnumerable<Claim> Given_a_claimset_with_a_claim_for_LocalEducationAgencies(params int[] localEducationAgencyIds)
        {
            return new[]
            {
                JsonClaimHelper.CreateClaim(
                    "someResource",
                    new EdFiResourceClaimValue("something", new List<int>(localEducationAgencyIds)))
            };
        }

        private static IEnumerable<Claim> Given_a_claimset_with_a_claim_that_has_no_educationOrganizations()
        {
            return new[]
            {
                JsonClaimHelper.CreateClaim(
                    "someResource",
                    new EdFiResourceClaimValue("something", null))
            };
        }

        private static RelationshipsAuthorizationContextData
            Given_authorization_context_data_with_some_StaffUniqueId()
        {
            return new RelationshipsAuthorizationContextData
            {
                StaffUSI = 1234,
            };
        }

        #endregion

        public class When_building_a_single_segment_for_a_claimset_that_has_no_education_organization_identifiers 
            : TestFixtureBase
        {
            // Supplied values

            // Actual values

            protected override void Act()
            {
                // Execute code under test
                var builder = new AuthorizationBuilder<RelationshipsAuthorizationContextData>(Given_a_claimset_with_a_claim_that_has_no_educationOrganizations(),
                    Given_a_cache_that_indicates_no_organizations_exist(), Given_authorization_context_data_with_some_StaffUniqueId());

                builder.ClaimsMustBeAssociatedWith(x => x.StaffUSI);

                builder.GetSegments();
            }

            [Assert]
            public void Should_throw_a_security_exception_indicating_there_were_no_claim_values_available()
            {
                ActualException.ShouldBeExceptionType<EdFiSecurityException>();
            }
        }

        public class When_building_a_single_segment_for_a_Local_Education_Agency_that_does_not_exist 
            : TestFixtureBase
        {
            // Supplied values

            // Actual values

            protected override void Act()
            {
                // Execute code under test
                var builder = new AuthorizationBuilder<RelationshipsAuthorizationContextData>(Given_a_claimset_with_a_claim_for_some_LocalEducationAgency(),
                    Given_a_cache_that_indicates_no_organizations_exist(), Given_authorization_context_data_with_some_StaffUniqueId());

                builder.ClaimsMustBeAssociatedWith(x => x.StaffUSI);

                builder.GetSegments();
            }

            [Assert]
            public void Should_throw_a_security_exception_indicating_there_were_no_claim_values_available()
            {
                ActualException.ShouldBeExceptionType<EdFiSecurityException>();
            }
        }

        public class When_building_a_single_segment_for_two_Local_Education_Agencies_where_one_exists_and_one_does_not
            : TestFixtureBase
        {
            // Supplied values

            // Actual values
            private AuthorizationSegmentCollection _actualSegments;

            protected override void Act()
            {
                // Execute code under test
                var builder = new AuthorizationBuilder<RelationshipsAuthorizationContextData>(Given_a_claimset_with_a_claim_for_LocalEducationAgencies(888, 999),
                    Given_a_cache_that_indicates_the_only_EducationOrganizationId_that_exists_is(888), Given_authorization_context_data_with_some_StaffUniqueId());

                builder.ClaimsMustBeAssociatedWith(x => x.StaffUSI);

                _actualSegments = builder.GetSegments();
            }

            [Assert]
            public void Should_return_a_segment_with_the_existing_LocalEducationAgencyId_as_the_value()
            {
                _actualSegments.ClaimsAuthorizationSegments.Count.ShouldEqual(1);
                _actualSegments.ClaimsAuthorizationSegments.First().ClaimsEndpoints.Single().Value.ShouldEqual(888);
            }
        }

        public class When_building_a_single_segment_using_a_string_array_of_2_property_names 
            : TestFixtureBase
        {
            // Supplied values

            // Actual values
            private AuthorizationSegmentCollection _actualSegments;

            protected override void Act()
            {
                var builder = new AuthorizationBuilder<RelationshipsAuthorizationContextData>(Given_a_claimset_with_a_claim_for_some_LocalEducationAgency(),
                    Given_a_cache_that_indicates_all_education_organizations_exist_and_are_schools(), Given_authorization_context_data_with_some_StaffUniqueId());

                builder.ClaimsMustBeAssociatedWith(new[] { "StudentUSI", "StaffUSI" });

                _actualSegments = builder.GetSegments();
            }

            [Assert]
            public void Should_create_a_segment_for_each_supplied_property_name()
            {
                _actualSegments.ClaimsAuthorizationSegments.Count.ShouldEqual(2);
            }

            [Assert]
            public void Should_create_first_segment_as_a_school_to_the_first_supplied_property_name()
            {
                _actualSegments.ClaimsAuthorizationSegments.ElementAt(0).ClaimsEndpoints.All(x => x.Name == "SchoolId").ShouldBeTrue();
                _actualSegments.ClaimsAuthorizationSegments.ElementAt(0).TargetEndpoint.Name.ShouldEqual("StudentUSI");
            }

            [Assert]
            public void Should_create_second_segment_as_a_school_to_the_second_supplied_property_name()
            {
                _actualSegments.ClaimsAuthorizationSegments.ElementAt(1).ClaimsEndpoints.All(x => x.Name == "SchoolId").ShouldBeTrue();
                _actualSegments.ClaimsAuthorizationSegments.ElementAt(1).TargetEndpoint.Name.ShouldEqual("StaffUSI");
            }
        }
    }

    public class When_building_authorization_segments_for_LocalEducationAgency_claims_to_be_associated_with_a_StaffUniqueId_and_a_simple_value_association_rule_for_the_contextual_School_to_be_associated_with_the_StaffUniqueId
        : TestFixtureBase
    {
        private ClaimsAuthorizationSegment _actualLocalEducationAgencySegment;
        private ExistingValuesAuthorizationSegment _actualSchoolSegment;

        // Dependencies
        private IEducationOrganizationCache _educationOrganizationCache;
        private EdFiResourceClaimValue _suppliedEdFiResourceClaimValue;

        // Actual values
        private AuthorizationSegmentCollection actualAuthorizationSegments;
        private List<Claim> suppliedClaims;

        // Supplied values
        private RelationshipsAuthorizationContextData suppliedContextData;

        protected override void EstablishContext()
        {
            #region Commented out code for integration testing against SQL Server

            //private IDatabaseConnectionStringProvider connectionStringProvider;

            //connectionStringProvider = mocks.Stub<IDatabaseConnectionStringProvider>();
            //connectionStringProvider.Stub(x => x.GetConnectionString())
            //    .Return(@"Server=(local);Database=database;User Id=user;Password=xxxxx");

            //var executor = new EdFiOdsAuthorizationRulesExecutor(connectionStringProvider);
            //executor.Execute(actualAuthorizationRules);

            #endregion

            suppliedContextData = new RelationshipsAuthorizationContextData
            {
                SchoolId = 880001,
                StaffUSI= 738953 //340DFAFA-D39B-4A38-BEA4-AD705CC7EB7C
            };

            _suppliedEdFiResourceClaimValue = new EdFiResourceClaimValue(
                "manage",
                new List<int> {780, 880, 980});

            suppliedClaims = new List<Claim>
            {
                JsonClaimHelper.CreateClaim(
                    "http://ed-fi.org/ods/identity/claims/domains/generalData",
                    _suppliedEdFiResourceClaimValue)
            };

            _educationOrganizationCache = Stub<IEducationOrganizationCache>();
            _educationOrganizationCache.Stub(x => 
                x.GetEducationOrganizationIdentifiers(Arg<int>.Is.Anything))
                .Return(new EducationOrganizationIdentifiers(0, "LocalEducationAgency", null, null, null, null));
        }

        protected override void ExecuteBehavior()
        {
            var builder = new AuthorizationBuilder<RelationshipsAuthorizationContextData>(suppliedClaims,
                _educationOrganizationCache, suppliedContextData);

            actualAuthorizationSegments = builder
                .ClaimsMustBeAssociatedWith(x => x.StaffUSI)
                .ExistingValuesMustBeAssociated(x => x.SchoolId, x => x.StaffUSI)
                .GetSegments();

            _actualLocalEducationAgencySegment = actualAuthorizationSegments.ClaimsAuthorizationSegments.SingleOrDefault(s => 
                s.TargetEndpoint.Name == "LocalEducationAgencyId"
                || s.ClaimsEndpoints.All(x => x.Name == "LocalEducationAgencyId") );
            
            _actualSchoolSegment = actualAuthorizationSegments.ExistingValuesAuthorizationSegments.SingleOrDefault(s =>
                s.Endpoints.Any(ep => ep.Name == "SchoolId"));
        }

        [Test]
        public void Should_create_1_claims_authorization_segment()
        {
            actualAuthorizationSegments.ClaimsAuthorizationSegments.Count().ShouldEqual(1);
        }

        [Test]
        public void Should_create_1_existing_values_authorization_segment()
        {
            actualAuthorizationSegments.ExistingValuesAuthorizationSegments.Count().ShouldEqual(1);
        }

        [Test]
        public void Should_return_a_LocalEducationAgency_segment()
        {
            _actualLocalEducationAgencySegment.ShouldNotBeNull();
        }

        [Test]
        public void Should_require_the_StaffUSI_to_be_associated_with_one_of_the_claims_LocalEducationAgencyIds()
        {
            var staffUniqueIdEndpointWithValue = _actualLocalEducationAgencySegment.TargetEndpoint
                as AuthorizationSegmentEndpointWithValue;

            staffUniqueIdEndpointWithValue.ShouldNotBeNull(
                "The staffUSI endpoint in the claims based authorization segment did not have a contextual value.");

            staffUniqueIdEndpointWithValue.Name.ShouldEqual("StaffUSI");
            staffUniqueIdEndpointWithValue.Value.ShouldEqual(suppliedContextData.StaffUSI);

            // Make sure the counts are the same
            _actualLocalEducationAgencySegment.ClaimsEndpoints.Count()
                               .ShouldEqual(_suppliedEdFiResourceClaimValue.EducationOrganizationIds.Count);

            // Make sure all the LEA Ids are present
            _actualLocalEducationAgencySegment.ClaimsEndpoints.Select(x => (int)x.Value)
                               .All(cv => _suppliedEdFiResourceClaimValue.EducationOrganizationIds.Contains(cv))
                               .ShouldBeTrue();

            _actualLocalEducationAgencySegment.ClaimsEndpoints.Select(x => x.Name)
                       .All(n => n == "LocalEducationAgencyId")
                       .ShouldBeTrue();
        }

        [Test]
        public void Should_target_the_StaffUniqueId_by_name_and_contextual_value_in_the_LocalEducationAgency_segment()
        {
            _actualLocalEducationAgencySegment.TargetEndpoint.Name.ShouldEqual("StaffUSI");
            var endpointWithValue =_actualLocalEducationAgencySegment.TargetEndpoint as AuthorizationSegmentEndpointWithValue;

            endpointWithValue.ShouldNotBeNull("The target endpoint of the claim authorization segment endpoint did not contain a value from the supplied context.");

            endpointWithValue.Value.ShouldEqual(suppliedContextData.StaffUSI);
        }

        [Test]
        public void Should_return_a_School_segment()
        {
            _actualSchoolSegment.ShouldNotBeNull();
        }

        [Test]
        public void Should_require_the_contextual_SchoolId_to_be_associated_with_the_contextual_StaffUSI()
        {
            // School should appear first (alphabetically)
            var schoolEndpoint = _actualSchoolSegment.Endpoints.SingleOrDefault(x => x.Name == "SchoolId")
                as AuthorizationSegmentEndpointWithValue;

            schoolEndpoint.ShouldNotBeNull(
                "No SchoolId endpoint with a contextual value was found in the existing values authorization segment.");

            schoolEndpoint.Name.ShouldEqual("SchoolId");
            schoolEndpoint.Value.ShouldEqual(suppliedContextData.SchoolId);

            var staffUsiEndpoint = _actualSchoolSegment.Endpoints.SingleOrDefault(x => x.Name == "StaffUSI")
                as AuthorizationSegmentEndpointWithValue;

            staffUsiEndpoint.ShouldNotBeNull(
                "No staffUSI endpoint with a contextual value was found in the existing values authorization segment.");

            staffUsiEndpoint.Name.ShouldEqual("StaffUSI");
            staffUsiEndpoint.Value.ShouldEqual(suppliedContextData.StaffUSI);
        }
    }
}