// ****************************************************************************
//  Copyright (C) 2014 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System;
using System.Collections.Generic;
using EdFi.Ods.Tests._Bases;
using EdFi.Ods.Utilities.LoadGeneration.ReferenceValueBuilders;

namespace EdFi.Ods.Utilities.LoadGeneration.Tests.ResourceReferenceBuilders
{
    public class ReportCardStudentLearningObjective_References_ValueBuilder_Fixtures
    {
        #region Supported SDK model class structures
        public class ReportCardStudentLearningObjective
        {
            /* A reference to the related StudentLearningObjective resource. */
            public StudentLearningObjectiveReference studentLearningObjectiveReference { get; set; }
        }

        public class StudentLearningObjectiveReference
        {
            /* A unique alpha-numeric code assigned to a student. */
            public string studentUniqueId { get; set; }

            /*  */
            public int? gradingPeriodEducationOrganizationId { get; set; }

            /*  */
            public string gradingPeriodDescriptor { get; set; }

            /*  */
            public DateTime? gradingPeriodBeginDate { get; set; }
        }

        public class ReportCard
        {
            /* A reference to the related GradingPeriod resource. */
            public GradingPeriodReference gradingPeriodReference { get; set; }

            /* A reference to the related Student resource. */
            public StudentReference studentReference { get; set; }
        }
        
        public class GradingPeriodReference
        {
            /* EducationOrganization Identity Column */
            public int? educationOrganizationId { get; set; }

            /* The name of the grading period during the school year in which the grade is offered (e.g., 1st cycle, 1st semester)   */
            public string descriptor { get; set; }

            /* Month, day, and year of the first day of the grading period.     */
            public DateTime? beginDate { get; set; }
        }

        public class StudentReference
        {
            /* A unique alpha-numeric code assigned to a student. */
            public string studentUniqueId { get; set; }
        }

        public class StudentLearningObjective { }

        #endregion

        public class When_building_a_StudentLearningObjectiveReference_for_a_ReportCardStudentLearningObjective :
            When_building_references_where_key_unification_context_has_been_established<StudentLearningObjectiveReference, ReportCardStudentLearningObjective, StudentLearningObjective>
        {
            // Supplied values
            private const int SuppliedEdOrgId = 2222;
            private const string SuppliedStudentUniqueId = "345xyz";
            private const string SuppliedDescriptor = "Desc-001";
            private static readonly DateTime SuppliedBeginDate = new DateTime(2005, 11, 30);
            // Actual values

            // External dependencies

            protected override void Act()
            {
                // Create the value builder
                var builder = new ReportCardStudentLearningObjective_StudentLearningObjectiveReference_ValueBuilder();
                InitializeDependencies(builder);

                var instance = new ReportCard
                {
                    studentReference = new StudentReference
                    {
                        studentUniqueId = SuppliedStudentUniqueId
                    },
                    gradingPeriodReference = new GradingPeriodReference
                    {
                        educationOrganizationId = SuppliedEdOrgId,
                        descriptor = SuppliedDescriptor,
                        beginDate = SuppliedBeginDate
                    }
                };

                // Invoke the reference builder
                var buildContext = CreateBuildContext(instance);
                _actualValueBuildResult = builder.TryBuild(buildContext);
            }

            [Assert]
            public void Should_create_4_property_value_constraints()
            {
                Call_to_create_referenced_resource_was_given_property_constraints(constraints =>
                    constraints.Count == 4);
            }

            [Assert]
            public void Should_extract_property_value_constraint_for_studentUniqueId_from_the_parents_StudentReference()
            {
                Call_to_create_referenced_resource_was_given_property_constraints(constraints =>
                    constraints.ShouldContain("studentUniqueId", SuppliedStudentUniqueId));
            }

            [Assert]
            public void Should_extract_property_value_constraint_for_grading_period_key_values_from_the_parents_GradingPeriodReference()
            {
                Call_to_create_referenced_resource_was_given_property_constraints(constraints =>
                    constraints.ShouldContain("gradingPeriodEducationOrganizationId", SuppliedEdOrgId)
                    && constraints.ShouldContain("gradingPeriodDescriptor", SuppliedDescriptor)
                    && constraints.ShouldContain("gradingPeriodBeginDate", SuppliedBeginDate));
            }
        }
    }
}