namespace EdFi.Ods.Admin.UITests.Support.Account
{
    using System.Web.Security;

    using EdFi.Ods.Admin.Models;
    using EdFi.Ods.Admin.UITests.Support.Extensions;

    using TechTalk.SpecFlow;

    using WebMatrix.WebData;

    public class AccountManager
    {
        private readonly IClientAppRepo clientAppRepo;
        static AccountManager()
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

        public AccountManager(IClientAppRepo clientAppRepo)
        {
            this.clientAppRepo = clientAppRepo;
        }

        public void RemoveAccount(string email)
        {
            var existing = this.clientAppRepo.GetUser(email);
            if (existing != null)
            {
                var roles = Roles.GetRolesForUser(email);
                if (roles != null && roles.Length > 0)
                    Roles.RemoveUserFromRoles(email, roles);
                this.clientAppRepo.DeleteUser(existing);
            }
        }

        public void CreateAccount(Account account)
        {
            this.RemoveAccount(account.Email);

            WebSecurity.CreateUserAndAccount(account.Email, account.Password, new { FullName = account.Name });

            if (account.Roles != null)
            {
                foreach (var role in account.Roles)
                {
                    Roles.AddUserToRole(account.Email, role);
                }
            }
            this.clientAppRepo.SetDefaultVendorOnUserFromEmailAndName(account.Email, account.Name);
            FeatureContext.Current.StoreAccount(account);
        }
    }
}