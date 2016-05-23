using EdFi.Common.Messaging;
using EdFi.Ods.Common.Context;

namespace EdFi.Ods.Messaging.EnvelopeProcessors
{
    public class SchoolYearOutboundEnvelopeDataProcessor : IOutboundEnvelopeDataProcessor
    {
        private readonly ISchoolYearContextProvider _schoolYearContextProvider;

        public SchoolYearOutboundEnvelopeDataProcessor(ISchoolYearContextProvider schoolYearContextProvider)
        {
            _schoolYearContextProvider = schoolYearContextProvider;
        }

        public void ProcessEnvelope<TCommand>(IEnvelope<TCommand> envelopedMessage) where TCommand : ICommand
        {
            envelopedMessage.SchoolYear = _schoolYearContextProvider.GetSchoolYear();
        }
    }
}