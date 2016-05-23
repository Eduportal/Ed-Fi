using System;
using System.Net;
using System.Security.Authentication;
using log4net;
using RestSharp;

namespace EdFi.Ods.Utilities.LoadGeneration
{
    /// <summary>
    /// Implements a unique Id factory that returns a GUID-based string value.
    /// </summary>
    public class SimpleGuidBasedUniqueIdFactory : IUniqueIdFactory
    {
        /// <summary>
        /// Creates a new GUID-based UniqueId, ignoring the supplied identity information.
        /// </summary>
        /// <param name="firstName">This parameter is ignored by this implementation.</param>
        /// <param name="lastSurname">This parameter is ignored by this implementation.</param>
        /// <param name="birthDate">This parameter is ignored by this implementation.</param>
        /// <param name="sexType">This parameter is ignored by this implementation.</param>
        /// <returns>The newly created UniqueId value.</returns>
        public string CreateUniqueId(string firstName, string lastSurname, DateTime? birthDate, string sexType)
        {
            return Guid.NewGuid().ToString("n");
        }
    }

    public class EduIdBasedUniqueIdFactory : IUniqueIdFactory
    {
        private ILog _logger = LogManager.GetLogger(typeof(EduIdBasedUniqueIdFactory));
        
        private readonly IApiSdkReflectionProvider _apiSdkReflectionProvider;
        private readonly IApiSdkFacade _apiSdkFacade;

        public EduIdBasedUniqueIdFactory(IApiSdkReflectionProvider apiSdkReflectionProvider, IApiSdkFacade apiSdkFacade)
        {
            _apiSdkReflectionProvider = apiSdkReflectionProvider;
            _apiSdkFacade = apiSdkFacade;
        }

        public string CreateUniqueId(string firstName, string lastSurname, DateTime? birthDate, string sexType)
        {
            Type identityModelType;

            if (!_apiSdkReflectionProvider.TryGetModelType("Identity", out identityModelType))
                throw new Exception("Unable to locate 'Identity' model type in the REST API SDK.");

            dynamic identity = Activator.CreateInstance(identityModelType);

            identity.uniqueId = Guid.NewGuid().ToString("n");
            identity.givenNames = firstName;
            identity.familyNames = lastSurname;
            identity.birthDate = birthDate.HasValue ? birthDate.Value.ToString("yyyy-MM-dd") : string.Empty;
            identity.birthGender = sexType;

            _logger.InfoFormat("Creating an identity...");
            var response = _apiSdkFacade.Post(identity);

            if (response.StatusCode == HttpStatusCode.OK
                || response.StatusCode == HttpStatusCode.Created)
            {
                _logger.DebugFormat("Identity POST succeeded, returning status code '{0}' ({1}):\r\nContent:{2}.", response.StatusCode, response.StatusDescription, response.Content);
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
                _logger.ErrorFormat("Identity POST failed, returning status code '{0}' ({1}):\r\nContent:{2}.", response.StatusCode, response.StatusDescription, response.Content);
            }

            string uniqueId = response.Data.uniqueId;

            if (string.IsNullOrEmpty(uniqueId))
                throw new Exception("UniqueId value was not created.");
                
            _logger.InfoFormat("UniqueId '{0}' created.", uniqueId);

            return uniqueId;

            // Example Identity API usage
            //var identitiesApi = new IdentitiesApi(withoutSchoolYear);
            //var identity = new Sdk.Models.Identity
            //{
            //    birthGender = "Female",
            //    birthDate = "2011/04/05",
            //    familyNames = "Johnson",
            //    givenNames = "Cindy"
            //};

            //var postResponse = identitiesApi.PostIdentity(identity);
        }
    }
}