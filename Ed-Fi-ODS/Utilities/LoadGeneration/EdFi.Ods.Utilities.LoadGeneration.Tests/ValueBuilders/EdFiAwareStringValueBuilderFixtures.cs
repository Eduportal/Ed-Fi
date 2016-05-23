// ****************************************************************************
//  Copyright (C) 2014 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System;
using System.Collections.Generic;
using EdFi.Ods.Tests._Bases;
using EdFi.Ods.Utilities.LoadGeneration.ValueBuilders;
using EdFi.TestObjects;
using Should;

namespace EdFi.Ods.Utilities.LoadGeneration.Tests.ValueBuilders
{
    public class EdFiAwareStringValueBuilderFixtures
    {
        public class SomeClass { }

        public class EducationContent { }

        public class When_building_a_string_for_EdFi_API_load_generation : TestFixtureBase
        {
            protected virtual BuildContext CreateBuildContext<TContaining>(string propertyName)
                where TContaining : new()
            {
                return new BuildContext("xxxx.yyyy."+ typeof(TContaining).Name +"." + propertyName,
                    typeof(string), new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase), typeof(TContaining),
                    new LinkedList<object>(new object[] {new TContaining()}),
                    BuildMode.Create);
            }
        }

        public class When_building_a_string_for_a_property_not_explicitly_identified_as_having_a_maximum_length_less_than_20 : When_building_a_string_for_EdFi_API_load_generation
        {
            // Supplied values

            // Actual values
            private ValueBuildResult _actualBuildResult;

            // External dependencies

            protected override void Act()
            {
                // Perform the action to be tested
                var builder = new EdFiAwareStringValueBuilder();
                _actualBuildResult = builder.TryBuild(
                    CreateBuildContext<SomeClass>("SomeProperty"));
            }

            [Assert]
            public void Should_build_a_20_character_long_string()
            {
                string value = (string) _actualBuildResult.Value;

                value.Length.ShouldEqual(20);
            }
        }

        public class When_building_a_string_for_a_property_that_is_explicitly_identified_as_having_a_maximum_length_of_10 : When_building_a_string_for_EdFi_API_load_generation
        {
            // Supplied values

            // Actual values
            private ValueBuildResult _actualBuildResult;

            // External dependencies

            protected override void Act()
            {
                // Perform the action to be tested
                var builder = new EdFiAwareStringValueBuilder();
                _actualBuildResult = builder.TryBuild(
                    CreateBuildContext<EducationContent>("Version"));
            }

            [Assert]
            public void Should_build_a_10_character_long_string()
            {
                string value = (string)_actualBuildResult.Value;

                value.Length.ShouldEqual(10);
            }
        }

        public class When_building_a_string_for_a_property_that_is_explicitly_identified_as_having_a_maximum_length_of_10_but_the_property_name_casing_doesnt_match : When_building_a_string_for_EdFi_API_load_generation
        {
            // Supplied values

            // Actual values
            private ValueBuildResult _actualBuildResult;

            // External dependencies

            protected override void Act()
            {
                // Perform the action to be tested
                var builder = new EdFiAwareStringValueBuilder();
                _actualBuildResult = builder.TryBuild(
                    CreateBuildContext<EducationContent>("vErSiOn"));
            }

            [Assert]
            public void Should_build_a_10_character_long_string()
            {
                string value = (string)_actualBuildResult.Value;

                value.Length.ShouldEqual(10);
            }
        }
    }
}