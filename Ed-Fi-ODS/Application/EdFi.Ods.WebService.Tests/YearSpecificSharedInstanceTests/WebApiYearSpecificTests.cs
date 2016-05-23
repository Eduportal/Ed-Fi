using Castle.MicroKernel.Registration;
using Castle.Windsor;
using EdFi.Common.Database;
using EdFi.Ods.Api.Common.Authorization;
using EdFi.Ods.Common._Installers.ComponentNaming;
using EdFi.Ods.Common.Context;
using EdFi.Ods.Common.Database;
using EdFi.Ods.Security.Metadata.Repositories;
using EdFi.Ods.WebService.Tests._Helpers;
using EdFi.Ods.WebService.Tests.Extensions;
using EdFi.Ods.WebService.Tests.Owin;
using Microsoft.Owin.Testing;
using Newtonsoft.Json;
using NUnit.Framework;
using Rhino.Mocks;
using Should;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using EdFi.Ods.Api._Installers;
using EdFi.Ods.Entities.Common.IdentityValueMappers;
using Test.Common;

namespace EdFi.Ods.WebService.Tests.YearSpecificSharedInstanceTests
{
    [TestFixture]
    public class When_putting_a_resource_to_a_shared_year_specific_instance
    {
        #region Setup & Teardown
        private delegate string ResolveDatabaseDelegate();

        private static ISchoolYearContextProvider _schoolYearContextProvider;

        private static string ResolveDatabase()
        {
            int schoolYear = _schoolYearContextProvider.GetSchoolYear();
            
            if (schoolYear == 2014)
                return DatabaseName_2014;

            if (schoolYear == 2015)
                return DatabaseName_2015;

            return string.Empty;
        }
        private const string DatabaseName_2014 = "EdFi_Tests_When_putting_a_resource_to_a_shared_year_specific_instance_2014";
        private const string DatabaseName_2015 = "EdFi_Tests_When_putting_a_resource_to_a_shared_year_specific_instance_2015";

        private class OwinStartup : OwinTestStartupBase
        {
            protected override void RegisterOdsDatabase(IWindsorContainer container)
            {
                container.Register(Component.For<IDatabaseConnectionStringProvider>()
                                            .Named("IDatabaseConnectionStringProvider.Ods")
                                            .ImplementedBy<PrototypeWithDatabaseNameOverrideDatabaseConnectionStringProvider>()
                                            .DependsOn(Dependency.OnValue("prototypeConnectionStringName", "EdFi_Ods"))
                                            .DependsOn(Dependency.OnComponent(typeof(IDatabaseNameProvider), DatabaseNameStrategyRegistrationKeys.YearSpecificOds)));
            }

            protected override void InstallTestSpecificInstaller(IWindsorContainer container)
            {
                // Year-specific integration tests perform UniqueId operations (POST to /identities)
                container.Install(new UniqueIdIntegrationInstaller<ParsedGuidUniqueIdToIdValueMapper>());

                _schoolYearContextProvider = container.Resolve<ISchoolYearContextProvider>();
                
                var databaseNameProvider = MockRepository.GenerateStub<IDatabaseNameProvider>();
                databaseNameProvider.Stub(d => d.GetDatabaseName()).Do(new ResolveDatabaseDelegate(ResolveDatabase));

                container.Register(Component.For<IDatabaseNameProvider>().Instance(databaseNameProvider).IsDefault().Named(DatabaseNameStrategyRegistrationKeys.YearSpecificOds));
                container.Register(Component.For<ISecurityRepository>().Instance(new OwinSecurityRepository()).IsDefault());

                var oAuthTokenValidator = MockRepository.GenerateStub<IOAuthTokenValidator>();
                oAuthTokenValidator.Stub(t => t.GetClientDetailsForToken(Arg<Guid>.Is.Anything)).Return(new ApiClientDetails
                {
                    ApiKey = DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture),
                    ApplicationId = DateTime.Now.Millisecond,
                    ClaimSetName = "SIS Vendor",
                    NamespacePrefix = "http://www.ed-fi.org",
                    EducationOrganizationIds = new List<int> { 255901 },
                });
                container.Register(Component.For<IOAuthTokenValidator>().Instance(oAuthTokenValidator).IsDefault());
            }
        }

        private DatabaseHelper _databaseHelper;

        [TestFixtureSetUp]
        public void RunBeforeAnyTests()
        {
            _databaseHelper = new DatabaseHelper();
            _databaseHelper.CopyDatabase("EdFi_Ods_Populated_Template", DatabaseName_2014);
            _databaseHelper.CopyDatabase("EdFi_Ods_Populated_Template", DatabaseName_2015);
        }

        [TestFixtureTearDown]
        public void RunAfterAnyTests()
        {
            _databaseHelper.DropDatabase(DatabaseName_2014);
            _databaseHelper.DropDatabase(DatabaseName_2015);
        }
        #endregion

        public bool StudentExists(int year, string uniqueId)
        {
            using (var conn = new SqlConnection(GetConnectionString(year)))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = "SELECT 1 FROM [edfi].[Student] WHERE StudentUniqueId = @uniqueId";
                cmd.Parameters.AddWithValue("@uniqueId", uniqueId);
                var retval = cmd.ExecuteScalar();
                return retval != null;
            }
        }

        public string GetConnectionString(int schoolYear)
        {
            
            var builder = new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["EdFi_Ods"].ConnectionString);

            if (schoolYear == 2014)
                builder.InitialCatalog = DatabaseName_2014;

            if (schoolYear == 2015)
                builder.InitialCatalog = DatabaseName_2015;

            return builder.ConnectionString;
        }
       
        [Test]
        public void Should_update_specified_instance_db()
        {
            System.Diagnostics.Trace.Listeners.Clear();

            using (var server = TestServer.Create<OwinStartup>())
            {
                using (var client = new HttpClient(server.Handler))
                {
                    client.Timeout = new TimeSpan(0, 0, 15, 0);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                    //create 2014
                    var uniqueId2014Response = client.PostAsync(OwinUriHelper.BuildApiUri(null, "identities"), new StringContent(JsonConvert.SerializeObject(UniqueIdCreator.InitializeAPersonWithUniqueData()), Encoding.UTF8, "application/json")).Result;
                    var uniqueId2014 = UniqueIdCreator.ExtractIdFromHttpResponse(uniqueId2014Response);

                    var create2014Response = client.PostAsync(OwinUriHelper.BuildApiUri("2014", "students"), new StringContent(ResourceHelper.CreateStudent(uniqueId2014, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture)), Encoding.UTF8, "application/json")).Result;
                    create2014Response.EnsureSuccessStatusCode();

                    //create 2015
                    var uniqueId2015Response = client.PostAsync(OwinUriHelper.BuildApiUri(null, "identities"), new StringContent(JsonConvert.SerializeObject(UniqueIdCreator.InitializeAPersonWithUniqueData()), Encoding.UTF8, "application/json")).Result;
                    var uniqueId2015 = UniqueIdCreator.ExtractIdFromHttpResponse(uniqueId2015Response);

                    var create2015Response = client.PostAsync(OwinUriHelper.BuildApiUri("2015", "students"), new StringContent(ResourceHelper.CreateStudent(uniqueId2015, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture)), Encoding.UTF8, "application/json")).Result;
                    create2015Response.EnsureSuccessStatusCode();

                    StudentExists(2014, uniqueId2014).ShouldBeTrue();
                    StudentExists(2015, uniqueId2014).ShouldBeFalse();

                    StudentExists(2014, uniqueId2015).ShouldBeFalse();
                    StudentExists(2015, uniqueId2015).ShouldBeTrue();
                }
            }            
        }
    }
}
