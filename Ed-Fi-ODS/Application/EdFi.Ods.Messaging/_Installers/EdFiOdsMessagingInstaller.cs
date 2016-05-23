using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using EdFi.Common.Messaging;
using EdFi.Ods.Messaging.EnvelopeProcessors;

namespace EdFi.Ods.Messaging._Installers
{
    public class EdFiOdsMessagingInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component.For<IInboundEnvelopeDataProcessor>().ImplementedBy<SchoolYearInboundEnvelopeDataProcessor>(),

                Component.For<IOutboundEnvelopeDataProcessor>().ImplementedBy<SchoolYearOutboundEnvelopeDataProcessor>()
                );
        }
    }
}
