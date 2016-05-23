namespace EdFi.Ods.Tests.EdFi.Ods.Security.Authorization.Repositories
{
    using global::EdFi.Common.Security.Claims;
    using global::EdFi.Ods.Security.Authorization;

    internal class AuthorizationContextProviderStub<T> : IAuthorizationContextProvider
    {
        private readonly string _authorizationAction;

        public AuthorizationContextProviderStub(string authorizationAction)
        {
            _authorizationAction = authorizationAction;
        }
        public string GetResource()
        {
           return typeof(T).GetResourceName();
        }

        public void SetResource(string value)
        {
            throw new System.NotImplementedException();
        }

        public string GetAction()
        {
            return _authorizationAction; 
        }

        public void SetAction(string value)
        {
            throw new System.NotImplementedException();
        }

        public void VerifyAuthorizationContextExists()
        {
        }

        public bool IsAuthorizationInjectedIntoPagedQuery()
        {
            throw new System.NotImplementedException();
        }

        public void SetIsAuthorizationInjectedIntoPagedQuery(bool value)
        {
            throw new System.NotImplementedException();
        }
    }
}