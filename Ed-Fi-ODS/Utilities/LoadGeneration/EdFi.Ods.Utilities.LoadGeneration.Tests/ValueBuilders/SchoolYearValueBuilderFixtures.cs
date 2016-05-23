// ****************************************************************************
//  Copyright (C) 2014 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System;
using System.Text;
using EdFi.Ods.Tests._Bases;
using EdFi.Ods.Utilities.LoadGeneration.ValueBuilders;
using EdFi.TestObjects;
using Rhino.Mocks;
using Should;

namespace EdFi.Ods.Utilities.LoadGeneration.Tests.ValueBuilders
{
    public class SchoolYearValueBuilderFixtures
    {
        public class SchoolYearType
        {
            /* Key for School */
            public int? schoolYear { get; set; }

            /* Code for SchoolYear type. */
            public bool? currentSchoolYear { get; set; }
        }

        public class When_considering_handling_building_a_school_year_value : TestFixtureBase
        {
            // Supplied values

            // Actual values
            private ValueBuildResult _actualResult;
            private ValueBuildResult _actualCaseInsensitiveResult;
            private ValueBuildResult _actualNonIntegerResult;

            // External dependencies
            private IApiSdkFacade _apiSdkFacade;
            private IApiSdkReflectionProvider _apiSdkReflectionProvider;

            protected override void Arrange()
            {
                // Set up mocked dependences and supplied values
                Type ignored;
                _apiSdkReflectionProvider = Stub<IApiSdkReflectionProvider>();
                _apiSdkReflectionProvider.Expect(x => x.TryGetModelType(null as string, out ignored))
                   .IgnoreArguments()
                   .OutRef(typeof(SchoolYearType))
                   .Return(true);

                _apiSdkFacade = Stub<IApiSdkFacade>();
                _apiSdkFacade.Expect(x => x.GetAll(null))
                    .IgnoreArguments()
                    .Repeat.Once() // Only call the API once!
                    .Return(new[]
                    {
                        new SchoolYearType {schoolYear = 1, currentSchoolYear = true},
                    });
            }

            protected override void Act()
            {
                // Perform the action to be tested
                var builder = new SchoolYearValueBuilder(_apiSdkFacade, _apiSdkReflectionProvider);
                var buildContext = new BuildContext("xxx.xxx.SchoolYear", typeof(int), null, null, null, BuildMode.Create);
                _actualResult = builder.TryBuild(buildContext);
                
                var buildContextCaseInsensitive = new BuildContext("xxx.xxx.sChOoLyEaR", typeof(int), null, null, null, BuildMode.Create);
                _actualCaseInsensitiveResult = builder.TryBuild(buildContextCaseInsensitive);

                var buildNonIntegerContext = new BuildContext("xxx.xxx.SchoolYear", typeof(string), null, null, null, BuildMode.Create);
                _actualNonIntegerResult = builder.TryBuild(buildNonIntegerContext);
            }

            [Assert]
            public void Should_handle_requests_for_properties_ending_in_SchoolYear()
            {
                // Assert the expected results
                _actualResult.Handled.ShouldBeTrue();
            }

            [Assert]
            public void Should_handle_requests_for_properties_ending_in_SchoolYear_regardless_of_casing()
            {
                // Assert the expected results
                _actualCaseInsensitiveResult.Handled.ShouldBeTrue();
            }

            [Assert]
            public void Should_not_handle_requests_for_non_integer_types()
            {
                _actualNonIntegerResult.Handled.ShouldBeFalse();
            }
        }

        public class When_building_a_value_for_SchoolYear : TestFixtureBase
        {
            // Supplied values

            // Actual values
            private ValueBuildResult _actualFirstResult;
            private ValueBuildResult _actualSecondResult;

            // External dependencies
            private IApiSdkFacade _apiSdkFacade;
            private IApiSdkReflectionProvider _apiSdkReflectionProvider;

            protected override void Arrange()
            {
                Type ignored;

                // Set up mocked dependences and supplied values
                _apiSdkReflectionProvider = Stub<IApiSdkReflectionProvider>();
                _apiSdkReflectionProvider.Expect(x => x.TryGetModelType(null as string, out ignored))
                    .IgnoreArguments()
                    .OutRef(typeof(SchoolYearType))
                    .Return(true);

                _apiSdkFacade = Stub<IApiSdkFacade>();
                _apiSdkFacade.Expect(x => x.GetAll(null))
                    .IgnoreArguments()
                    .Repeat.Once() // Only call the API once!
                    .Return(new[]
                    {
                        // Set up results with multiple years marked as "current"
                        new SchoolYearType {schoolYear = 1999, currentSchoolYear = false},
                        new SchoolYearType {schoolYear = 2002, currentSchoolYear = true}, // <--- This is the most recent year marked as current
                        new SchoolYearType {schoolYear = 2001, currentSchoolYear = false},
                        new SchoolYearType {schoolYear = 2000, currentSchoolYear = true},
                        new SchoolYearType {schoolYear = 2003, currentSchoolYear = false},
                    });
            }

            protected override void Act()
            {
                // Perform the action to be tested
                var builder = new SchoolYearValueBuilder(_apiSdkFacade, _apiSdkReflectionProvider);
                
                var buildContext = new BuildContext("xxx.xxx.SchoolYear", typeof(int), null, null, null, BuildMode.Create);
                try
                {
                    _actualFirstResult = builder.TryBuild(buildContext);
                    _actualSecondResult = builder.TryBuild(buildContext);
                }
                catch (Exception) { } // Don't blow up here
            }

            [Assert]
            public void Should_obtain_the_SchoolYearType_model_type_from_the_SDK()
            {
                // Assert the expected results
                _apiSdkReflectionProvider.AssertWasCalled(x => 
                    x.TryGetModelType(
                        Arg<string>.Is.Equal("SchoolYearType"),
                        out Arg<Type>.Out(null).Dummy));
            }

            [Assert]
            public void Should_call_the_API_to_obtain_all_the_school_years()
            {
                _apiSdkFacade.AssertWasCalled(x => x.GetAll(typeof(SchoolYearType)));
            }

            [Assert]
            public void Should_return_the_most_recent_school_year_marked_as_current()
            {
                _actualFirstResult.Value.ShouldEqual(2002);
                _actualSecondResult.Value.ShouldEqual(2002);
            }
        }

        public class When_building_a_value_for_SchoolYear_where_none_are_marked_as_current : TestFixtureBase
        {
            // Supplied values

            // Actual values
            private ValueBuildResult _actualFirstResult;
            private ValueBuildResult _actualSecondResult;

            // External dependencies
            private IApiSdkFacade _apiSdkFacade;
            private IApiSdkReflectionProvider _apiSdkReflectionProvider;

            protected override void Arrange()
            {
                Type ignored;

                // Set up mocked dependences and supplied values
                _apiSdkReflectionProvider = Stub<IApiSdkReflectionProvider>();
                _apiSdkReflectionProvider.Expect(x => x.TryGetModelType(null as string, out ignored))
                    .IgnoreArguments()
                    .OutRef(typeof(SchoolYearType))
                    .Return(true);

                _apiSdkFacade = Stub<IApiSdkFacade>();
                _apiSdkFacade.Expect(x => x.GetAll(null))
                    .IgnoreArguments()
                    .Repeat.Once() // Only call the API once!
                    .Return(new[]
                    {
                        // Set up results with multiple years marked as "current"
                        new SchoolYearType {schoolYear = 1999, currentSchoolYear = false},
                        new SchoolYearType {schoolYear = 2002, currentSchoolYear = false},
                        new SchoolYearType {schoolYear = 2001, currentSchoolYear = false},
                        new SchoolYearType {schoolYear = 2003, currentSchoolYear = false},  // <--- This is the most recent year
                        new SchoolYearType {schoolYear = 2000, currentSchoolYear = false},
                    });
            }

            protected override void Act()
            {
                // Perform the action to be tested
                var builder = new SchoolYearValueBuilder(_apiSdkFacade, _apiSdkReflectionProvider);

                var buildContext = new BuildContext("xxx.xxx.SchoolYear", typeof(int), null, null, null, BuildMode.Create);
                try
                {
                    _actualFirstResult = builder.TryBuild(buildContext);
                    _actualSecondResult = builder.TryBuild(buildContext);
                }
                catch (Exception) { } // Don't blow up here
            }

            [Assert]
            public void Should_obtain_the_SchoolYearType_model_type_from_the_SDK()
            {
                // Assert the expected results
                _apiSdkReflectionProvider.AssertWasCalled(x =>
                    x.TryGetModelType(
                        Arg<string>.Is.Equal("SchoolYearType"),
                        out Arg<Type>.Out(null).Dummy));
            }

            [Assert]
            public void Should_call_the_API_to_obtain_all_the_school_years()
            {
                _apiSdkFacade.AssertWasCalled(x => x.GetAll(typeof(SchoolYearType)));
            }

            [Assert]
            public void Should_return_the_most_recent_school_year()
            {
                _actualFirstResult.Value.ShouldEqual(2003);
                _actualSecondResult.Value.ShouldEqual(2003);
            }
        }
    }
}