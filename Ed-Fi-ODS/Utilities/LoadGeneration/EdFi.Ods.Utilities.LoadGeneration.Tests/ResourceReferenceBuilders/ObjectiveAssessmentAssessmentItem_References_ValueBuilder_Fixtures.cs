// ****************************************************************************
//  Copyright (C) 2014 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System.Collections.Generic;
using EdFi.Ods.Tests._Bases;
using EdFi.Ods.Utilities.LoadGeneration.ReferenceValueBuilders;

namespace EdFi.Ods.Utilities.LoadGeneration.Tests.ResourceReferenceBuilders
{
    public class ObjectiveAssessmentAssessmentItem_References_ValueBuilder_Fixtures
    {
        #region Supported SDK model class structures
        public class ObjectiveAssessment
        {
            /* The unique identifier of the resource. */
            public string id { get; set; }

            /* A reference to the related Assessment resource. */
            public AssessmentReference assessmentReference { get; set; }
        }
        
        public class ObjectiveAssessmentAssessmentItem
        {
            /* A reference to the related AssessmentItem resource. */
            public AssessmentItemReference assessmentItemReference { get; set; }

        }

        public class AssessmentReference
        {
            /* The title or name of the assessment.  NEDM: Assessment Title   */
            public string title { get; set; }

            /* The description of the content or subject area (e.g., arts, mathematics, reading, stenography, or a foreign language) of an assessment.  NEDM: Assessment Content, Academic Subject   */
            public string academicSubjectDescriptor { get; set; }

            /* The typical grade level for which an assessment is designed. If the test assessment spans a range of grades, then this attribute holds the highest grade assessed.  If only one grade level is assessed, then only this attribute is used. For example:  Adult  Prekindergarten  First grade  Second grade  ...   */
            public string assessedGradeLevelDescriptor { get; set; }

            /* The version identifier for the test assessment.  NEDM: Assessment Version   */
            public int? version { get; set; }

        }

        public class AssessmentItemReference
        {
            /* The title or name of the assessment.  NEDM: Assessment Title   */
            public string assessmentTitle { get; set; }

            /* The description of the content or subject area (e.g., arts, mathematics, reading, stenography, or a foreign language) of an assessment.  NEDM: Assessment Content, Academic Subject   */
            public string academicSubjectDescriptor { get; set; }

            /* The typical grade level for which an assessment is designed. If the test assessment spans a range of grades, then this attribute holds the highest grade assessed.  If only one grade level is assessed, then only this attribute is used. For example:  Adult  Prekindergarten  First grade  Second grade  ...   */
            public string assessedGradeLevelDescriptor { get; set; }

            /* The version identifier for the test assessment.  NEDM: Assessment Version   */
            public int? version { get; set; }
        }

        public class AssessmentItem
        {
            /* A reference to the related Assessment resource. */
            public AssessmentReference assessmentReference { get; set; }
        }

        public class Assessment
        {
            /* The title or name of the assessment.  NEDM: Assessment Title   */
            public string title { get; set; }

            /* The description of the content or subject area (e.g., arts, mathematics, reading, stenography, or a foreign language) of an assessment.  NEDM: Assessment Content, Academic Subject   */
            public string academicSubjectDescriptor { get; set; }

            /* The typical grade level for which an assessment is designed. If the test assessment spans a range of grades, then this attribute holds the highest grade assessed.  If only one grade level is assessed, then only this attribute is used. For example:  Adult  Prekindergarten  First grade  Second grade  ...   */
            public string assessedGradeLevelDescriptor { get; set; }

            /* The version identifier for the test assessment.  NEDM: Assessment Version   */
            public int? version { get; set; }
        }
        #endregion

        public class When_building_a_AssessmentItemReference_for_an_ObjectiveAssessmentAssessmentItem :
            When_building_references_where_key_unification_context_has_been_established<AssessmentItemReference, ObjectiveAssessmentAssessmentItem, AssessmentItem>
        {
            // Supplied values
            private const string SuppliedAssessmentTitle = "Assessment Title";
            private const string SuppliedAcademicSubjectDescriptorId = "Subject ID";
            private const string SuppliedAssessedGradeLevelDescriptorId = "Grade Level ID";
            private const int SuppliedVersion = 2;
            
            // Actual values

            // External dependencies

            protected override void Act()
            {
                // Create the value builder
                var builder = new ObjectiveAssessmentAssessmentItem_AssessmentItemReference_ValueBuilder();
                InitializeDependencies(builder);

                var objectiveAssessment = new ObjectiveAssessment
                {
                    assessmentReference = new AssessmentReference
                    {
                        title = SuppliedAssessmentTitle,
                        academicSubjectDescriptor = SuppliedAcademicSubjectDescriptorId,
                        assessedGradeLevelDescriptor = SuppliedAssessedGradeLevelDescriptorId,
                        version = SuppliedVersion
                    }
                };
                
                // Invoke the reference builder
                var buildContext = CreateBuildContext(objectiveAssessment);
                _actualValueBuildResult = builder.TryBuild(buildContext);
            }

            [Assert]
            public void Should_create_4_property_value_constraints()
            {
                Call_to_create_referenced_resource_was_given_property_constraints(constraints =>
                    constraints.Count == 4);
            }

            [Assert]
            public void Should_extract_property_value_constraint_for_specifics_from_the_parents_AssessmentReference()
            {
                Call_to_create_referenced_resource_was_given_property_constraints(constraints =>
                    constraints.ShouldContain("assessmentTitle", SuppliedAssessmentTitle)
                    && constraints.ShouldContain("academicSubjectDescriptor", SuppliedAcademicSubjectDescriptorId)
                    && constraints.ShouldContain("assessedGradeLevelDescriptor", SuppliedAssessedGradeLevelDescriptorId)
                    && constraints.ShouldContain("version", SuppliedVersion));
            }
        }
    }
}