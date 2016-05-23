// ****************************************************************************
//  Copyright (C) 2014 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System;
using System.Collections.Generic;
using EdFi.Ods.Tests._Bases;
using EdFi.Ods.Utilities.LoadGeneration.ReferenceValueBuilders;
using EdFi.TestObjects;
using Rhino.Mocks;
using Should;

namespace EdFi.Ods.Utilities.LoadGeneration.Tests.ResourceReferenceBuilders
{
    public class SchoolYearTypeReferenceValueBuilderFixtures
    {
        public class SchoolYearXTypeReference { }

        public class SchoolYearTypeReference
        {
            public int? sChOoLyEaR { get; set; }    
        }

        public class SCHOOLYEARTYPEREFERENCE
        {
            public int? schoolYear { get; set; }    
        }

        public class When_considering_handling_a_SchoolYearTypeReference : TestFixtureBase
        {
            // Supplied values

            // Actual values
            private ValueBuildResult _actualCorrectTypeNameResult;
            private ValueBuildResult _actualDifferentTypeNameResult;
            private ValueBuildResult _actualCaseInsensitiveTypeNameResult;

            // External dependencies
            private ITestObjectFactory _testObjectFactory;

            protected override void Arrange()
            {
                // Set up mocked dependences and supplied values
                _testObjectFactory = Stub<ITestObjectFactory>();
                _testObjectFactory.Expect(x => x.Create(null, null, null, null)).Return(0);
            }

            protected override void Act()
            {
                // Perform the action to be tested
                var builder = new SchoolYearTypeReferenceValueBuilder
                {
                    Factory = _testObjectFactory,
                };
                
                var correctTargetTypeNameBuildContext = new BuildContext("xxx.xxx.xxx",
                    typeof(SchoolYearTypeReference), null, null, null, BuildMode.Create);

                _actualCorrectTypeNameResult = builder.TryBuild(correctTargetTypeNameBuildContext); 
                                
                var caseInsensitiveTargetTypeNameBuildContext = new BuildContext("xxx.xxx.xxx",
                    typeof(SCHOOLYEARTYPEREFERENCE), null, null, null, BuildMode.Create);

                _actualCaseInsensitiveTypeNameResult = builder.TryBuild(caseInsensitiveTargetTypeNameBuildContext); 
                
                var incorrectTargetTypeNameBuilderContext = new BuildContext("xxx.xxx.xxx",
                    typeof(SchoolYearXTypeReference), null, null, null, BuildMode.Create);

                _actualDifferentTypeNameResult = builder.TryBuild(incorrectTargetTypeNameBuilderContext); 
            }

            [Assert]
            public void Should_handle_requests_for_target_type_of_SchoolYearTypeReference()
            {
                // Assert the expected results
                _actualCorrectTypeNameResult.Handled.ShouldBeTrue();
            }

            [Assert]
            public void Should_handle_requests_for_target_type_of_SchoolYearTypeReference_regardless_of_casing()
            {
                // Assert the expected results
                _actualCaseInsensitiveTypeNameResult.Handled.ShouldBeTrue();
            }
            
            [Assert]
            public void Should_NOT_handle_requests_for_target_types_other_than_SchoolYearTypeReference()
            {
                // Assert the expected results
                _actualDifferentTypeNameResult.Handled.ShouldBeFalse();
            }
        }

        public class When_building_a_SchoolYearTypeReference : TestFixtureBase
        {
            // Supplied values

            // Actual values
            private ValueBuildResult _actualBuildResult;

            // External dependencies
            private ITestObjectFactory _testObjectFactory;

            protected override void Arrange()
            {
                // Need to call the factory to create a schoolYear value
                _testObjectFactory = Stub<ITestObjectFactory>();
                _testObjectFactory.Expect(x => x.Create(null, null, null, null))
                    .IgnoreArguments()
                    .Return(1234);
            }

            protected override void Act()
            {
                // Perform the action to be tested
                var builder = new SchoolYearTypeReferenceValueBuilder
                {
                    Factory = _testObjectFactory,
                };

                var buildContext = new BuildContext("xxx.xxx.xxx",
                    typeof(SchoolYearTypeReference), null, null, null, BuildMode.Create);

                _actualBuildResult = builder.TryBuild(buildContext); 
            }

            [Assert]
            public void Should_handle_the_request_to_set_the_value()
            {
                _actualBuildResult.ShouldSetValue.ShouldBeTrue();    
            }

            [Assert]
            public void Should_call_the_factory_to_obtain_a_value_for_SchoolYear()
            {
                // Assert the expected results
                _testObjectFactory.AssertWasCalled(x => 
                    x.Create(
                        Arg<string>.Is.Equal("SchoolYear"),
                        Arg<Type>.Matches(t => t == typeof(int)),
                        Arg<IDictionary<string, object>>.Is.Anything,
                        Arg<LinkedList<object>>.Is.Anything));
            }

            [Assert]
            public void Should_return_the_reference_setting_the_SchoolYear_property_on_the_target_type_using_a_case_insensitive_match_and_assigning_it_to_the_school_year_value_obtained_from_the_factory()
            {
                var reference = _actualBuildResult.Value as SchoolYearTypeReference;
                reference.sChOoLyEaR.ShouldEqual(1234);
            }
        }
    }
}