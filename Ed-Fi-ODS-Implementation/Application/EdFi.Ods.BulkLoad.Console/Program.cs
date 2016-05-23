using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Castle.MicroKernel.Registration;
using EdFi.Common.IO;
using EdFi.Ods.BulkLoad.Core;
using EdFi.Ods.BulkLoad.Core.Data;
using EdFi.Ods.Common;

namespace EdFi.Ods.BulkLoad.Console
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var config = ParseArguments(args);
            var container = Bootstrapper.Bootstrap(config);

            var localBulkOperation = new LocalBulkOperationInitializer(
                new InterchangeFileTypeManifestTranslator(config.Manifest),
                container.Resolve<ICreateBulkOperation>(),
                config,
                container.Resolve<IFileSystem>())
                .CreateOperationAndGetLocalFiles();

            container.Register(Component
                .For<IValidateAndSourceFiles>()
                .ImplementedBy<ValidateAndSourceLocalOnlyFiles>()
                .DependsOn(Dependency.OnValue<IEnumerable<LocalUploadFile>>(localBulkOperation.LocalUploadFiles))
                .IsDefault()); // Interface is externally defined, so must be marked as default

            if (localBulkOperation.IsEmpty)
            {
                System.Console.WriteLine("Found zero files to process in the source directory.");
                ShowHelpInformationAndExit();
            }

            var exitCode = 0;
            var bulkExecutor = container.Resolve<IBulkExecutor>();
            try
            {
                exitCode = bulkExecutor.Execute(localBulkOperation.OperationId) ? 0 : 1;
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e);
                ShowHelpInformationAndExit(-1);
            }
            return exitCode;
        }


        private static BulkLoaderConfiguration ParseArguments(string[] args)
        {
            if (!args.Any())
                ShowHelpInformationAndExit();

            try
            {
                var parsed = new CommandLineParser().Parse(args);
                if (parsed.ContainsKey(TokenType.Help))
                    ShowHelpInformationAndExit();
                return new ConfigurationBuilder().BuildConfiguration(parsed);
            }
            catch (ArgumentException e)
            {
                System.Console.WriteLine();
                System.Console.WriteLine("Bad command line options: {0}", e);
                System.Console.WriteLine();
                System.Console.WriteLine("Help:");
                ShowHelpInformationAndExit(-1);
            }
            return null;
        }

        private static void ShowHelpInformationAndExit(int exitCode = 0)
        {
            var exeName = string.Format("{0}.exe", Assembly.GetExecutingAssembly().GetName().Name);

            System.Console.WriteLine();
            System.Console.WriteLine(@"Loads interchange files located in specified folder into database ");
            System.Console.WriteLine();
            System.Console.WriteLine(@"USAGE::");
            System.Console.WriteLine(@"    {0} <options>", exeName);
            System.Console.WriteLine();
            System.Console.WriteLine(@"OPTIONS::");
            System.Console.WriteLine(@"    /f sourceFolder      path to folder containing interchange files");
            System.Console.WriteLine(@"    [/d databaseName]    use the provided database name");
            System.Console.WriteLine(@"    [/c conectionString] use the provided database connection");
            System.Console.WriteLine(@"    [/m manifest]        path to file manifest");
            System.Console.WriteLine(@"    [/h]                  Show this help message.");
            System.Console.WriteLine();
            
            Environment.Exit(exitCode);
        }
    }
}
