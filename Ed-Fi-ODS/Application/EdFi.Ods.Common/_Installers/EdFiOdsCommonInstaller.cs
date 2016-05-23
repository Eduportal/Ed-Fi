using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using EdFi.Common.Database;
using EdFi.Common.InversionOfControl;
using EdFi.Ods.Common.Context;
using EdFi.Ods.Common.Database;
using EdFi.Ods.Common.Security;
using log4net;

namespace EdFi.Ods.Common._Installers
{
    public class EdFiOdsCommonInstaller : RegistrationMethodsInstallerBase
    {
        // Add registrations for components that may have alternative implementations
        // based on different environmental configurations or implementation-specific requirements
        // protected virtual void RegisterISomething(IWindsorContainer container)
        // {
        //     container.Register(Component
        //            .For<ISomething>()
        //            .ImplementedBy<Something>());
        // }

        protected virtual void RegisterIApiKeyContextProvider(IWindsorContainer container)
        {
            container.Register(Component
                .For<IApiKeyContextProvider>()
                .ImplementedBy<ApiKeyContextProvider>());
        }

        protected virtual void RegisterISchoolYearContextProvider(IWindsorContainer container)
        {
            container.Register(Component
                .For<ISchoolYearContextProvider>()
                .ImplementedBy<SchoolYearContextProvider>());
        }
    }
}