namespace EdFi.Common.Security.Claims
{
    /// <summary>
    /// Provides keys to use when saving data into contextual storage.
    /// </summary>
    public static class AuthorizationContextKeys
    {
        public const string Action = "AuthorizationContext.Action";
        public const string Resource = "AuthorizationContext.Resource";
        public const string AuthorizationInjectedIntoPagedQuery = "AuthorizationContext.AuthorizationInjectedIntoPagedQuery";
    }
}