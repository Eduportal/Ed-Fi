using EdFi.Ods.Utilities.LoadGeneration.Persistence;
using EdFi.TestObjects;

namespace EdFi.Ods.Utilities.LoadGeneration
{
    public interface IResourceGenerationWorkerFactory
    {
        IResourceGenerationWorker CreateWorker();
    }

    public class ResourceGenerationWorkerFactory : IResourceGenerationWorkerFactory
    {
        private readonly IResourceSelector resourceSelector;
        private readonly ITestObjectFactory objectFactory;
        private readonly IResourcePersister resourcePersister;
        private readonly IApiSdkReflectionProvider apiSdkReflectionProvider;
        private readonly IResourceCountManager resourceCountManager;

        public ResourceGenerationWorkerFactory(IResourceSelector resourceSelector, ITestObjectFactory objectFactory, IResourcePersister resourcePersister, IApiSdkReflectionProvider apiSdkReflectionProvider, IResourceCountManager resourceCountManager)
        {
            this.resourceSelector = resourceSelector;
            this.objectFactory = objectFactory;
            this.resourcePersister = resourcePersister;
            this.apiSdkReflectionProvider = apiSdkReflectionProvider;
            this.resourceCountManager = resourceCountManager;
        }

        public IResourceGenerationWorker CreateWorker()
        {
            return new ResourceGenerationWorker(resourceSelector, objectFactory, resourcePersister, apiSdkReflectionProvider, resourceCountManager);
        }
    }
}
