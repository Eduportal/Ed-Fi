using System.Globalization;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using EdFi.Common.Database;
using EdFi.Ods.Api.Common.Authorization;
using EdFi.Ods.Common._Installers.ComponentNaming;
using EdFi.Ods.Security.Metadata.Repositories;
using NUnit.Framework;
using Rhino.Mocks;
using System;
using System.Collections.Generic;
using EdFi.Ods.Api._Installers;
using EdFi.Ods.Entities.Common.IdentityValueMappers;
using Test.Common;

namespace EdFi.Ods.WebService.Tests.Owin
{
    public abstract class OwinTestBase
    {
        internal class OwinStartup : OwinTestStartupBase
        {
            private readonly string _databaseName;
            private readonly string _vendorNamespacePrefix;
            private readonly bool _useUniqueIdIntegration;
            private readonly List<int> _educationOrganizationIds;
            private readonly Func<IDatabaseNameProvider> _createDatabaseNameProvider;
            private readonly Func<IOAuthTokenValidator> _createOAuthTokenValidator;
            private readonly Func<OwinSecurityRepository> _securityRepository;

            internal OwinStartup(string databaseName,
                                    List<int> educationOrganizationIds,
                                    string vendorNamespacePrefix = "http://www.ed-fi.org",
                                     Func<IDatabaseNameProvider> createDatabaseNameProvider = null,
                                    Func<IOAuthTokenValidator> createOAuthTokenValidator = null,
                                    Func<OwinSecurityRepository> securityRepository = null,
                                    bool useUniqueIdIntegration = false)
            {
                System.Diagnostics.Trace.Listeners.Clear();

                _databaseName = databaseName;
                _educationOrganizationIds = educationOrganizationIds;
                _vendorNamespacePrefix = vendorNamespacePrefix;
                _useUniqueIdIntegration = useUniqueIdIntegration;
                _createDatabaseNameProvider = createDatabaseNameProvider ?? CreateDatabaseNameProvider;
                _createOAuthTokenValidator = createOAuthTokenValidator ?? CreateOAuthTokenValidator;
                _securityRepository = securityRepository ?? (() => new OwinSecurityRepository());
            }

            protected override void InstallTestSpecificInstaller(IWindsorContainer container)
            {
                container.Register(Component.For<IDatabaseNameProvider>().Instance(_createDatabaseNameProvider()).IsDefault().Named(DatabaseNameStrategyRegistrationKeys.Sandbox));
                container.Register(Component.For<IOAuthTokenValidator>().Instance(_createOAuthTokenValidator()).IsDefault());
                container.Register(Component.For<ISecurityRepository>().Instance(_securityRepository()).IsDefault());

                // Conditionally, add support for UniqueId integration
                if (_useUniqueIdIntegration)
                    container.Install(new UniqueIdIntegrationInstaller<ParsedGuidUniqueIdToIdValueMapper>());
            }

            protected virtual IDatabaseNameProvider CreateDatabaseNameProvider()
            {
                var databaseNameProvider = MockRepository.GenerateStub<IDatabaseNameProvider>();
                databaseNameProvider.Stub(d => d.GetDatabaseName()).Return(_databaseName);
                return databaseNameProvider;
            }

            protected virtual IOAuthTokenValidator CreateOAuthTokenValidator()
            {
                var oAuthTokenValidator = MockRepository.GenerateStub<IOAuthTokenValidator>();
                oAuthTokenValidator.Stub(t => t.GetClientDetailsForToken(Arg<Guid>.Is.Anything)).Return(new ApiClientDetails
                {
                    ApiKey = DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture),
                    ApplicationId = DateTime.Now.Millisecond,
                    ClaimSetName = "SIS Vendor",
                    NamespacePrefix = _vendorNamespacePrefix,
                    EducationOrganizationIds = _educationOrganizationIds,
                });
                return oAuthTokenValidator;
            }
        }

        protected abstract string BaseDatabase { get; }
        protected abstract string DatabaseName { get; }
        protected virtual bool CreateDatabase
        {
            get { return true; }
        }

        private DatabaseHelper _databaseHelper;

        [TestFixtureSetUp]
        public virtual void SetUp()
        {
            if (!CreateDatabase) return;
            _databaseHelper = new DatabaseHelper();
            _databaseHelper.CopyDatabase(BaseDatabase, DatabaseName);
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            if (CreateDatabase)
                _databaseHelper.DropDatabase(DatabaseName);
        }
    }
}
