using EdFi.Common.Messaging;
using EdFi.Ods.Common.Security;

namespace EdFi.Ods.Security.Messaging
{
    public class SecurityOutboundEnvelopeDataProcessor : IOutboundEnvelopeDataProcessor
    {
        private readonly IApiKeyContextProvider _apiKeyContextProvider;

        public SecurityOutboundEnvelopeDataProcessor(IApiKeyContextProvider apiKeyContextProvider)
        {
            _apiKeyContextProvider = apiKeyContextProvider;
        }

        public void ProcessEnvelope<TCommand>(IEnvelope<TCommand> envelopedMessage) where TCommand : ICommand
        {
            var apiKeyContext = _apiKeyContextProvider.GetApiKeyContext();
            envelopedMessage.ApiKey = apiKeyContext.ApiKey;
            envelopedMessage.EducationOrganizationIds = apiKeyContext.EducationOrganizationIds;
            envelopedMessage.ClaimSetName = apiKeyContext.ClaimSetName;
            envelopedMessage.NamespacePrefix = apiKeyContext.NamespacePrefix;
        }
    }
}