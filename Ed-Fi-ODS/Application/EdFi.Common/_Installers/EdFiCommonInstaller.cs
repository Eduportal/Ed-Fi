// ****************************************************************************
//  Copyright (C) 2015 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using Castle.MicroKernel.Registration;
using Castle.Windsor;
using EdFi.Common.InversionOfControl;
using EdFi.Common.IO;

namespace EdFi.Common._Installers
{
    public class EdFiCommonInstaller : RegistrationMethodsInstallerBase
    {
        protected virtual void RegisterIFileSystem(IWindsorContainer container)
        {
            container.Register(Component
                .For<IFileSystem>()
                .ImplementedBy<FileSystemWrapper>());
        }
    }
}