using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Authentication;
using log4net;
using Newtonsoft.Json;

namespace EdFi.Ods.Utilities.LoadGeneration.Persistence
{
    /// <summary>
    /// Provides a resource persister that performs general REST API persistence, including supplying the
    /// reference to the <see cref="IExistingReferenceProvider"/> and adjusting generation counts with the
    /// <see cref="IResourceCountManager"/>.
    /// </summary>
    public class ResourcePersister : IResourcePersister
    {
        private ILog _logger = LogManager.GetLogger(typeof(ResourcePersister));

        private readonly IApiSdkFacade _apiSdkFacade;
        private readonly IResourceReferenceFactory _resourceReferenceFactory;
        private readonly IExistingResourceReferenceProvider _existingResourceReferenceProvider;
        private readonly IResourceCountManager _resourceCountManager;

        public ResourcePersister(IApiSdkFacade apiSdkFacade, 
            IResourceReferenceFactory resourceReferenceFactory,
            IExistingResourceReferenceProvider existingResourceReferenceProvider,
            IResourceCountManager resourceCountManager)
        {
            _apiSdkFacade = apiSdkFacade;
            _resourceReferenceFactory = resourceReferenceFactory;
            _existingResourceReferenceProvider = existingResourceReferenceProvider;
            _resourceCountManager = resourceCountManager;
        }

        public void PersistResource(object resource, IDictionary<string, object> context, out object resourceReference)
        {
            // TODO: Needs a unit test for explicit not null check.
            if (context == null)
                throw new ArgumentNullException("context");

            // Initialize the out parameter
            resourceReference = null;

            // TODO: Needs unit test for 'null' resource handling
            if (resource != null)
            {
                try
                {
                    // Save the resource
                    var response = _apiSdkFacade.Post(resource);

                    // If insertion was successful, create the reference
                    if (response.StatusCode == HttpStatusCode.Created
                        || response.StatusCode == HttpStatusCode.OK)
                    {
                        // TODO: Need unit test for condition where resource factory throws an exception
                        // Create the resource's reference
                        try
                        {
                            resourceReference = _resourceReferenceFactory.CreateResourceReference(resource);
                        }
                        catch (Exception ex)
                        {
                            _logger.WarnFormat("Unable to create a resource reference for type '{0}' due to the following exception:\r\n{1}",
                                resource.GetType().Name, ex);
                        }

                        // TODO: Need unit test for condition where resource factory doesn't create a reference
                        if (resourceReference != null)
                        {
                            // Save the reference for reuse
                            _existingResourceReferenceProvider.AddResourceReference(resourceReference);
                        }
                    }
                    else if (response.StatusCode == HttpStatusCode.Unauthorized
                        && response.Content.Contains("\"Authorization has been denied for this request.\""))
                    {
                        // TODO: Need unit tests for receiving Unauthorized response from SDK
                        // Not authorized response needs to stop everything now.
                        throw new AuthenticationException(response.StatusDescription);
                    }
                    else
                    {
                        _logger.WarnFormat(
                            "Persistence of '{0}' failed (POST returned '{1}' with status '{2}'):\r\nResponse Content:\r\n{3}\r\nPost Body:\r\n{4}",
                            resource.GetType().Name, response.StatusCode, response.StatusDescription, response.Content, JsonConvert.SerializeObject(resource, Formatting.Indented));
                    }
                }
                finally
                {
                    // Adjust the resource's count regardless of success (we need to be able to finish the load generation, even when errors occur)
                    _resourceCountManager.DecrementCount(resource.GetType().Name);
                }
            }
        }
    }
}
