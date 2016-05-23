using System;
using System.Collections.Generic;
using System.Threading;
using EdFi.Ods.Tests._Bases;
using EdFi.Ods.Utilities.LoadGeneration.Persistence;
using EdFi.TestObjects;
using Rhino.Mocks;

namespace EdFi.Ods.Utilities.LoadGeneration.Tests
{
    public class ResourceGenerationWorkerFixture
    {
        public class When_generating_a_resource : TestFixtureBase
        {
            public class TestResource
            {
                public string Name { get; set; }
            }

            protected IResourceSelector SuppliedResourceSelector;
            protected ITestObjectFactory SuppliedObjectFactory;
            protected IResourcePersister SuppliedResourcePersister;
            protected IApiSdkReflectionProvider SuppliedApiSdkReflectionProvider;

            protected string suppliedResourceName;
            protected TestResource suppliedResource;
            protected Type suppliedResourceType;
            private IProgress<decimal> _suppliedProgress;
            private IResourceCountManager _suppliedResourceCountManager;

            protected override void Arrange()
            {
                
                suppliedResource = new TestResource{ Name = suppliedResourceName };
                suppliedResourceName = "TestResource";
                suppliedResourceType = typeof (TestResource);

                SuppliedResourceSelector = Stub<IResourceSelector>();
                SuppliedObjectFactory = Stub<ITestObjectFactory>();
                SuppliedResourcePersister = Stub<IResourcePersister>();
                SuppliedApiSdkReflectionProvider = Stub<IApiSdkReflectionProvider>();

                SuppliedResourceSelector.Expect(r=>r.GetNextResourceToGenerate()).Return(suppliedResourceName).Repeat.Once();
                SuppliedResourceSelector.Expect(r=>r.GetNextResourceToGenerate()).Return(null).Repeat.Once();

                Type ignoredType;
                SuppliedApiSdkReflectionProvider.Expect(rp => rp.TryGetModelType(suppliedResourceName, out ignoredType))
                    .OutRef(suppliedResourceType)
                    .Return(true);

                SuppliedObjectFactory.Expect(f => f.Create(suppliedResourceType)).Return(suppliedResource);
                
                object reference;
                SuppliedResourcePersister.Expect(p => p.PersistResource(suppliedResource, null, out reference));

                _suppliedResourceCountManager = Stub<IResourceCountManager>();

                _suppliedProgress = Stub<IProgress<decimal>>();
            }

            protected override void Act()
            {
                var resourceGenerator = new ResourceGenerationWorker(SuppliedResourceSelector, SuppliedObjectFactory, 
                    SuppliedResourcePersister, SuppliedApiSdkReflectionProvider, _suppliedResourceCountManager);

                resourceGenerator.Execute(_suppliedProgress, new CancellationToken());
            }

            [Assert]
            public void Should_call_resource_selector_and_get_the_next_resource_to_generate()
            {
                SuppliedResourceSelector.AssertWasCalled(r=>r.GetNextResourceToGenerate());
                SuppliedResourceSelector.AssertWasCalled(r=>r.GetNextResourceToGenerate());
            }

            [Assert]
            public void Should_call_the_api_reflection_provider_to_get_the_type_for_the_resource()
            {
                SuppliedApiSdkReflectionProvider.AssertWasCalled(rp => 
                    rp.TryGetModelType(
                        Arg<string>.Is.Equal(suppliedResourceName),
                        out Arg<Type>.Out(typeof(Type)).Dummy
                        ));
            }

            [Assert]
            public void Should_call_object_factory_with_the_resource_type_to_create()
            {
                SuppliedObjectFactory.AssertWasCalled(f => f.Create(suppliedResourceType));
            }

            [Assert]
            public void Should_call_resource_persister_with_the_resource_from_the_factory()
            {
                object reference = null;
                SuppliedResourcePersister.AssertWasCalled(rp => 
                    rp.PersistResource(
                        Arg<object>.Is.Same(suppliedResource),
                        Arg<IDictionary<string, object>>.Is.NotNull, 
                        out Arg<object>.Out(reference).Dummy));
            }
        }
    }
}
