using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.SqlClient;
using System.Linq;
using EdFi.Common.Configuration;

namespace EdFi.Ods.Admin.Models
{
    public class ClientAppRepo : IClientAppRepo
    {
        private const int MaximumClientCountPerApplication = 5;

        private readonly UsersContext _context;
        private readonly ISandboxProvisioner _provisioner;
        private readonly IConfigValueProvider _configValueProvider;

        public ClientAppRepo(UsersContext context, ISandboxProvisioner provisioner,
                             IConfigValueProvider configValueProvider)
        {
            _context = context;
            _provisioner = provisioner;
            _configValueProvider = configValueProvider;
        }

        internal class EmailResult
        {
            public string Email { get; set; }
        }

        public string GetUserNameFromToken(string token)
        {
            var result = _context.Database.SqlQuery<EmailResult>(
                @"select top 1 U.Email from webpages_Membership M join Users U on M.UserId = U.UserId and M.ConfirmationToken = {0}",
                token)
                                 .FirstOrDefault();
            return result == null ? null : result.Email;
        }

        internal class ConfirmationTokenResult
        {
            public string ConfirmationToken { get; set; }
        }

        public string GetTokenFromUserName(string userName)
        {
            var result = _context.Database.SqlQuery<ConfirmationTokenResult>(
                @"select top 1 M.ConfirmationToken from webpages_Membership M join Users U on M.UserId = U.UserId and U.Email = {0}",
                userName)
                                 .FirstOrDefault();
            return result == null ? null : result.ConfirmationToken;
        }


        public IQueryable<User> GetUsers()
        {
            return _context.Users.Include("ApiClients").Include("ApiClients.Application").AsQueryable();
        }

        public User GetUser(int userId)
        {
            return
                _context.Users.Include("ApiClients")
                        .Include("ApiClients.Application")
                        .FirstOrDefault(u => u.UserId == userId);
        }

        public User GetUser(string userName)
        {
            return
                _context.Users.Include("ApiClients")
                        .Include("ApiClients.Application")
                        .FirstOrDefault(x => x.Email == userName);
        }

        public void DeleteUser(User userProfile)
        {
            var user =
                _context.Users.Include("ApiClients")
                        .Include("ApiClients.Application")
                        .FirstOrDefault(x => x.UserId == userProfile.UserId);
            if (user == null) return;
            var arraySoThatUnderlyingCollectionCanBeModified = user.ApiClients.ToArray();
            foreach (var client in arraySoThatUnderlyingCollectionCanBeModified)
            {
                _context.Clients.Remove(client);
            }

            _context.Users.Remove(user);
            _context.SaveChanges();
        }

        public ApiClient GetClient(string key)
        {
            return _context.Clients.FirstOrDefault(c => c.Key == key);
        }

        public ApiClient GetClient(string key, string secret)
        {
            return _context.Clients.FirstOrDefault(c => c.Key == key && c.Secret == secret);
        }

        public ApiClient AddClient(ApiClient client)
        {
            _context.Clients.Add(client);
            _context.SaveChanges();
            if (client.UseSandbox)
                _provisioner.AddSandbox(client.Key, client.SandboxType);
            return client;
        }

        public ApiClient UpdateClient(ApiClient client)
        {
            _context.Clients.AddOrUpdate(client);
            _context.SaveChanges();
            return client;
        }

        public void DeleteClient(string key)
        {
            var client = _context.Clients.First(x => x.Key == key);
            _context.Database.ExecuteSqlCommand(@"delete ClientAccessTokens where ApiClient_ApiClientId = @clientId; 
delete ClientAuthorizationCodes where ApiClient_ApiClientId = @clientId; 
delete ApiClients where ApiClientId = @clientId",
                                                new SqlParameter("@clientId", client.ApiClientId));
            if (client.UseSandbox)
                _provisioner.DeleteSandboxes(key);
        }

        public ClientAuthorizationCode AddClientAuthorizationCode(int clientId, string scope = null)
        {
            var client = _context.Clients.FirstOrDefault(c => c.ApiClientId == clientId);
            if (client == null) return null;
            var code = new ClientAuthorizationCode
                           {
                               Scope = scope
                           };
            client.ClientAuthorizationCodes.Add(code);
            _context.SaveChanges();
            return code;
        }

        public ClientAuthorizationCode GetClientAuthorizationCode(Guid code)
        {
            return _context.ClientAuthorizationCodes.FirstOrDefault(
                c => c.Id == code
                );
        }

        public void DeleteClientAuthorizationCode(ClientAuthorizationCode authCode)
        {
            _context.ClientAuthorizationCodes.Remove(authCode);
            _context.SaveChanges();
        }

        public ClientAccessToken AddClientAccessToken(ClientAuthorizationCode code)
        {
            var client = code.ApiClient;
            var token = new ClientAccessToken
                            {
                                Scope = code.Scope
                            };
            client.ClientAccessTokens.Add(token);
            _context.SaveChanges();
            return token;
        }

        public void UpdateUser(User user)
        {
            //TODO: DEA - This is updating existings clients by setting the application.  Look for calls to this and set the application on client creation instead.
            if (user.ApiClients.Count > MaximumClientCountPerApplication)
            {
                var msg = string.Format("A maximum of {0} client applications may be created", MaximumClientCountPerApplication);
                throw new ArgumentOutOfRangeException(msg);
            }
            foreach (var client in user.ApiClients.Where(c => c.Application == null))
            {
                var application = _context.Applications.Where(a => a.Vendor.VendorId == user.Vendor.VendorId);
                if (application.Count() == 1)
                    client.Application = application.First();
            }
            _context.SaveChanges();
        }

        public Application[] GetVendorApplications(int vendorId)
        {
            return _context.Applications.Where(a => a.Vendor.VendorId == vendorId).ToArray();
        }

        public void SetDefaultVendorOnUserFromEmailAndName(string userEmail, string userName)
        {
            var namePrefix = "http://" + userEmail.Split('@')[1].ToLower();
            var vendorName = userName.Split(',')[0].Trim();
            var vendor = FindOrCreateVendorByDomainName(vendorName, namePrefix);
            var usr = _context.Users.Single(u => u.Email == userEmail);
            usr.Vendor = vendor;
            _context.SaveChanges();
        }

        private Vendor FindOrCreateVendorByDomainName(string vendorName, string namePrefix)
        {
            var vendor = _context.Vendors.SingleOrDefault(v => v.VendorName == vendorName);
            if (vendor == null)
            {
                vendor = new Vendor { VendorName = vendorName, NamespacePrefix =  namePrefix };
                _context.Vendors.AddOrUpdate(vendor);
                //TODO: DEA - Move this behavior to happen during client creation.  No need to do this in two places.  At a minimum, remove the duplicated code.
                CreateDefaultApplicationForVendor(vendor);
            }
            return vendor;
        }

        private void CreateDefaultApplicationForVendor(Vendor vendor)
        {
            var defaultAppName = _configValueProvider.GetValue("DefaultApplicationName");
            var defaultClaimSetName = _configValueProvider.GetValue("DefaultClaimSetName");

            var app =
                _context.Applications.SingleOrDefault(
                    a => a.ApplicationName == defaultAppName && a.Vendor.VendorId == vendor.VendorId);
            if (app != null) return;

            _context.Applications.AddOrUpdate(new Application
                                                  {
                                                      ApplicationName = defaultAppName,
                                                      Vendor = vendor,
                                                      ClaimSetName = defaultClaimSetName
                                                  });
        }
    }
}