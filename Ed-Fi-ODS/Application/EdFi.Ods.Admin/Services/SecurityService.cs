using System;
using System.Data.SqlClient;
using System.Web.Security;
using EdFi.Ods.Admin.Models;
using WebMatrix.WebData;

namespace EdFi.Ods.Admin.Services
{
    public class UserLookupResult
    {
        public static UserLookupResult ForUser(User user)
        {
            var isAdmin = false;
            if (user != null)
            {
                isAdmin = Roles.IsUserInRole(user.Email, "Administrator");
            }
            return new UserLookupResult
                       {
                           CurrentUser = user,
                           IsAdmin = isAdmin
                       };
        }

        public static readonly UserLookupResult Empty = new UserLookupResult();

        public User CurrentUser { get; private set; }
        public bool IsAdmin { get; private set; }

        public bool HasCurrentUser
        {
            get { return CurrentUser != null; }
        }
    }

    public class UserIdLookupResult
    {
        public static UserIdLookupResult ForUserId(int id)
        {
            return new UserIdLookupResult {CurrentUserId = id};
        }

        public static readonly UserIdLookupResult Empty = new UserIdLookupResult();

        public int CurrentUserId { get; private set; }

        public bool HasCurrentUser
        {
            get { return CurrentUserId != default(int); }
        }
    }

    public class SecurityService : ISecurityService
    {
        private readonly IClientAppRepo _clientAppRepo;

        public SecurityService(IClientAppRepo clientAppRepo)
        {
            _clientAppRepo = clientAppRepo;
        }

        public UserLookupResult GetCurrentUser()
        {
            try
            {
                var idLookupResult = GetCurrentUserId();
                if (idLookupResult.HasCurrentUser)
                {
                    var userProfile = _clientAppRepo.GetUser(idLookupResult.CurrentUserId);
                    return UserLookupResult.ForUser(userProfile);
                }
            }
            catch (SqlException)
            {
                //Gulp.  Just return the empty result
            }
            return UserLookupResult.Empty;
        }

        public UserIdLookupResult GetCurrentUserId()
        {
            try
            {
                return UserIdLookupResult.ForUserId(WebSecurity.CurrentUserId);
            }

            catch (InvalidOperationException) //When there isn't a current user...
            {
                return UserIdLookupResult.Empty;
            }
        }
    }
}