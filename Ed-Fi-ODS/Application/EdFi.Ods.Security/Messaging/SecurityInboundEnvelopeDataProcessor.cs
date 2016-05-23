using EdFi.Common.Messaging;
using EdFi.Ods.Common.Security;

namespace EdFi.Ods.Security.Messaging
{
    public class SecurityInboundEnvelopeDataProcessor : IInboundEnvelopeDataProcessor
    {
        private readonly IApiKeyContextProvider _apiKeyContextProvider;

        public SecurityInboundEnvelopeDataProcessor(IApiKeyContextProvider apiKeyContextProvider)
        {
            _apiKeyContextProvider = apiKeyContextProvider;
        }

        public void ProcessEnvelope<TCommand>(IEnvelope<TCommand> envelopedMessage) where TCommand : ICommand
        {
            _apiKeyContextProvider.SetApiKeyContext(
                new ApiKeyContext(
                    envelopedMessage.ApiKey, 
                    envelopedMessage.ClaimSetName, 
                    envelopedMessage.EducationOrganizationIds, 
                    envelopedMessage.NamespacePrefix, 
                    null //TODO:CMJ - Profiles - Profiles are currently not needed in xml bulkload so it was not included here
                    ));
        }
    }
}