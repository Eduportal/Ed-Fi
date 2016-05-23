using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using EdFi.Common.Messaging;

namespace EdFi.Messaging.MSMQ._Installers
{
    public class MSMQSendOnlyInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component.For<IBusSender>().ImplementedBy<BusSender>(),
                Component.For<IQueueLocator>().ImplementedBy<QueueLocator>(),
                Component.For<IMsmqMessageFactory>().ImplementedBy<MsmqMessageFactory>()
                );
        }
    }
}