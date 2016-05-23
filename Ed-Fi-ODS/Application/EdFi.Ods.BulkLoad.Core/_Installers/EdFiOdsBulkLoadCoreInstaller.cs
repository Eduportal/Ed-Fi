using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using EdFi.Ods.Common.ExceptionHandling._Installers;
using EdFi.Ods.Common._Installers;
using EdFi.Ods.Entities.Common._Installers;
using EdFi.Ods.Api.Data._Installers;
using EdFi.Ods.Api.Pipelines._Installers;
using EdFi.Ods.CodeGen.XmlShredding._Installers;

namespace EdFi.Ods.BulkLoad.Core._Installers
{
    /// <summary>
    ///     These are the dependencies that should be registered regardless of what type of queues are being used,
    ///     and regardless of whether we are bulk loading through the console or the service wrappers.
    /// </summary>
    public class EdFiOdsBulkLoadCoreInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Install(new BulkLoadCoreInstaller());
            container.Install(new EdFiOdsCommonInstaller());
            container.Install(new EdFiOdsPipelinesInstaller());
            container.Install(new EdFiOdsEntitiesCommonInstaller());
            container.Install(new EdFiOdsWebApiDataEntityFrameworkInstaller());
            container.Install(new EdFiOdsCommonExceptionHandlingInstaller());
            container.Install(new EdFiOdsXmlShreddingCodeGenInstaller());
        }
    }
}