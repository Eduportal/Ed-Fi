using System;
using EdFi.Common.Database;
using EdFi.Ods.Common.Security;

namespace EdFi.Ods.Common.Database
{
    public class SandboxDatabaseNameProvider : IDatabaseNameProvider
    {
        private readonly IApiKeyContextProvider apiKeyContextProvider;

        public SandboxDatabaseNameProvider(IApiKeyContextProvider apiKeyContextProvider)
        {
            this.apiKeyContextProvider = apiKeyContextProvider;
        }

        public string GetDatabaseName()
        {
            //Convention: "EdFi_Ods_Sandbox_" + vendor's api key.
            string apiKey = apiKeyContextProvider.GetApiKeyContext().ApiKey;

            if (string.IsNullOrEmpty(apiKey))
                throw new InvalidOperationException("The sandbox ODS database name cannot be derived because the API key was not set in the current context.");

            return string.Format("EdFi_Ods_Sandbox_{0}", apiKey);
        }
    }
}
