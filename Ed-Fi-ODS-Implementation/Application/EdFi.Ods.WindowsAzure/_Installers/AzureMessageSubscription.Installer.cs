using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using EdFi.Common.Messaging;
using EdFi.Ods.WindowsAzure.Messaging;

namespace EdFi.Ods.WindowsAzure._Installers
{
    public class AzureMessageReceivingInstaller : IWindsorInstaller
    {
       public void Install(IWindsorContainer container, IConfigurationStore store)
       {
           container.Register(
                Component.For(typeof(IQueueListener<>))
                         .ImplementedBy(typeof(AzureQueueListener<>))
           );
       }
    }
}
