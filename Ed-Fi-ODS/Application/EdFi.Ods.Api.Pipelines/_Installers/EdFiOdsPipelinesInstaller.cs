using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using EdFi.Ods.Api.Pipelines.Factories;
using EdFi.Ods.Pipelines;
using EdFi.Ods.Pipelines.Factories;
using EdFi.Ods.Pipelines._Installers;

namespace EdFi.Ods.Api.Pipelines._Installers
{
    public class EdFiOdsPipelinesInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component
                    .For<IPipelineFactory>()
                    .ImplementedBy<PipelineFactory>()
                    .DependsOn(new { locator = container })
                );
   
            container.Register(
                Classes.FromAssemblyContaining<Marker_EdFi_Ods_Pipelines>()
                    .BasedOn(typeof (ICreateOrUpdatePipeline<,>))
                    .WithService.AllInterfaces()
                );

            container.RegisterPipelineStepsForAssembliesMarkedBy(typeof(Marker_EdFi_Ods_Pipelines));

            // Register the providers of the core pipeline steps
            container.Register(
                Classes
                    .FromAssemblyContaining<Marker_EdFi_Ods_Pipelines>()
                    .BasedOn<IPipelineStepsProvider>()
                    .WithServiceFirstInterface());
        }
    }
}