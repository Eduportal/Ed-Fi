using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using CommandLine;
using CommandLine.Text;
using EdFi.Common.InversionOfControl;
using EdFi.Ods.Utilities.LoadGeneration._Installers;
using log4net;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;

namespace EdFi.Ods.Utilities.LoadGeneration.GenerateLoad
{
    public class GenerateLoadCommandLineArguments
    {
        // Load generation context
        [Option('s', "StudentCount", Required = true, HelpText = "The number of students worth of data to be generated.")]
        public int StudentCount { get; set; }

        [Option('t', "ThreadCount", DefaultValue = 1, HelpText = "The number of threads to use to generate load.")]
        public int ThreadCount { get; set; }

        [Option('p', "DataProfilePath", Required = true, HelpText = "The path to the file containing the data profile.")]
        public string DataProfilePath { get; set; }

        // Security context
        [Option('k', "ApiKey", Required = true, HelpText = "The API key to use when calling the Ed-Fi REST API.")]
        public string ApiKey { get; set; }

        [Option('x', "ApiSecret", Required = true, HelpText = "The secret to use when calling the Ed-Fi REST API.")]
        public string ApiSecret { get; set; }

        [Option('u', "ApiUrl", Required = true, HelpText = "The URL of the Ed-Fi REST API to target.")]
        public string ApiUrl { get; set; }

        [Option('o', "OAuthUrl", Required = true, HelpText = "The URL for performing API key authentication.")]
        public string OAuthUrl { get; set; }

        [Option('l', "LocalEducationAgencyIds", Required = true, HelpText = "A comma-separated list of authorized Local Education Agency Ids to use for generating data.")]
        public string LocalEducationAgencyIds { get; set; }

        [Option("SdkPath", Required = true, HelpText = "The absolute or relative path (from the executing assembly) to the REST API SDK assembly.")]
        public string ApiSdkAssemblyPath { get; set; }

        [Option('y', "SchoolYear", DefaultValue = 9999, HelpText = "The current school year to use in the base route for accessing REST API resources, types and descriptors.")]
        public int SchoolYear { get; set; }

        [Option('r', "ResourceName", HelpText = "The single resource to generate instead of using the data profile file.")]
        public string ResourceName { get; set; }

        [Option("RefreshCache", DefaultValue = false, HelpText = "Clears the local cache of Ed-Fi types and descriptor values causing the load generator to re-request the values.")]
        public bool RefreshCache { get; set; }

        [Option("UseAzure", DefaultValue = false, HelpText = "Indicates the load generator should use deployed Windows Azure workers to perform load generation.")]
        public bool UseAzure { get; set; }

        [Option("Cancel", DefaultValue = false, HelpText = "Indicates the load generator should cancel an already in-progress Windows Azure-base load generation.")]
        public bool Cancel { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this,
              (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }

    class Program
    {
        private static ILog logger = log4net.LogManager.GetLogger(typeof(Program));

        static void Main(string[] args)
        {
            try
            {
                // Parse the command line arguments
                logger.Info("Processing command line arguments...");

                // https://commandline.codeplex.com/
                var arguments = new GenerateLoadCommandLineArguments();
                Parser.Default.ParseArguments(args, arguments);

                // Help requested?
                if (args.Any(s => s.Equals("--help", StringComparison.InvariantCultureIgnoreCase) || s.Equals("-h", StringComparison.InvariantCultureIgnoreCase)))
                    return;

                if (arguments.UseAzure || arguments.Cancel)
                {
                    GenerateLoadAzure(arguments);
                    return;
                }

                // Initialize the container
                logger.Info("Initializing IoC...");
                InitializeIoc();

                // Initiate load generation
                logger.Info("Starting load generation...");

                // Create a cancellation token for killing the load
                var cancellationTokenSource = new CancellationTokenSource();
                var cancellationToken = cancellationTokenSource.Token;

                DisplayLaunchMessage();

                // Start a separate worker thread
                var loadGenerationTask = new Task(() => GenerateLoad(arguments, cancellationToken));
                loadGenerationTask.Start();

                // Keep the worker alive
                while (!IsFinished(loadGenerationTask))
                {
                    // Check for pending keyboard input
                    if (Console.KeyAvailable)
                    {
                        Console.WriteLine("======= Cancellation Requested =======");
                        CancelLoad(cancellationTokenSource, loadGenerationTask);

                        // Go ahead and end main process
                        break;
                    }                    
                }

                logger.Info("Load generation finished.");
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("An unexpected exception occurred:\r\n{0}", ex);

                Console.WriteLine("Hit ENTER to quit.");
                Console.ReadLine();
            }
        }

        private static void DisplayLaunchMessage()
        {
            Console.BackgroundColor = ConsoleColor.Blue;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Hit a key at any time to end load generation.");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.BackgroundColor = ConsoleColor.Black;
        }

        private static bool IsFinished(Task t)
        {
            return t.Wait(TimeSpan.FromSeconds(1));
        }

        private static void CancelLoad(CancellationTokenSource cancellationTokenSource, Task loadGenerationTask)
        {
            // Cancel the processing
            logger.Warn("Cancellation requested.");
            cancellationTokenSource.Cancel();

            logger.Info("Waiting up to 60 seconds for worker cancellation...");

            // Wait for up to 1 minute
            if (!loadGenerationTask.Wait(TimeSpan.FromSeconds(60)))
                logger.Error("Load generation did not stop within 60 seconds of cancellation request.");
        }

        private const string TopicPath = "LoadGeneration";

        private static void GenerateLoadAzure(GenerateLoadCommandLineArguments arguments)
        {
            string connectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");
            var namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);

            if (!namespaceManager.TopicExists(TopicPath))
                namespaceManager.CreateTopic(TopicPath);

            var request = new GenerateLoadRequest
            {
                ApiKey = arguments.ApiKey,
                ApiSecret = arguments.ApiSecret,
                ApiUrl = arguments.ApiUrl,
                ApiSdkAssemblyPath = arguments.ApiSdkAssemblyPath,
                OAuthUrl = arguments.OAuthUrl,
                SchoolYear = arguments.SchoolYear,
                DataProfilePath = arguments.DataProfilePath,
                StudentCount = arguments.StudentCount,
                ThreadCount = arguments.ThreadCount,
                LocalEducationAgencyIds =
                    arguments.LocalEducationAgencyIds.Split(new[] { ';', ',' })
                        .Select(x => Convert.ToInt32(x)).ToArray(),
                ResourceName = arguments.ResourceName,
                RefreshCache = arguments.RefreshCache,
            };

            var topicClient = TopicClient.CreateFromConnectionString(connectionString, TopicPath);

            BrokeredMessage brokeredMessage;
            
            if (arguments.Cancel)
                brokeredMessage = new BrokeredMessage { Label = "Cancel" };
            else 
                brokeredMessage = new BrokeredMessage(request) { Label = "GenerateLoad" };

            topicClient.Send(brokeredMessage);
            topicClient.Close();

            Console.WriteLine(brokeredMessage.Label + " message sent.");
        }

        private static void GenerateLoad(GenerateLoadCommandLineArguments arguments, CancellationToken cancellationToken)
        {
            var request = new GenerateLoadRequest
            {
                ApiKey = arguments.ApiKey,
                ApiSecret = arguments.ApiSecret,
                ApiUrl = arguments.ApiUrl,
                ApiSdkAssemblyPath = arguments.ApiSdkAssemblyPath,
                OAuthUrl = arguments.OAuthUrl,
                SchoolYear = arguments.SchoolYear,
                DataProfilePath = arguments.DataProfilePath,
                StudentCount = arguments.StudentCount,
                ThreadCount = arguments.ThreadCount,
                LocalEducationAgencyIds =
                    arguments.LocalEducationAgencyIds.Split(new[] {';', ','})
                        .Select(x => Convert.ToInt32(x)).ToArray(),
                ResourceName = arguments.ResourceName,
                RefreshCache = arguments.RefreshCache,
            };

            var controller = IoC.Resolve<ILoadTestController>();
            controller.GenerateLoad(request, new LoggerProgressNotifier(), cancellationToken);
        }

        private static void InitializeIoc()
        {
            var container = new WindsorContainer();
            container.Kernel.Resolver.AddSubResolver(new ArrayResolver(container.Kernel));

            container.Install(new ConfigurationSpecificInstaller());

            IoC.Initialize(container);
        }
    }
}
