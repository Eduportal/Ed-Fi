using System;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Web.Security;
using EdFi.Common.Configuration;
using EdFi.Ods.Admin.DeployedUsers;
using EdFi.Ods.Admin.Models;
using EdFi.Ods.Admin.Security;
using EdFi.Ods.Common.Utils.Extensions;
using WebMatrix.WebData;

namespace EdFi.Ods.Admin
{
    public class TestUserInitializer
    {
        private readonly IConfigValueProvider _configValueProvider;

        public TestUserInitializer() : this (new AppConfigValueProvider())
        { }

        public TestUserInitializer(IConfigValueProvider configValueProvider)
        {
            _configValueProvider = configValueProvider;
        }

        public void Initialize(UsersContext context)
        {
            SeedMembership(context);
        }

        protected void ExecuteSql(DbContext dbContext, string sqlResourceName)
        {
            var objectContext = (dbContext as IObjectContextAdapter).ObjectContext;
            var sql = typeof(TestUserInitializer).Assembly.ReadResource(sqlResourceName);
            objectContext.ExecuteStoreCommand(sql);
        }

        private void SeedMembership(UsersContext context)
        {
            foreach (var role in SecurityRoles.AllRoles)
            {
                if (!Roles.RoleExists(role))
                    Roles.CreateRole(role);
            }

            SeedTestUsers(context);
        }

        private void SeedTestUsers(UsersContext context)
        {
            var createTestUsers = _configValueProvider.GetValue("CreateTestUsers");
            if (createTestUsers != null && createTestUsers.ToLower().Equals("true"))
            {
                var testUserNames = ConfigurationManager.AppSettings["TestUsers"].Split(';');
                foreach (var testUser in DeployedAdminUsers.GetTestUsers(testUserNames))
                {
                    SeedUser(testUser, context);
                }
            }
            SeedUser(DeployedAdminUsers.GetNamedUser("Ed-Fi Alliance, Admin User"), context);
        }

        private ClientAppRepo GetClientAppRepo(UsersContext context, bool fakeSandbox)
        {
            var sandboxProvisionerType = fakeSandbox
                                             ? typeof(StubSandboxProvisioner)
                                             : SandboxProvisionerTypeCalculator.GetSandboxProvisionerTypeForNewSandboxes();
            var sandboxProvisioner = (ISandboxProvisioner)Activator.CreateInstance(sandboxProvisionerType);
            var clientAppRepo = new ClientAppRepo(context, sandboxProvisioner, _configValueProvider);
            return clientAppRepo;
        }

        private void SeedUser(IDeployedUser user, UsersContext context)
        {
            if (WebSecurity.UserExists(user.Email)) return;

            WebSecurity.CreateUserAndAccount(
                user.Email,
                user.Password,
                new { FullName = user.Name });

            foreach (var role in user.Roles)
            {
                Roles.AddUserToRole(user.Email, role);
            }

            WebSecurityService.UpdatePasswordAndActivate(user.Email, user.Password);

            var repo = GetClientAppRepo(context, true);
            repo.SetDefaultVendorOnUserFromEmailAndName(user.Email, user.Name);

            SeedClients(user, context);
        }

        private void SeedClients(IDeployedUser userDescription, UsersContext context)
        {
            foreach (var clientApi in userDescription.ClientApis)
            {
                var clientAppRepo = GetClientAppRepo(context, clientApi.FakeSandbox);
                var user = clientAppRepo.GetUser(userDescription.Email);
                var apps = clientAppRepo.GetVendorApplications(user.Vendor.VendorId);
                var client = new ApiClient(false)
                {
                    Name = clientApi.Name,
                    IsApproved = true,
                    UseSandbox = true,
                    SandboxType = clientApi.SandboxType,
                    Key = clientApi.Key,
                    Secret = clientApi.Secret,
                };
                if (apps.Length == 1)
                {
                    client.Application = apps[0];
                    foreach (var educationOrganizationId in clientApi.EducationOrganizationIds)
                    {
                        var applicationEducationOrganization = new ApplicationEducationOrganization
                        {
                            Application = apps[0],
                            EducationOrganizationId = educationOrganizationId
                        };
                        client.ApplicationEducationOrganizations.Add(applicationEducationOrganization);
                    }
                }

                clientAppRepo.AddClient(client);
                user.ApiClients.Add(client);

                clientAppRepo.UpdateUser(user);
            }
        }
    }
}