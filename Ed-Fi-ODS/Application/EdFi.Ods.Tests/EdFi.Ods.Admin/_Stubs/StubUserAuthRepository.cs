namespace EdFi.Ods.Tests.EdFi.Ods.Admin._Stubs
{
    using System;
    using System.Collections.Generic;

    public class StubUserAuthRepository : IUserAuthRepository
    {
        public class CreatedUser
        {
            public UserAuth Auth { get; set; }
            public string Password { get; set; }
        }

        public class UpdatedUser
        {
            public UserAuth Existing { get; set; }
            public UserAuth New { get; set; }
            public string Password { get; set; }
        }

        private UserAuth _user;
        private readonly List<CreatedUser> _createdUsers = new List<CreatedUser>();
        private readonly List<UpdatedUser> _updatedUsers = new List<UpdatedUser>();
        private readonly List<UserAuth> _savedUsers = new List<UserAuth>();
        private int _createdUserId;
        private string _authenticationPassword;

        public CreatedUser[] CreatedUsers
        {
            get { return _createdUsers.ToArray(); }
        }

        public UpdatedUser[] UpdatedUsers
        {
            get { return _updatedUsers.ToArray(); }
        }

        public UserAuth[] SavedUsers
        {
            get { return _savedUsers.ToArray(); }
        }

        public StubUserAuthRepository WithUser(UserAuth user)
        {
            _user = user;
            return this;
        }

        public StubUserAuthRepository WithCreatedUserId(int id)
        {
            _createdUserId = id;
            return this;
        }

        public UserAuth CreateUserAuth(UserAuth newUser, string password)
        {
            _createdUsers.Add(new CreatedUser {Auth = newUser, Password = password});
            newUser.Id = _createdUserId;
            return newUser;
        }

        public UserAuth UpdateUserAuth(UserAuth existingUser, UserAuth newUser, string password)
        {
            _updatedUsers.Add(new UpdatedUser{Existing = existingUser, New = newUser, Password = password});
            return newUser;
        }

        public UserAuth GetUserAuthByUserName(string userNameOrEmail)
        {
            return _user;
        }

        public bool TryAuthenticate(string userName, string password, out UserAuth userAuth)
        {
            if (userName.Equals(_user.UserName) && password.Equals(_authenticationPassword))
            {
                userAuth = _user;
                return true;
            }
            userAuth = null;
            return false;
        }

        public bool TryAuthenticate(Dictionary<string, string> digestHeaders, string PrivateKey, int NonceTimeOut, string sequence, out UserAuth userAuth)
        {
            throw new NotImplementedException();
        }

        public void LoadUserAuth(IAuthSession session, IOAuthTokens tokens)
        {
            throw new NotImplementedException();
        }

        public UserAuth GetUserAuth(string userAuthId)
        {
            throw new NotImplementedException();
        }

        public void SaveUserAuth(IAuthSession authSession)
        {
            throw new NotImplementedException();
        }

        public void SaveUserAuth(UserAuth userAuth)
        {
            _savedUsers.Add(userAuth);
        }

        public List<UserOAuthProvider> GetUserOAuthProviders(string userAuthId)
        {
            throw new NotImplementedException();
        }

        public UserAuth GetUserAuth(IAuthSession authSession, IOAuthTokens tokens)
        {
            throw new NotImplementedException();
        }

        public string CreateOrMergeAuthSession(IAuthSession authSession, IOAuthTokens tokens)
        {
            throw new NotImplementedException();
        }

        public StubUserAuthRepository AllowAuthenticationWithPassword(string password)
        {
            _authenticationPassword = password;
            return this;
        }
    }
}