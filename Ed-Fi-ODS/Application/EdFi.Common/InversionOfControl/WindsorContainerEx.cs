using System;
using System.Linq;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.Configuration;

namespace EdFi.Common.InversionOfControl
{
    /// <summary>
    /// Extended Windsor Container implementation that 
    /// provides a Service Locator Interface and 
    /// disallows self-injection 
    /// </summary>
    public class WindsorContainerEx : WindsorContainer, IServiceLocator
    {
        private readonly Type[] _invalidTypes = { typeof(WindsorContainer) };

        public WindsorContainerEx() : base()
        {
        }

        public WindsorContainerEx(IConfigurationInterpreter interpreter) : base(interpreter)
        {
        }

        public new IWindsorContainer Register(params IRegistration[] registrations)
        {
            var result = base.Register(registrations);
            DisallowInvalidTypes();
            return result;
        }

        private void DisallowInvalidTypes()
        {
            foreach (var invalidType in _invalidTypes)
            {
                if (Kernel.GetAssignableHandlers(invalidType).Any())
                {
                    throw new Exception(
                        string.Format("Self-registering {0} is not allowed", invalidType.FullName));
                }
            }
        }
    }
}
