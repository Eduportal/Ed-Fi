using System;

namespace EdFi.Ods.Pipelines._Installers
{
    /// <summary>
    /// Indicates that there's a particular IoC container registration context to which the component applies 
    /// (must be explicitly supported and inspected by the registering logic).
    /// </summary>
    /// <remarks>This attribute was added to allow selective IoC container registrations of multiple components
    /// implementing a common interface while avoiding the need to introduce new assemblies to achieve the separation.
    /// In particular, this was added to allow the <see cref="UniqueIdIntegrationInstaller"/> (and only that installer)
    /// to install certain pipeline steps using the <see cref="PipelineRegistrationExtensions"/> class.</remarks>
    [AttributeUsage(AttributeTargets.Class)]
    public class RegistrationContextAttribute : Attribute
    {
        public string Context { get; private set; }

        public RegistrationContextAttribute(string context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            Context = context;
        }
    }
}