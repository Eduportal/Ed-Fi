using EdFi.Common.Context;

namespace EdFi.Ods.Utilities.LoadGeneration.Security
{
    public class ApiSecurityContextProvider : IApiSecurityContextProvider
    {
        private readonly IContextStorage _contextStorage;

        private const string ContextKey = "ApiSecurityContextProvider.ApiSecurityContext";

        public ApiSecurityContextProvider(IContextStorage contextStorage)
        {
            this._contextStorage = contextStorage;
        }

        public ApiSecurityContext GetSecurityContext()
        {
            var context = _contextStorage.GetValue<ApiSecurityContext>(ContextKey);

            return context;
        }

        public void SetSecurityContext(ApiSecurityContext apiSecurityContext)
        {
            _contextStorage.SetValue(ContextKey, apiSecurityContext);
        }
    }
}