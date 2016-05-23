using EdFi.Ods.Admin.Models;
using EdFi.Ods.Admin.Models.Client;

namespace EdFi.Ods.Admin.Services
{
    public class ClientCreator : IClientCreator
    {
        private readonly IUpdateUser _userUpdater;
        private readonly ISandboxProvisioner _sandboxProvisioner;
        private readonly IDefaultApplicationCreator _defaultApplicationCreator;

        public ClientCreator(IUpdateUser userUpdater, ISandboxProvisioner sandboxProvisioner,
                             IDefaultApplicationCreator defaultApplicationCreator)
        {
            _userUpdater = userUpdater;
            _sandboxProvisioner = sandboxProvisioner;
            _defaultApplicationCreator = defaultApplicationCreator;
        }

        public ApiClient CreateNewSandboxClient(SandboxClientCreateModel createModel, User user)
        {
            var newClient = user.AddSandboxClient(createModel.Name, createModel.SandboxType);
            var defaultApplication =
                _defaultApplicationCreator.FindOrCreateUpdatedDefaultSandboxApplication(user.Vendor.VendorId,
                                                                                        createModel.SandboxType);
            newClient.Application = defaultApplication;
            foreach (var applicationEducationOrganization in defaultApplication.ApplicationEducationOrganizations)
                newClient.ApplicationEducationOrganizations.Add(applicationEducationOrganization);
            _sandboxProvisioner.AddSandbox(newClient.Key, createModel.SandboxType);
            _userUpdater.UpdateUser(user);
            return newClient;
        }
    }
}