using EdFi.Common.Messaging;
using EdFi.Ods.Common.Context;

namespace EdFi.Ods.Messaging.EnvelopeProcessors
{
    public class SchoolYearInboundEnvelopeDataProcessor : IInboundEnvelopeDataProcessor
    {
        private readonly ISchoolYearContextProvider _schoolYearContextProvider;

        public SchoolYearInboundEnvelopeDataProcessor(ISchoolYearContextProvider schoolYearContextProvider)
        {
            _schoolYearContextProvider = schoolYearContextProvider;
        }

        public void ProcessEnvelope<TCommand>(IEnvelope<TCommand> envelopedMessage) where TCommand : ICommand
        {
            _schoolYearContextProvider.SetSchoolYear(envelopedMessage.SchoolYear);
        }
    }
}