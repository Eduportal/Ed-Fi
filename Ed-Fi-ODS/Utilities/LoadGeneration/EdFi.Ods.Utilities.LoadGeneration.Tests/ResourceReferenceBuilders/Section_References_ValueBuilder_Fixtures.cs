using EdFi.Ods.Tests._Bases;
using EdFi.Ods.Utilities.LoadGeneration.ReferenceValueBuilders;
using EdFi.TestObjects;
using Should;

namespace EdFi.Ods.Utilities.LoadGeneration.Tests.ResourceReferenceBuilders
{
    public class Section_References_ValueBuilder_Fixtures
    {
        #region Supported SDK model class structures
        public class Section
        {
            /* A reference to the related ClassPeriod resource. */
            public ClassPeriodReference classPeriodReference { get; set; }

            /* A reference to the related CourseOffering resource. */
            public CourseOfferingReference courseOfferingReference { get; set; }

            /* A reference to the related Location resource. */
            public LocationReference locationReference { get; set; }

            /* A reference to the related School resource. */
            public SchoolReference schoolReference { get; set; }

            /* A reference to the related Section resource. */
            public SessionReference sessionReference { get; set; }
        }

        public class ClassPeriodReference
        {
            /* School Identity Column */
            public int? schoolId { get; set; }
        }

        public class CourseOfferingReference
        {
            /* School Identity Column */
            public int? schoolId { get; set; }

            /* The term for the Section during the school year. */
            public string termType { get; set; }

            /* The identifier for the school year (e.g., 2010/11).   */
            public int? schoolYear { get; set; }
        }

        public class LocationReference
        {
            /* Location Identity Column */
            public int? schoolId { get; set; }
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

            /* The term for the Section during the school year. */
            public string termType { get; set; }

            /* The identifier for the school year (e.g., 2010/11).   */
            public int? schoolYear { get; set; }
        }

        public class Location { }
        public class School { }
        public class ClassPeriod { }
        public class Session { }
        public class CourseOffering { }
        #endregion

        public class When_building_a_LocationReference_for_a_Section_where_no_other_related_references_exist :
            When_building_references<LocationReference, Section>
        {
            // Supplied values
            private const string SuppliedLogicalPropertyPath = "xxx.xxx.xxx";

            // Actual values
            protected ValueBuildResult _actualValueBuildResult;

            // External dependencies

            protected override void Act()
            {
                // Create the value builder
                var builder = new Section_LocationReference_ValueBuilder();

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

        public class When_building_a_LocationReference_for_a_Section_where_a_CourseOfferingReference_already_exists :
            When_building_references_where_key_unification_context_has_been_established<LocationReference, Section, Location>
        {
            // Supplied values
            private const int SuppliedSchoolId = 9001;

            // Actual values

            // External dependencies

            protected override Section CreateContainingInstance()
            {
                // Create a containing model with the other reference already set
                return new Section
                {
                    courseOfferingReference = new CourseOfferingReference
                    {
                        schoolId = SuppliedSchoolId
                    },
                };
            }

            protected override void Act()
            {
                // Create the value builder
                var builder = new Section_LocationReference_ValueBuilder();
                InitializeDependencies(builder);

                // Invoke the reference builder
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
            public void Should_extract_property_value_constraint_for_schoolId_from_CourseOfferingReference()
            {
                Call_to_create_referenced_resource_was_given_property_constraints(constraints =>
                    constraints.ShouldContain("schoolId", SuppliedSchoolId)
                    );
            }
        }

        /*
         * Skipped similar tests
         */

        public class When_building_a_SessionReference_for_a_Section_where_no_other_references_yet_exists :
            When_building_references<SessionReference, Section>
        {
            // Supplied values
            private const string SuppliedLogicalPropertyPath = "xxx.xxx.xxx";

            // Actual values
            protected ValueBuildResult _actualValueBuildResult;

            // External dependencies

            protected override void Act()
            {
                // Create the value builder
                var builder = new Section_SessionReference_ValueBuilder();

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

        public class When_building_a_SessionReference_for_a_Section_where_a_CourseOfferingReference_already_exists :
            When_building_references_where_key_unification_context_has_been_established<SessionReference, Section, Session>
        {
            // Supplied values
            private const int SuppliedSchoolId = 4;
            private const int SuppliedSchoolYear = 2010;
            private const string SuppliedTermType = "someTermType";

            // Actual values

            // External dependencies

            protected override Section CreateContainingInstance()
            {
                // Create a containing model with the other reference already set
                return new Section
                {
                    courseOfferingReference = new CourseOfferingReference
                    {
                        schoolId = SuppliedSchoolId,
                        schoolYear = SuppliedSchoolYear,
                        termType = SuppliedTermType
                    }
                };
            }

            protected override void Act()
            {
                // Create the value builder
                var builder = new Section_SessionReference_ValueBuilder();
                InitializeDependencies(builder);

                // Invoke the reference builder
                var buildContext = CreateBuildContext();
                _actualValueBuildResult = builder.TryBuild(buildContext);
            }

            [Assert]
            public void Should_create_3_property_value_constraints()
            {
                Call_to_create_referenced_resource_was_given_property_constraints(constraints =>
                    constraints.Count == 3);
            }

            [Assert]
            public void Should_extract_property_value_constraint_for_schoolId_schoolYear_and_termType_from_CourseOfferingReference()
            {
                Call_to_create_referenced_resource_was_given_property_constraints(constraints =>
                    constraints.ShouldContain("schoolId", SuppliedSchoolId)
                    && constraints.ShouldContain("schoolYear", SuppliedSchoolYear)
                    && constraints.ShouldContain("termType", SuppliedTermType));
            }
        }

        public class When_building_a_SessionReference_for_a_Section_where_a_SchoolReference_already_exists_but_no_SessionReference_and_CourseOfferingReference_yet_exist :
            When_building_references_where_key_unification_context_has_been_established<SessionReference, Section, Session>
        {
            // Supplied values
            private const int SuppliedSchoolId = 4;

            // Actual values

            // External dependencies

            protected override Section CreateContainingInstance()
            {
                // Create a containing model with the other reference already set
                return new Section
                {
                    schoolReference = new SchoolReference
                    {
                        schoolId = SuppliedSchoolId
                    }
                };
            }

            protected override void Act()
            {
                // Create the value builder
                var builder = new Section_SessionReference_ValueBuilder();
                InitializeDependencies(builder);

                // Invoke the reference builder
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
            public void Should_extract_property_value_constraint_for_schoolId_from_SessionReference()
            {
                Call_to_create_referenced_resource_was_given_property_constraints(constraints =>
                    constraints.ShouldContain("schoolId", SuppliedSchoolId)
                    );
            }
        }

        public class When_building_a_SessionReference_for_a_Section_where_a_SchoolReference_LocationReference_ClassPeriod_and_CourseOfferingReference_already_exist :
            When_building_references_where_key_unification_context_has_been_established<SessionReference, Section, Session>
        {
            // Supplied values
            private const int SuppliedSchoolId = 4;
            private const int SuppliedSchoolYear = 2010;
            private const string SuppliedTermType = "someTermType";

            // Actual values

            // External dependencies

            protected override Section CreateContainingInstance()
            {
                // Create a containing model with the other reference already set
                return new Section
                {
                    classPeriodReference = new ClassPeriodReference
                    {
                        schoolId = 1
                    },
                    courseOfferingReference = new CourseOfferingReference
                    {
                        schoolId = SuppliedSchoolId,
                        schoolYear = SuppliedSchoolYear,
                        termType = SuppliedTermType
                    },
                    locationReference = new LocationReference
                    {
                        schoolId = 1
                    },
                    schoolReference = new SchoolReference
                    {
                        schoolId = 1
                    },
                };
            }

            protected override void Act()
            {
                // Create the value builder
                var builder = new Section_SessionReference_ValueBuilder();
                InitializeDependencies(builder);

                // Invoke the reference builder
                var buildContext = CreateBuildContext();
                _actualValueBuildResult = builder.TryBuild(buildContext);
            }

            [Assert]
            public void Should_create_3_property_value_constraints()
            {
                Call_to_create_referenced_resource_was_given_property_constraints(constraints =>
                    constraints.Count == 3);
            }

            [Assert]
            public void Should_extract_property_value_constraint_for_schoolId_schoolYear_and_termType_from_CourseOfferingReference()
            {
                Call_to_create_referenced_resource_was_given_property_constraints(constraints =>
                    constraints.ShouldContain("schoolId", SuppliedSchoolId) 
                    && constraints.ShouldContain("schoolYear", SuppliedSchoolYear)
                    && constraints.ShouldContain("termType", SuppliedTermType)
                    );
            }
        }

    }
}