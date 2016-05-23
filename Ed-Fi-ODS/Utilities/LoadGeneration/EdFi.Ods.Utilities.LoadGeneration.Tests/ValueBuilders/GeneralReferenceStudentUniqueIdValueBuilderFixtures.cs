// ****************************************************************************
//  Copyright (C) 2014 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System;
using System.Collections.Generic;
using EdFi.Ods.Tests._Bases;
using EdFi.Ods.Utilities.LoadGeneration.Infrastructure;
using EdFi.Ods.Utilities.LoadGeneration.Persistence;
using EdFi.Ods.Utilities.LoadGeneration.ValueBuilders;
using EdFi.TestObjects;
using Rhino.Mocks;
using Should;

namespace EdFi.Ods.Utilities.LoadGeneration.Tests.ValueBuilders
{
    // Pseduo-logic
    // Property does not end with "UniqueId"?
    //      Don't handle

    // NOTE: Class will either be a XyzReference, StudentReference, or Student

    // If this is an XyzReference
    // Attempt to gather EdOrg context 
    // First - from containing instance (a reference class, i.e. StudentAcademicRecordReference)
    // Second - move to the parent, and inspect its properties, and then traverse its references

    // If EdOrg source property exists
    // But no ed org context found, 
    //      then ValueBuildResult.Defer
    // Else (No source for Ed Org found)
    // Get an EdOrg value from the Factory, use it as the context (for Students/Parents, get a school, for Staff get an LEA)

    // Determine whether to create a new Student/Staff/Parent, or reuse an existing one
    //   Staff/Parent - Compare progress against the Student, as with the logic for references. 
    //   Student - attempt to reuse first

    // To attempt to resue an existing one, pass the contraints to the ExistingResourceReferenceProvider
    //   Staff - StaffEdOrgAssignmentAssocReference
    //   Student - StudentSchoolAssociationReference
    //   Parent - Use StudentSchoolAssociationReference to get a Student for the school, 
    // then StudentParentAssociationReference to try and get the Parent

    // NOTE: At this point, we MUST have established edOrg context, and possibly studentUniqueId context

    // If creating a new UniqueId...
    //  * Create a new identity in the UniqueId system (call Factory, no containing type)
    //  * Create the new Student/Staff/Parent resource, with "[student|staff|parent]UniqueId" as property constraint
    //  * Create the necessary new primary relationship record(s), (e.g. parents may require a corresponding student)

    public class GeneralReferenceStudentUniqueIdValueBuilderFixtures
    {
        #region API SDK models
        public class Student
        {
            /* The unique identifier of the resource. */
            public string id { get; set; }

            /* A unique alpha-numeric code assigned to a student. */
            public string studentUniqueId { get; set; }
        }

        public class StudentReference
        {
            /* A unique alpha-numeric code assigned to a student. */
            public string studentUniqueId { get; set; }
        }

        public class StudentProgramAssociationReference
        {
            /* A unique alpha-numeric code assigned to a student. */
            public string studentUniqueId { get; set; }

            /* The program associated with the student. */
            public string programType { get; set; }

            /* The formal name of the program of instruction, training, services or benefits available through federal, state, or local agencies. */
            public string programName { get; set; }

            /* The education organization where the student is participating in or receiving the program services. */
            public int? programEducationOrganizationId { get; set; }

            /* The month, day, and year on which the student first received services.  NEDM: Beginning Date */
            public DateTime? beginDate { get; set; }

            /*  */
            public int? educationOrganizationId { get; set; }
        }

        public class StudentSectionAssociationReference
        {
            /* A unique alpha-numeric code assigned to a student. */
            public string studentUniqueId { get; set; }

            /* School Identity Column */
            public int? schoolId { get; set; }
        }

        // This is a general reference class that does not have clear context (ed org without a role name)
        // but a studentUniqueId may never be called due to standard reference building procedures, and the existence of the studentUniqueId/schoolId on upstream references setting the context.
        public class StudentCompetencyObjectiveReference
        {
            /* A unique alpha-numeric code assigned to a student. */
            public string studentUniqueId { get; set; }

            /* EducationOrganization Identity Column */
            public int? gradingPeriodEducationOrganizationId { get; set; }

            /* The name of the grading period during the school year in which the grade is offered (e.g., 1st cycle, 1st semester) */
            public string gradingPeriodDescriptor { get; set; }

            /* Month, day, and year of the first day of the grading period. */
            public DateTime? gradingPeriodBeginDate { get; set; }

            /* The designated title of the learning objective. */
            public string objective { get; set; }

            /* The grade level for which the learning objective is targeted, */
            public string objectiveGradeLevelDescriptor { get; set; }

            /* EducationOrganization Identity Column */
            public int? objectiveEducationOrganizationId { get; set; }

        }
        #endregion

        public class When_considering_building_a_student_UniqueId_for_a_general_reference_type : TestFixtureBase
        {
            // Supplied values

            // Actual values
            private ValueBuildResult _actualGeneralReferenceBuildResult;
            private ValueBuildResult _actualStudentReferenceBuildResult;
            private ValueBuildResult _actualCaseInsensitiveBuildResult;
            private ValueBuildResult _actualStudentBuildResult;
            private ValueBuildResult _actualIntTargetBuildResult;
            private ValueBuildResult _actualNoContainingTypeBuildResult;

            // External dependencies
            private IRandomStudentUniqueIdSelector _randomStudentUniqueIdSelector;

            protected override void Arrange()
            {
                // Set up mocked dependences and supplied values
                _randomStudentUniqueIdSelector = Stub<IRandomStudentUniqueIdSelector>();
            }

            protected override void Act()
            {
                // Perform the action to be tested
                var constraints = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);
                var lineage = new LinkedList<object>(new [] {new StudentProgramAssociationReference()});

                var builder = new GeneralReferenceStudentUniqueIdValueBuilder(
                    _randomStudentUniqueIdSelector, null, null, null);

                // Happy path
                var generalReferenceBuildContext = new BuildContext(
                    "xxx.xxx.studentUniqueId",
                    typeof(string), constraints, typeof(StudentProgramAssociationReference),
                    lineage, BuildMode.Create);

                _actualGeneralReferenceBuildResult = builder.TryBuild(generalReferenceBuildContext);

                // Happy path, case sensitivity
                var caseInsensitiveBuildContext = new BuildContext(
                    "xxx.xxx.studentUnIqUeId",
                    typeof(string), constraints, typeof(StudentProgramAssociationReference),
                    lineage, BuildMode.Create);

                _actualCaseInsensitiveBuildResult = builder.TryBuild(caseInsensitiveBuildContext);

                // Non-string target
                var intTargetBuildContext = new BuildContext(
                    "xxx.xxx.studentUnIqUeId",
                    typeof(int), constraints, typeof(StudentProgramAssociationReference),
                    lineage, BuildMode.Create);

                _actualIntTargetBuildResult = builder.TryBuild(intTargetBuildContext);

                // No containing type
                var noContainingTypeBuildContext = new BuildContext(
                    "xxx.xxx.studentUnIqUeId",
                    typeof(int), constraints, null,
                    new LinkedList<object>(), BuildMode.Create);

                _actualNoContainingTypeBuildResult = builder.TryBuild(noContainingTypeBuildContext);

                // Student reference
                var studentReferenceBuildContext = new BuildContext(
                    "xxx.xxx.studentUniqueId",
                    typeof(string), constraints, typeof(StudentReference),
                    lineage, BuildMode.Create);

                _actualStudentReferenceBuildResult = builder.TryBuild(studentReferenceBuildContext);
                
                // Student
                var studentBuildContext = new BuildContext(
                    "xxx.xxx.studentUniqueId",
                    typeof(string), constraints, typeof(Student),
                    lineage, BuildMode.Create);

                _actualStudentBuildResult = builder.TryBuild(studentBuildContext);
            }

            [Assert]
            public void Should_handle_requests_for_student_UniqueIds_on_general_reference_types()
            {
                _actualGeneralReferenceBuildResult.Handled.ShouldBeTrue();
            }

            [Assert]
            public void Should_handle_requests_for_student_UniqueIds_on_general_reference_types_where_property_name_casing_doesnt_match()
            {
                _actualCaseInsensitiveBuildResult.Handled.ShouldBeTrue();
            }

            [Assert]
            public void Should_not_handle_requests_for_non_string_UniqueIds()
            {
                _actualIntTargetBuildResult.Handled.ShouldBeFalse();
            }

            [Assert]
            public void Should_not_handle_requests_where_there_is_no_containing_type()
            {
                _actualNoContainingTypeBuildResult.Handled.ShouldBeFalse();
            }

            [Assert]
            public void Should_not_handle_requests_for_unique_id_on_a_StudentReference()
            {
                _actualStudentReferenceBuildResult.Handled.ShouldBeFalse();
            }

            [Assert]
            public void Should_not_handle_requests_for_unique_id_on_a_Student()
            {
                _actualStudentBuildResult.Handled.ShouldBeFalse();
            }
        }

        public abstract class When_building_student_uniqueId_for_a_general_reference_type<T> : TestFixtureBase
            where T : class 
        {
            // Supplied values

            // Actual values
            protected ValueBuildResult _actualBuildResult;

            // External dependencies
            protected IRandomStudentUniqueIdSelector _randomStudentUniqueIdSelector;
            protected IApiSdkReflectionProvider _apiSdkReflectionProvider;
            protected IResourcePersister _resourcePersister;
            protected IEducationOrganizationIdentifiersProvider _educationOrganizationIdentifiersProvider;

            protected override void Arrange()
            {
                // Set up mocked dependences and supplied values
                _randomStudentUniqueIdSelector = Stub<IRandomStudentUniqueIdSelector>();
                _educationOrganizationIdentifiersProvider = Stub<IEducationOrganizationIdentifiersProvider>();
            }

            protected override void Act()
            {
                var builder = new GeneralReferenceStudentUniqueIdValueBuilder(
                    _randomStudentUniqueIdSelector, null, null, _educationOrganizationIdentifiersProvider);

                var buildContext = BuildContextWithInstance(GetContainingInstance());
                _actualBuildResult = builder.TryBuild(buildContext);
            }

            protected abstract T GetContainingInstance();

            protected BuildContext BuildContextWithInstance(T instance)
            {
                var constraints = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);
                var lineage = new LinkedList<object>(new T[] {instance});

                lineage.AddFirst(new LinkedListNode<object>(instance));

                // Happy path context, with supplied instance
                var buildContext = new BuildContext(
                    "xxx.xxx.studentUniqueId", typeof(string),
                    constraints,
                    typeof(T),
                    lineage,
                    BuildMode.Create);

                return buildContext;
            }
        }

        public class When_building_student_uniqueId_for_a_general_reference_type_where_there_is_role_named_organization_context_available_but_value_of_the_non_role_named_education_organization_has_not_yet_been_assigned
            : When_building_student_uniqueId_for_a_general_reference_type<StudentProgramAssociationReference>
        {
            protected override StudentProgramAssociationReference GetContainingInstance()
            {
                return new StudentProgramAssociationReference()
                {
                    programEducationOrganizationId = 123, // With role name, it should be ignored for establishin ed org context
                    educationOrganizationId = null,
                };
            }

            [Assert]
            public void Should_defer_building_the_value()
            {
                // Assert the expected results
                _actualBuildResult.ShouldDefer.ShouldBeTrue();
            }
        }

        public class When_building_student_uniqueId_for_a_general_reference_type_where_there_is_organization_context_available_but_value_has_not_yet_been_assigned
            : When_building_student_uniqueId_for_a_general_reference_type<StudentProgramAssociationReference>
        {
            protected override StudentProgramAssociationReference GetContainingInstance()
            {
                return new StudentProgramAssociationReference()
                {
                    educationOrganizationId = null,
                };
            }

            [Assert]
            public void Should_defer_building_the_value()
            {
                // Assert the expected results
                _actualBuildResult.ShouldDefer.ShouldBeTrue();
            }
        }

        public class When_building_student_uniqueId_for_a_general_reference_type_where_there_is_organization_context_available_and_the_value_has_been_assigned_and_there_is_a_student
            : When_building_student_uniqueId_for_a_general_reference_type<StudentProgramAssociationReference>
        {
            private string _suppliedStudentUnqiueId;

            protected override void Arrange()
            {
                string ignored;
                _suppliedStudentUnqiueId = "ABC";

                // Mock that we find the student at the LEA
                _randomStudentUniqueIdSelector = Stub<IRandomStudentUniqueIdSelector>();
                _randomStudentUniqueIdSelector.Expect(x => x.TryGetRandomStudentUniqueId(1111, out ignored))
                    .OutRef(_suppliedStudentUnqiueId)
                    .Return(true);
            }

            protected override StudentProgramAssociationReference GetContainingInstance()
            {
                return new StudentProgramAssociationReference()
                {
                    educationOrganizationId = 1111,
                };
            }

            [Assert]
            public void Should_handle_the_request()
            {
                // Assert the expected results
                _actualBuildResult.Handled.ShouldBeTrue();
            }

            [Assert]
            public void Should_assign_the_value_to_the_return_student_unique_id()
            {
                _actualBuildResult.Value.ShouldEqual("ABC");
            }
        }

        public class When_building_student_uniqueId_for_a_general_reference_type_where_there_is_organization_context_available_and_assigned_but_there_is_NO_student_there
            : When_building_student_uniqueId_for_a_general_reference_type<StudentProgramAssociationReference>
        {
            private ITestObjectFactory _factory;
            private Student _suppliedStudent;
            private IEducationOrganizationIdentifiersProvider _educationOrganizationIdentifiersProvider;

            protected override void Arrange()
            {
                // Mock that there is NO student at the organization
                _randomStudentUniqueIdSelector = Stub<IRandomStudentUniqueIdSelector>();
                _randomStudentUniqueIdSelector.Expect(x => 
                    x.TryGetRandomStudentUniqueId(Arg.Is(1111), out Arg<string>.Out(null).Dummy))
                    .Return(false);

                // Factory will be called to create a new Student
                _factory = Stub<ITestObjectFactory>();
                _suppliedStudent = new Student { studentUniqueId = "ABC" };
                _factory.Expect(x => x.Create(null, null, null, null))
                    .IgnoreArguments()
                    .Return(_suppliedStudent);

                Type ignoredType;
                _apiSdkReflectionProvider = Stub<IApiSdkReflectionProvider>();
                _apiSdkReflectionProvider.Expect(x => x.TryGetModelType("Student", out ignoredType))
                    .OutRef(typeof(Student))
                    .Return(true);

                _resourcePersister = Stub<IResourcePersister>();
            }

            protected override void Act()
            {
                var builder = new GeneralReferenceStudentUniqueIdValueBuilder(
                    _randomStudentUniqueIdSelector, _apiSdkReflectionProvider, _resourcePersister, null)
                {
                    Factory = _factory
                };
                
                var buildContext = BuildContextWithInstance(GetContainingInstance());
                _actualBuildResult = builder.TryBuild(buildContext);
            }

            protected override StudentProgramAssociationReference GetContainingInstance()
            {
                return new StudentProgramAssociationReference()
                {
                    educationOrganizationId = 1111, // Organization is available and assigned
                };
            }

            [Assert]
            public void Should_handle_the_request()
            {
                // Assert the expected results
                _actualBuildResult.Handled.ShouldBeTrue();
            }

            [Assert]
            public void Should_create_a_student_with_the_education_agency_id_as_the_property_constraint()
            {
                // Make sure EdOrg constraint was passed through correctly
                _factory.AssertWasCalled(x => x.Create(
                    Arg<string>.Is.Anything,
                    Arg<Type>.Is.Same(typeof(Student)),
                    Arg<IDictionary<string, object>>
                        .Matches(d => d["educationOrganizationId"].Equals(1111)),
                    Arg<LinkedList<object>>.Is.Anything));
            }

            [Assert]
            public void Should_persist_the_new_student_with_the_education_organization_context()
            {
                _resourcePersister.AssertWasCalled(x => 
                    x.PersistResource(
                        Arg<object>.Is.Same(_suppliedStudent), 
                        Arg<IDictionary<string, object>>.Matches(d =>
                            d["educationOrganizationId"].Equals(1111)
                        ), 
                        out Arg<object>.Out(null).Dummy));
            }

            [Assert]
            public void Should_return_the_newly_built_Students_uniqueId()
            {
                _actualBuildResult.Value.ShouldEqual("ABC");
            }
        }

        public class When_building_student_uniqueId_for_a_general_reference_type_where_there_is_NO_organization_context_available
        : When_building_student_uniqueId_for_a_general_reference_type<StudentCompetencyObjectiveReference>
        {
            private string _suppliedStudentUniqueId;
            private int _suppliedSchoolId;

            protected override void Arrange()
            {
                _suppliedStudentUniqueId = "ABC";
                _suppliedSchoolId = 2222;

                // Mock that we find the student at the LEA
                string ignored;
                _randomStudentUniqueIdSelector = Stub<IRandomStudentUniqueIdSelector>();
                _randomStudentUniqueIdSelector.Expect(x => x.TryGetRandomStudentUniqueId(_suppliedSchoolId, out ignored))
                    .OutRef(_suppliedStudentUniqueId)
                    .Return(true);

                // Mock random selection of index 1
                var random = Stub<IRandom>();
                random.Expect(x => x.Next(0, 2)).Return(1);
                EnumerableExtensions.Random = () => random;

                // Mock the return of two schools for establishing the context
                _educationOrganizationIdentifiersProvider = Stub<IEducationOrganizationIdentifiersProvider>();
                _educationOrganizationIdentifiersProvider.Expect(x => x.GetEducationOrganizationIdentifiers(null))
                    .IgnoreArguments()
                    .Return(new List<EducationOrganizationIdentifiers>()
                    {
                        new EducationOrganizationIdentifiers
                        {
                            EducationOrganizationType = "School",
                            EducationOrganizationId = 1111,
                        },
                        new EducationOrganizationIdentifiers
                        {
                            EducationOrganizationType = "School",
                            EducationOrganizationId = _suppliedSchoolId,
                        },
                    });
            }

            protected override StudentCompetencyObjectiveReference GetContainingInstance()
            {
                // No ed org context available on this reference type
                return new StudentCompetencyObjectiveReference() { };
            }

            [Assert]
            public void Should_handle_the_request()
            {
                // Assert the expected results
                _actualBuildResult.Handled.ShouldBeTrue();
            }

            [Assert]
            public void Should_assign_the_value_to_the_return_student_unique_id()
            {
                _actualBuildResult.Value.ShouldEqual(_suppliedStudentUniqueId);
            }
        }
    }
}