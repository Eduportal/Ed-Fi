using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web.Http;
using EdFi.Ods.Admin.Models;
using EdFi.Ods.Admin.Models.Client;
using EdFi.Ods.Admin.Services;

namespace EdFi.Ods.Admin.Controllers.Api
{
    [Authorize]
    public class ClientController : ApiController
    {
        /// <summary>
        ///     Set via dependency injection
        /// </summary>
        private readonly IClientAppRepo _repository;

        private readonly ISecurityService _securityService;

        private readonly ISandboxProvisioner _sandboxProvisioner;
        private readonly IClientCreator _clientCreator;

        public ClientController(IClientAppRepo repository, ISecurityService securityService,
                                ISandboxProvisioner sandboxProvisioner, IClientCreator clientCreator)
        {
            _repository = repository;
            _securityService = securityService;
            _sandboxProvisioner = sandboxProvisioner;
            _clientCreator = clientCreator;
        }

        public int UserId
        {
            get { return _securityService.GetCurrentUserId().CurrentUserId; }
        }

        private User UserProfile
        {
            get { return _securityService.GetCurrentUser().CurrentUser; }
        }

        private void AddClientStatusInfo(ApiClient client)
        {
            if (client.UseSandbox)
            {
                var status = _sandboxProvisioner.GetSandboxStatus(client.Key);
                client.Status = status.Description;
            }
            else
            {
                client.Status = ApiClientStatus.Online;
            }
        }

        private void AddClientStatusInfo(IEnumerable<ApiClient> clients)
        {
            foreach (var client in clients)
            {
                AddClientStatusInfo(client);
            }
        }

        [HttpGet]
        public IEnumerable<ClientIndexViewModel> GetClients()
        {
            //TODO: This is an ugly patch for now. We should really do this type of thing in a top-level exception handler.
            try
            {
                var clients = UserProfile.ApiClients;
                AddClientStatusInfo(clients);
                var models = clients.Select(ToClientIndexViewModel).ToArray();
                return models;
            }
            catch (Exception e)
            {
                var currentException = e;
                var counter = 0;
                var message = string.Empty;
                while (currentException != null)
                {
                    Trace.TraceError("{0}: {1}", currentException.Message, currentException.StackTrace);
                    message += string.Format("{0}:{1}", counter, currentException.Message);
                    counter++;
                    currentException = currentException.InnerException;
                }

                throw new Exception(string.Format("Unhandled exception with these messages: {0}", message));
            }
        }


        private ClientIndexViewModel ToClientIndexViewModel(ApiClient client)
        {
            var apiClientId = client.ApiClientId;
            var application = client.Application ?? new Application{ApplicationName = "<Empty>"};
            var applicationName = application.ApplicationName;
            var name = client.Name;
            var key = client.Key;
            var secret = client.Secret;
            var status = client.Status;
            var sandboxTypeName = client.SandboxTypeName;

            ClientIndexViewModel viewModel = new ClientIndexViewModel
                                                 {
                                                     Id = apiClientId,
                                                     ApplicationName = applicationName,
                                                     Name = name,
                                                     Key = key,
                                                     Secret = secret,
                                                     Status = status,
                                                     SandboxTypeName = sandboxTypeName
                                                 };
            return viewModel;
        }

        [HttpGet]
        public ClientIndexViewModel GetClient(string id)
        {
            var client = UserProfile.ApiClients.FirstOrDefault(c => c.Key == id);
            return ToClientIndexViewModel(client);
        }

        [HttpPost]
        public ClientIndexViewModel PostClient(SandboxClientCreateModel sandboxClient)
        {
            if (ModelState["client.Name"] == null)
                try
                {
                    var profile = _repository.GetUser(UserProfile.Email);
                    var newClient = _clientCreator.CreateNewSandboxClient(sandboxClient, profile);
                    AddClientStatusInfo(newClient);
                    return ToClientIndexViewModel(newClient);
                }
                catch (ArgumentOutOfRangeException)
                {
                    throw new HttpResponseException(HttpStatusCode.BadRequest);
                }
            throw new HttpResponseException(HttpStatusCode.NotAcceptable);
        }

        [HttpPut]
        public ClientIndexViewModel PutClient(ClientIndexViewModel client)
        {
            var test = UserProfile.ApiClients.FirstOrDefault(c => c.Key == client.Key && c.ApiClientId == client.Id);
            if (test == null) throw new KeyNotFoundException();
            test.GenerateSecret();
            _repository.UpdateClient(test);
            AddClientStatusInfo(test);
            return ToClientIndexViewModel(test);
        }

        [HttpDelete]
        public void DeleteClient(string id)
        {
            var client = _repository.GetClient(id);
            if (client == null) throw new HttpResponseException(HttpStatusCode.NotFound);
            _repository.DeleteClient(id);
        }
    }
}