// ****************************************************************************
//  Copyright (C) 2014 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using EdFi.Ods.Tests._Bases;
using EdFi.Ods.Utilities.LoadGeneration.ReferenceValueBuilders;
using EdFi.TestObjects;
using Should;

namespace EdFi.Ods.Utilities.LoadGeneration.Tests.ResourceReferenceBuilders
{
    public class StudentSchoolAttendanceEvent_References_ValueBuilder_Fixtures
    {
        #region Supported SDK model class structures
        public class StudentSchoolAttendanceEvent
        {
            /* A reference to the related School resource. */
            public SchoolReference schoolReference { get; set; }

            /* A reference to the related Session resource. */
            public SessionReference sessionReference { get; set; }
        }

        public class SchoolReference
        {
            /* School Identity Column */
            public int? schoolId { get; set; }
        }

        public class SessionReference
        {
            /* School Identity Column */
            public int? schoolId { get; set; }
        }

        public class School { }
        public class Session { }

        #endregion

        public class When_building_a_SessionReference_for_a_StudentSchoolAttendanceEvent_where_no_SchoolReference_yet_exists :
            When_building_references<SessionReference, StudentSchoolAttendanceEvent>
        {
            // Supplied values

            // Actual values
            protected ValueBuildResult _actualValueBuildResult;

            // External dependencies

            protected override void Act()
            {
                // Create the value builder
                var builder = new StudentSchoolAttendanceEvent_SessionReference_ValueBuilder();

                // Invoke the reference builder
                var buildContext = CreateBuildContext("x.x.x");
                _actualValueBuildResult = builder.TryBuild(buildContext);
            }

            [Assert]
            public void Should_not_handle_the_request_and_allow_it_to_pass_to_the_StandardReferenceValueBuilder()
            {
                _actualValueBuildResult.Handled.ShouldBeFalse();
            }
        }

        public class When_building_a_SessionReference_for_a_StudentSchoolAttendanceEvent_where_a_SchoolReference_already_exists :
            When_building_references_where_key_unification_context_has_been_established<SessionReference, StudentSchoolAttendanceEvent, Session>
        {
            // Supplied values
            private const int SuppliedSchoolId = 222;

            // Actual values

            // External dependencies

            protected override StudentSchoolAttendanceEvent CreateContainingInstance()
            {
                // Create a containing model with the other reference already set
                return new StudentSchoolAttendanceEvent
                {
                    schoolReference = new SchoolReference {schoolId = SuppliedSchoolId}
                };
            }

            protected override void Act()
            {
                // Create the value builder
                var builder = new StudentSchoolAttendanceEvent_SessionReference_ValueBuilder();
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
            public void Should_extract_the_SchoolId_value_from_the_SchoolReference_and_provide_as_a_property_value_constraint()
            {
                Call_to_create_referenced_resource_was_given_property_constraints(constraints =>
                    constraints.ShouldContain("schoolId", SuppliedSchoolId));
            }
        }

        public class When_building_a_SchoolReference_for_a_StudentSchoolAttendanceEvent_where_no_SessionReference_yet_exists :
            When_building_references<SchoolReference, StudentSchoolAttendanceEvent>
        {
            // Supplied values

            // Actual values
            protected ValueBuildResult _actualValueBuildResult;

            // External dependencies

            protected override void Act()
            {
                // Create the value builder
                var builder = new StudentSchoolAttendanceEvent_SchoolReference_ValueBuilder();

                // Invoke the reference builder
                var buildContext = CreateBuildContext("x.x.x");
                _actualValueBuildResult = builder.TryBuild(buildContext);
            }

            [Assert]
            public void Should_not_handle_the_request_and_allow_it_to_pass_to_the_StandardReferenceValueBuilder()
            {
                _actualValueBuildResult.Handled.ShouldBeFalse();
            }
        }

        public class When_building_a_SchoolReference_for_a_StudentSchoolAttendanceEvent_where_a_SessionReference_already_exists :
            When_building_references_where_key_unification_context_has_been_established<SchoolReference, StudentSchoolAttendanceEvent, School>
        {
            // Supplied values
            private const int SuppliedSchoolId = 222;

            // Actual values

            // External dependencies

            protected override StudentSchoolAttendanceEvent CreateContainingInstance()
            {
                // Create a containing model with the other reference already set
                return new StudentSchoolAttendanceEvent
                {
                    sessionReference = new SessionReference {schoolId = SuppliedSchoolId}
                };
            }

            protected override void Act()
            {
                // Create the value builder
                var builder = new StudentSchoolAttendanceEvent_SchoolReference_ValueBuilder();
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
            public void Should_extract_the_SchoolId_value_from_the_SessionReference_and_provide_as_a_property_value_constraint()
            {
                Call_to_create_referenced_resource_was_given_property_constraints(constraints =>
                    constraints.ShouldContain("schoolId", SuppliedSchoolId));
            }
        }

    }
}