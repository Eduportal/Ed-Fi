namespace EdFi.Ods.CodeGen.XmlShredding._Installers
{
    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.Windsor;

    public class EdFiOdsXmlShreddingCodeGenInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component
                              .For<IXPathMapBuilder>()
                              .ImplementedBy<XPathMapBuilder>());
        }
    }
}