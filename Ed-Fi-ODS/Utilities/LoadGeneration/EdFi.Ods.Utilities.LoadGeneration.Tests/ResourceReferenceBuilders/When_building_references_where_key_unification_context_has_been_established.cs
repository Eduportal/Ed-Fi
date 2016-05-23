using System;
using System.Collections.Generic;
using EdFi.Ods.Tests._Bases;
using EdFi.Ods.Utilities.LoadGeneration.Persistence;
using EdFi.TestObjects;
using Rhino.Mocks;
using Rhino.Mocks.Exceptions;
using Should;

namespace EdFi.Ods.Utilities.LoadGeneration.Tests.ResourceReferenceBuilders
{
    public class When_building_references_where_key_unification_context_has_been_established<TReference, TContainer, TReferencedResource> : When_building_references<TReference,TContainer> 
        where TReference : new() 
        where TReferencedResource : new() 
        where TContainer : new() 
    {
        // Supplied values
        private const decimal suppliedResourcePercentage = 0.3999999m;
        private const decimal suppliedStudentPercentage = 0.4m;
        protected string suppliedLogicalPropertyPath = "Some.Logical.Path";
        private TReferencedResource _suppliedCreatedResource;
        private TReference _suppliedCreatedReference;

        // Actual values
        protected ValueBuildResult _actualValueBuildResult;

        // External dependencies
        protected IResourceCountManager _resourceCountManager;
        protected IResourcePersister _resourcePersister;
        protected IExistingResourceReferenceProvider _existingResourceReferenceProvider;
        protected ITestObjectFactory _testObjectFactory;
        protected IApiSdkReflectionProvider _apiSdkReflectionProvider;

        protected override void Arrange()
        {
            // Set up mocked dependences and supplied values
            _resourcePersister = Stub<IResourcePersister>();
            object ignored;
            _suppliedCreatedReference = new TReference();

            _resourcePersister.Expect(x => x.PersistResource(null, null, out ignored))
                .IgnoreArguments()
                .OutRef(_suppliedCreatedReference);

            // Mocks counts so progress is behind the student, so that resources are created
            _resourceCountManager = mocks.StrictMock<IResourceCountManager>();
            
            _resourceCountManager.Expect(r => r.GetProgressForResource(typeof(TReferencedResource).Name))
                .Return(suppliedResourcePercentage);

            _resourceCountManager.Expect(r => r.GetProgressForStudent())
                .Return(suppliedStudentPercentage);

            // Use strict mock with no expectations to prevent any calls to obtain an existing dependency
            _existingResourceReferenceProvider = mocks.StrictMock<IExistingResourceReferenceProvider>();

            _suppliedCreatedResource = new TReferencedResource();
            _testObjectFactory = Stub<ITestObjectFactory>();
            InitializeTestObjectFactoryStub();

            Type ignoredType;
            _apiSdkReflectionProvider = Stub<IApiSdkReflectionProvider>();
            _apiSdkReflectionProvider.Expect(p => p.TryGetModelType(typeof(TReference), out ignoredType))
                .OutRef(typeof(TReferencedResource))    
                .Return(true);
        }

        protected virtual void InitializeTestObjectFactoryStub()
        {
            _testObjectFactory.Expect(t => t.Create(
                    Arg<string>.Is.Equal(string.Empty), 
                    Arg<Type>.Is.Same(typeof(TReferencedResource)), 
                    Arg<IDictionary<string,object>>.Is.Anything, 
                    Arg<LinkedList<object>>.Is.Anything))
                .Return(_suppliedCreatedResource);
        }

        protected BuildContext CreateBuildContext(params object[] objectGraphAncestors)
        {
            return base.CreateBuildContext(suppliedLogicalPropertyPath, objectGraphAncestors);
        }

        protected void Call_to_create_referenced_resource_was_given_property_constraints(Func<IDictionary<string, object>, bool> dictionaryPredicate)
        {
            try
            {
                _testObjectFactory.AssertWasCalled(factory => factory.Create(
                    Arg<string>.Is.Equal(string.Empty),
                    Arg<Type>.Is.Equal(typeof(TReferencedResource)),
                    Arg<IDictionary<string, object>>.Matches(d => dictionaryPredicate(d)), 
                    Arg<LinkedList<object>>.Is.Anything));
            }
            catch (ExpectationViolationException ex)
            {
                throw new Exception("Specified property constraints were not found.", ex);
            }
        }

        [Assert]
        public void Should_return_the_reference_for_the_newly_created_resource()
        {
            _actualValueBuildResult.Value.ShouldBeSameAs(_suppliedCreatedReference);
        }

        [Assert]
        public void Should_handle_building_the_reference()
        {
            // Assert the expected results
            _actualValueBuildResult.Handled.ShouldBeTrue();
        }

        protected void InitializeDependencies(dynamic referenceValueBuilder)
        {
            // Boilerplate initialization of injected properties
            referenceValueBuilder.ResourceCountManager = _resourceCountManager;
            referenceValueBuilder.ResourcePersister = _resourcePersister;
            referenceValueBuilder.ExistingResourceReferenceProvider = _existingResourceReferenceProvider;
            referenceValueBuilder.Factory = _testObjectFactory;
            referenceValueBuilder.ApiSdkReflectionProvider = _apiSdkReflectionProvider;
        }
    }
}