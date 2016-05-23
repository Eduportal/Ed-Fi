// ****************************************************************************
//  Copyright (C) 2014 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System;
using EdFi.Ods.Tests._Bases;
using EdFi.Ods.Utilities.LoadGeneration.ReferenceValueBuilders;
using EdFi.TestObjects;
using Rhino.Mocks;
using Should;

namespace EdFi.Ods.Utilities.LoadGeneration.Tests.ResourceReferenceBuilders
{
    public class SectionAttendanceTakenEvent_References_ValueBuilder_Fixtures
    {
        #region Supported SDK model class structures

        public class SectionAttendanceTakenEvent
        {
            /* A reference to the related CalendarDate resource. */
            public CalendarDateReference calendarDateReference { get; set; }

            /* A reference to the related Section resource. */
            public SectionReference sectionReference { get; set; }
        }

        public class CalendarDateReference
        {
            /* EducationOrganization Identity Column */
            public int? educationOrganizationId { get; set; }

            /* Month, day, and year of the first day of the grading period.     */
            public DateTime? date { get; set; }

        }

        public class CalendarDate {}

        public class SectionReference
        {
            /* School Identity Column */
            public int? schoolId { get; set; }

        }

        public class Section { }

        #endregion

        public class When_building_a_CalendarDateReference_for_a_SectionAttendanceTakenEvent_where_no_SectionReference_yet_exists : When_building_references_where_key_unification_context_has_been_established<CalendarDateReference, SectionAttendanceTakenEvent, CalendarDate>
        {
            // Supplied values

            // Actual values

            // External dependencies

            protected override void InitializeTestObjectFactoryStub()
            {
                // Mock a direct call to the factory to create a School Id
                _testObjectFactory.Expect(f => f.Create("SchoolId", typeof(int), null, null))
                    .Return(111);

                base.InitializeTestObjectFactoryStub();
            }

            protected override void Act()
            {
                // Create the value builder
                var builder = new SectionAttendanceTakenEvent_CalendarDateReference_ValueBuilder();
                InitializeDependencies(builder);

                // Invoke the reference builder
                var buildContext = CreateBuildContext();
                _actualValueBuildResult = builder.TryBuild(buildContext);
            }

            [Assert]
            public void Should_create_1_property_value_constraint()
            {
                Call_to_create_referenced_resource_was_given_property_constraints(constraints =>
                    constraints.Count == 1);
            }

            [Assert]
            public void Should_use_factory_to_create_a_SchoolId_value_and_provide_as_a_property_value_constraint_for_the_EducationOrganizationId()
            {
                Call_to_create_referenced_resource_was_given_property_constraints(constraints =>
                    constraints.ShouldContain("educationOrganizationId", 111));
            }
        }

        public class When_building_a_CalendarDateReference_for_a_SectionAttendanceTakenEvent_where_a_SectionReference_already_exists : When_building_references_where_key_unification_context_has_been_established<CalendarDateReference, SectionAttendanceTakenEvent, CalendarDate>
        {
            // Supplied values

            // Actual values

            // External dependencies

            protected override SectionAttendanceTakenEvent CreateContainingInstance()
            {
                // Create a containing model with the other reference already set
                return new SectionAttendanceTakenEvent()
                {
                    sectionReference = new SectionReference {schoolId = 222}
                };
            }

            protected override void Act()
            {
                // Create the value builder
                var builder = new SectionAttendanceTakenEvent_CalendarDateReference_ValueBuilder();
                InitializeDependencies(builder);

                // Invoke the reference builder
                var buildContext = CreateBuildContext();
                _actualValueBuildResult = builder.TryBuild(buildContext);
            }

            [Assert]
            public void Should_create_1_property_value_constraint()
            {
                Call_to_create_referenced_resource_was_given_property_constraints(constraints =>
                    constraints.Count == 1);
            }

            [Assert]
            public void Should_extract_the_SchoolId_value_from_the_SectionReference_and_provide_as_a_property_value_constraint_for_the_EducationOrganizationId()
            {
                Call_to_create_referenced_resource_was_given_property_constraints(constraints =>
                    constraints.ShouldContain("educationOrganizationId", 222));
            }
        }

        public class When_building_a_SectionReference_for_a_SectionAttendanceTakenEvent_where_no_CalendarDateReference_yet_exists : When_building_references<SectionReference, SectionAttendanceTakenEvent>
        {
            // Supplied values
            private const string SuppliedLogicalPropertyPath = "Some.Logical.Path";

            // Actual values
            protected ValueBuildResult _actualValueBuildResult;

            // External dependencies

            protected override void Act()
            {
                // Create the value builder
                var builder = new SectionAttendanceTakenEvent_SectionReference_ValueBuilder();

                // Invoke the reference builder
                var buildContext = CreateBuildContext(SuppliedLogicalPropertyPath);
                _actualValueBuildResult = builder.TryBuild(buildContext);
            }

            [Assert]
            public void Should_not_handle_the_request_and_allow_it_to_pass_to_the_StandardReferenceValueBuilder()
            {
                _actualValueBuildResult.Handled.ShouldBeFalse();
            }
        }

        public class When_building_a_SectionReference_for_a_SectionAttendanceTakenEvent_where_a_CalendarDateReference_already_exists : When_building_references_where_key_unification_context_has_been_established<SectionReference, SectionAttendanceTakenEvent, Section>
        {
            // Supplied values

            // Actual values

            // External dependencies

            protected override SectionAttendanceTakenEvent CreateContainingInstance()
            {
                // Create a containing model with the other reference already set
                return new SectionAttendanceTakenEvent()
                {
                    calendarDateReference = new CalendarDateReference() { educationOrganizationId = 999 }
                };
            }

            protected override void Act()
            {
                // Create the value builder
                var builder = new SectionAttendanceTakenEvent_SectionReference_ValueBuilder();
                InitializeDependencies(builder);

                // Invoke the reference builder
                var buildContext = CreateBuildContext();
                _actualValueBuildResult = builder.TryBuild(buildContext);
            }

            [Assert]
            public void Should_create_1_property_value_constraint()
            {
                Call_to_create_referenced_resource_was_given_property_constraints(constraints =>
                    constraints.Count == 1);
            }

            [Assert]
            public void Should_extract_the_EducationOrganizationId_value_from_the_CalendarDateReference_and_provide_as_a_property_value_constraint_for_the_SchoolId()
            {
                Call_to_create_referenced_resource_was_given_property_constraints(constraints =>
                    constraints.ShouldContain("schoolId", 999));
            }
        }
    }
}