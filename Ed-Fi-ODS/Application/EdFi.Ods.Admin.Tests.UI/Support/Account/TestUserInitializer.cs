namespace EdFi.Ods.Admin.UITests.Support.Account
{
    using System;
    using System.Configuration;
    using System.Web.Security;

    using EdFi.Common.Configuration;
    using EdFi.Ods.Admin.DeployedUsers;
    using EdFi.Ods.Admin.Models;
    using EdFi.Ods.Admin.Security;

    using WebMatrix.WebData;

    public static class TestUserInitializer
    {
        static TestUserInitializer()
        {
            try
            {
                WebSecurity.InitializeDatabaseConnection(
                    "EdFi_Admin",
                    UsersContext.UserTableName,
                    UsersContext.UserIdColumn,
                    UsersContext.UserNameColumn,
                    autoCreateTables: true);
            }
            catch (System.InvalidOperationException)
            {
                // suppress this exception because we may try to initialize more than once
            }
        }

        public static void Initialize(UsersContext context)
        {
            SeedMembership(context);
        }

        private static void SeedMembership(UsersContext context)
        {
            foreach (var role in SecurityRoles.AllRoles)
            {
                if (!Roles.RoleExists(role))
                    Roles.CreateRole(role);
            }

            SeedTestUsers(context);
        }

        private static void SeedTestUsers(UsersContext context)
        {
            IConfigValueProvider configValueProvider = new AppConfigValueProvider();
            var createTestUsers = configValueProvider.GetValue("CreateTestUsers");
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

        private static ClientAppRepo GetClientAppRepo(UsersContext context, bool fakeSandbox)
        {
            IConfigValueProvider configValueProvider = new AppConfigValueProvider();
            var sandboxProvisionerType = fakeSandbox
                                             ? typeof(StubSandboxProvisioner)
                                             : SandboxProvisionerTypeCalculator.GetSandboxProvisionerTypeForNewSandboxes();
            var sandboxProvisioner = (ISandboxProvisioner)Activator.CreateInstance(sandboxProvisionerType);
            var clientAppRepo = new ClientAppRepo(context, sandboxProvisioner, configValueProvider);
            return clientAppRepo;
        }

        private static void SeedUser(IDeployedUser user, UsersContext context)
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

        private static void SeedClients(IDeployedUser userDescription, UsersContext context)
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