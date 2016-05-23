using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using EdFi.Common.InversionOfControl;
using EdFi.Ods.Utilities.LoadGeneration._Installers;
using log4net;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace EdFi.Ods.Utilities.LoadGeneration.Azure.LoadGenerationWorker
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(WorkerRole));

        // The name of the topic
        private const string TopicPath = "LoadGeneration";
        private static readonly string SubscriptionName = "AllMessages-" + Environment.MachineName;

        // QueueClient is thread-safe. Recommended that you cache 
        // rather than recreating it on every request
        private NamespaceManager _namespaceManager;
        private SubscriptionClient _subscriptionClient;

        private readonly ManualResetEvent _completedEvent = new ManualResetEvent(false);
        private readonly AutoResetEvent _loadGeneratingEvent = new AutoResetEvent(true);

        private CancellationTokenSource _cancellationTokenSource;

        public override void Run()
        {
            var startTime = DateTime.Now;

            _logger.Info("Starting processing of messages.");
            _logger.InfoFormat("LocalAppData path: '{0}'", Environment.GetEnvironmentVariable("LOCALAPPDATA"));

            try
            {
                // Initiates the message pump and callback is invoked for each message that is received, calling close on the client will stop the pump.
                _subscriptionClient.OnMessage((receivedMessage) =>
                {
                    try
                    {
                        _logger.InfoFormat("Processing Service Bus message: {0} (#{1})", receivedMessage.Label, receivedMessage.SequenceNumber);

                        // Process the message
                        if (receivedMessage.Label.StartsWith("GenerateLoad", StringComparison.InvariantCultureIgnoreCase))
                        {
                            // Check to see if load generation is already underway
                            if (!_loadGeneratingEvent.WaitOne(TimeSpan.Zero))
                            {
                                _logger.Warn("GenerateLoad message received, but load generation is already ongoing.");
                                receivedMessage.Complete();
                                return;
                            }

                            try
                            {
                                var generateLoadRequest = receivedMessage.GetBody<GenerateLoadRequest>();
                                receivedMessage.Complete();

                                _logger.Info("Starting load generation...");

                                // Create a cancellation token for killing the load
                                _cancellationTokenSource = new CancellationTokenSource();

                                // Execute a load, synchronously on this thread (but cancellable)
                                GenerateLoad(generateLoadRequest, _cancellationTokenSource.Token);

                                // Start a separate worker thread
                                //_loadGenerationTask = new Task(() => GenerateLoad(generateLoadRequest, cancellationToken));
                                //_loadGenerationTask.ContinueWith(t => _cancellationTokenSource = null, cancellationToken);
                                //_loadGenerationTask.Start();

                                _logger.Info("Load generation complete.");
                            }
                            finally
                            {
                                _cancellationTokenSource.Dispose();
                                _cancellationTokenSource = null;

                                _loadGeneratingEvent.Set();
                            }
                        }
                        else if (receivedMessage.Label.StartsWith("Cancel", StringComparison.InvariantCultureIgnoreCase))
                        {
                            // Mark cancellation message as processed
                            receivedMessage.Complete();

                            // If there is no load currently being generated...
                            if (_loadGeneratingEvent.WaitOne(TimeSpan.Zero))
                            {
                                _logger.Warn("Cancellation message received, but no load generation is currently underway.");
                                _loadGeneratingEvent.Set();
                                return;
                            }

                            // Cancel the current load generation
                            _logger.Info("Cancelling load generation.");
                            _cancellationTokenSource.Cancel();
                        }
                    }
                    catch (Exception ex)
                    {
                        // Handle any message processing specific exceptions here
                        _logger.ErrorFormat("An unexpected error occurred while handling the message.\r\n{0}", ex);
                    }
                }, new OnMessageOptions {MaxConcurrentCalls = 2}); // 2 concurrent calls allows the processing of a cancellation while a load is being generated

                _completedEvent.WaitOne();            
            }
            catch (Exception ex)
            {
                _logger.FatalFormat("The Azure worker Run method failed:\r\n{0}", ex);
            }

            // Don't let this Run method exit too quickly, as we're attempting to ensure logs are written to storage
            while ((DateTime.Now - startTime) < TimeSpan.FromMinutes(1))
            {
                Thread.Sleep(TimeSpan.FromSeconds(5));
            }
        }

        public override bool OnStart()
        {
            try
            {
                // Set the maximum number of concurrent connections to a reasonably large number since we're doing load generation
                ServicePointManager.DefaultConnectionLimit = 200;
                ThreadPool.SetMinThreads(200, 200);

                // Create the queue if it does not exist already
                string connectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");
                _namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);

                // Ensure the Topic exists
                if (!_namespaceManager.TopicExists(TopicPath))
                    _namespaceManager.CreateTopic(TopicPath);

                // Ensure the Subscription for this worker exists
                if (!_namespaceManager.SubscriptionExists(TopicPath, SubscriptionName))
                    _namespaceManager.CreateSubscription(TopicPath, SubscriptionName);

                _subscriptionClient = SubscriptionClient.CreateFromConnectionString(connectionString, TopicPath, SubscriptionName);
            
                // Initialize IoC
                InitializeIoc();
            }
            catch (Exception ex)
            {
                _logger.FatalFormat("The Azure worker's OnStart method failed:\r\n{0}", ex);
            }

            return base.OnStart();
        }

        public override void OnStop()
        {
            _logger.Info("Stopping Azure worker.");

            // Close the connection to Service Bus Queue
            //_generateLoadClient.Close();
            //_cancelLoadClient.Close();
            _subscriptionClient.Close();
            _namespaceManager.DeleteSubscription(TopicPath, SubscriptionName);

            if (_cancellationTokenSource != null
                && !_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
                
                if (!_loadGeneratingEvent.WaitOne(TimeSpan.FromSeconds(120)))
                    _logger.Warn("Load generation did not stop after 120 seconds.  Proceeding with shutdown.");
            }

            _completedEvent.Set();
            
            base.OnStop();
        }

        private static void GenerateLoad(GenerateLoadRequest request, CancellationToken cancellationToken)
        {
            var controller = IoC.Resolve<ILoadTestController>();
            controller.GenerateLoad(request, new LoggerProgressNotifier(), cancellationToken);
        }

        //private void CancelLoad(CancellationTokenSource cancellationTokenSource, Task loadGenerationTask)
        //{
        //    // Cancel the processing
        //    _logger.Warn("Cancellation requested.");
        //    cancellationTokenSource.Cancel();

        //    _logger.Info("Waiting up to 60 seconds for worker cancellation...");

        //    // Wait for up to 1 minute
        //    if (!loadGenerationTask.Wait(TimeSpan.FromSeconds(60)))
        //        _logger.Error("Load generation did not stop within 60 seconds of cancellation request.");
        //}

        private static void InitializeIoc()
        {
            var container = new WindsorContainer();
            container.Kernel.Resolver.AddSubResolver(new ArrayResolver(container.Kernel));

            container.Install(new ConfigurationSpecificInstaller());

            IoC.Initialize(container);
        }
    }
}
