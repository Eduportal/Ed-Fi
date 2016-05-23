using System;
using EdFi.Common.Services;
using Topshelf;

namespace EdFi.Services.Windows
{
    public static class WindowsServiceInitiator
    {
        public static Host InitiateService<T>(Func<T> iocFunc) where T : class, IHostedService
        {
            if(iocFunc == null) throw new ArgumentException("You must provide a Func that returns the hosted service (presumably from IoC).");

            return HostFactory.New(hostConfigurator =>
            {
                hostConfigurator.Service<T>(serviceConfigurator =>
                {
                    serviceConfigurator.ConstructUsing(iocFunc);
                    serviceConfigurator.WhenStarted(hs => hs.Start());
                    serviceConfigurator.WhenStopped(hs => hs.Stop());
                });

                //TODO: Is there a better way to obtain the strings from the service?
                var serviceInstanceForStrings = iocFunc();
                hostConfigurator.SetDescription(serviceInstanceForStrings.Description);
                hostConfigurator.SetDisplayName(serviceInstanceForStrings.DisplayName);
                hostConfigurator.SetServiceName(serviceInstanceForStrings.ServiceName);
                //TODO: this and the account to use should be configurable - need to address
                hostConfigurator.StartAutomatically();
            });
        }
    }
}
