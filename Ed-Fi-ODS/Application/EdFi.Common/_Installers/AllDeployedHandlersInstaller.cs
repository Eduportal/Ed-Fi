using System.IO;
using System.Reflection;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using EdFi.Common.Messaging;

namespace EdFi.Common._Installers
{
    public class AllDeployedHandlersInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            string assemblyLocation = Assembly.GetExecutingAssembly().Location;
            string directory = Path.GetDirectoryName(assemblyLocation);

            container.Register(Classes.FromAssemblyInDirectory(new AssemblyFilter(directory, "EdFi.Ods."))
                          .BasedOn(typeof(IMessageHandler<>))
                          .WithService.AllInterfaces());
        }
    }
}