// ****************************************************************************
//  Copyright (C) 2015 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System;
using System.Collections.Generic;
using System.Security.Claims;
using EdFi.Common.Extensions;
using EdFi.Common.Security;
using EdFi.Common.Security.Authorization;
using EdFi.Common.Security.Claims;
using EdFi.Ods.Security.Authorization;
using EdFi.Ods.Security.AuthorizationStrategies.Relationships;
using EdFi.Ods.Tests._Bases;
using EdFi.Ods.Tests._Extensions;
using QuickGraph;
using Rhino.Mocks;
using Rhino.Mocks.Interfaces;
using Should;

namespace EdFi.Ods.Tests.EdFi.Ods.Security.AuthorizationStrategies.Relationships
{
    // Dependency type aliases (for readability)
    using context_data_transformer = IConcreteEducationOrganizationIdAuthorizationContextDataTransformer<RelationshipsAuthorizationContextData>;
    using context_data_provider_factory = IRelationshipsAuthorizationContextDataProviderFactory<RelationshipsAuthorizationContextData>;
    using context_data_provider = IRelationshipsAuthorizationContextDataProvider<RelationshipsAuthorizationContextData>;
    using education_organization_cache = IEducationOrganizationCache;
    using education_organization_hierarchy_provider = IEducationOrganizationHierarchyProvider;
    using edorgs_and_people_strategy = RelationshipsWithEdOrgsAndPeopleAuthorizationStrategy<RelationshipsAuthorizationContextData>;
    using segments_to_filters_converter = IAuthorizationSegmentsToFiltersConverter;

    // -------------------------------------------------------
    // NOTE: This is an exploratory style of unit testing.
    // -------------------------------------------------------
    public static class FeatureExtensions
    {
        public static IRelationshipsAuthorizationContextDataProvider<RelationshipsAuthorizationContextData> that_returns_property_names(
            this IRelationshipsAuthorizationContextDataProvider<RelationshipsAuthorizationContextData> dependency,
            params string[] propertyNames)
        {
            dependency.Stub(x => x.GetAuthorizationContextPropertyNames())
                    .Return(propertyNames);

            return dependency;
        }

        public static IRelationshipsAuthorizationContextDataProviderFactory<RelationshipsAuthorizationContextData> that_always_returns(
            this IRelationshipsAuthorizationContextDataProviderFactory<RelationshipsAuthorizationContextData> dependency, 
            IRelationshipsAuthorizationContextDataProvider<RelationshipsAuthorizationContextData> provider)
        {
            dependency.Stub(x => x.GetProvider(Arg<Type>.Is.Anything))
                   .Return(provider);

            return dependency;
        }

        public static IEducationOrganizationCache that_always_returns_a_Local_Education_Agency_for(
            this IEducationOrganizationCache dependency,
            int educationOrganizationId)
        {
            dependency.Stub(x => x.GetEducationOrganizationIdentifiers(educationOrganizationId))
                      .Return(new EducationOrganizationIdentifiers(educationOrganizationId, "LocalEducationAgency", null, null, educationOrganizationId, null));

            return dependency;
        }

        public static IMethodOptions<EducationOrganizationIdentifiers> that_given_an_education_organization_id_of(
            this IEducationOrganizationCache dependency,
            int educationOrganizationId)
        {
            return dependency.Stub(x => x.GetEducationOrganizationIdentifiers(educationOrganizationId));
        }

        public static IEducationOrganizationHierarchyProvider that_always_returns_an_empty_graph(
            this IEducationOrganizationHierarchyProvider dependency)
        {
            dependency.Stub(x => x.GetEducationOrganizationHierarchy())
                      .Return(new AdjacencyGraph<string, Edge<string>>());

            return dependency;
        }

        public static IMethodOptions<RelationshipsAuthorizationContextData> that_given_entity(
            this context_data_provider dependency,
            object entity)
        {
            return dependency.Stub(x => x.GetContextData(entity));
        }

        public static context_data_transformer that_just_performs_a_passthrough_on_the_context_data(
            this context_data_transformer dependency,
            RelationshipsAuthorizationContextData contextData)
        {
            dependency.Stub(x => x.GetConcreteAuthorizationContextData(contextData))
                .Return(contextData);
            
            return dependency;
        }
    }

    // -------------------------------------------------------
    // NOTE: This is an exploratory style of unit testing.
    // -------------------------------------------------------
    public class Feature_Authorizing_a_request
    {
        private class passthrough_context_data_transformer : context_data_transformer
        {
            public RelationshipsAuthorizationContextData GetConcreteAuthorizationContextData(
                RelationshipsAuthorizationContextData authorizationContextData)
            {
                return authorizationContextData;
            }
        }

        private static Claim Given_a_claim_for_an_arbitrary_resource_for_EducationOrganization_identifiers(params int[] educationOrganizationIds)
        {
            return JsonClaimHelper.CreateClaim("xyz", new EdFiResourceClaimValue("read", new List<int>(educationOrganizationIds)));
        }

        private static EdFiAuthorizationContext Given_an_authorization_context_with_entity_data(object entity)
        {
            return new EdFiAuthorizationContext(new ClaimsPrincipal(), "resource", "action", entity);
        }

        public class When_authorizing_a_multiple_item_request 
            : ScenarioFor<edorgs_and_people_strategy>
        {
            protected override void Arrange()
            {
                Given<context_data_provider>()
                    .that_returns_property_names(
                        Supplied("propertyNames", new[] { "LocalEducationAgencyId", "StaffUSI" } ));

                Given<context_data_provider_factory>()
                    .that_always_returns(The<context_data_provider>());

                Given<education_organization_cache>()
                    .that_always_returns_a_Local_Education_Agency_for(Supplied("LocalEducationAgencyId", 999));

                Supplied("entity", new object());

                Supplied(Given_an_authorization_context_with_entity_data(Supplied("entity")));

                Supplied(Given_a_claim_for_an_arbitrary_resource_for_EducationOrganization_identifiers(
                            Supplied<int>("LocalEducationAgencyId")));
            }

            protected override void Act()
            {
                TestSubject.ApplyAuthorizationFilters(
                    new[] {Supplied<Claim>()}, 
                    Supplied<EdFiAuthorizationContext>(), 
                    Supplied(new ParameterizedFilterBuilder()));
            }

            [Assert]
            public void Should_call_segments_converter_to_convert_segments_built_based_on_the_supplied_claims_with_the_supplied_entity_type_and_filter_builder()
            {
                The<segments_to_filters_converter>()
                    .AssertWasCalled(x =>
                    {
                        int expectedSegmentLength = Supplied<string[]>("propertyNames").Length;

                        x.Convert(
                            Arg<Type>.Is.Same(Supplied("entity").GetType()),
                            Arg<AuthorizationSegmentCollection>.Matches(asc => asc.ClaimsAuthorizationSegments.Count == expectedSegmentLength),
                            Arg<ParameterizedFilterBuilder>.Is.Same(Supplied<ParameterizedFilterBuilder>()));
                    });
            }
        }

        public class When_authorizing_a_multiple_item_request_with_multiple_edorg_types
            : ScenarioFor<edorgs_and_people_strategy>
        {
            protected override void Arrange()
            {
                Given<context_data_provider>()
                    .that_returns_property_names(
                        Supplied("propertyNames", new[] { "SchoolId", "StaffUSI" }));

                Given<context_data_provider_factory>()
                    .that_always_returns(The<context_data_provider>());

                Supplied("LocalEducationAgencyId", 999);
                Supplied("SchoolId", 888);

                Given<education_organization_cache>()
                    .that_given_an_education_organization_id_of(Supplied<int>("LocalEducationAgencyId"))
                    .returns(new EducationOrganizationIdentifiers(Supplied<int>("LocalEducationAgencyId"), "LocalEducationAgency", null, null, null, null));

                Given<education_organization_cache>()
                    .that_given_an_education_organization_id_of(Supplied<int>("SchoolId"))
                    .returns(new EducationOrganizationIdentifiers(Supplied<int>("SchoolId"), "School", null, null, null, null));

                Supplied("entity", new object());

                Supplied(Given_an_authorization_context_with_entity_data(Supplied("entity")));

                Supplied(Given_a_claim_for_an_arbitrary_resource_for_EducationOrganization_identifiers(
                            Supplied<int>("LocalEducationAgencyId"), 
                            Supplied<int>("SchoolId")));
            }

            protected override void Act()
            {
                TestSubject.ApplyAuthorizationFilters(
                    new[] {Supplied<Claim>()},
                    Supplied<EdFiAuthorizationContext>(),
                    Supplied(new ParameterizedFilterBuilder()));
            }

            [Assert]
            public void Should_throw_a_NotSupportedException_with_a_message_related_to_multiple_education_organization_types()
            {
                ActualException.ShouldBeExceptionType<NotSupportedException>();
                ActualException.Message.ShouldContain("multiple types");
            }
        }

        public class When_authorizing_a_single_item_request_with_correct_claims_for_request 
            : ScenarioFor<RelationshipsWithEdOrgsAndPeopleAuthorizationStrategy<RelationshipsAuthorizationContextData>>
        {
            private class TestEntity
            {
                public int LocalEducationAgencyId { get; set; }
                public int StaffUSI{ get; set; }

                public TestEntity(int localEducationAgencyId, int staffUSI)
                {
                    LocalEducationAgencyId = localEducationAgencyId;
                    StaffUSI = staffUSI;
                }
            }

            protected override void Arrange()
            {
                Supplied("entity",
                    new TestEntity(
                        Supplied("LocalEducationAgencyId", 999),
                        Supplied("StaffUSI", 123))); 
                
                Supplied(new RelationshipsAuthorizationContextData
                {
                    LocalEducationAgencyId = Supplied<int>("LocalEducationAgencyId"),
                    StaffUSI = Supplied<int>("StaffUSI"),
                });

                Given<education_organization_cache>()
                    .that_always_returns_a_Local_Education_Agency_for(Supplied<int>("LocalEducationAgencyId"));

                Given<education_organization_hierarchy_provider>()
                    .that_always_returns_an_empty_graph();

                Given<context_data_transformer>(
                    new passthrough_context_data_transformer());

                Given<context_data_provider>()
                    .that_returns_property_names(
                        Supplied("propertyNames", new[] {"LocalEducationAgencyId", "StaffUSI"}))
                    .that_given_entity(Supplied("entity"))
                    .returns(Supplied<RelationshipsAuthorizationContextData>());

                Given<context_data_provider_factory>()
                    .that_always_returns(The<context_data_provider>());

                Supplied(
                    Given_a_claim_for_an_arbitrary_resource_for_EducationOrganization_identifiers(
                        Supplied<int>("LocalEducationAgencyId")));
            }

            protected override void Act()
            {
                TestSubject.AuthorizeSingleItem(
                    new[] {Supplied<Claim>()}, 
                    Given_an_authorization_context_with_entity_data(Supplied("entity")));
            }

            [Assert]
            public void Should_not_throw_an_exception()
            {
                ActualException.ShouldBeNull();
            }
        }

        public class When_authorizing_a_single_item_request_with_all_claim_values_needed_for_local_request_authorization
            : ScenarioFor<RelationshipsWithEdOrgsAndPeopleAuthorizationStrategy<RelationshipsAuthorizationContextData>>
        {
            private class TestEntity
            {
                public int LocalEducationAgencyId { get; set; }

                public TestEntity(int localEducationAgencyId)
                {
                    LocalEducationAgencyId = localEducationAgencyId;
                }
            }

            protected override void Arrange()
            {
                Supplied("entity",
                    new TestEntity(
                        Supplied("LocalEducationAgencyId", 999)));
                
                Supplied(new RelationshipsAuthorizationContextData
                {
                    LocalEducationAgencyId = Supplied<int>("LocalEducationAgencyId"),
                });

                Given<education_organization_cache>()
                    .that_always_returns_a_Local_Education_Agency_for(Supplied<int>("LocalEducationAgencyId"));

                Given<education_organization_hierarchy_provider>()
                    .that_always_returns_an_empty_graph();

                Given<context_data_transformer>(
                    new passthrough_context_data_transformer());

                Given<context_data_provider>()
                    .that_returns_property_names(
                        Supplied("propertyNames", new[] {"LocalEducationAgencyId"}))
                    .that_given_entity(Supplied("entity"))
                    .returns(Supplied<RelationshipsAuthorizationContextData>());

                Given<context_data_provider_factory>()
                    .that_always_returns(The<context_data_provider>());

                Supplied(
                    Given_a_claim_for_an_arbitrary_resource_for_EducationOrganization_identifiers(
                        Supplied<int>("LocalEducationAgencyId")));
            }

            protected override void Act()
            {
                TestSubject.AuthorizeSingleItem(
                    new[] {Supplied<Claim>()}, 
                    Given_an_authorization_context_with_entity_data(Supplied("entity")));
            }

            [Assert]
            public void Should_not_call_through_to_verify_the_segments()
            {
                The<IAuthorizationSegmentsVerifier>()
                    .AssertWasNotCalled(x => x.Verify(Arg<AuthorizationSegmentCollection>.Is.Anything));
            }
        }

        public class When_authorizing_a_single_item_request_with_claim_needed_for_local_request_authorization_but_EdOrg_values_dont_match
            : ScenarioFor<RelationshipsWithEdOrgsAndPeopleAuthorizationStrategy<RelationshipsAuthorizationContextData>>
        {
            private class TestEntity
            {
                public int LocalEducationAgencyId { get; set; }

                public TestEntity(int localEducationAgencyId)
                {
                    LocalEducationAgencyId = localEducationAgencyId;
                }
            }

            protected override void Arrange()
            {
                Supplied("entity",
                    new TestEntity(
                        Supplied("LocalEducationAgencyId", 999)));

                Supplied(new RelationshipsAuthorizationContextData
                {
                    LocalEducationAgencyId = Supplied<int>("LocalEducationAgencyId"),
                });

                Given<education_organization_cache>()
                    .that_always_returns_a_Local_Education_Agency_for(Supplied<int>("LocalEducationAgencyId"))
                    .that_always_returns_a_Local_Education_Agency_for(777);

                Given<education_organization_hierarchy_provider>()
                    .that_always_returns_an_empty_graph();

                Given<context_data_transformer>(
                    new passthrough_context_data_transformer());

                Given<context_data_provider>()
                    .that_returns_property_names(
                        Supplied("propertyNames", new[] { "LocalEducationAgencyId" }))
                    .that_given_entity(Supplied("entity"))
                    .returns(Supplied<RelationshipsAuthorizationContextData>());

                Given<context_data_provider_factory>()
                    .that_always_returns(The<context_data_provider>());

                Supplied(
                    Given_a_claim_for_an_arbitrary_resource_for_EducationOrganization_identifiers(777));
            }

            protected override void Act()
            {
                TestSubject.AuthorizeSingleItem(
                    new[] {Supplied<Claim>()},
                    Given_an_authorization_context_with_entity_data(Supplied("entity")));
            }

            [Assert]
            public void Should_throw_an_EdFiSecurityException_indicating_the_claims_did_not_provide_authorization_for_the_request()
            {
                ActualException.ShouldBeExceptionType<EdFiSecurityException>();
                ActualException.MessageShouldContain("Access to the requested");
            }
        }
    }
}