using System;
using System.Collections.Generic;
using EdFi.Ods.Tests._Bases;
using EdFi.Ods.Utilities.LoadGeneration.ReferenceValueBuilders;
using EdFi.TestObjects;
using Should;

namespace EdFi.Ods.Utilities.LoadGeneration.Tests.ResourceReferenceBuilders
{

    public class SelfRecursiveRelationshipPropertySkipperFixtures
    {
        public class ObjectiveAssessment
        {
            /* The unique identifier of the resource. */
            public string id { get; set; }

            /* A reference to the related Assessment resource. */
            public AssessmentReference assessmentReference { get; set; }

            /* A unique number or alphanumeric code assigned to a space, room, site, building, individual, organization, program, or institution by a school, school system, a state, or other agency or entity. */
            public string identificationCode { get; set; }

            /*  */
            public string parentIdentificationCode { get; set; }
        }

        public class AssessmentReference { }

        public class SomethingParentRelated
        {
            /* The unique identifier of the resource. */
            public string id { get; set; }

            /* A unique alpha-numeric code assigned to a parent. */
            public string parentUniqueId { get; set; }
        }

        public class When_building_a_property_whose_name_starts_with_Parent_on_a_containing_type_without_Parent_in_its_name : TestFixtureBase
        {
            // Supplied values

            // Actual values
            private ValueBuildResult _actualBuildResult;

            // External dependencies

            protected override void Arrange()
            {
                // Set up mocked dependences and supplied values
                
            }

            protected override void Act()
            {
                // Perform the action to be tested
                var builder = new SelfRecursiveRelationshipPropertySkipper();

                _actualBuildResult = builder.TryBuild(new BuildContext(
                    "xxxx.parentIdentificationCode",
                    typeof(string),
                    new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase),
                    typeof(ObjectiveAssessment),
                    new LinkedList<object>(new[] { new ObjectiveAssessment() }),
                    BuildMode.Create));
            }

            [Assert]
            public void Should_identify_the_property_as_matching_the_convention_for_self_recursive_relationships_and_skip_building_the_value()
            {
                _actualBuildResult.ShouldSkip.ShouldBeTrue();
            }
        }

        public class When_building_a_property_whose_name_DOES_NOT_start_with_Parent : TestFixtureBase
        {
            // Supplied values

            // Actual values
            private ValueBuildResult _actualBuildResult;

            // External dependencies

            protected override void Arrange()
            {
                // Set up mocked dependences and supplied values

            }

            protected override void Act()
            {
                // Perform the action to be tested
                var builder = new SelfRecursiveRelationshipPropertySkipper();

                _actualBuildResult = builder.TryBuild(new BuildContext(
                    "xxxx.notAparentIdentificationCode",
                    typeof(string),
                    new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase),
                    typeof(ObjectiveAssessment),
                    new LinkedList<object>(new[] { new ObjectiveAssessment() }),
                    BuildMode.Create));
            }

            [Assert]
            public void Should_identify_the_property_as_matching_the_convention_for_self_recursive_relationships_and_skip_building_the_value()
            {
                _actualBuildResult.Handled.ShouldBeFalse();
            }
        }

        public class When_building_a_property_whose_name_starts_with_Parent_on_a_containing_type_WITH_Parent_in_its_name : TestFixtureBase
        {
            // Supplied values

            // Actual values
            private ValueBuildResult _actualBuildResult;

            // External dependencies

            protected override void Arrange()
            {
                // Set up mocked dependences and supplied values

            }

            protected override void Act()
            {
                // Perform the action to be tested
                var builder = new SelfRecursiveRelationshipPropertySkipper();

                _actualBuildResult = builder.TryBuild(new BuildContext(
                    "xxxx.parentIdentificationCode",
                    typeof(string),
                    new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase),
                    typeof(SomethingParentRelated),
                    new LinkedList<object>(new[] { new SomethingParentRelated() }),
                    BuildMode.Create));
            }

            [Assert]
            public void Should_identify_the_property_as_NOT_matching_the_convention_for_self_recursive_relationships_and_not_handle_building_the_value()
            {
                _actualBuildResult.Handled.ShouldBeFalse();
            }
        }

        public class When_building_a_property_whose_name_starts_with_Parent_but_does_not_have_a_containing_type : TestFixtureBase
        {
            // Supplied values

            // Actual values
            private ValueBuildResult _actualBuildResult;

            // External dependencies

            protected override void Arrange()
            {
                // Set up mocked dependences and supplied values

            }

            protected override void Act()
            {
                // Perform the action to be tested
                var builder = new SelfRecursiveRelationshipPropertySkipper();

                _actualBuildResult = builder.TryBuild(new BuildContext(
                    "xxxx.parentIdentificationCode",
                    typeof(string),
                    new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase),
                    null,
                    new LinkedList<object>(),
                    BuildMode.Create));
            }

            [Assert]
            public void Should_identify_the_property_as_NOT_matching_the_convention_for_self_recursive_relationships_and_not_handle_building_the_value()
            {
                _actualBuildResult.Handled.ShouldBeFalse();
            }
        }
    }
}
