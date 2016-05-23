using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using EdFi.Common.Messaging;

namespace EdFi.Common._Installers
{
    public class EdFiMessagingInstaller : IWindsorInstaller
    {
        //TODO: The Singleton object lifecycle may not be appropriate - especially in some deployments
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component.For(typeof(ICommandListener<>))
                    .ImplementedBy(typeof(CommandListener<>))
                    .LifestyleSingleton(),

                Component.For<ICommandSender>()
                    .ImplementedBy<CommandSender>(),

                Component.For<IMessageProcessor>()
                    .ImplementedBy<MessageProcessor>()
                    .DependsOn(new { locator = container })
                    .LifestyleSingleton(),

                Component.For<IQueueNameProvider>()
                    .ImplementedBy<ConventionBasedQueueNameProvider>()
                    .LifestyleSingleton(),

                Component.For<IInboundEnvelopeProcessingMgr>()
                    .ImplementedBy<InboundEnvelopeProcessingMgr>()
                    .LifestyleSingleton(),

                Component.For<IOutboundEnvelopeProcessingMgr>()
                    .ImplementedBy<OutboundEnvelopeProcessingMgr>()
                    .LifestyleSingleton()
                );
        }
    }
}