using EdFi.Common.Context;

namespace EdFi.Common.Security.Claims
{
    public interface IAuthorizationContextProvider
    {
        string GetResource();
        void SetResource(string value);
        string GetAction();
        void SetAction(string actionUri);
        void VerifyAuthorizationContextExists();
    }

    public class AuthorizationContextProvider : IAuthorizationContextProvider
    {
        private readonly IContextStorage _contextStorage;

        public AuthorizationContextProvider(IContextStorage contextStorage)
        {
            _contextStorage = contextStorage;
        }

        public string GetResource()
        {
           return _contextStorage.GetValue<string>(AuthorizationContextKeys.Resource);
        }

        public string GetAction()
        {
            return _contextStorage.GetValue<string>(AuthorizationContextKeys.Action);
        }

        public void VerifyAuthorizationContextExists()
        {
            // Verify that the authorization context has been set correctly in upstream processing
            if (string.IsNullOrWhiteSpace(GetAction()))
                throw new AuthorizationContextException("Authorization cannot be performed because no action has been stored in the current context.");

            if (string.IsNullOrWhiteSpace(GetResource()))
                throw new AuthorizationContextException("Authorization cannot be performed because no resource has been stored in the current context.");
        }

        public void SetResource(string value)
        {
            _contextStorage.SetValue(AuthorizationContextKeys.Resource, value);
        }

        public void SetAction(string actionUri)
        {
            _contextStorage.SetValue(AuthorizationContextKeys.Action, actionUri);
        }
    }
}