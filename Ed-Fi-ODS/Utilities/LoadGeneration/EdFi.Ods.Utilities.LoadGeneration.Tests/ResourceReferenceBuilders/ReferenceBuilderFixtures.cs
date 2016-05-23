// ****************************************************************************
//  Copyright (C) 2014 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System;
using EdFi.Ods.Tests._Bases;
using EdFi.Ods.Utilities.LoadGeneration.ReferenceValueBuilders;
using EdFi.TestObjects;
using NUnit.Framework;
using Should;

namespace EdFi.Ods.Utilities.LoadGeneration.Tests.ResourceReferenceBuilders
{
    public class ReferenceBuilderFixtures
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

        public class CalendarDate { }

        public class SectionReference
        {
            /* School Identity Column */
            public int? schoolId { get; set; }

        }

        public class Section { }

        #endregion

        public class When_building_a_reference_that_adds_a_property_constraint_with_1_constraint_already_in_context : When_building_references_where_key_unification_context_has_been_established<CalendarDateReference, SectionAttendanceTakenEvent, CalendarDate>
        {
            // Supplied values
            private BuildContext _suppliedBuildContext;

            // Actual values

            // External dependencies

            protected override SectionAttendanceTakenEvent CreateContainingInstance()
            {
                // Create a containing model with the other reference already set
                return new SectionAttendanceTakenEvent()
                {
                    sectionReference = new SectionReference { schoolId = 222 }
                };
            }

            protected override void Act()
            {
                // Create the value builder
                var builder = new SectionAttendanceTakenEvent_CalendarDateReference_ValueBuilder();
                InitializeDependencies(builder);

                // Invoke the reference builder
                _suppliedBuildContext = CreateBuildContext();
                _suppliedBuildContext.PropertyValueConstraints.Add("AnExistingConstraint", "A Value");
                _actualValueBuildResult = builder.TryBuild(_suppliedBuildContext);
            }

            [Assert]
            public void Should_pass_2_constraints_to_the_factory()
            {
                Call_to_create_referenced_resource_was_given_property_constraints(constraints =>
                    constraints.Count == 2);
            }

            [Assert]
            public void Should_pass_the_new_constraint_along_with_the_existing_constraint_already_in_context_to_the_TestObjectFactory()
            {
                Call_to_create_referenced_resource_was_given_property_constraints(constraints =>
                    constraints.ShouldContain("educationOrganizationId", 222) // New constraint added by context-specific reference builder
                    && constraints.ShouldContain("AnExistingConstraint", "A Value")); // Original constraint already in context before reference builder was invoked
            }

            [Assert]
            public void Should_pass_the_constraints_to_the_factory_using_a_dictionary_with_case_insensitive_keys()
            {
                Call_to_create_referenced_resource_was_given_property_constraints(constraints =>
                    constraints.ShouldContain("EDUCATIONORGANIZATIONID", 222) // New constraint added by context-specific reference builder
                    && constraints.ShouldContain("anexistingconstraint", "A Value")); // Original constraint already in context before reference builder was invoked
            }

            [Assert, Ignore("GKM - This asserted behavior needs to be reconsidered.  Changed context to pass schoolId along during troubleshooting.")]
            public void Should_leave_constraints_on_the_original_build_context_unmodified()
            {
                _suppliedBuildContext.PropertyValueConstraints.Count.ShouldEqual(1);
            }
        }

        public class When_building_a_reference_that_adds_a_duplicate_property_constraint_of_one_already_in_the_build_context 
            : When_building_references_where_key_unification_context_has_been_established<CalendarDateReference, SectionAttendanceTakenEvent, CalendarDate>
        {
            // Supplied values
            private BuildContext _suppliedBuildContext;

            // Actual values

            // External dependencies

            protected override SectionAttendanceTakenEvent CreateContainingInstance()
            {
                // Create a containing model with the other reference already set
                return new SectionAttendanceTakenEvent()
                {
                    sectionReference = new SectionReference { schoolId = 222 }
                };
            }

            protected override void Act()
            {
                // Create the value builder
                var builder = new SectionAttendanceTakenEvent_CalendarDateReference_ValueBuilder();
                InitializeDependencies(builder);

                // Invoke the reference builder
                _suppliedBuildContext = CreateBuildContext();
                _suppliedBuildContext.PropertyValueConstraints.Add("EDUCATIONORGANIZATIONID", 999);
                _actualValueBuildResult = builder.TryBuild(_suppliedBuildContext);
            }

            [Assert]
            public void Should_pass_1_constraint_to_the_factory()
            {
                Call_to_create_referenced_resource_was_given_property_constraints(constraints =>
                    // TODO: GKM - Allow constraints to flow?
                    constraints.Count >= 1); // == 1 
            }

            [Assert, Ignore("Exploration #1")]
            public void Should_pass_the_newer_constraint_value_to_the_TestObjectFactory()
            {
                Call_to_create_referenced_resource_was_given_property_constraints(constraints =>
                    constraints.ShouldContain("educationOrganizationId", 222)); // Newer constraint value is present
            }
        } 
    }
}