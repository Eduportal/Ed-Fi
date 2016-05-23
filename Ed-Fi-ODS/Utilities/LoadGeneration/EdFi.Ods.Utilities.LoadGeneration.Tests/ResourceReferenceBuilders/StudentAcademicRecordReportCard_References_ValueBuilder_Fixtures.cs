// ****************************************************************************
//  Copyright (C) 2014 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System.Collections.Generic;
using EdFi.Ods.Tests._Bases;
using EdFi.Ods.Utilities.LoadGeneration.ReferenceValueBuilders;

namespace EdFi.Ods.Utilities.LoadGeneration.Tests.ResourceReferenceBuilders
{
    public class StudentAcademicRecordReportCard_References_ValueBuilder_Fixtures
    {
        #region Supported SDK model class structures
        public class StudentAcademicRecord
        {
            /* A reference to the related EducationOrganization resource. */
            public EducationOrganizationReference educationOrganizationReference { get; set; }

            /* A reference to the related Student resource. */
            public StudentReference studentReference { get; set; }
        }

        public class StudentAcademicRecordReportCard {}

        public class StudentReference
        {
            /* A unique alpha-numeric code assigned to a student. */
            public string studentUniqueId { get; set; }
        }

        public class EducationOrganizationReference
        {
            /* EducationOrganization Identity Column */
            public int? educationOrganizationId { get; set; }
        }

        public class ReportCardReference { }
        
        public class ReportCard { }

        #endregion

        public class When_building_a_ReportCardReference_for_a_StudentAcademicRecordReportCard :
            When_building_references_where_key_unification_context_has_been_established<ReportCardReference, StudentAcademicRecordReportCard, ReportCard>
        {
            // Supplied values
            private const int SuppliedEdOrgId = 2222;
            private const string SuppliedStudentUniqueId = "345xyz";

            // Actual values

            // External dependencies

            protected override void Act()
            {
                // Create the value builder
                var builder = new StudentAcademicRecordReportCard_ReportCardReference_ValueBuilder();
                InitializeDependencies(builder);

                var instance = new StudentAcademicRecord
                {
                    educationOrganizationReference = new EducationOrganizationReference
                    {
                        educationOrganizationId = SuppliedEdOrgId
                    },
                    studentReference = new StudentReference
                    {
                        studentUniqueId = SuppliedStudentUniqueId
                    }
                };
                
                // Invoke the reference builder
                var buildContext = CreateBuildContext(instance);
                _actualValueBuildResult = builder.TryBuild(buildContext);
            }

            [Assert]
            public void Should_create_2_property_value_constraints()
            {
                Call_to_create_referenced_resource_was_given_property_constraints(constraints =>
                    constraints.Count == 2);
            }

            [Assert]
            public void Should_extract_property_value_constraint_for_studentUniqueId_from_the_parents_StudentReference()
            {
                Call_to_create_referenced_resource_was_given_property_constraints(constraints =>
                    constraints.ShouldContain("studentUniqueId", SuppliedStudentUniqueId));
            }

            [Assert]
            public void Should_extract_property_value_constraint_for_studentUniqueId_from_the_parents_EducationOrganizationReference()
            {
                Call_to_create_referenced_resource_was_given_property_constraints(constraints =>
                    constraints.ShouldContain("educationOrganizationId", SuppliedEdOrgId));
            }
        }
    }
}