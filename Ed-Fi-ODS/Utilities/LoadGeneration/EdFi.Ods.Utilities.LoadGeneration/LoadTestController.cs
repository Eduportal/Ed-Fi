using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EdFi.Ods.Utilities.LoadGeneration.Security;
using EdFi.TestObjects;
using log4net;

namespace EdFi.Ods.Utilities.LoadGeneration
{
    public interface ILoadTestController
    {
        void GenerateLoad(GenerateLoadRequest request, IProgress<decimal> progressNotifier, CancellationToken cancellationToken);
    }

    public class GenerateLoadRequest
    {
        // Load generation context
        public int StudentCount { get; set; }
        public int ThreadCount { get; set; }
        public string ResourceName { get; set; }
        public string DataProfilePath { get; set; }

        // Security context
        public string ApiKey { get; set; }
        public string ApiSecret { get; set; }
        public string ApiUrl { get; set; }
        public string OAuthUrl { get; set; }
        public int[] LocalEducationAgencyIds { get; set; }
        public string ApiSdkAssemblyPath { get; set; }
        public int SchoolYear { get; set; }
        public bool RefreshCache { get; set; }
    }

    public class LoadTestController : ILoadTestController
    {
        private ILog _logger = LogManager.GetLogger(typeof(LoadTestController));

        private readonly IResourceCountManager _resourceCountManager;
        private readonly IResourceDataProfileProvider _resourceDataProfileProvider;
        private readonly IResourceGenerationWorkerFactory _resourceGenerationWorkerFactory;
        private readonly IApiSecurityContextProvider _apiSecurityContextProvider;
        private readonly ITestObjectFactory _testObjectFactory;
        private readonly IApiSdkReflectionProvider _apiSdkReflectionProvider;
        private Task[] _tasks;

        public LoadTestController(IResourceCountManager resourceCountManager, 
                                  IResourceDataProfileProvider resourceDataProfileProvider, 
                                  IResourceGenerationWorkerFactory resourceGenerationWorkerFactory,
                                  IApiSecurityContextProvider apiSecurityContextProvider,
                                  ITestObjectFactory testObjectFactory,
                                  IApiSdkReflectionProvider apiSdkReflectionProvider)
        {
            _resourceCountManager = resourceCountManager;
            _resourceDataProfileProvider = resourceDataProfileProvider;
            _resourceGenerationWorkerFactory = resourceGenerationWorkerFactory;
            _apiSecurityContextProvider = apiSecurityContextProvider;
            _testObjectFactory = testObjectFactory;
            _apiSdkReflectionProvider = apiSdkReflectionProvider;
        }

        public void GenerateLoad(GenerateLoadRequest request, IProgress<decimal> progressNotifier, CancellationToken cancellationToken)
        {
            ThreadPool.SetMinThreads(request.ThreadCount, request.ThreadCount);
            System.Net.ServicePointManager.DefaultConnectionLimit = request.ThreadCount;

            // Create the security context
            _apiSecurityContextProvider.SetSecurityContext(
                new ApiSecurityContext
                {
                    ApiKey = request.ApiKey, 
                    ApiSecret = request.ApiSecret, 
                    ApiUrl = request.ApiUrl, 
                    OAuthUrl = request.OAuthUrl,
                    LocalEducationAgencyIds = request.LocalEducationAgencyIds,
                    ApiSdkAssemblyPath = request.ApiSdkAssemblyPath,
                    SchoolYear = request.SchoolYear,
                    RefreshCache = request.RefreshCache,
                });

            // Reset the resource counts
            // TODO: Need unit test for this call
            _resourceCountManager.Reset();

            // TODO: Need unit tests for only creating 1 resource
            if (!string.IsNullOrEmpty(request.ResourceName))
            {
                _resourceCountManager.InitializeResourceCount(request.ResourceName, 1);
            }
            else
            {
                // Read profile
                var profiles = _resourceDataProfileProvider.GetResourceDataProfiles(request.DataProfilePath).ToList();

                // Add the resource counts.
                foreach (var resourceDataProfile in profiles)
                {
                    // TODO: Need unit test for checking factory's CanCreate method before adding to the resource count manager
                    Type resourceModelType; 
                    
                    if (!_apiSdkReflectionProvider.TryGetModelType(resourceDataProfile.ResourceName, out resourceModelType))
                    {
                        _logger.WarnFormat("Model type for resource '{0}' could not be found in the supplied REST API SDK.  No instances of this resource will be generated.", 
                            resourceDataProfile.ResourceName);

                        continue;
                    }

                    if (!_testObjectFactory.CanCreate(resourceModelType))
                    {
                        _logger.WarnFormat("The TestObjectFactory's CanCreate delegate has indicated that resource model type '{0}' cannot be created by the factory. No instances of this resource will be generated.",
                            resourceModelType.Name);

                        continue;
                    }

                    var countToGenerate = resourceDataProfile.FixedCount.HasValue
                        ? resourceDataProfile.FixedCount.Value
                        : request.StudentCount * resourceDataProfile.PerStudentRatio;

                    _resourceCountManager.InitializeResourceCount(resourceDataProfile.ResourceName, Convert.ToInt32(countToGenerate));
                }
            }

            // Create thread count workers and start them.
            _logger.InfoFormat("Starting '{0}' worker thread(s).", request.ThreadCount);

            _tasks = new Task[request.ThreadCount];

            for (int i = 0; i < _tasks.Length; i++)
            {
                _tasks[i] = new Task(() =>
                {
                    var worker = _resourceGenerationWorkerFactory.CreateWorker();
                    worker.Execute(progressNotifier, cancellationToken);
                });

                _tasks[i].Start();
            }

            Task.WaitAll(_tasks);
        }
    }
}
