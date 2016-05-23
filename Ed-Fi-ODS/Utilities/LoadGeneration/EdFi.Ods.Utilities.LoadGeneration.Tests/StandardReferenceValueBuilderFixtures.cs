using System;
using System.Collections.Generic;
using EdFi.Ods.Utilities.LoadGeneration.Persistence;
using EdFi.Ods.Utilities.LoadGeneration.ReferenceValueBuilders;
using EdFi.TestObjects;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.Mocks.Expectations;
using Should;
using EdFi.Ods.Tests._Bases;

namespace EdFi.Ods.Utilities.LoadGeneration.Tests
{
    public class StandardReferenceValueBuilderFixtures
    {
        internal class TestTypeReference
        {
            public bool PreviouslyExisting { get; set; }
        }

        internal class TestType { }

        internal class TestTypeNonReferenceName
        {
        }

        public class When_the_target_type_does_not_end_in_Resource : TestFixtureBase
        {
            private StandardReferenceValueBuilder _standardReferenceValueBuilder;
            private ValueBuildResult _buildResult;

            // External dependencies
            protected IResourceCountManager _resourceCountManager;
            protected IResourcePersister _resourcePersister;
            protected IExistingResourceReferenceProvider _existingResourceReferenceProvider;
            protected ITestObjectFactory _testObjectFactory;
            protected IApiSdkReflectionProvider _apiSdkReflectionProvider;

            protected override void Arrange()
            {
                // Instances are required, but no behavior used in this fixture
                _resourceCountManager = Stub<IResourceCountManager>();
                _resourcePersister = Stub<IResourcePersister>();
                _existingResourceReferenceProvider = Stub<IExistingResourceReferenceProvider>();
                _testObjectFactory = Stub<ITestObjectFactory>();
                _apiSdkReflectionProvider = Stub<IApiSdkReflectionProvider>();
            }

            protected override void Act()
            {
                _standardReferenceValueBuilder = new StandardReferenceValueBuilder()
                {
                    // Boilerplate initialization
                    ResourceCountManager = _resourceCountManager,
                    ResourcePersister = _resourcePersister,
                    ExistingResourceReferenceProvider = _existingResourceReferenceProvider,
                    Factory = _testObjectFactory,
                    ApiSdkReflectionProvider = _apiSdkReflectionProvider,
                };

                _buildResult = _standardReferenceValueBuilder.TryBuild(new BuildContext(null, typeof(TestTypeNonReferenceName), null, null, null, BuildMode.Create));
            }

            [Assert]
            public void Should_not_handle_building_the_value()
            {
                Assert.That(_buildResult.Handled, Is.False);
            }
        }

        public class When_student_progress_is_greater_than_resource_progress : TestFixtureBase
        {
            private IResourceCountManager _resourceCountManager;
            private IResourcePersister _resourcePersister;
            private IExistingResourceReferenceProvider _existingResourceReferenceProvider;
            private StandardReferenceValueBuilder _standardReferenceValueBuilder;
            private ValueBuildResult _buildResult;
            private ITestObjectFactory _testObjectFactory;
            private IApiSdkReflectionProvider _apiSdkReflectionProvider;

            private const decimal suppliedResourcePercentage = 0.3999999m;
            private const decimal suppliedStudentPercentage = 0.4m;
            IDictionary<string, object> suppliedPropertyValueConstraints = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);
            private object _suppliedNewlyCreatedReference;

            protected override void Arrange()
            {
                _resourcePersister = Stub<IResourcePersister>();
                object ignored;
                _suppliedNewlyCreatedReference = new TestTypeReference() { PreviouslyExisting = false };

                _resourcePersister.Expect(x => x.PersistResource(null, null, out ignored))
                    .IgnoreArguments()
                    .OutRef(_suppliedNewlyCreatedReference);

                _resourceCountManager = mocks.StrictMock<IResourceCountManager>();
                _resourceCountManager.Expect(r => r.GetProgressForResource(typeof(TestType).Name)).Return(suppliedResourcePercentage);
                _resourceCountManager.Expect(r => r.GetProgressForStudent()).Return(suppliedStudentPercentage);

                _existingResourceReferenceProvider = Stub<IExistingResourceReferenceProvider>();
                _existingResourceReferenceProvider.Expect(e => e.GetResourceReference(typeof(TestTypeReference), null))
                    .Return(new TestTypeReference() { PreviouslyExisting = true }); // If existingResourceReference provider is called, return "existing" instance

                _testObjectFactory = Stub<ITestObjectFactory>();
                _testObjectFactory.Expect(t => t.Create(Arg<string>.Is.Anything, Arg<Type>.Is.Same(typeof(TestType)), Arg<IDictionary<string, object>>.Is.Anything, Arg<LinkedList<object>>.Is.Anything)) //If the testObjectFactory is called, build a "new" resource.
                    .Return(new TestType());

                Type ignoredType;
                _apiSdkReflectionProvider = Stub<IApiSdkReflectionProvider>();
                _apiSdkReflectionProvider.Expect(p => p.TryGetModelType(typeof(TestTypeReference), out ignoredType))
                    .OutRef(typeof(TestType))
                    .Return(true);
            }

            protected override void Act()
            {
                _standardReferenceValueBuilder = new StandardReferenceValueBuilder()
                {
                    ResourceCountManager = _resourceCountManager, 
                    ResourcePersister = _resourcePersister, 
                    ExistingResourceReferenceProvider = _existingResourceReferenceProvider,
                    Factory = _testObjectFactory,
                    ApiSdkReflectionProvider = _apiSdkReflectionProvider,
                };

                var buildContext = new BuildContext("", typeof(TestTypeReference), suppliedPropertyValueConstraints, null, null, BuildMode.Create);
                _buildResult = _standardReferenceValueBuilder.TryBuild(buildContext);
            }

            [Assert]
            public void Should_handle_building_the_value()
            {
                Assert.That(_buildResult.Handled, Is.True);
            }

            [Assert]
            public void Should_create_a_new_resource_object()
            {
                _buildResult.Value.ShouldBeSameAs(_suppliedNewlyCreatedReference);
            }
        }

        public class When_resource_progress_is_greater_than_student_progress_and_a_reference_exists : TestFixtureBase
        {
            private IResourceCountManager _resourceCountManager;
            private IResourcePersister _resourcePersister;
            private IExistingResourceReferenceProvider _existingResourceReferenceProvider;
            private StandardReferenceValueBuilder _standardReferenceValueBuilder;
            private ValueBuildResult _actualBuildResult;
            private ITestObjectFactory _testObjectFactory;
            private IApiSdkReflectionProvider _apiSdkReflectionProvider;

            private const decimal suppliedResourcePercentage = 0.4m;
            private const decimal suppliedStudentPercentage = 0.3999999m;
            IDictionary<string, object> suppliedPropertyValueConstraints = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);

            protected override void Arrange()
            {
                _resourcePersister = Stub<IResourcePersister>();

                _resourceCountManager = mocks.StrictMock<IResourceCountManager>();
                _resourceCountManager.Expect(r => r.GetProgressForResource(typeof(TestType).Name)).Return(suppliedResourcePercentage);
                _resourceCountManager.Expect(r => r.GetProgressForStudent()).Return(suppliedStudentPercentage);

                _existingResourceReferenceProvider = mocks.StrictMock<IExistingResourceReferenceProvider>();
                _existingResourceReferenceProvider.Expect(e => e.GetResourceReference(null, null))
                    .IgnoreArguments()
                    .Return(new TestTypeReference() { PreviouslyExisting = true }); // If existingResourceReference provider is called, return "existing" instance

                _testObjectFactory = Stub<ITestObjectFactory>();
                _testObjectFactory.Expect(t => t.Create(null, null, null, null)) //If the testObjectFactory is called, build a "new" resource.
                    .IgnoreArguments()
                    .Return(new TestTypeReference() { PreviouslyExisting = false });

                Type ignoredType;
                _apiSdkReflectionProvider = Stub<IApiSdkReflectionProvider>();
                _apiSdkReflectionProvider.Expect(p => p.TryGetModelType(typeof(TestTypeReference), out ignoredType))
                    .OutRef(typeof(TestType))
                    .Return(true);
            }

            protected override void Act()
            {
                _standardReferenceValueBuilder = new StandardReferenceValueBuilder()
                {
                    ResourceCountManager = _resourceCountManager,
                    ResourcePersister = _resourcePersister,
                    ExistingResourceReferenceProvider = _existingResourceReferenceProvider,
                    Factory = _testObjectFactory,
                    ApiSdkReflectionProvider = _apiSdkReflectionProvider,
                };

                var buildContext = new BuildContext("", typeof (TestTypeReference), null, null, null, BuildMode.Create);
                _actualBuildResult = _standardReferenceValueBuilder.TryBuild(buildContext);
            }

            [Assert]
            public void Should_handle_building_the_value()
            {
                Assert.That(_actualBuildResult.Handled, Is.True);
            }

            [Assert]
            public void Should_use_existing_reference_object()
            {
                var builtTestTypeReference = (TestTypeReference)_actualBuildResult.Value;
                Assert.That(builtTestTypeReference.PreviouslyExisting, Is.True);
                // Only existing types will have this set as true. If it was generated, it will be false.
            }
        }

        public class When_resource_progress_is_greater_than_student_progress_and_an_existing_reference_DOES_NOT_exist : TestFixtureBase
        {
            // Supplied values
            private const decimal suppliedResourcePercentage = 0.4m;
            private const decimal suppliedStudentPercentage = 0.3999999m;
            private object _suppliedNewlyCreatedReference;

            // Actual values
            private ValueBuildResult _actualValueBuildResult;

            // External dependencies
            private StandardReferenceValueBuilder _standardReferenceValueBuilder;
            private IResourceCountManager _resourceCountManager;
            private IExistingResourceReferenceProvider _existingResourceReferenceProvider;
            private ITestObjectFactory _testObjectFactory;
            private IResourcePersister _resourcePersister;
            private IApiSdkReflectionProvider _apiSdkReflectionProvider;

            protected override void Arrange()
            {
                // Set up mocked dependences and supplied values
                _resourcePersister = Stub<IResourcePersister>();
                object ignored;
                _suppliedNewlyCreatedReference = new TestTypeReference() { PreviouslyExisting = false };

                _resourcePersister.Expect(x => x.PersistResource(null, null, out ignored))
                    .IgnoreArguments()
                    .OutRef(_suppliedNewlyCreatedReference);

                _resourceCountManager = mocks.StrictMock<IResourceCountManager>();
                _resourceCountManager.Expect(r => r.GetProgressForResource(typeof(TestType).Name)).Return(suppliedResourcePercentage);
                _resourceCountManager.Expect(r => r.GetProgressForStudent()).Return(suppliedStudentPercentage);

                _existingResourceReferenceProvider = Stub<IExistingResourceReferenceProvider>();
                _existingResourceReferenceProvider.Expect(e => e.GetResourceReference(typeof(TestTypeReference), null))
                    .Return(null); // If existingResourceReference provider is called, return null instance

                _testObjectFactory = Stub<ITestObjectFactory>();
                _testObjectFactory.Expect(t => t.Create(null, null, null, null)) //If the testObjectFactory is called, build a "new" resource.
                    .IgnoreArguments()
                    .Return(new TestTypeReference() { PreviouslyExisting = false });

                Type ignoredType;
                _apiSdkReflectionProvider = Stub<IApiSdkReflectionProvider>();
                _apiSdkReflectionProvider.Expect(p => p.TryGetModelType(typeof(TestTypeReference), out ignoredType))
                    .OutRef(typeof(TestType))
                    .Return(true);
            }

            protected override void Act()
            {
                // Perform the action to be tested
                _standardReferenceValueBuilder = new StandardReferenceValueBuilder()
                {
                    ResourceCountManager = _resourceCountManager,
                    ResourcePersister = _resourcePersister,
                    ExistingResourceReferenceProvider = _existingResourceReferenceProvider,
                    Factory = _testObjectFactory,
                    ApiSdkReflectionProvider = _apiSdkReflectionProvider,
                };

                _actualValueBuildResult = _standardReferenceValueBuilder.TryBuild(new BuildContext("", typeof(TestTypeReference), null, null, null, BuildMode.Create));
            }

            [Assert]
            public void Should_handle_building_the_value()
            {
                _actualValueBuildResult.Handled.ShouldBeTrue();
            }

            [Assert]
            public void Should_fall_back_to_creating_a_new_reference()
            {
                ((TestTypeReference) _actualValueBuildResult.Value).PreviouslyExisting.ShouldBeFalse();
            }
        }
    }
}
