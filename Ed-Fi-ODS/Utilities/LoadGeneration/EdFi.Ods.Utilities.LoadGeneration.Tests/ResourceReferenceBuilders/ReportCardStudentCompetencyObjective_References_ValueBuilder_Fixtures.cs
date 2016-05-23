// ****************************************************************************
//  Copyright (C) 2014 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System;
using System.Collections.Generic;
using EdFi.Ods.Tests._Bases;
using EdFi.Ods.Utilities.LoadGeneration.ReferenceValueBuilders;

namespace EdFi.Ods.Utilities.LoadGeneration.Tests.ResourceReferenceBuilders
{
    public class ReportCardStudentCompetencyObjective_References_ValueBuilder_Fixtures
    {
        #region Supported SDK model class structures

        public class ReportCard
        {
            /* A reference to the related GradingPeriod resource. */
            public GradingPeriodReference gradingPeriodReference { get; set; }

            /* A reference to the related Student resource. */
            public StudentReference studentReference { get; set; }

        }

        public class StudentReference
        {
            /* A unique alpha-numeric code assigned to a student. */
            public string studentUniqueId { get; set; }

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

        public class ReportCardStudentCompetencyObjective
        {
            /* A reference to the related StudentCompetencyObjective resource. */
            public StudentCompetencyObjectiveReference studentCompetencyObjectiveReference { get; set; }

        }

        public class StudentCompetencyObjectiveReference { }

        public class StudentCompetencyObjective { }

        #endregion

        public class When_building_a_StudentCompetencyObjectiveReference_for_a_ReportCardStudentCompetencyObjective : When_building_references_where_key_unification_context_has_been_established<StudentCompetencyObjectiveReference, ReportCardStudentCompetencyObjective, StudentCompetencyObjective>
        {
            // Supplied values

            // Actual values

            // External dependencies

            protected override void Act()
            {
                // Create the value builder
                var builder = new ReportCardStudentCompetencyObjective_StudentCompetencyObjectiveReference_ValueBuilder();
                InitializeDependencies(builder);

                // Initialize pertinent contextual data to be used by value builder to 
                // create the property value constraints
                var reportCardInstance = new ReportCard
                {
                    studentReference = new StudentReference
                    {
                        studentUniqueId = "ABC123"
                    },
                    gradingPeriodReference = new GradingPeriodReference
                    {
                        educationOrganizationId = 999,
                        beginDate = new DateTime(1999, 9, 9),
                        descriptor = "Some Grading Period"
                    }
                };

                // Invoke the reference builder
                var buildContext = CreateBuildContext(reportCardInstance);
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
                    constraints.ShouldContain("studentUniqueId", "ABC123"));
            }

            [Assert]
            public void Should_extract_property_value_constraint_for_gradingPeriodEductionOrganizationId_gradingPeriodDescriptor_and_gradingPeriodBeginDate_from_the_parents_GradingPeriodReference()
            {
                Call_to_create_referenced_resource_was_given_property_constraints(constraints =>
                    constraints.ShouldContain("gradingPeriodEducationOrganizationId", 999)
                    && constraints.ShouldContain("gradingPeriodDescriptor", "Some Grading Period")
                    && constraints.ShouldContain("gradingPeriodBeginDate", new DateTime(1999, 9, 9)));
            }
        }
    }
}