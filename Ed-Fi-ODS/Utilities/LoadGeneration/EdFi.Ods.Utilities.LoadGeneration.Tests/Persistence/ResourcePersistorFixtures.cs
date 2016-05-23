// ****************************************************************************
//  Copyright (C) 2014 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System;
using System.Collections.Generic;
using System.Net;
using EdFi.Ods.Tests._Bases;
using EdFi.Ods.Utilities.LoadGeneration.Persistence;
using RestSharp;
using Rhino.Mocks;
using Should;

namespace EdFi.Ods.Utilities.LoadGeneration.Tests.Persistence
{
    public class ResourcePersistorFixtures
    {
        public class SomeClass { }
        public class SomeClassReference { }

        public class When_persisting_a_resource : TestFixtureBase
        {
            // Supplied values
            private object _suppliedResource;
            private object _suppliedResourceReference;

            // Actual values
            private object _actualReference;

            // External dependencies
            private IApiSdkFacade _apiSdkFacade;
            private IResourceReferenceFactory _resourceReferenceFactory;
            private IExistingResourceReferenceProvider _existingResourceReferenceProvider;
            private IResourceCountManager _resourceCountManager;

            protected override void Arrange()
            {
                // API will always return 200 - OK
                _apiSdkFacade = Stub<IApiSdkFacade>();
                _apiSdkFacade.Expect(x => x.Post(null))
                    .IgnoreArguments()
                    .Return(new RestResponse() {StatusCode = HttpStatusCode.OK});

                // Set up mocked dependences and supplied values
                _suppliedResource = new SomeClass();
                _suppliedResourceReference = new SomeClassReference();
                
                // Mock the reference factory to return the reference
                _resourceReferenceFactory = Stub<IResourceReferenceFactory>();
                _resourceReferenceFactory.Expect(x => x.CreateResourceReference(_suppliedResource))
                    .Return(_suppliedResourceReference);

                _existingResourceReferenceProvider = Stub<IExistingResourceReferenceProvider>();
                _resourceCountManager = Stub<IResourceCountManager>();
            }

            protected override void Act()
            {
                // Perform the action to be tested
                var persister = new ResourcePersister(
                    _apiSdkFacade, _resourceReferenceFactory,
                    _existingResourceReferenceProvider, _resourceCountManager);

                persister.PersistResource(_suppliedResource, new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase), out _actualReference);
            }

            [Assert]
            public void Should_post_the_resource_to_the_API()
            {
                _apiSdkFacade.AssertWasCalled(x => 
                    x.Post(Arg<object>.Is.Same(_suppliedResource)));
            }

            [Assert]
            public void Should_create_a_reference_from_the_resource()
            {
                _resourceReferenceFactory.AssertWasCalled(x => 
                    x.CreateResourceReference(Arg<object>.Is.Same(_suppliedResource)));
            }

            [Assert]
            public void Should_save_the_resource_reference()
            {
                _existingResourceReferenceProvider.AssertWasCalled(x => 
                    x.AddResourceReference(Arg<object>.Is.Same(_suppliedResourceReference)));
            }

            [Assert]
            public void Should_return_the_newly_created_reference_to_the_caller()
            {
                _actualReference.ShouldBeSameAs(_suppliedResourceReference);
            }

            [Assert]
            public void Should_decrement_the_count_for_the_resource_by_name()
            {
                _resourceCountManager.AssertWasCalled(x =>
                    x.DecrementCount(Arg<string>.Is.Equal(typeof(SomeClass).Name)));
            }
        }

        public class When_persisting_a_resource_and_the_POST_operation_fails : TestFixtureBase
        {
            // Supplied values
            private object _suppliedResource;

            // Actual values
            private object _actualReference;

            // External dependencies
            private IApiSdkFacade _apiSdkFacade;
            private IResourceCountManager _resourceCountManager;
            //private IResourceReferenceFactory _resourceReferenceFactory;
            //private IExistingResourceReferenceProvider _existingResourceReferenceProvider;

            protected override void Arrange()
            {
                _apiSdkFacade = Stub<IApiSdkFacade>();
                _apiSdkFacade.Expect(x => x.Post(null))
                    .IgnoreArguments()
                    .Return(new RestResponse() { StatusCode = HttpStatusCode.InternalServerError });

                _resourceCountManager = Stub<IResourceCountManager>();

                _suppliedResource = new SomeClass();
            }

            protected override void Act()
            {
                // Perform the action to be tested
                var persister = new ResourcePersister(_apiSdkFacade, null, null, _resourceCountManager);
                persister.PersistResource(_suppliedResource, new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase), out _actualReference);
            }

            [Assert]
            public void Should_attempt_to_POST_the_resource()
            {
                _apiSdkFacade.AssertWasCalled(x =>
                    x.Post(Arg<object>.Is.Same(_suppliedResource)));
            }

            [Assert]
            public void Should_decrement_the_resource_count_in_spite_of_the_API_failure()
            {
                _resourceCountManager.AssertWasCalled(x =>
                    x.DecrementCount(Arg<string>.Is.Equal(typeof(SomeClass).Name)));
            }

            [Assert]
            public void Should_return_a_null_resource_reference()
            {
                _actualReference.ShouldBeNull();
            }
        }
    }
}