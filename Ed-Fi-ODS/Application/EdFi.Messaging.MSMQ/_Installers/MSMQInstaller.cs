using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using EdFi.Common.Messaging;

namespace EdFi.Messaging.MSMQ._Installers
{
    public class MSMQInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                //First the MSMQ implementations of the common messaging components . . .
                Component.For<IBusSender>().ImplementedBy<BusSender>(),
                Component.For(typeof(IQueueListener<>)).ImplementedBy(typeof(QueueListener<>)),
                Component.For(typeof(IMessageManager<>)).ImplementedBy(typeof(MessageManager<>)),

                //Now the MSMQ specific components to support the implementations above . . .
                Component.For(typeof(IErrorSender<>)).ImplementedBy(typeof(ErrorSender<>)),
                Component.For(typeof(IManagerFactory<>)).ImplementedBy(typeof(ManagerFactory<>)),
                Component.For<IMsmqMessageFactory>().ImplementedBy<MsmqMessageFactory>(),
                Component.For<IQueueLocator>().ImplementedBy<QueueLocator>()
                );
        }
    }
}