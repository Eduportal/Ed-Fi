using System;
using System.Collections.Generic;
using EdFi.Ods.Tests._Bases;
using EdFi.Ods.Utilities.LoadGeneration.Persistence;
using EdFi.Ods.Utilities.LoadGeneration.ReferenceValueBuilders;
using EdFi.TestObjects;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Rhino.Mocks;
using Should;

namespace EdFi.Ods.Utilities.LoadGeneration.Tests.ResourceReferenceBuilders
{
    public class StudentCompetencyObjective_References_ValueBuilder_Fixtures
    {
        #region Supported SDK model class structures
        public class StudentCompetencyObjective
        {
            /* A reference to the related StudentProgramAssociation resource. */
            public StudentProgramAssociationReference studentProgramAssociationReference { get; set; }

            /* A reference to the related StudentSectionAssociation resource. */
            public StudentSectionAssociationReference studentSectionAssociationReference { get; set; }
        }

        public class StudentProgramAssociation { }
        public class StudentProgramAssociationReference { }

        public class StudentSectionAssociation { }
        public class StudentSectionAssociationReference { }

        #endregion

        [Ignore("There's a bug in the REST API on StudentCompetencyObjective IStudentCompetencyObjective.StudentUniqueId ONLY checks the program association. Implementation has been modified in the load generator to work around this problem, but this test should be restored once the bug is fixed.")]
        public class When_building_a_StudentProgramAssociationReference_for_a_StudentCompetencyObjective_where_no_StudentSectionAssociationReference_yet_exists_and_program_progress_is_equal_to_section_progress :
            When_building_references<StudentProgramAssociationReference, StudentCompetencyObjective>
        {
            // Supplied values
            private const string SuppliedLogicalPropertyPath = "xxx.xxx.xxx";

            // Actual values
            protected ValueBuildResult _actualValueBuildResult;

            // External dependencies

            protected override void Act()
            {
                // Create the value builder
                var builder = new StudentCompetencyObjective_StudentProgramAssociationReference_ValueBuilder();


                var counterManager = MockRepository.GenerateMock<IResourceCountManager>();
                counterManager.Expect(m => m.GetProgressForResource("StudentSectionAssociation")).Return(0.3m);
                counterManager.Expect(m => m.GetProgressForResource("StudentProgramAssociation")).Return(0.3m);

                builder.ResourceCountManager = counterManager;

                // Invoke the reference builder
                var buildContext = CreateBuildContext(SuppliedLogicalPropertyPath);
                _actualValueBuildResult = builder.TryBuild(buildContext);
            }

            [Assert]
            public void Should_handle_the_request()
            {
                _actualValueBuildResult.Handled.ShouldBeTrue();
            }

            [Assert]
            public void Should_skip_building_the_value_because_the_progress_is_equal_to_the_Section_association_reference_and_it_is_arbitrarily_given_precedence()
            {
                _actualValueBuildResult.ShouldSkip.ShouldBeTrue();
            }
        }

        [Ignore("There's a bug in the REST API on StudentCompetencyObjective IStudentCompetencyObjective.StudentUniqueId ONLY checks the program association. Implementation has been modified in the load generator to work around this problem, but this test should be restored once the bug is fixed.")]
        public class When_building_a_StudentProgramAssociationReference_for_a_StudentCompetencyObjective_where_no_StudentSectionAssociationReference_yet_exists_and_program_progress_is_greater_than_section_progress :
            When_building_references<StudentProgramAssociationReference, StudentCompetencyObjective>
        {
            // Supplied values
            private const string SuppliedLogicalPropertyPath = "xxx.xxx.xxx";

            // Actual values
            protected ValueBuildResult _actualValueBuildResult;

            // External dependencies

            protected override void Act()
            {
                // Create the value builder
                var builder = new StudentCompetencyObjective_StudentProgramAssociationReference_ValueBuilder();


                var counterManager = MockRepository.GenerateMock<IResourceCountManager>();
                counterManager.Expect(m => m.GetProgressForResource("StudentSectionAssociation")).Return(0.3m);
                counterManager.Expect(m => m.GetProgressForResource("StudentProgramAssociation")).Return(0.30001m);
                
                builder.ResourceCountManager = counterManager;

                // Invoke the reference builder
                var buildContext = CreateBuildContext(SuppliedLogicalPropertyPath);
                _actualValueBuildResult = builder.TryBuild(buildContext);
            }

            [Assert]
            public void Should_handle_the_request()
            {
                _actualValueBuildResult.Handled.ShouldBeTrue();
            }

            [Assert]
            public void Should_skip_building_the_value_because_the_progress_of_this_reference_is_ahead_of_the_other_mutually_exclusive_relationship()
            {
                _actualValueBuildResult.ShouldSkip.ShouldBeTrue();
            }
        }

        public class When_building_a_StudentProgramAssociationReference_for_a_StudentCompetencyObjective_where_no_StudentSectionAssociationReference_yet_exists_and_program_progress_is_less_than_section_progress :
            When_building_references<StudentProgramAssociationReference, StudentCompetencyObjective>
        {
            // Supplied values
            private const string SuppliedLogicalPropertyPath = "xxx.xxx.xxx";
            private StudentProgramAssociationReference _suppliedNewlyCreatedReference;
            private StudentProgramAssociation _suppliedStudentProgramAssociation;

            // Actual values
            protected ValueBuildResult _actualValueBuildResult;

            // External dependencies
            private IResourcePersister _resourcePersister;
            private IResourceCountManager _resourceCountManager;
            private ITestObjectFactory _testObjectFactory;
            private IApiSdkReflectionProvider _apiSdkReflectionProvider;

            protected override void Arrange()
            {
                // Arrange for referenced resource to be persisted
                _resourcePersister = Stub<IResourcePersister>();
                
                object ignored;
                _suppliedNewlyCreatedReference = new StudentProgramAssociationReference();

                _resourcePersister.Expect(x => x.PersistResource(null, null, out ignored))
                    .IgnoreArguments()
                    .OutRef(_suppliedNewlyCreatedReference);

                // Test object should only be called to create the underlying StudentProgramAssociation
                // This is not following the AAA style testing because this base class functionality is already tested elsewhere
                _testObjectFactory = mocks.Stub<ITestObjectFactory>();
                _suppliedStudentProgramAssociation = new StudentProgramAssociation();
                _testObjectFactory.Expect(t => t.Create(
                    Arg<string>.Is.Anything, 
                    Arg<Type>.Is.Same(typeof(StudentProgramAssociation)), 
                    Arg<IDictionary<string, object>>.Is.Anything, 
                    Arg<LinkedList<object>>.Is.Anything)) 
                    .Return(_suppliedStudentProgramAssociation);

                // Set up results for the resource count manager so we should handle creating the reference, and create the underlying resource in the process
                _resourceCountManager = mocks.Stub<IResourceCountManager>();
                _resourceCountManager.Expect(m => m.GetProgressForResource("StudentSectionAssociation")).Repeat.Any().Return(0.3m);
                _resourceCountManager.Expect(m => m.GetProgressForResource("StudentProgramAssociation")).Repeat.Any().Return(0.2999m);
                _resourceCountManager.Expect(r => r.GetProgressForStudent()).Repeat.Any().Return(1.0m); // Needed for call to base class to force creation/saving of new resource

                Type ignoredType;
                _apiSdkReflectionProvider = Stub<IApiSdkReflectionProvider>();
                _apiSdkReflectionProvider.Expect(p =>
                    p.TryGetModelType(typeof(StudentProgramAssociationReference), out ignoredType))
                    .OutRef(typeof(StudentProgramAssociation))
                    .Return(true);
            }

            protected override void Act()
            {
                // Create the value builder
                var builder = new StudentCompetencyObjective_StudentProgramAssociationReference_ValueBuilder();
                builder.ResourceCountManager = _resourceCountManager;
                builder.Factory = _testObjectFactory;
                builder.ApiSdkReflectionProvider = _apiSdkReflectionProvider;
                builder.ResourcePersister = _resourcePersister;

                // Invoke the reference builder
                var buildContext = CreateBuildContext(SuppliedLogicalPropertyPath);
                _actualValueBuildResult = builder.TryBuild(buildContext);
            }

            [Assert]
            public void Should_handle_the_request_because_it_is_behind_in_its_progress_and_the_other_reference_has_not_been_generated()
            {
                _actualValueBuildResult.Handled.ShouldBeTrue();
            }

            [Assert]
            public void Should_set_the_value_to_the_resource_reference_returned_from_persisting_the_underlying_resource()
            {
                _actualValueBuildResult.Value.ShouldBeSameAs(_suppliedNewlyCreatedReference);    
            }
        }

        public class When_building_a_StudentProgramAssociationReference_for_a_StudentCompetencyObjective_where_a_StudentSectionAssociationReference_already_exists :
            When_building_references<StudentProgramAssociationReference, StudentCompetencyObjective>
        {
            // Supplied values
            private const string SuppliedLogicalPropertyPath = "xxx.xxx.xxx";

            // Actual values
            protected ValueBuildResult _actualValueBuildResult;

            // External dependencies

            protected override StudentCompetencyObjective CreateContainingInstance()
            {
                // Create a containing model with the other reference already set
                return new StudentCompetencyObjective
                {
                    studentSectionAssociationReference = new StudentSectionAssociationReference()
                };
            }

            protected override void Act()
            {
                // Create the value builder
                var builder = new StudentCompetencyObjective_StudentProgramAssociationReference_ValueBuilder();

                // Invoke the reference builder
                var buildContext = CreateBuildContext(SuppliedLogicalPropertyPath);
                _actualValueBuildResult = builder.TryBuild(buildContext);
            }

            [Assert]
            public void Should_handle_the_request()
            {
                _actualValueBuildResult.Handled.ShouldBeTrue();
            }

            [Assert]
            public void Should_skip_building_the_value_because_the_references_are_intended_to_be_mutually_exclusive()
            {
                _actualValueBuildResult.ShouldSkip.ShouldBeTrue();
            }
        }

        [Ignore("There's a bug in the REST API on StudentCompetencyObjective IStudentCompetencyObjective.StudentUniqueId ONLY checks the program association. Implementation has been modified in the load generator to work around this problem, but this test should be restored once the bug is fixed.")]
        public class When_building_a_StudentSectionAssociationReference_for_a_StudentCompetencyObjective_where_no_StudentProgramAssociationReference_yet_exists_and_section_progress_is_equal_to_program_progress :
            When_building_references<StudentSectionAssociationReference, StudentCompetencyObjective>
        {
            // Supplied values
            private const string SuppliedLogicalPropertyPath = "xxx.xxx.xxx";
            private StudentSectionAssociationReference _suppliedNewlyCreatedReference;
            private StudentSectionAssociation _suppliedStudentSectionAssociation;

            // Actual values
            protected ValueBuildResult _actualValueBuildResult;

            // External dependencies
            private IResourcePersister _resourcePersister;
            private IResourceCountManager _resourceCountManager;
            private ITestObjectFactory _testObjectFactory;
            private IApiSdkReflectionProvider _apiSdkReflectionProvider;

            protected override void Arrange()
            {
                // Arrange for referenced resource to be persisted
                _resourcePersister = Stub<IResourcePersister>();

                object ignored;
                _suppliedNewlyCreatedReference = new StudentSectionAssociationReference();

                _resourcePersister.Expect(x => x.PersistResource(null, null, out ignored))
                    .IgnoreArguments()
                    .OutRef(_suppliedNewlyCreatedReference);

                // Test object should only be called to create the underlying StudentSectionAssociation
                // This is not following the AAA style testing because this base class functionality is already tested elsewhere
                _testObjectFactory = mocks.Stub<ITestObjectFactory>();
                _suppliedStudentSectionAssociation = new StudentSectionAssociation();
                _testObjectFactory.Expect(t => t.Create(
                    Arg<string>.Is.Anything,
                    Arg<Type>.Is.Same(typeof(StudentSectionAssociation)),
                    Arg<IDictionary<string, object>>.Is.Anything,
                    Arg<LinkedList<object>>.Is.Anything))
                    .Return(_suppliedStudentSectionAssociation);

                // Set up results for the resource count manager so we should handle creating the reference, and create the underlying resource in the process
                _resourceCountManager = mocks.Stub<IResourceCountManager>();
                _resourceCountManager.Expect(m => m.GetProgressForResource("StudentProgramAssociation")).Repeat.Any().Return(0.3m);
                _resourceCountManager.Expect(m => m.GetProgressForResource("StudentSectionAssociation")).Repeat.Any().Return(0.3m);
                _resourceCountManager.Expect(r => r.GetProgressForStudent()).Repeat.Any().Return(1.0m); // Needed for call to base class to force creation/saving of new resource

                Type ignoredType;
                _apiSdkReflectionProvider = Stub<IApiSdkReflectionProvider>();
                _apiSdkReflectionProvider.Expect(p =>
                    p.TryGetModelType(typeof(StudentSectionAssociationReference), out ignoredType))
                    .OutRef(typeof(StudentSectionAssociation))
                    .Return(true);
            }

            protected override void Act()
            {
                // Create the value builder
                var builder = new StudentCompetencyObjective_StudentSectionAssociationReference_ValueBuilder();
                builder.ResourceCountManager = _resourceCountManager;
                builder.Factory = _testObjectFactory;
                builder.ApiSdkReflectionProvider = _apiSdkReflectionProvider;
                builder.ResourcePersister = _resourcePersister;

                // Invoke the reference builder
                var buildContext = CreateBuildContext(SuppliedLogicalPropertyPath);
                _actualValueBuildResult = builder.TryBuild(buildContext);
            }

            [Assert]
            public void Should_handle_the_request_because_it_is_behind_in_its_progress_and_the_other_reference_has_not_been_generated()
            {
                _actualValueBuildResult.Handled.ShouldBeTrue();
            }

            [Assert]
            public void Should_set_the_value_to_the_resource_reference_returned_from_persisting_the_underlying_resource()
            {
                _actualValueBuildResult.Value.ShouldBeSameAs(_suppliedNewlyCreatedReference);
            }
        }

        public class When_building_a_StudentSectionAssociationReference_for_a_StudentCompetencyObjective_where_no_StudentProgramAssociationReference_yet_exists_and_section_progress_is_greater_than_program_progress :
            When_building_references<StudentSectionAssociationReference, StudentCompetencyObjective>
        {
            // Supplied values
            private const string SuppliedLogicalPropertyPath = "xxx.xxx.xxx";

            // Actual values
            protected ValueBuildResult _actualValueBuildResult;

            // External dependencies

            protected override void Act()
            {
                // Create the value builder
                var builder = new StudentCompetencyObjective_StudentSectionAssociationReference_ValueBuilder();


                var counterManager = MockRepository.GenerateMock<IResourceCountManager>();
                counterManager.Expect(m => m.GetProgressForResource("StudentProgramAssociation")).Return(0.3m);
                counterManager.Expect(m => m.GetProgressForResource("StudentSectionAssociation")).Return(0.30001m);

                builder.ResourceCountManager = counterManager;

                // Invoke the reference builder
                var buildContext = CreateBuildContext(SuppliedLogicalPropertyPath);
                _actualValueBuildResult = builder.TryBuild(buildContext);
            }

            [Assert]
            public void Should_handle_the_request()
            {
                _actualValueBuildResult.Handled.ShouldBeTrue();
            }

            [Assert]
            public void Should_skip_building_the_value_because_the_progress_of_this_reference_is_ahead_of_the_other_mutually_exclusive_relationship()
            {
                _actualValueBuildResult.ShouldSkip.ShouldBeTrue();
            }
        }

        [Ignore("There's a bug in the REST API on StudentCompetencyObjective IStudentCompetencyObjective.StudentUniqueId ONLY checks the program association. Implementation has been modified in the load generator to work around this problem, but this test should be restored once the bug is fixed.")]
        public class When_building_a_StudentSectionAssociationReference_for_a_StudentCompetencyObjective_where_no_StudentProgramAssociationReference_yet_exists_and_section_progress_is_less_than_program_progress :
            When_building_references<StudentSectionAssociationReference, StudentCompetencyObjective>
        {
            // Supplied values
            private const string SuppliedLogicalPropertyPath = "xxx.xxx.xxx";
            private StudentSectionAssociationReference _suppliedNewlyCreatedReference;
            private StudentSectionAssociation _suppliedStudentSectionAssociation;

            // Actual values
            protected ValueBuildResult _actualValueBuildResult;

            // External dependencies
            private IResourcePersister _resourcePersister;
            private IResourceCountManager _resourceCountManager;
            private ITestObjectFactory _testObjectFactory;
            private IApiSdkReflectionProvider _apiSdkReflectionProvider;

            protected override void Arrange()
            {
                // Arrange for referenced resource to be persisted
                _resourcePersister = Stub<IResourcePersister>();

                object ignored;
                _suppliedNewlyCreatedReference = new StudentSectionAssociationReference();

                _resourcePersister.Expect(x => x.PersistResource(null, null, out ignored))
                    .IgnoreArguments()
                    .OutRef(_suppliedNewlyCreatedReference);

                // Test object should only be called to create the underlying StudentSectionAssociation
                // This is not following the AAA style testing because this base class functionality is already tested elsewhere
                _testObjectFactory = mocks.Stub<ITestObjectFactory>();
                _suppliedStudentSectionAssociation = new StudentSectionAssociation();
                _testObjectFactory.Expect(t => t.Create(
                    Arg<string>.Is.Anything,
                    Arg<Type>.Is.Same(typeof(StudentSectionAssociation)),
                    Arg<IDictionary<string, object>>.Is.Anything,
                    Arg<LinkedList<object>>.Is.Anything))
                    .Return(_suppliedStudentSectionAssociation);

                // Set up results for the resource count manager so we should handle creating the reference, and create the underlying resource in the process
                _resourceCountManager = mocks.Stub<IResourceCountManager>();
                _resourceCountManager.Expect(m => m.GetProgressForResource("StudentProgramAssociation")).Repeat.Any().Return(0.3m);
                _resourceCountManager.Expect(m => m.GetProgressForResource("StudentSectionAssociation")).Repeat.Any().Return(0.2999m);
                _resourceCountManager.Expect(r => r.GetProgressForStudent()).Repeat.Any().Return(1.0m); // Needed for call to base class to force creation/saving of new resource

                Type ignoredType;
                _apiSdkReflectionProvider = Stub<IApiSdkReflectionProvider>();
                _apiSdkReflectionProvider.Expect(p =>
                    p.TryGetModelType(typeof(StudentSectionAssociationReference), out ignoredType))
                    .OutRef(typeof(StudentSectionAssociation))
                    .Return(true);
            }

            protected override void Act()
            {
                // Create the value builder
                var builder = new StudentCompetencyObjective_StudentSectionAssociationReference_ValueBuilder();
                builder.ResourceCountManager = _resourceCountManager;
                builder.Factory = _testObjectFactory;
                builder.ApiSdkReflectionProvider = _apiSdkReflectionProvider;
                builder.ResourcePersister = _resourcePersister;

                // Invoke the reference builder
                var buildContext = CreateBuildContext(SuppliedLogicalPropertyPath);
                _actualValueBuildResult = builder.TryBuild(buildContext);
            }

            [Assert]
            public void Should_handle_the_request_because_it_is_behind_in_its_progress_and_the_other_reference_has_not_been_generated()
            {
                _actualValueBuildResult.Handled.ShouldBeTrue();
            }

            [Assert]
            public void Should_set_the_value_to_the_resource_reference_returned_from_persisting_the_underlying_resource()
            {
                _actualValueBuildResult.Value.ShouldBeSameAs(_suppliedNewlyCreatedReference);
            }
        }

        public class When_building_a_StudentSectionAssociationReference_for_a_StudentCompetencyObjective_where_a_StudentProgramAssociationReference_already_exists :
            When_building_references<StudentSectionAssociationReference, StudentCompetencyObjective>
        {
            // Supplied values
            private const string SuppliedLogicalPropertyPath = "xxx.xxx.xxx";

            // Actual values
            protected ValueBuildResult _actualValueBuildResult;

            // External dependencies

            protected override StudentCompetencyObjective CreateContainingInstance()
            {
                // Create a containing model with the other reference already set
                return new StudentCompetencyObjective
                {
                    studentProgramAssociationReference = new StudentProgramAssociationReference()
                };
            }

            protected override void Act()
            {
                // Create the value builder
                var builder = new StudentCompetencyObjective_StudentSectionAssociationReference_ValueBuilder();

                // Invoke the reference builder
                var buildContext = CreateBuildContext(SuppliedLogicalPropertyPath);
                _actualValueBuildResult = builder.TryBuild(buildContext);
            }

            [Assert]
            public void Should_handle_the_request()
            {
                _actualValueBuildResult.Handled.ShouldBeTrue();
            }

            [Assert]
            public void Should_skip_building_the_value_because_the_references_are_intended_to_be_mutually_exclusive()
            {
                _actualValueBuildResult.ShouldSkip.ShouldBeTrue();
            }
        }
    }
}