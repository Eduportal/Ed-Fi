using EdFi.Common.Context;

namespace EdFi.Ods.Common.Security
{
    public class ApiKeyContextProvider : IApiKeyContextProvider
    {
        private const string ApiKeyContextKeyName = "ApiKeyContextProvider.ApiKeyContext";
        private readonly IContextStorage _contextStorage;

        public ApiKeyContextProvider(IContextStorage contextStorage)
        {
            _contextStorage = contextStorage;
        }

        public ApiKeyContext GetApiKeyContext()
        {
            return _contextStorage.GetValue<ApiKeyContext>(ApiKeyContextKeyName)
                   ?? ApiKeyContext.Empty;
        }

        public void SetApiKeyContext(ApiKeyContext apiKeyContext)
        {
            _contextStorage.SetValue(ApiKeyContextKeyName, apiKeyContext);
        }
    }
}