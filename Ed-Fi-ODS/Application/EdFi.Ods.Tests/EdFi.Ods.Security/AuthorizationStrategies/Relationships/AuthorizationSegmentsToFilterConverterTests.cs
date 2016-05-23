// ****************************************************************************
//  Copyright (C) 2015 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using EdFi.Common.Extensions;
using EdFi.Common.Security;
using EdFi.Common.Security.Authorization;
using EdFi.Common.Security.Claims;
using EdFi.Ods.Security.AuthorizationStrategies.Relationships;
using EdFi.Ods.Tests._Bases;
using EdFi.Ods.Tests._Extensions;
using NHibernate;
using NHibernate.Metadata;
using Rhino.Mocks;
using Rhino.Mocks.Interfaces;
using Should;

namespace EdFi.Ods.Tests.EdFi.Ods.Security.AuthorizationStrategies.Relationships
{
    // -------------------------------------------------------
    // NOTE: This is an exploratory style of unit testing.
    // -------------------------------------------------------
    public static class Feature_Converting_segments_to_filters_Extensions
    {
        public static IMethodOptions<IClassMetadata> that_given_entity_type(
            this ISessionFactory dependency,
            Type entity)
        {
            return dependency.Stub(x => x.GetClassMetadata(entity));
        }

        public static IClassMetadata that_returns_property_names(
            this IClassMetadata dependency,
            string[] propertyNames)
        {
            dependency.Stub(x => x.PropertyNames)
                      .Return(propertyNames);

            return dependency;
        }

        public static IEducationOrganizationCache that_returns_everything_as_an_education_organization_type_of(
            this IEducationOrganizationCache dependency,
            string educationOrganizationType)
        {
            dependency.Stub(x => x.GetEducationOrganizationIdentifiers(999))
                      .Return(new EducationOrganizationIdentifiers(999, educationOrganizationType, null, null, null, null));

            return dependency;
        }

        public static IMethodOptions<EducationOrganizationIdentifiers> that_given_education_organization_id(
            this IEducationOrganizationCache dependency,
            int identifier)
        {
            return dependency.Stub(x => x.GetEducationOrganizationIdentifiers(identifier));
        }
    }

    public class Feature_Converting_segments_to_filters
    {
        public class TestEntityType { }

        private static AuthorizationBuilder<RelationshipsAuthorizationContextData> Given_an_authorization_builder_with_claim_assigned_education_organization_ids(
            IEducationOrganizationCache cache, params int[] identifiers)
        {
            var builder = new AuthorizationBuilder<RelationshipsAuthorizationContextData>(
                new[]
                {
                    JsonClaimHelper.CreateClaim("someResource", new EdFiResourceClaimValue("action", new List<int>(identifiers)))
                },
                cache);

            return builder;
        }

        public class When_converting_to_filters_from_an_empty_segments_collection
            : ScenarioFor<AuthorizationSegmentsToFiltersConverter>
        {
            protected override void Act()
            {
                TestSubject.Convert(
                    typeof(TestEntityType), 
                    new AuthorizationSegmentCollection(
                        new ClaimsAuthorizationSegment[0], 
                        new ExistingValuesAuthorizationSegment[0]),
                        Supplied(new ParameterizedFilterBuilder()));
            }

            [Assert]
            public void Should_return_a_null_filters_dictionary()
            {
                Supplied<ParameterizedFilterBuilder>().Value.Count.ShouldEqual(0);
            }
        }

        public class When_converting_to_filters_from_a_single_segment_for_a_LocalEducationAgency_claim_to_SchoolId 
            : ScenarioFor<AuthorizationSegmentsToFiltersConverter>
        {
            private IDictionary<string, IDictionary<string, object>> _actualFilters;

            // Actual values
            protected override void Arrange()
            {
                Given<IClassMetadata>()
                    .that_returns_property_names(new string[0]);

                Given<ISessionFactory>()
                    .that_given_entity_type(Supplied("entityType", typeof(TestEntityType)))
                    .returns(The<IClassMetadata>());

                Given<IEducationOrganizationCache>()
                    .that_returns_everything_as_an_education_organization_type_of("LocalEducationAgency");

                var builder = Given_an_authorization_builder_with_claim_assigned_education_organization_ids(
                    The<IEducationOrganizationCache>(), 999);

                builder.ClaimsMustBeAssociatedWith(x => x.SchoolId);
                Supplied(builder.GetSegments());

                Supplied(new ParameterizedFilterBuilder());
            }

            protected override void Act()
            {
                // Execute code under test
                TestSubject.Convert(
                    Supplied<Type>("entityType"),
                    Supplied<AuthorizationSegmentCollection>(),
                    Supplied<ParameterizedFilterBuilder>());

                _actualFilters = Supplied<ParameterizedFilterBuilder>().Value;
            }

            [Assert]
            public void Should_create_a_single_filter_named_for_the_segment_endpoint_values()
            {
                _actualFilters.Count.ShouldEqual(1);
                _actualFilters.Keys.Single().ShouldEqual("LocalEducationAgencyIdToSchoolId");
            }

            [Assert]
            public void Should_assign_parameter_value_matching_the_claim_education_organization_type_and_value()
            {
                var parameterValuesByName = _actualFilters.Values.Single();

                parameterValuesByName.Count().ShouldEqual(1);
                parameterValuesByName["LocalEducationAgencyId"].ShouldEqual(new object[] {999});
            }
        }

        public class When_converting_to_filters_from_segments_that_have_multiple_education_organization_types
            : ScenarioFor<AuthorizationSegmentsToFiltersConverter>
        {
            // Supplied values

            // Actual values
            private IDictionary<string, IDictionary<string, object>> _actualFilters;

            // Dependencies

            protected override void Arrange()
            {
                Given<IClassMetadata>()
                    .that_returns_property_names(new string[0]);

                Given<ISessionFactory>()
                    .that_given_entity_type(Supplied("entityType", typeof(TestEntityType)))
                    .returns(The<IClassMetadata>());

                Given<IEducationOrganizationCache>()
                    .that_given_education_organization_id(999)
                    .returns(new EducationOrganizationIdentifiers(999, "LocalEducationAgency", null, null, null, null));

                Given<IEducationOrganizationCache>()
                    .that_given_education_organization_id(1000)
                    .returns(new EducationOrganizationIdentifiers(1000, "School", null, null, null, null));

                var builder = Given_an_authorization_builder_with_claim_assigned_education_organization_ids(
                    The<IEducationOrganizationCache>(), 999, 1000);

                builder.ClaimsMustBeAssociatedWith(x => x.SchoolId);
                Supplied(builder.GetSegments());

                Supplied(new ParameterizedFilterBuilder());
            }

            protected override void Act()
            {
                // Execute code under test
                TestSubject.Convert(
                    Supplied<Type>("entityType"),
                    Supplied<AuthorizationSegmentCollection>(),
                    Supplied<ParameterizedFilterBuilder>());

                _actualFilters = Supplied<ParameterizedFilterBuilder>().Value;
            }

            [Assert]
            public void Should_throw_a_NotSupportedException_stating_that_multiple_endpoint_types_were_found_and_are_not_supported()
            {
                ActualException.ShouldBeExceptionType<NotSupportedException>();
                ActualException.Message.ShouldContain("multiple");
            }
        }

        public class When_converting_to_filters_from_a_segment_that_have_the_same_endpoint_types
            : ScenarioFor<AuthorizationSegmentsToFiltersConverter>
        {
            private IDictionary<string, IDictionary<string, object>> _actualFilters;

            protected override void Arrange()
            {
                Given<IClassMetadata>()
                    .that_returns_property_names(new string[0]);

                Given<ISessionFactory>()
                    .that_given_entity_type(Supplied("entityType", typeof(TestEntityType)))
                    .returns(The<IClassMetadata>());

                Given<IEducationOrganizationCache>()
                    .that_given_education_organization_id(999)
                    .returns(new EducationOrganizationIdentifiers(999, "LocalEducationAgency", null, null, null, null));

                var builder = Given_an_authorization_builder_with_claim_assigned_education_organization_ids(
                    The<IEducationOrganizationCache>(), 999);

                builder.ClaimsMustBeAssociatedWith(x => x.LocalEducationAgencyId);
                Supplied(builder.GetSegments());

                Supplied(new ParameterizedFilterBuilder());
            }

            protected override void Act()
            {
                // Execute code under test
                TestSubject.Convert(
                    Supplied<Type>("entityType"),
                    Supplied<AuthorizationSegmentCollection>(),
                    Supplied<ParameterizedFilterBuilder>());

                _actualFilters = Supplied<ParameterizedFilterBuilder>().Value;
            }

            [Assert]
            public void Should_return_an_empty_filters_collection()
            {
                _actualFilters.Any().ShouldBeFalse();
            }
        }

        public class When_converting_to_filters_from_multiple_segments_with_the_same_target_types
            : ScenarioFor<AuthorizationSegmentsToFiltersConverter>
        {
            private IDictionary<string, IDictionary<string, object>> _actualFilters;

            protected override void Arrange()
            {
                Given<IClassMetadata>()
                    .that_returns_property_names(new string[0]);

                Given<ISessionFactory>()
                    .that_given_entity_type(Supplied("entityType", typeof(TestEntityType)))
                    .returns(The<IClassMetadata>());

                Given<IEducationOrganizationCache>()
                    .that_given_education_organization_id(999)
                    .returns(new EducationOrganizationIdentifiers(999, "LocalEducationAgency", null, null, null, null));

                var builder = Given_an_authorization_builder_with_claim_assigned_education_organization_ids(
                    The<IEducationOrganizationCache>(), 999);

                builder.ClaimsMustBeAssociatedWith(x => x.SchoolId);
                builder.ClaimsMustBeAssociatedWith(x => x.SchoolId);
                Supplied(builder.GetSegments());

                Supplied(new ParameterizedFilterBuilder());
            }

            protected override void Act()
            {
                // Execute code under test
                TestSubject.Convert(
                    Supplied<Type>("entityType"),
                    Supplied<AuthorizationSegmentCollection>(),
                    Supplied<ParameterizedFilterBuilder>());

                _actualFilters = Supplied<ParameterizedFilterBuilder>().Value;
            }

            [Assert]
            public void Should_not_include_redundant_values_in_the_filter_values()
            {
                var parametersByName = _actualFilters.Single().Value;

                parametersByName.Count.ShouldEqual(1);
                parametersByName["LocalEducationAgencyId"].ShouldEqual(new object[] {999});
            }
        }

        public class When_converting_to_filters_from_a_segment_with_a_target_endpoint_that_is_a_uniqueId_for_an_entity_WITHOUT_a_uniqueId_property
            : ScenarioFor<AuthorizationSegmentsToFiltersConverter>
        {
            private IDictionary<string, IDictionary<string, object>> _actualFilters;

            protected override void Arrange()
            {
                Given<IClassMetadata>()
                    .that_returns_property_names(new[] { "Property1", "Property2" });

                Given<ISessionFactory>()
                    .that_given_entity_type(Supplied("entityType", typeof(TestEntityType)))
                    .returns(The<IClassMetadata>());

                Given<IEducationOrganizationCache>()
                    .that_given_education_organization_id(999)
                    .returns(new EducationOrganizationIdentifiers(999, "LocalEducationAgency", null, null, null, null));

                var builder = Given_an_authorization_builder_with_claim_assigned_education_organization_ids(
                    The<IEducationOrganizationCache>(), 999);

                builder.ClaimsMustBeAssociatedWith(x => x.StaffUSI);
                Supplied(builder.GetSegments());

                Supplied(new ParameterizedFilterBuilder());
            }

            protected override void Act()
            {
                // Execute code under test
                TestSubject.Convert(
                    Supplied<Type>("entityType"),
                    Supplied<AuthorizationSegmentCollection>(),
                    Supplied<ParameterizedFilterBuilder>());

                _actualFilters = Supplied<ParameterizedFilterBuilder>().Value;
            }

            [Assert]
            public void Should_convert_UniqueId_properties_to_USI_in_the_constructed_view_name_when_the_property_DOES_NOT_exist_on_the_targeted_entity()
            {
                var filter = _actualFilters.Single();
                var viewName = filter.Key;

                viewName.ShouldNotContain("ToStaffUniqueId");
                viewName.ShouldContain("ToStaffUSI");
            }
        }
    }
}