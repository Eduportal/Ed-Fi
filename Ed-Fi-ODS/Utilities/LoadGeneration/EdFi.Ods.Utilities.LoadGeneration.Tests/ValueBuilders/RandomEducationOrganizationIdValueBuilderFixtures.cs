// ****************************************************************************
//  Copyright (C) 2014 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using EdFi.Ods.Tests._Bases;
using EdFi.Ods.Utilities.LoadGeneration.Infrastructure;
using EdFi.Ods.Utilities.LoadGeneration.ValueBuilders;
using EdFi.TestObjects;
using NUnit.Framework;
using Rhino.Mocks;
using Should;

namespace EdFi.Ods.Utilities.LoadGeneration.Tests.ValueBuilders
{
    public class RandomEducationOrganizationIdValueBuilderFixtures
    {
        public class SomeClass { }

        public class When_considering_building_a_value_for_a_education_organization_id : TestFixtureBase
        {
            // Supplied values

            // Actual values
            private ValueBuildResult _actualBuildResultWithContainingType;
            private ValueBuildResult _actualBuildResultWithoutContainingType;
            private ValueBuildResult _actualBuildResultCaseSensitivity;
            private ValueBuildResult _actualBuildResultWithNonIntTargetType;
            private ValueBuildResult _actualBuildResultNotEndingWithEducationOrganizationId;

            // External dependencies
            private IEducationOrganizationIdentifiersProvider _identifiersProvider;
            private IRandom _random;

            protected override void Arrange()
            {
                // Set up mocked dependences and supplied values
                _identifiersProvider = Stub<IEducationOrganizationIdentifiersProvider>();
                _identifiersProvider.Expect(x => x.GetEducationOrganizationIdentifiers())
                    .Return(new[] { new EducationOrganizationIdentifiers() { EducationOrganizationType = "School"} }); // Note: Added school when actual implemented behavior was modified out of expedience for authorization during load generation to select only Schools for EdOrgs

                _random = Stub<IRandom>();
            }

            protected override void Act()
            {
                // Perform the action to be tested
                var builder = new RandomEducationOrganizationIdValueBuilder(_identifiersProvider, _random);

                var buildContextWithContainingType = new BuildContext("xxx.xxx.educationOrganizationId", typeof(int),
                    null, typeof(SomeClass), null, BuildMode.Create);
                _actualBuildResultWithContainingType = builder.TryBuild(buildContextWithContainingType);

                var buildContextWithoutContainingType = new BuildContext("xxx.xxx.educationOrganizationId", typeof(int),
                    null, null, null, BuildMode.Create);
                _actualBuildResultWithoutContainingType = builder.TryBuild(buildContextWithoutContainingType);

                var buildContextWithCaseSensitivity = new BuildContext("xxx.xxx.eDuCaTiOnOrganizationId", typeof(int),
                    null, null, null, BuildMode.Create);
                _actualBuildResultCaseSensitivity = builder.TryBuild(buildContextWithCaseSensitivity);

                var buildContextNotEndingWithEducationOrganizationId = new BuildContext("xxx.xxx.educationOrganizationIdX", typeof(int),
                    null, null, null, BuildMode.Create);
                _actualBuildResultNotEndingWithEducationOrganizationId = builder.TryBuild(buildContextNotEndingWithEducationOrganizationId);

                var buildContextWithNonIntTargetType = new BuildContext("xxx.xxx.educationOrganizationId", typeof(string),
                    null, null, null, BuildMode.Create);
                _actualBuildResultWithNonIntTargetType = builder.TryBuild(buildContextWithNonIntTargetType);

            }

            [Assert]
            public void Should_handle_and_build_values_with_containing_types()
            {
                _actualBuildResultWithContainingType.Handled.ShouldBeTrue();
                _actualBuildResultWithContainingType.ShouldSetValue.ShouldBeTrue();
            }

            [Assert]
            public void Should_handle_and_build_values_without_containing_types()
            {
                _actualBuildResultWithoutContainingType.Handled.ShouldBeTrue();
                _actualBuildResultWithoutContainingType.ShouldSetValue.ShouldBeTrue();
            }

            [Assert]
            public void Should_handle_and_build_values_regardless_of_casing()
            {
                _actualBuildResultCaseSensitivity.Handled.ShouldBeTrue();
                _actualBuildResultCaseSensitivity.ShouldSetValue.ShouldBeTrue();
            }

            [Assert]
            public void Should_not_handle_values_NOT_ending_with_educationOrganizationId()
            {
                _actualBuildResultNotEndingWithEducationOrganizationId.Handled.ShouldBeFalse();
            }

            [Assert]
            public void Should_not_handle_values_that_are_NOT_integers()
            {
                _actualBuildResultWithNonIntTargetType.Handled.ShouldBeFalse();
            }
        }

        public class When_building_a_education_organization_id_value : TestFixtureBase
        {
            // Supplied values

            // Actual values
            private ValueBuildResult _actualBuildResult;

            // External dependencies
            private IEducationOrganizationIdentifiersProvider _identifiersProvider;
            private IRandom _random;

            protected override void Arrange()
            {
                _identifiersProvider = Stub<IEducationOrganizationIdentifiersProvider>();
                _identifiersProvider.Expect(x => x.GetEducationOrganizationIdentifiers())
                    .Return(new[]
                    {
                        CreateIdentifiers("LocalEducationAgency", 10),
                        CreateIdentifiers("School", 15),
                        CreateIdentifiers("LocalEducationAgency", 20),
                        CreateIdentifiers("School", 25),
                        CreateIdentifiers("LocalEducationAgency", 30),
                    });

                // Generate a supplied "random" number
                _random = Stub<IRandom>();
                _random.Expect(x => x.Next(-1, -1))
                    .IgnoreArguments()
                    .Return(1);
            }

            private static EducationOrganizationIdentifiers CreateIdentifiers(string educationOrganizationType, int educationOrganizationId)
            {
                return new EducationOrganizationIdentifiers
                {
                    EducationOrganizationId = educationOrganizationId,
                    EducationOrganizationType = educationOrganizationType
                };
            }

            protected override void Act()
            {
                // Perform the action to be tested
                var builder = new RandomEducationOrganizationIdValueBuilder(_identifiersProvider, _random);
                var buildContext = new BuildContext("EducationOrganizationId", typeof(int), null, null, null, BuildMode.Create);

                _actualBuildResult = builder.TryBuild(buildContext);
            }

            [Assert]
            public void Should_handle_the_request_and_set_the_value()
            {
                _actualBuildResult.Handled.ShouldBeTrue();
                _actualBuildResult.ShouldSetValue.ShouldBeTrue();
            }

            [Assert, Ignore("Modified actual implemented behavior to limit selections to Schools for expedience in resolving authorization issues during load generation.")]
            public void Should_provide_range_to_random_number_generator_covering_all_identifiers()
            {
                _random.AssertWasCalled(x => x.Next(
                    Arg<int>.Is.Equal(0), 
                    Arg<int>.Is.Equal(5))); // Number of total Ed Orgs
            }

            [Assert, Ignore("Modified actual implemented behavior to limit selections to Schools for expedience in resolving authorization issues during load generation.")]
            public void Should_randomly_select_a_value_from_all_the_available_identifiers_obtained_from_the_education_organization_identifiers()
            {
                // Get the second entry **overall** based on "random" number
                _actualBuildResult.Value.ShouldEqual(15);
            }
        }
    }
}