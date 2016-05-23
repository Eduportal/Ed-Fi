// ****************************************************************************
//  Copyright (C) 2014 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using EdFi.Ods.Tests._Bases;
using EdFi.Ods.Utilities.LoadGeneration.ReferenceValueBuilders;
using EdFi.TestObjects;
using Should;

namespace EdFi.Ods.Utilities.LoadGeneration.Tests.ResourceReferenceBuilders
{
    public class StudentGradebookEntry_References_ValueBuilder_Fixtures
    {
        #region Supported SDK model class structures
        public class StudentGradebookEntry
        {
            /* A reference to the related GradebookEntry resource. */
            public GradebookEntryReference gradebookEntryReference { get; set; }

            /* A reference to the related StudentSectionAssociation resource. */
            public StudentSectionAssociationReference studentSectionAssociationReference { get; set; }
        }

        public class GradebookEntryReference
        {
            /* School Identity Column */
            public int? schoolId { get; set; }

            /* An indication of the portion of a typical daily session in which students receive instruction in a specified subject (e.g., morning, sixth period, block period, or AB schedules).   NEDM: Class Period   */
            public string classPeriodName { get; set; }

            /* A unique number or alphanumeric code assigned to a room by a school, school system, state, or other agency or entity.   */
            public string classroomIdentificationCode { get; set; }

            /* The local code assigned by the LEA or Campus that identifies the organization of subject matter and related learning experiences provided for the instruction of students.   */
            public string localCourseCode { get; set; }

            /* The name of the term in which the section is offered (e.g., First semester, Second semester, Year long, summer school)   */
            public string termType { get; set; }

            /* The identifier for the school year (e.g., 2010/11).   */
            public int? schoolYear { get; set; }
        }

        public class StudentSectionAssociationReference
        {
            /* A unique alpha-numeric code assigned to a student. */
            public string studentUniqueId { get; set; }

            /* School Identity Column */
            public int? schoolId { get; set; }

            /* An indication of the portion of a typical daily session in which students receive instruction in a specified subject (e.g., morning, sixth period, block period, or AB schedules).   NEDM: Class Period   */
            public string classPeriodName { get; set; }

            /* A unique number or alphanumeric code assigned to a room by a school, school system, state, or other agency or entity.   */
            public string classroomIdentificationCode { get; set; }

            /* The local code assigned by the LEA or Campus that identifies the organization of subject matter and related learning experiences provided for the instruction of students.   */
            public string localCourseCode { get; set; }

            /* The name of the term in which the section is offered (e.g., First semester, Second semester, Year long, summer school)   */
            public string termType { get; set; }

            /* The identifier for the school year (e.g., 2010/11).   */
            public int? schoolYear { get; set; }
        }

        public class GradebookEntry { }
        public class StudentSectionAssociation { }

        #endregion

        public class When_building_a_StudentSectionAssociationReference_for_a_StudentGradebookEntry_where_no_GradebookEntryReference_yet_exists : 
            When_building_references<StudentSectionAssociationReference, StudentGradebookEntry>
        {
            // Supplied values

            // Actual values
            protected ValueBuildResult _actualValueBuildResult;

            // External dependencies

            protected override void Act()
            {
                // Create the value builder
                var builder = new StudentGradebookEntry_StudentSectionAssociationReference_ValueBuilder();

                // Invoke the reference builder
                var buildContext = CreateBuildContext("x.x.x");
                _actualValueBuildResult = builder.TryBuild(buildContext);
            }

            [Assert]
            public void Should_not_handle_the_request_and_allow_it_to_pass_to_the_StandardReferenceValueBuilder()
            {
                _actualValueBuildResult.Handled.ShouldBeFalse();
            }
        }

        public class When_building_a_StudentSectionAssociationReference_for_a_StudentGradebookEntry_where_a_GradebookEntryReference_already_exists : 
            When_building_references_where_key_unification_context_has_been_established<StudentSectionAssociationReference, StudentGradebookEntry, StudentSectionAssociation>
        {
            // Supplied values
            private const int SuppliedSchoolId = 222;
            private const string SuppliedClassPeriodName = "CP-1";
            private const string SuppliedClassroomIdentificationCode = "CLASS-23";
            private const string SuppliedLocalCourseCode = "CRX-00022";
            private const string SuppliedTermType = "T0";
            private const int SuppliedSchoolYear = 2040;

            // Actual values

            // External dependencies

            protected override StudentGradebookEntry CreateContainingInstance()
            {
                // Create a containing model with the other reference already set
                return new StudentGradebookEntry
                {
                    gradebookEntryReference = new GradebookEntryReference
                    {
                        schoolId = SuppliedSchoolId,
                        classPeriodName = SuppliedClassPeriodName,
                        classroomIdentificationCode = SuppliedClassroomIdentificationCode,
                        localCourseCode = SuppliedLocalCourseCode,
                        termType = SuppliedTermType,
                        schoolYear = SuppliedSchoolYear
                    }
                };
            }
            
            protected override void Act()
            {
                // Create the value builder
                var builder = new StudentGradebookEntry_StudentSectionAssociationReference_ValueBuilder();
                InitializeDependencies(builder);

                // Invoke the reference builder
                var buildContext = CreateBuildContext();
                _actualValueBuildResult = builder.TryBuild(buildContext);
            }

            [Assert]
            public void Should_create_6_property_value_constraint()
            {
                Call_to_create_referenced_resource_was_given_property_constraints(constraints =>
                    constraints.Count == 6);
            }

            [Assert]
            public void Should_extract_the_6_unifying_key_values_from_the_GradebookEntryReference_and_provide_as_a_property_value_constraints()
            {
                Call_to_create_referenced_resource_was_given_property_constraints(constraints =>
                    constraints.ShouldContain("schoolId",                       SuppliedSchoolId) 
                    && constraints.ShouldContain("classPeriodName",             SuppliedClassPeriodName)
                    && constraints.ShouldContain("classroomIdentificationCode", SuppliedClassroomIdentificationCode)
                    && constraints.ShouldContain("localCourseCode",             SuppliedLocalCourseCode)
                    && constraints.ShouldContain("termType",                    SuppliedTermType) 
                    && constraints.ShouldContain("schoolYear",                  SuppliedSchoolYear) );
            }
        }

        public class When_building_a_GradebookEntryReference_for_a_StudentGradebookEntry_where_no_StudentSectionAssociationReference_yet_exists : 
            When_building_references<GradebookEntryReference, StudentGradebookEntry>
        {
            // Supplied values

            // Actual values
            protected ValueBuildResult _actualValueBuildResult;

            // External dependencies

            protected override void Act()
            {
                // Create the value builder
                var builder = new StudentGradebookEntry_GradebookEntryReference_ValueBuilder();

                // Invoke the reference builder
                var buildContext = CreateBuildContext("x.x.x");
                _actualValueBuildResult = builder.TryBuild(buildContext);
            }

            [Assert]
            public void Should_not_handle_the_request_and_allow_it_to_pass_to_the_StandardReferenceValueBuilder()
            {
                _actualValueBuildResult.Handled.ShouldBeFalse();
            }
        }

        public class When_building_a_GradebookEntryReference_for_a_StudentGradebookEntry_where_a_StudentSectionAssociationReference_already_exists :
            When_building_references_where_key_unification_context_has_been_established<GradebookEntryReference, StudentGradebookEntry, GradebookEntry>
        {
            // Supplied values
            private const int SuppliedSchoolId = 222;
            private const string SuppliedClassPeriodName = "CP-1";
            private const string SuppliedClassroomIdentificationCode = "CLASS-23";
            private const string SuppliedLocalCourseCode = "CRX-00022";
            private const string SuppliedTermType = "T0";
            private const int SuppliedSchoolYear = 2040;

            // Actual values

            // External dependencies

            protected override StudentGradebookEntry CreateContainingInstance()
            {
                // Create a containing model with the other reference already set
                return new StudentGradebookEntry
                {
                    studentSectionAssociationReference = new StudentSectionAssociationReference
                    {
                        schoolId = SuppliedSchoolId,
                        classPeriodName = SuppliedClassPeriodName,
                        classroomIdentificationCode = SuppliedClassroomIdentificationCode,
                        localCourseCode = SuppliedLocalCourseCode,
                        termType = SuppliedTermType,
                        schoolYear = SuppliedSchoolYear
                    }
                };
            }

            protected override void Act()
            {
                // Create the value builder
                var builder = new StudentGradebookEntry_GradebookEntryReference_ValueBuilder();
                InitializeDependencies(builder);

                // Invoke the reference builder
                var buildContext = CreateBuildContext();
                _actualValueBuildResult = builder.TryBuild(buildContext);
            }

            [Assert]
            public void Should_create_6_property_value_constraint()
            {
                Call_to_create_referenced_resource_was_given_property_constraints(constraints =>
                    constraints.Count == 6);
            }

            [Assert]
            public void Should_extract_the_6_unifying_key_values_from_the_StudentSectionAssociationReference_and_provide_as_a_property_value_constraints()
            {
                Call_to_create_referenced_resource_was_given_property_constraints(constraints =>
                    constraints.ShouldContain("schoolId",                    SuppliedSchoolId) 
                    && constraints.ShouldContain("classPeriodName",             SuppliedClassPeriodName)
                    && constraints.ShouldContain("classroomIdentificationCode", SuppliedClassroomIdentificationCode)
                    && constraints.ShouldContain("localCourseCode",             SuppliedLocalCourseCode)
                    && constraints.ShouldContain("termType",                    SuppliedTermType)
                    && constraints.ShouldContain("schoolYear",                  SuppliedSchoolYear) );
           }
        }
    }
}