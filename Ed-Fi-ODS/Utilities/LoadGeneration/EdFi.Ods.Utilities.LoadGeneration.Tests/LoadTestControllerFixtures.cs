using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EdFi.Ods.Tests._Bases;
using EdFi.Ods.Utilities.LoadGeneration.Security;
using EdFi.TestObjects;
using NUnit.Framework;
using Rhino.Mocks;
using Should;

namespace EdFi.Ods.Utilities.LoadGeneration.Tests
{
    [TestFixture]
    public class LoadTestControllerFixtures : TestFixtureBase
    {
        public class FakeWorker : IResourceGenerationWorker
        {
            public void Execute(IProgress<decimal> progress, CancellationToken cancellationToken)
            {
                // Make note that this has been started
                Started = true;
            }

            public bool Started { get; set; }
        }

        private IResourceCountManager suppliedResourceCountManager;
        private IResourceDataProfileProvider suppliedResourceDataProfileProvider;
        private IResourceGenerationWorkerFactory suppliedWorkerFactory;

        private GenerateLoadRequest suppliedRequest;
        private IEnumerable<ResourceDataProfile> suppliedResourceDataProfiles;
        private List<FakeWorker> suppliedWorkers;
        private IApiSecurityContextProvider _suppliedApiSecurityContextProvider;
        private IApiSdkReflectionProvider _apiSdkReflectionProvider;
        private ITestObjectFactory _testObjectFactory;

        protected override void Arrange()
        {
            suppliedRequest = new GenerateLoadRequest
            {
                StudentCount = 2,
                ThreadCount = 3,
                DataProfilePath = "Some.csv",
            };

            suppliedResourceDataProfiles = GetSuppliedResourceDataProfile();

            suppliedResourceDataProfileProvider = Stub<IResourceDataProfileProvider>();
            suppliedResourceCountManager = Stub<IResourceCountManager>();
            suppliedWorkerFactory = Stub<IResourceGenerationWorkerFactory>();
            _suppliedApiSecurityContextProvider = Stub<IApiSecurityContextProvider>();

            Type ignored;
            _apiSdkReflectionProvider = Stub<IApiSdkReflectionProvider>();
            _apiSdkReflectionProvider.Expect(x => x.TryGetModelType(null as string, out ignored))
                .IgnoreArguments()
                .OutRef(typeof(object))
                .Return(true);

            _testObjectFactory = Stub<ITestObjectFactory>();
            _testObjectFactory.CanCreate = x => true;

            // The method in test is void so we just want to test that everything gets called correctly.
            suppliedResourceDataProfileProvider.Expect(p => p.GetResourceDataProfiles(suppliedRequest.DataProfilePath))
                .Return(suppliedResourceDataProfiles);

            foreach (var suppliedProfile in suppliedResourceDataProfiles)
            {
                var expectedCount = suppliedProfile.FixedCount.HasValue
                    ? suppliedProfile.FixedCount.Value
                    : suppliedRequest.StudentCount * suppliedProfile.PerStudentRatio;

                suppliedResourceCountManager.Expect(
                    r => r.InitializeResourceCount(suppliedProfile.ResourceName, (int) expectedCount));
            }

            suppliedWorkers = new List<FakeWorker>();

            for (int i = 0; i < suppliedRequest.ThreadCount; i++)
            {
                var worker = new FakeWorker();
                suppliedWorkers.Add(worker);

                suppliedWorkerFactory.Expect(wf => wf.CreateWorker()).Return(worker).Repeat.Once();
            }

            //suppliedWorkerFactory.Expect(wf=>wf.CreateWorker()).Return(new FakeWorker()).Repeat.Times(suppliedArguments.ThreadCount);
        }

        protected IEnumerable<ResourceDataProfile> GetSuppliedResourceDataProfile()
        {
            return new List<ResourceDataProfile>
            {
                new ResourceDataProfile{ ResourceName = "One", FixedCount = 1},
                new ResourceDataProfile{ ResourceName = "Two", FixedCount = null, PerStudentRatio = 2},
            };
        }

        protected override void Act()
        {
            var controller = new LoadTestController(suppliedResourceCountManager,
                suppliedResourceDataProfileProvider,
                suppliedWorkerFactory,
                _suppliedApiSecurityContextProvider,
                _testObjectFactory,
                _apiSdkReflectionProvider);

            controller.GenerateLoad(suppliedRequest, null, new CancellationToken());
        }

        [Assert]
        public void Should_call_ResourceDataProfileProvider_GetResourceDataProfiles_with_supplied_profile_path()
        {
            suppliedResourceDataProfileProvider.AssertWasCalled(p => p.GetResourceDataProfiles(suppliedRequest.DataProfilePath));
        }

        [Assert]
        public void Should_call_ResourceCountManager_InitializeResourceCount_with_supplied_profile_data()
        {
            foreach (var suppliedProfile in suppliedResourceDataProfiles)
            {
                var expectedCount = suppliedProfile.FixedCount.HasValue
                    ? suppliedProfile.FixedCount.Value
                    : suppliedRequest.StudentCount * suppliedProfile.PerStudentRatio;

                suppliedResourceCountManager.AssertWasCalled(
                    r => r.InitializeResourceCount(suppliedProfile.ResourceName, (int)expectedCount));
            }
        }

        [Assert]
        public void Should_create_workers_for_n_number_of_suppied_arguments_thread_count()
        {
            suppliedWorkerFactory.AssertWasCalled(wf => wf.CreateWorker(),options=>options.Repeat.Times(suppliedRequest.ThreadCount));
        }

        [Assert]
        public void Should_start_all_workers()
        {
            suppliedWorkers.All(w => w.Started).ShouldBeTrue();
        }
    }
}
