using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace EdFi.Workers
{
    public abstract class WorkerRoleBase : RoleEntryPoint
    {
        private IHostedService _hostedService;
        protected abstract void ConfigureIoC(IWindsorContainer container);

        public override void Run()
        {
            try
            {
                if (_hostedService != null)
                {
                    var message = string.Format("{0}.{1} has started.",
                                                GetType().Namespace, GetType().Name);
                    Trace.TraceInformation(message);
                    _hostedService.Start();
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
                var message = string.Format("Something untoward happened in {0} (Run Method):{1}{2}", GetType().FullName,
                                            Environment.NewLine, e);
                Trace.TraceError(message);
                Thread.Sleep(TimeSpan.FromSeconds(30));
            }
        }

        public override bool OnStart()
        {
            try
            {
                DiagnosticsConfiguration.ConfigureDiagnostics();

                Trace.TraceInformation("Configuring IoC");
                ConfigureIoC();

                Trace.TraceInformation("DONE Configuring IoC.  Attempting to resolve IHostedService");
                _hostedService = IoC.Resolve<IHostedService>();

                if (_hostedService == null)
                    throw new InvalidOperationException("Couldn't find a registered class for IHostedService");
                Trace.TraceInformation(string.Format("Successfully created instance of type {0}",
                                                     _hostedService.GetType().FullName));
            }
            catch (Exception e)
            {
                var message = string.Format("Something untoward happened in {0} (OnStart Method):{1}{2}",
                                            GetType().FullName, Environment.NewLine, e);
                Trace.TraceError(message);
            }

            return base.OnStart();
        }

        public override void OnStop()
        {
            Trace.TraceWarning("Worker Role stopping...");
            Thread.Sleep(TimeSpan.FromSeconds(30));
            _hostedService.Stop();
            base.OnStop();
        }

        private void ConfigureIoC()
        {
            var container = new WindsorContainer();
            IoC.Initialize(container);
            IoCCommon.SupportCollections(container);

            ConfigureIoC(container);
        }

        public static void ConfigureDiagnostics()
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