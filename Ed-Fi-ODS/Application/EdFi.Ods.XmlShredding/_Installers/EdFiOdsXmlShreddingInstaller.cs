using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace EdFi.Ods.XmlShredding._Installers
{
    public class EdFiOdsXmlShreddingInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container
                .Register(Classes.FromAssemblyContaining<EdFiOdsXmlShreddingInstaller>()
                                 .BasedOn(typeof (IResourceFactory<>))
                                 .WithService.AllInterfaces());
        }
    }
}