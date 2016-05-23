using EdFi.Ods.Tests._Bases;
using EdFi.Ods.Utilities.LoadGeneration.ReferenceValueBuilders;
using EdFi.TestObjects;
using Should;

namespace EdFi.Ods.Utilities.LoadGeneration.Tests.ResourceReferenceBuilders
{
    public class GradingPeriod_References_ValueBuilder_Fixtures
    {
        #region Supported SDK model class structures
        public class GradingPeriod
        {
            /* A reference to the related CalendarDate resource. */
            public CalendarDateReference beginCalendarDateReference { get; set; }

            /* A reference to the related CalendarDate resource. */
            public CalendarDateReference endCalendarDateReference { get; set; }
        }

        public class CalendarDateReference
        {
            /* EducationOrganization Identity Column */
            public int? educationOrganizationId { get; set; }
        }

        public class CalendarDate { }

        #endregion

        public class When_building_a_BeginCalendarDateReference_for_a_GradingPeriod_where_no_EndCalendarDateReference_yet_exists : 
            When_building_references<CalendarDateReference, GradingPeriod>
        {
            // Supplied values
            private const string SuppliedLogicalPropertyPath = "xxx.xxx.beginCalendarDateReference";

            // Actual values
            protected ValueBuildResult _actualValueBuildResult;

            // External dependencies

            protected override void Act()
            {
                // Create the value builder
                var builder = new GradingPeriod_BeginCalendarDate_ValueBuilder();

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

        public class When_building_a_BeginCalendarDateReference_for_a_GradingPeriod_where_an_EndCalendarDateReference_already_exists :
            When_building_references_where_key_unification_context_has_been_established<CalendarDateReference, GradingPeriod ,CalendarDate>
        {
            // Supplied values
            private const int SuppliedEdOrgId = 1001;

            // Actual values

            // External dependencies

            protected override GradingPeriod CreateContainingInstance()
            {
                // Create a containing model with the other reference already set
                return new GradingPeriod
                {
                    endCalendarDateReference = new CalendarDateReference
                    {
                        educationOrganizationId = SuppliedEdOrgId,
                    },
                };
            }

            protected override void Act()
            {
                // Create the value builder
                var builder = new GradingPeriod_BeginCalendarDate_ValueBuilder();
                InitializeDependencies(builder);

                // Invoke the reference builder
                suppliedLogicalPropertyPath = "xxx.xxx.beginCalendarDateReference";
                var buildContext = CreateBuildContext();
                _actualValueBuildResult = builder.TryBuild(buildContext);
            }

            [Assert]
            public void Should_create_1_property_value_constraints()
            {
                Call_to_create_referenced_resource_was_given_property_constraints(constraints =>
                    constraints.Count == 1);
            }

            [Assert]
            public void Should_extract_property_value_constraint_for_eductionOrganizationId_from_the_other_CalendarDateReference()
            {
                Call_to_create_referenced_resource_was_given_property_constraints(constraints =>
                    constraints.ShouldContain("educationOrganizationId", SuppliedEdOrgId)
                    );
            }
        }

        public class When_building_a_EndCalendarDateReference_for_an_GradingPeriod_where_no_BeginCalendarDateReference_yet_exists :
            When_building_references<CalendarDateReference, GradingPeriod>
        {
            // Supplied values
            private const string SuppliedLogicalPropertyPath = "xxx.xxx.endCalendarDateReference";

            // Actual values
            protected ValueBuildResult _actualValueBuildResult;

            // External depbeginencies

            protected override void Act()
            {
                // Create the value builder
                var builder = new GradingPeriod_EndCalendarDate_ValueBuilder();

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

        public class When_building_a_EndCalendarDateReference_for_an_GradingPeriod_where_an_BeginCalendarDateReference_already_exists :
            When_building_references_where_key_unification_context_has_been_established<CalendarDateReference, GradingPeriod, CalendarDate>
        {
            // Supplied values
            private const int SuppliedEdOrgId = 1001;

            // Actual values

            // External depbeginencies

            protected override GradingPeriod CreateContainingInstance()
            {
                // Create a containing model with the other reference already set
                return new GradingPeriod
                {
                    beginCalendarDateReference = new CalendarDateReference
                    {
                        educationOrganizationId = SuppliedEdOrgId,
                    },
                };
            }

            protected override void Act()
            {
                // Create the value builder
                var builder = new GradingPeriod_EndCalendarDate_ValueBuilder();
                InitializeDependencies(builder);

                // Invoke the reference builder
                suppliedLogicalPropertyPath = "xxx.xxx.endCalendarDateReference";
                var buildContext = CreateBuildContext();
                _actualValueBuildResult = builder.TryBuild(buildContext);
            }

            [Assert]
            public void Should_create_1_property_value_constraints()
            {
                Call_to_create_referenced_resource_was_given_property_constraints(constraints =>
                    constraints.Count == 1);
            }

            [Assert]
            public void Should_extract_property_value_constraint_for_eductionOrganizationId_from_the_other_CalbeginarDateReference()
            {
                Call_to_create_referenced_resource_was_given_property_constraints(constraints =>
                    constraints.ShouldContain("educationOrganizationId", SuppliedEdOrgId)
                    );
            }
        }
    }
}