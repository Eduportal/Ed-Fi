// ****************************************************************************
//  Copyright (C) 2014 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System.Collections.Generic;
using EdFi.Ods.Tests._Bases;
using EdFi.Ods.Utilities.LoadGeneration.ReferenceValueBuilders;

namespace EdFi.Ods.Utilities.LoadGeneration.Tests.ResourceReferenceBuilders
{
    public class ObjectiveAssessmentLearningObjective_References_ValueBuilder_Fixtures
    {
        #region Supported SDK model class structures
        public class ObjectiveAssessment
        {
            /* A reference to the related Assessment resource. */
            public AssessmentReference assessmentReference { get; set; }
        }

        public class ObjectiveAssessmentLearningObjective
        {
            /* A reference to the related LearningObjective resource. */
            public LearningObjectiveReference learningObjectiveReference { get; set; }

        }

        public class AssessmentReference
        {
            /* The description of the content or subject area (e.g., arts, mathematics, reading, stenography, or a foreign language) of an assessment.  NEDM: Assessment Content, Academic Subject   */
            public string academicSubjectDescriptor { get; set; }

            /* The typical grade level for which an assessment is designed. If the test assessment spans a range of grades, then this attribute holds the highest grade assessed.  If only one grade level is assessed, then only this attribute is used. For example:  Adult  Prekindergarten  First grade  Second grade  ...   */
            public string assessedGradeLevelDescriptor { get; set; }
        }

        public class LearningObjectiveReference
        {
            /* The description of the content or subject area (e.g., arts, mathematics, reading, stenography, or a foreign language) of an assessment.   */
            public string academicSubjectDescriptor { get; set; }

            /* The grade level for which the learning objective is targeted,   */
            public string objectiveGradeLevelDescriptor { get; set; }

        }

        public class LearningObjective
        {
            /* The description of the content or subject area (e.g., arts, mathematics, reading, stenography, or a foreign language) of an assessment.   */
            public string academicSubjectDescriptor { get; set; }

            /* The grade level for which the learning objective is targeted,   */
            public string objectiveGradeLevelDescriptor { get; set; }
        }
        #endregion

        public class When_building_a_LearningObjectiveReference_for_an_ObjectiveAssessmentLearningObjective :
            When_building_references_where_key_unification_context_has_been_established<LearningObjectiveReference, ObjectiveAssessmentLearningObjective, LearningObjective>
        {
            // Supplied values
            private const string SuppliedAcademicSubjectDescriptor = "Subject ID";
            private const string SuppliedAssessedGradeLevelDescriptor = "Grade Level ID";
            
            // Actual values

            // External dependencies

            protected override void Act()
            {
                // Create the value builder
                var builder = new ObjectiveAssessmentLearningObjective_LearningObjectiveReference_ValueBuilder();
                InitializeDependencies(builder);

                var objectiveAssessment = new ObjectiveAssessment
                {
                    assessmentReference = new AssessmentReference
                    {
                        academicSubjectDescriptor = SuppliedAcademicSubjectDescriptor,
                        assessedGradeLevelDescriptor = SuppliedAssessedGradeLevelDescriptor,
                    }
                };
                
                // Invoke the reference builder
                var buildContext = CreateBuildContext(objectiveAssessment);
                _actualValueBuildResult = builder.TryBuild(buildContext);
            }

            [Assert]
            public void Should_create_2_property_value_constraints()
            {
                Call_to_create_referenced_resource_was_given_property_constraints(constraints =>
                    constraints.Count == 2);
            }

            [Assert]
            public void Should_extract_property_value_constraint_for_specifics_from_the_parents_AssessmentReference()
            {
                Call_to_create_referenced_resource_was_given_property_constraints(constraints =>
                    constraints.ShouldContain("academicSubjectDescriptor", SuppliedAcademicSubjectDescriptor) &&
                    constraints.ShouldContain("assessedGradeLevelDescriptor", SuppliedAssessedGradeLevelDescriptor));
            }
        }
    }
}