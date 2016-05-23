using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Authentication;
using System.Threading;
using EdFi.Ods.Utilities.LoadGeneration.Persistence;
using EdFi.Ods.Utilities.LoadGeneration._Extensions;
using EdFi.TestObjects;
using log4net;

namespace EdFi.Ods.Utilities.LoadGeneration
{
    public interface IResourceGenerationWorker
    {
        void Execute(IProgress<decimal> progress, CancellationToken cancellationToken);
    }

    public class ResourceGenerationWorker : IResourceGenerationWorker
    {
        private ILog _logger = LogManager.GetLogger(typeof(ResourceGenerationWorker));

        private readonly IResourceSelector resourceSelector;
        private readonly ITestObjectFactory objectFactory;
        private readonly IResourcePersister resourcePersister;
        private readonly IApiSdkReflectionProvider apiSdkReflectionProvider;
        private readonly IResourceCountManager resourceCountManager;

        public ResourceGenerationWorker(IResourceSelector resourceSelector, ITestObjectFactory objectFactory, IResourcePersister resourcePersister, IApiSdkReflectionProvider apiSdkReflectionProvider, IResourceCountManager resourceCountManager)
        {
            this.resourceSelector = resourceSelector;
            this.objectFactory = objectFactory;
            this.resourcePersister = resourcePersister;
            this.apiSdkReflectionProvider = apiSdkReflectionProvider;
            this.resourceCountManager = resourceCountManager;
        }

        public void Execute(IProgress<decimal> progress, CancellationToken cancellationToken)
        {
            var factory = (objectFactory as TestObjectFactory);

            if (factory != null && factory.CancellationToken == default(CancellationToken))
                factory.CancellationToken = cancellationToken;

            while (true)
            {
                try
                {
                    // Exit now
                    if (cancellationToken.IsCancellationRequested)
                    {
                        _logger.WarnFormat("Received cancellation request.");
                        return;
                    }

                    progress.Report(resourceCountManager.TotalProgress);

                    var resourceName = resourceSelector.GetNextResourceToGenerate();

                    // Exit if there are no more resources to generate.
                    if (resourceName == null)
                    {
                        _logger.Info("No more resources found to generate.  Generation worker will now exit.");
                        break;
                    }

                    Type resourceType;

                    // TODO: Unit test needed for failure to find the model
                    if (!apiSdkReflectionProvider.TryGetModelType(resourceName, out resourceType))
                        throw new Exception(string.Format("Unable to find model type for resource '{0}'.", resourceName));

                    var correlationId = Guid.NewGuid().ToString("n").GetHashCode();
                    
                    if (_logger.IsDebugEnabled)
                    {
                        _logger.DebugFormat("Generating an instance of '{0}'. {{CorrelationId: {1}}}", resourceType.Name, correlationId);
                    }
                    else
                    {
                        _logger.InfoFormat("Generating an instance of '{0}'.", resourceType.Name);
                    }
                    
                    var resource = objectFactory.Create(resourceType);

                    object reference;
                    resourcePersister.PersistResource(
                        resource, 
                        new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase), 
                        out reference);

                    _logger.DebugFormat("Finished generating/persisting an instance of '{0}'. {{CorrelationId: {1}}}", resourceType.Name, correlationId);
                }
                catch (OperationCanceledException ex)
                {
                    _logger.FatalFormat("The load generation worker captured a cancellation exception and will now quit.\r\n{0}", ex);

                    // Kill the worker
                    throw;
                }
                catch (Exception ex)
                {
                    if (ex.GetInnerExceptions().Any(x => x is AuthenticationException))
                    {
                        _logger.FatalFormat("The load generation worker encountered an authentication exception while trying to access the REST API and will now quit.\r\n{0}", ex);
                        
                        // Kill the worker
                        throw;
                    }

                    // Log and continue processing
                    _logger.ErrorFormat("Exception encountered by generation worker:\r\n{0}", ex);
                }
            }
        }
    }
}
