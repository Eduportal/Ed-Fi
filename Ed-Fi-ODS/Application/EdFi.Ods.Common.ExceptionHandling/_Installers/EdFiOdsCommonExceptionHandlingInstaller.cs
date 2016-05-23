using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace EdFi.Ods.Common.ExceptionHandling._Installers
{
    public class EdFiOdsCommonExceptionHandlingInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component
                .For<IRESTErrorProvider>()
                .ImplementedBy<RESTErrorProvider>());

            container.Register(Component
                .For<IDatabaseMetadataProvider>()
                .ImplementedBy<DatabaseMetadataProvider>());

            container.Register(Types
                .FromAssemblyContaining<Marker_EdFi_Ods_Common_ExceptionHandling_Translators>()
                .BasedOn<IExceptionTranslator>()
                .WithService.FromInterface());
        }
    }
}