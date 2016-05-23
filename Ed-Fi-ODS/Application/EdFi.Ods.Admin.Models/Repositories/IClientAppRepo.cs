using System;
using System.Linq;

namespace EdFi.Ods.Admin.Models
{
    public interface IUpdateUser
    {
        void UpdateUser(User user);        
    }

    public interface IClientAppRepo : IUpdateUser
    {
        string GetTokenFromUserName(string userName);
        string GetUserNameFromToken(string token);

        IQueryable<User> GetUsers();
        User GetUser(int userId);
        User GetUser(string userName);
        void DeleteUser(User userProfile);

        ApiClient GetClient(string key);
        ApiClient GetClient(string key, string secret);
        ApiClient AddClient(ApiClient client);
        ApiClient UpdateClient(ApiClient client);
        void DeleteClient(string key);

        ClientAuthorizationCode AddClientAuthorizationCode(int clientId, string scope = null);
        ClientAuthorizationCode GetClientAuthorizationCode(Guid code);
        void DeleteClientAuthorizationCode(ClientAuthorizationCode authCode);

        ClientAccessToken AddClientAccessToken(ClientAuthorizationCode code);

        void SetDefaultVendorOnUserFromEmailAndName(string userEmail, string userName);

        Application[] GetVendorApplications(int vendorId);
    }
}