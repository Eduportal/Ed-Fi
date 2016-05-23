// ****************************************************************************
// Copyright (C) 2014 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using EdFi.Ods.Tests._Bases;
using EdFi.Ods.Utilities.LoadGeneration.ReferenceValueBuilders;
namespace EdFi.Ods.Utilities.LoadGeneration.Tests.ResourceReferenceBuilders
{
    public class StudentAssessmentItem_References_ValueBuilder_Fixtures
    {
        #region Supported SDK model class structures
        public class StudentAssessmentItem
        {
            /* A reference to the related AssessmentItem resource. */
            public AssessmentItemReference assessmentItemReference { get; set; }
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

        public class StudentAssessment
        {
            /* A reference to the related Assessment resource. */
            public AssessmentReference assessmentReference { get; set; }
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

        public class AssessmentItem { }
        #endregion

        public class When_building_a_AssessmentItemReference_for_a_StudentAssessmentItem :
        When_building_references_where_key_unification_context_has_been_established<AssessmentItemReference, StudentAssessmentItem, AssessmentItem>
        {
            // Supplied values
            private const string SuppliedAssessmentTitle = "Asmnt-777";
            private const string SuppliedSubjectDescriptor = "Subject-001";
            private const string SuppliedGradeLevelDescriptor = "GL-001";
            private const int SuppliedVersion = 14;
            // Actual values
            // External dependencies
            protected override void Act()
            {
                // Create the value builder
                var builder = new StudentAssessmentItem_AssessmentItemReference_ValueBuilder();
                InitializeDependencies(builder);
                var instance = new StudentAssessment
                {
                    assessmentReference = new AssessmentReference
                    {
                        title = SuppliedAssessmentTitle,
                        academicSubjectDescriptor = SuppliedSubjectDescriptor,
                        assessedGradeLevelDescriptor = SuppliedGradeLevelDescriptor,
                        version = SuppliedVersion
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
            public void Should_extract_property_value_constraint_for_assessment_key_values_from_the_parents_AssessmentReference()
            {
                Call_to_create_referenced_resource_was_given_property_constraints(constraints =>
                constraints.ShouldContain("assessmentTitle", SuppliedAssessmentTitle)
                && constraints.ShouldContain("academicSubjectDescriptor", SuppliedSubjectDescriptor)
                && constraints.ShouldContain("assessedGradeLevelDescriptor", SuppliedGradeLevelDescriptor)
                && constraints.ShouldContain("version", SuppliedVersion));
            }
        }
    }
}
