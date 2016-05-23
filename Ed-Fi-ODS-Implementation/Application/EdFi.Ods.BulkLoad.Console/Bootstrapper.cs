using Castle.Windsor;
using EdFi.Common.InversionOfControl;
using EdFi.Ods.BulkLoad.Console._Installers;
using EdFi.Ods.BulkLoad.Core;
using EdFi.Ods.Entities.Repositories.NHibernate.Architecture;
using log4net;


namespace EdFi.Ods.BulkLoad.Console
{
    public class Bootstrapper
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(Bootstrapper));

        public static IWindsorContainer Bootstrap(BulkLoaderConfiguration bulkLoaderConfiguration)
        {
            DumpConfiguration(bulkLoaderConfiguration);

            var iocFactory = new InversionOfControlContainerFactory();

            // Create the container
            var container = iocFactory.CreateContainer(c =>
            {
                // Apply connection string resolution conventions
                c.AddFacility<DatabaseConnectionStringProviderFacility>();

                // This is here because there's currently no way to pass outside context to the 
                // config-specific installer (i.e. the literal connection string from bulk load config)
                c.Install(new DatabaseConnectionStringProviderInstaller(bulkLoaderConfiguration.OdsConnectionString));
            });

            InitializeNHibernate(container);

            return container;
        }

        private static void DumpConfiguration(BulkLoaderConfiguration config)
        {
            System.Console.WriteLine();
            System.Console.WriteLine("Running Bulk Loader with the following configuration: ");
            System.Console.WriteLine();
            System.Console.WriteLine("Source Folder   -> [{0}]", config.SourceFolder);
            System.Console.WriteLine("Manifest        -> [{0}]", config.Manifest);
            System.Console.WriteLine();
        }

        private static void InitializeNHibernate(IWindsorContainer container)
        {
            (new NHibernateConfigurator()).Configure(container);
        }
    }
}