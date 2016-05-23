// ****************************************************************************
//  Copyright (C) 2014 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using EdFi.Ods.Tests._Bases;
using EdFi.Ods.Utilities.LoadGeneration.ReferenceValueBuilders;
using EdFi.TestObjects;
using Rhino.Mocks;
using Should;

namespace EdFi.Ods.Utilities.LoadGeneration.Tests.ResourceReferenceBuilders
{
    public class BellSchedule_References_ValueBuilder_Fixtures
    {
        #region Supported SDK model class structures
        public class BellSchedule
        {
            /* A reference to the related CalendarDate resource. */
            public CalendarDateReference calendarDateReference { get; set; }
        
            /* A reference to the related School resource. */
            public SchoolReference schoolReference { get; set; }

        }

        public class CalendarDateReference
        {
            /* EducationOrganization Identity Column */
            public int? educationOrganizationId { get; set; }
        }

        public class SchoolReference
        {
            /* School Identity Column */
            public int? schoolId { get; set; }
        }

        public class CalendarDate { }

        public class School { }

        #endregion

        public class When_building_a_CalendarDateReference_for_a_BellSchedule_where_no_SchoolReference_yet_exists : When_building_references_where_key_unification_context_has_been_established<CalendarDateReference, BellSchedule, CalendarDate>
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
                var builder = new BellSchedule_CalendarDateReference_ValueBuilder();
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

        public class When_building_a_CalendarDateReference_for_a_BellSchedule_where_a_SchoolReference_already_exists : When_building_references_where_key_unification_context_has_been_established<CalendarDateReference, BellSchedule, CalendarDate>
        {
            // Supplied values
            private const int SuppliedSchoolId = 222;

            // Actual values

            // External dependencies

            protected override BellSchedule CreateContainingInstance()
            {
                // Create a containing model with the other reference already set
                return new BellSchedule
                {
                    schoolReference = new SchoolReference {schoolId = SuppliedSchoolId}
                };
            }

            protected override void Act()
            {
                // Create the value builder
                var builder = new BellSchedule_CalendarDateReference_ValueBuilder();
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
            public void Should_extract_the_SchoolId_value_from_the_SchoolReference_and_provide_as_a_property_value_constraint_for_the_EducationOrganizationId()
            {
                Call_to_create_referenced_resource_was_given_property_constraints(constraints =>
                    constraints.ShouldContain("educationOrganizationId", SuppliedSchoolId));
            }
        }

        public class When_building_a_SchoolReference_for_a_BellSchedule_where_no_CalendarDateReference_yet_exists : When_building_references<SchoolReference, BellSchedule>
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

        public class When_building_a_SchoolReference_for_a_BellSchedule_where_a_CalendarDateReference_already_exists : When_building_references_where_key_unification_context_has_been_established<SchoolReference, BellSchedule, School>
        {
            // Supplied values
            private const int SuppliedEdOrgId = 999;

            // Actual values

            // External dependencies

            protected override BellSchedule CreateContainingInstance()
            {
                // Create a containing model with the other reference already set
                return new BellSchedule
                {
                    calendarDateReference = new CalendarDateReference { educationOrganizationId = SuppliedEdOrgId }
                };
            }

            protected override void Act()
            {
                // Create the value builder
                var builder = new BellSchedule_SchoolReference_ValueBuilder();
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
                    constraints.ShouldContain("schoolId", SuppliedEdOrgId));
            }
        }
    }
}