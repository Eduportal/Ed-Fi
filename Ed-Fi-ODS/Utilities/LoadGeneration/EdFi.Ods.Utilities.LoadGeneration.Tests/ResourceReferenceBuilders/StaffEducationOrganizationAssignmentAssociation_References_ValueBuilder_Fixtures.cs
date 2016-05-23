using EdFi.Ods.Tests._Bases;
using EdFi.Ods.Utilities.LoadGeneration.ReferenceValueBuilders;
using EdFi.TestObjects;
using Should;

namespace EdFi.Ods.Utilities.LoadGeneration.Tests.ResourceReferenceBuilders
{
    public class StaffEducationOrganizationAssignmentAssociation_References_ValueBuilder_Fixtures
    {
        #region Supported SDK model class structures
        public class StaffEducationOrganizationAssignmentAssociation
        {
            /* A reference to the related StaffEducationOrganizationEmploymentAssociation resource. */
            public StaffEducationOrganizationEmploymentAssociationReference employmentStaffEducationOrganizationEmploymentAssociationReference { get; set; }

            /* A reference to the related EducationOrganization resource. */
            public EducationOrganizationReference educationOrganizationReference { get; set; }

            /* A reference to the related Staff resource. */
            public StaffReference staffReference { get; set; }
        }

        public class StaffEducationOrganizationEmploymentAssociationReference
        {
            /* A unique alpha-numeric code assigned to a staff. */
            public string staffUniqueId { get; set; }

            /* EducationOrganization Identity Column */
            public int? educationOrganizationId { get; set; }
        }

        public class EducationOrganizationReference
        {
            /* EducationOrganization Identity Column */
            public int? educationOrganizationId { get; set; }
        }

        public class StaffReference
        {
            /* A unique alpha-numeric code assigned to a staff. */
            public string staffUniqueId { get; set; }
        }

        public class StaffEducationOrganizationEmploymentAssociation { }


        public class Staff { }

        #endregion

        public class When_building_a_StaffReference_for_a_StaffEducationOrganizationAssignmentAssociation_where_no_StaffEducationOrganizationEmploymentAssociationReference_yet_exists :
            When_building_references<StaffReference, StaffEducationOrganizationAssignmentAssociation>
        {
            // Supplied values
            private const string SuppliedLogicalPropertyPath = "xxx.xxx.xxx";

            // Actual values
            protected ValueBuildResult _actualValueBuildResult;

            // External dependencies

            protected override void Act()
            {
                // Create the value builder
                var builder = new StaffEducationOrganizationAssignmentAssociation_StaffReference_ValueBuilder();

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

        public class When_building_a_StaffReference_for_a_StaffEducationOrganizationAssignmentAssociation_where_a_StaffEducationOrganizationEmploymentAssociationReference_already_exists :
            When_building_references_where_key_unification_context_has_been_established<StaffReference, StaffEducationOrganizationAssignmentAssociation, Staff>
        {
            // Supplied values
            private const string SuppliedStaffId = "StaffId-500";

            // Actual values

            // External dependencies

            protected override StaffEducationOrganizationAssignmentAssociation CreateContainingInstance()
            {
                // Create a containing model with the other reference already set
                return new StaffEducationOrganizationAssignmentAssociation
                {
                    employmentStaffEducationOrganizationEmploymentAssociationReference = new StaffEducationOrganizationEmploymentAssociationReference
                    {
                        staffUniqueId = SuppliedStaffId
                    },
                };
            }

            protected override void Act()
            {
                // Create the value builder
                var builder = new StaffEducationOrganizationAssignmentAssociation_StaffReference_ValueBuilder();
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
            public void Should_extract_property_value_constraint_for_staffUniqueId_from_StaffEducationOrganizationEmploymentAssociationReference()
            {
                Call_to_create_referenced_resource_was_given_property_constraints(constraints =>
                    constraints.ShouldContain("staffUniqueId", SuppliedStaffId)
                    );
            }
        }

        /*
         * Skipped similar tests
         */

        public class When_building_a_StaffEducationOrganizationEmploymentAssociationReference_for_a_StaffEducationOrganizationAssignmentAssociation_where_no_other_references_yet_exists :
            When_building_references<StaffEducationOrganizationEmploymentAssociationReference, StaffEducationOrganizationAssignmentAssociation>
        {
            // Supplied values
            private const string SuppliedLogicalPropertyPath = "xxx.xxx.xxx";

            // Actual values
            protected ValueBuildResult _actualValueBuildResult;

            // External dependencies

            protected override void Act()
            {
                // Create the value builder
                var builder = new StaffEducationOrganizationAssignmentAssociation_StaffEducationOrganizationEmploymentAssociationReference_ValueBuilder();

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

        public class When_building_a_StaffEducationOrganizationEmploymentAssociationReference_for_a_StaffEducationOrganizationAssignmentAssociation_where_a_StaffReference_already_exists_but_no_EducationOrganizationReference_yet_exists :
            When_building_references_where_key_unification_context_has_been_established<StaffEducationOrganizationEmploymentAssociationReference, StaffEducationOrganizationAssignmentAssociation, StaffEducationOrganizationEmploymentAssociation>
        {
            // Supplied values
            private const string SuppliedStaffId = "StaffId-500";

            // Actual values

            // External dependencies

            protected override StaffEducationOrganizationAssignmentAssociation CreateContainingInstance()
            {
                // Create a containing model with the other reference already set
                return new StaffEducationOrganizationAssignmentAssociation
                {
                    staffReference = new StaffReference
                    {
                        staffUniqueId = SuppliedStaffId
                    }
                };
            }

            protected override void Act()
            {
                // Create the value builder
                var builder = new StaffEducationOrganizationAssignmentAssociation_StaffEducationOrganizationEmploymentAssociationReference_ValueBuilder();
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
            public void Should_extract_property_value_constraint_for_staffUniqueId_from_StaffReference()
            {
                Call_to_create_referenced_resource_was_given_property_constraints(constraints =>
                    constraints.ShouldContain("staffUniqueId", SuppliedStaffId)
                    );
            }
        }

        public class When_building_a_StaffEducationOrganizationEmploymentAssociationReference_for_a_StaffEducationOrganizationAssignmentAssociation_where_a_EducationOrganizationReference_already_exists_but_no_StaffReference_yet_exists :
            When_building_references_where_key_unification_context_has_been_established<StaffEducationOrganizationEmploymentAssociationReference, StaffEducationOrganizationAssignmentAssociation, StaffEducationOrganizationEmploymentAssociation>
        {
            // Supplied values
            private const int SuppliedEdOrgId = 445;

            // Actual values

            // External dependencies

            protected override StaffEducationOrganizationAssignmentAssociation CreateContainingInstance()
            {
                // Create a containing model with the other reference already set
                return new StaffEducationOrganizationAssignmentAssociation
                {
                    educationOrganizationReference = new EducationOrganizationReference
                    {
                        educationOrganizationId = SuppliedEdOrgId
                    }
                };
            }

            protected override void Act()
            {
                // Create the value builder
                var builder = new StaffEducationOrganizationAssignmentAssociation_StaffEducationOrganizationEmploymentAssociationReference_ValueBuilder();
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
            public void Should_extract_property_value_constraint_for_eductionOrganizationId_from_EducationOrganizationReference()
            {
                Call_to_create_referenced_resource_was_given_property_constraints(constraints =>
                    constraints.ShouldContain("educationOrganizationId", SuppliedEdOrgId)
                    );
            }
        }

        public class When_building_a_StaffEducationOrganizationEmploymentAssociationReference_for_a_StaffEducationOrganizationAssignmentAssociation_where_a_StaffReference_and_EducationOrganizationReference_already_exist :
            When_building_references_where_key_unification_context_has_been_established<StaffEducationOrganizationEmploymentAssociationReference, StaffEducationOrganizationAssignmentAssociation, StaffEducationOrganizationEmploymentAssociation>
        {
            // Supplied values
            private const string SuppliedStaffId = "StaffId-500";
            private const int SuppliedEdOrgId = 5454;

            // Actual values

            // External dependencies

            protected override StaffEducationOrganizationAssignmentAssociation CreateContainingInstance()
            {
                // Create a containing model with the other reference already set
                return new StaffEducationOrganizationAssignmentAssociation
                {
                    staffReference = new StaffReference
                    {
                        staffUniqueId = SuppliedStaffId
                    },
                    educationOrganizationReference = new EducationOrganizationReference
                    {
                        educationOrganizationId = SuppliedEdOrgId
                    }
                };
            }

            protected override void Act()
            {
                // Create the value builder
                var builder = new StaffEducationOrganizationAssignmentAssociation_StaffEducationOrganizationEmploymentAssociationReference_ValueBuilder();
                InitializeDependencies(builder);

                // Invoke the reference builder
                var buildContext = CreateBuildContext();
                _actualValueBuildResult = builder.TryBuild(buildContext);
            }

            [Assert]
            public void Should_create_2_property_value_constraints()
            {
                Call_to_create_referenced_resource_was_given_property_constraints(constraints =>
                    constraints.Count == 2);
            }

            [Assert]
            public void Should_extract_property_value_constraint_for_eductionOrganizationId_from_EducationOrganizationReference_and_for_staffUniqueId_from_StaffReference()
            {
                Call_to_create_referenced_resource_was_given_property_constraints(constraints =>
                    constraints.ShouldContain("staffUniqueId", SuppliedStaffId) &&
                    constraints.ShouldContain("educationOrganizationId", SuppliedEdOrgId)
                    );
            }
        }

    }
}