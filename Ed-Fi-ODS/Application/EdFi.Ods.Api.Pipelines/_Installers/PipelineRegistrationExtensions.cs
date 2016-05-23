using System;
using System.Linq;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using EdFi.Ods.Pipelines.Common;

namespace EdFi.Ods.Pipelines._Installers
{
    public static class PipelineRegistrationExtensions
    {
        public static void RegisterPipelineStepsForAssembliesMarkedBy(
            this IWindsorContainer container,
            Type markerType)
        {
            RegisterPipelineStepsForAssembliesMarkedBy(container, markerType, null);
        }

        public static void RegisterPipelineStepsForAssembliesMarkedBy(
            this IWindsorContainer container, 
            Type markerType, 
            string registrationContext)
        {
            container.Register(
                Classes
                    .FromAssemblyContaining(markerType)
                    .BasedOn(typeof (IStep<,>))
                    .If(
                        t =>
                        {
                            var attributes = (RegistrationContextAttribute[]) 
                                t.GetCustomAttributes(
                                    typeof(RegistrationContextAttribute),
                                    false);

                            if (attributes.Length == 0 && registrationContext == null)
                                return true;

                            var contextAttribute = attributes.FirstOrDefault();

                            string attributeContext = contextAttribute == null 
                                ? string.Empty : contextAttribute.Context;

                            return string.Compare(attributeContext, registrationContext, StringComparison.InvariantCultureIgnoreCase) == 0;
                        })
                     .WithService
                     .Self());
        }
    }
}