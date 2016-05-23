using Castle.MicroKernel.Registration;
using Castle.Windsor;
using EdFi.Common.Database;
using EdFi.Common.Services;
using EdFi.Ods.Api.Common.Authorization;
using EdFi.Ods.Common._Installers.ComponentNaming;
using EdFi.Ods.Security.Metadata.Repositories;
using NUnit.Framework;
using System;
using EdFi.Ods.Api._Installers;
using EdFi.Ods.Entities.Common.IdentityValueMappers;
using Test.Common;

namespace EdFi.Ods.WebService.Tests.Owin
{
    public abstract class OwinBulkTestBase
    {
        internal class OwinStartup : OwinTestStartupBase
        {
            private readonly Func<IDatabaseNameProvider> _createDatabaseNameProvider;
            private readonly Func<IOAuthTokenValidator> _createOAuthTokenValidator;
            private readonly Func<OwinSecurityRepository> _securityRepository;

            internal OwinStartup(Func<IDatabaseNameProvider> createDatabaseNameProvider = null, Func<IOAuthTokenValidator> createOAuthTokenValidator = null, Func<OwinSecurityRepository> securityRepository = null)
            {
                System.Diagnostics.Trace.Listeners.Clear();

                _createDatabaseNameProvider = createDatabaseNameProvider;
                _createOAuthTokenValidator = createOAuthTokenValidator;
                _securityRepository = securityRepository ?? (() => new OwinSecurityRepository());
            }

            protected override void InstallTestSpecificInstaller(IWindsorContainer container)
            {
                // Bulk integration tests perform UniqueId operations (POST to /identities)
                container.Install(new UniqueIdIntegrationInstaller<ParsedGuidUniqueIdToIdValueMapper>());

                container.Register(Component.For<IDatabaseNameProvider>().Instance(_createDatabaseNameProvider()).IsDefault().Named(DatabaseNameStrategyRegistrationKeys.Sandbox));
                container.Register(Component.For<IOAuthTokenValidator>().Instance(_createOAuthTokenValidator()).IsDefault());
                container.Register(Component.For<ISecurityRepository>().Instance(_securityRepository()).IsDefault());
            }
        }

        protected abstract string BaseDatabase { get; }
        protected abstract string DatabaseName { get; }
        protected abstract Func<IDatabaseNameProvider> CreateDatabaseNameProvider { get; }
        protected abstract Func<IOAuthTokenValidator> CreateOAuthTokenValidator { get; }
        protected abstract Func<OwinSecurityRepository> CreateSecurityRepository { get; }

        private DatabaseHelper _databaseHelper;
        private IHostedService _bulkService;
        private IHostedService _uploadService;
        private IWindsorContainer _container;

        [TestFixtureSetUp]
        public void SetUp()
        {
            _databaseHelper = new DatabaseHelper();
            _databaseHelper.CopyDatabase(BaseDatabase, DatabaseName);

            _container = OwinBulkHelper.ConfigureIoCForServices(CreateDatabaseNameProvider, CreateOAuthTokenValidator, CreateSecurityRepository);

            _bulkService = _container.Resolve<IHostedService>("BulkWorker");
            _bulkService.Start();

            _uploadService = _container.Resolve<IHostedService>("UploadWorker");
            _uploadService.Start();
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            try
            {
                _bulkService.Stop();
                _uploadService.Stop();
            }
            finally
            {
                _databaseHelper.DropDatabase(DatabaseName);

                _container.Dispose();
            }
        }
    }
}
