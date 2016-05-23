using System;
using System.Diagnostics;
using System.Threading;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using EdFi.Common.InversionOfControl;
using EdFi.Common.Services;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace EdFi.Ods.WindowsAzure.Services
{
    public abstract class WorkerRoleBase<THostedService> : RoleEntryPoint where THostedService : IHostedService
    {
        private IHostedService _hostedService;
        protected abstract void ConfigureIoC(IWindsorContainer container);

        public override void Run()
        {
            try
            {
                if (this._hostedService != null)
                {
                    var message = string.Format("{0}.{1} has started.",
                                                this.GetType().Namespace, this.GetType().Name);
                    Trace.TraceInformation(message);
                    this._hostedService.Start();
                    Trace.TraceWarning("Hosted service has terminated.  The Run method will terminate as well.");
                    Thread.Sleep(TimeSpan.FromSeconds(30));
                }
                else
                {
                    Trace.TraceError(
                        "Hosted service not intialized.  Terminating.  Waiting 30 minutes to die, because Azure is just going to recycle me anyway.");
                    Thread.Sleep(TimeSpan.FromMinutes(30));
                }
            }
            catch (Exception e)
            {
                var message = string.Format("Something untoward happened in {0} (Run Method):{1}{2}", this.GetType().FullName,
                                            Environment.NewLine, e);
                Trace.TraceError(message);
                Thread.Sleep(TimeSpan.FromSeconds(30));
            }
        }

        public override bool OnStart()
        {
            try
            {
                this.ConfigureDiagnostics();

                Trace.TraceInformation("Configuring IoC");
                var container = this.ConfigureIoC();

                Trace.TraceInformation("DONE Configuring IoC.  Attempting to resolve IHostedService");
                this._hostedService = container.Resolve<IHostedService>();

                if (this._hostedService == null)
                    throw new InvalidOperationException("Couldn't find a registered class for IHostedService");
                Trace.TraceInformation(string.Format((string)"Successfully created instance of type {0}",
                                                     (object)this._hostedService.GetType().FullName));
            }
            catch (Exception e)
            {
                var message = string.Format("Something untoward happened in {0} (OnStart Method):{1}{2}",
                                            this.GetType().FullName, Environment.NewLine, e);
                Trace.TraceError(message);
            }

            return base.OnStart();
        }

        public override void OnStop()
        {
            Trace.TraceWarning("Worker Role stopping...");
            Thread.Sleep(TimeSpan.FromSeconds(30));
            this._hostedService.Stop();
            base.OnStop();
        }

        private IWindsorContainer ConfigureIoC()
        {
            var container = new WindsorContainerEx();
            this.OnInitializingContainer(container);

            container.AddFacility<TypedFactoryFacility>();
            container.AddFacility<DatabaseConnectionStringProviderFacility>();
            container.Register(Component.For<IHostedService>().ImplementedBy<THostedService>());

            this.ConfigureIoC(container);
            return container;
        }

        /// <summary>
        /// Provides a method of performing pre-registration initialization of the container (such as installing support for collections).
        /// </summary>
        /// <param name="container">The Castle Windsor container.</param>
        protected virtual void OnInitializingContainer(IWindsorContainer container)
        {
            container.AddSupportForEmptyCollections();
        }

        public virtual void ConfigureDiagnostics()
        {
            DiagnosticMonitorConfiguration config =
                DiagnosticMonitor.GetDefaultInitialConfiguration();
            config.ConfigurationChangePollInterval = TimeSpan.FromSeconds(15);
            config.Logs.BufferQuotaInMB = 500;
            config.Logs.ScheduledTransferLogLevelFilter = LogLevel.Verbose;
            config.Logs.ScheduledTransferPeriod = TimeSpan.FromSeconds(15);

            DiagnosticMonitor.Start(
                "Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString",
                config);

            //It seems the diagnostics stuff needs a bit of a sleep before we can use it.
            Thread.Sleep(TimeSpan.FromSeconds(15));
            Trace.WriteLine("Diagnostics listener started.");
        }
    }
}