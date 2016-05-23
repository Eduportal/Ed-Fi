using Castle.MicroKernel.Registration;
using Castle.Windsor;
using EdFi.Common.Database;
using EdFi.Common.Services;
using EdFi.Ods.Api.Common.Authorization;
using EdFi.Ods.Api.Data.Repositories.BulkOperations;
using EdFi.Ods.Api.Models.Resources;
using EdFi.Ods.Api.Models.Resources.Course;
using EdFi.Ods.Api.Models.Resources.Enums;
using EdFi.Ods.Common;
using EdFi.Ods.Common._Installers.ComponentNaming;
using EdFi.Ods.Common.Context;
using EdFi.Ods.Common.Database;
using EdFi.Ods.Security.Metadata.Repositories;
using EdFi.Ods.WebService.Tests.Extensions;
using EdFi.Ods.WebService.Tests.Owin;
using Microsoft.Owin.Testing;
using Newtonsoft.Json;
using NUnit.Framework;
using Rhino.Mocks;
using Should;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Test.Common;

namespace EdFi.Ods.WebService.Tests.YearSpecificSharedInstanceTests
{

    [TestFixture]
    public class When_submitting_bulk_operations_to_mulitple_year_specific_instances
    {
        #region Setup & Teardown
        private delegate string ResolveDatabaseDelegate();

        private static ISchoolYearContextProvider _owinSchoolYearContextProvider;
        private static ISchoolYearContextProvider _bulkSchoolYearContextProvider;

        private static string ResolveOwinDatabase()
        {
            return ResolveDatabase(_owinSchoolYearContextProvider.GetSchoolYear());
        }

        private static string ResolveBulkDatabase()
        {
            return ResolveDatabase(_bulkSchoolYearContextProvider.GetSchoolYear());
        }

        private static string ResolveDatabase(int schoolYear)
        {
            if (schoolYear == 2014)
                return DatabaseName_2014;

            if (schoolYear == 2015)
                return DatabaseName_2015;

            return string.Empty;
        }
        private const string DatabaseName_2014 = "EdFi_Tests_When_submitting_bulk_operations_to_mulitple_year_specific_instances_2014";
        private const string DatabaseName_2015 = "EdFi_Tests_When_submitting_bulk_operations_to_mulitple_year_specific_instances_2015";

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
                _owinSchoolYearContextProvider = container.Resolve<ISchoolYearContextProvider>();
                
                var databaseNameProvider = MockRepository.GenerateStub<IDatabaseNameProvider>();
                databaseNameProvider.Stub(d => d.GetDatabaseName()).Do(new ResolveDatabaseDelegate(ResolveOwinDatabase));

                container.Register(Component.For<IDatabaseNameProvider>().Instance(databaseNameProvider).IsDefault().Named(DatabaseNameStrategyRegistrationKeys.YearSpecificOds));
                container.Register(Component.For<ISecurityRepository>().Instance(new OwinSecurityRepository()).IsDefault());
                container.Register(Component.For<IOAuthTokenValidator>().Instance(CreateOAuthTokenValidator()).IsDefault());
            }
        }

        private static IDatabaseNameProvider CreateBulkDatabaseNameProvider()
        {
            var databaseNameProvider = MockRepository.GenerateStub<IDatabaseNameProvider>();
            databaseNameProvider.Stub(d => d.GetDatabaseName()).Do(new ResolveDatabaseDelegate(ResolveBulkDatabase));
            return databaseNameProvider;
        }

        private static IOAuthTokenValidator CreateOAuthTokenValidator()
        {
            var oAuthTokenValidator = MockRepository.GenerateStub<IOAuthTokenValidator>();
            oAuthTokenValidator.Stub(t => t.GetClientDetailsForToken(Arg<Guid>.Is.Anything)).Return(new ApiClientDetails
            {
                ApiKey = DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture),
                ApplicationId = DateTime.Now.Millisecond,
                ClaimSetName = "SIS Vendor",
                NamespacePrefix = "http://www.TEST.org/",
                EducationOrganizationIds = new List<int> { 255901 },
            });
            return oAuthTokenValidator;
        }

        private DatabaseHelper _databaseHelper;
        private IHostedService _bulkService;
        private IHostedService _uploadService;
        private IWindsorContainer _container;

        [TestFixtureSetUp]
        public void RunBeforeAnyTests()
        {
            _databaseHelper = new DatabaseHelper();
            _databaseHelper.CopyDatabase("EdFi_Ods_Populated_Template", DatabaseName_2014);
            _databaseHelper.CopyDatabase("EdFi_Ods_Populated_Template", DatabaseName_2015);

            _container = OwinBulkHelper.ConfigureIoCForServices(CreateBulkDatabaseNameProvider, CreateOAuthTokenValidator, () => new OwinSecurityRepository(), true);
            _bulkSchoolYearContextProvider = _container.Resolve<ISchoolYearContextProvider>();

            _bulkService = _container.Resolve<IHostedService>("BulkWorker");
            _bulkService.Start();

            _uploadService = _container.Resolve<IHostedService>("UploadWorker");
            _uploadService.Start();
        }

        [TestFixtureTearDown]
        public void RunAfterAnyTests()
        {
            try
            {
                _bulkService.Stop();
                _uploadService.Stop();
            }
            finally
            {
                _databaseHelper.DropDatabase(DatabaseName_2014);
                _databaseHelper.DropDatabase(DatabaseName_2015);

                _container.Dispose();
            }            
        }

        #endregion
        private const string XmlTemplate = @"<?xml version='1.0' encoding='UTF-8'?>
            <InterchangeEducationOrganization xmlns:ann='http://ed-fi.org/annotation' xsi:schemaLocation='http://ed-fi.org/Interchange-EducationOrganization.xsd' xmlns='http://ed-fi.org/0200' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>
                <Course>
                    <CourseCode>##COURSECODE##</CourseCode>
                    <CourseTitle>##SomeUniqueInformation##</CourseTitle>
                    <NumberOfParts>1</NumberOfParts>
                    <AcademicSubject>
                        <CodeValue>Mathematics</CodeValue>
                    </AcademicSubject>
                    <EducationOrganizationReference>
                        <EducationOrganizationIdentity>
                            <EducationOrganizationId>255901001</EducationOrganizationId>
                        </EducationOrganizationIdentity>
                    </EducationOrganizationReference>
                </Course>
            </InterchangeEducationOrganization>";

        private readonly string[] _yearsToTest = { "2014", "2015" };    

        [Test]
        public void Bulk_operation_status_should_be_completed_after_process_is_done()
        {
            System.Diagnostics.Trace.Listeners.Clear();

            using (var server = TestServer.Create<OwinStartup>())
            {
                using (var client = new HttpClient(server.Handler))
                {
                    client.Timeout = new TimeSpan(0, 0, 15, 0);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                    var courseCode = DateTime.Now.Ticks.ToString();
                    var xml = XmlTemplate.Replace("##COURSECODE##", courseCode);
                    var testData = OwinBulkHelper.BuildTestData(xml, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture));
                    var testXml = testData.Xml;

                    foreach (var year in _yearsToTest)
                    {
                        var request = new BulkOperationCreateRequest {UploadFiles = new[] {new UploadFileRequest {Format = "text/xml", InterchangeType = InterchangeType.EducationOrganization.Name, Size = testXml.Length}}};
                        var postResult = client.PostAsync(OwinUriHelper.BuildApiUri(year, "BulkOperations"), new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json")).Result;
                        postResult.StatusCode.ShouldEqual(HttpStatusCode.Created);

                        var postContent = postResult.Content.ReadAsAsync<BulkOperationResource>().Result;
                        var uploadedFileId = postContent.UploadFiles.First().Id;

                        var url = string.Format("http://owin/api/v2.0/{3}/Uploads/{0}/chunk?offset={1}&size={2}", uploadedFileId, 0, testXml.Length, year);

                        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(testXml));
                        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                        var content = new MultipartFormDataContent {{fileContent, "File"}};

                        var uploadResult = client.PostAsync(url, content).Result;
                        uploadResult.EnsureSuccessStatusCode();
                        uploadResult.StatusCode.ShouldEqual(HttpStatusCode.Created);

                        url = string.Format("http://owin/api/v2.0/{1}/Uploads/{0}/commit", uploadedFileId, year);
                        content = new MultipartFormDataContent {new StringContent(JsonConvert.SerializeObject(new {uploadId = uploadedFileId}), Encoding.UTF8, "application/json")};

                        var commitResponse = client.PostAsync(url, content).Result;
                        commitResponse.StatusCode.ShouldEqual(HttpStatusCode.Accepted);

                        var status = OwinBulkHelper.WaitForBulkOperationToComplete(client, postContent.Id, year);
                        var failureMessage = String.Format("Application year specific for year '{0}' status failed.", year);
                        status.ShouldEqual(BulkOperationStatus.Completed, failureMessage);

                        url = string.Format("{0}/{1}/Exceptions/{2}", OwinUriHelper.BuildApiUri(year, "BulkOperations"), postContent.Id, uploadedFileId);
                        var getExceptions = client.GetAsync(url).Result;
                        getExceptions.EnsureSuccessStatusCode();

                        var exceptions = getExceptions.Content.ReadAsAsync<BulkOperationException[]>().Result;
                        exceptions.Length.ShouldEqual(0);

                        var getCourseResult = client.GetAsync(OwinUriHelper.BuildApiUri(year, "courses", string.Format("educationOrganizationId={0}&code={1}", "255901001", courseCode))).Result;
                        getCourseResult.EnsureSuccessStatusCode();
                        var course = JsonConvert.DeserializeObject<Course>(getCourseResult.Content.ReadAsStringAsync().Result);
                        course.CourseCode.ShouldEqual(courseCode);
                    }
                }
            }
        }
    }

    [TestFixture]
    public class When_submitting_a_bulk_operation_and_attempting_to_upload_to_a_different_year
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
        private const string DatabaseName_2014 = "EdFi_Tests_When_submitting_a_bulk_operation_and_attempting_to_upload_to_a_different_year_2014";
        private const string DatabaseName_2015 = "EdFi_Tests_When_submitting_a_bulk_operation_and_attempting_to_upload_to_a_different_year_2015";

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
        private const string XmlTemplate = @"<?xml version='1.0' encoding='UTF-8'?>
                <InterchangeStandards xmlns='http://ed-fi.org/0200' xmlns:ann='http://ed-fi.org/annotation' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xsi:schemaLocation='http://ed-fi.org/Interchange-Standards.xsd'>
                    <LearningStandard>
                        <LearningStandardId>111.32.NA.A.1.A</LearningStandardId>
                        <Description>describe independent and dependent quantities in functional relationships;</Description>
                        <ContentStandard>
                            <Title>Unknown</Title>
                        </ContentStandard>
                        <AcademicSubject>
                          <CodeValue>Science</CodeValue>
                        </AcademicSubject>
                        <CourseTitle>Algebra I</CourseTitle>
                    </LearningStandard>
                </InterchangeStandards>";

        [Test]
        public void Should_return_400_bad_request()
        {
            System.Diagnostics.Trace.Listeners.Clear();

            using (var server = TestServer.Create<OwinStartup>())
            {
                using (var client = new HttpClient(server.Handler))
                {
                    client.Timeout = new TimeSpan(0, 0, 15, 0);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                    var courseCode = DateTime.Now.Ticks.ToString();
                    var xmlData = XmlTemplate.Replace("##COURSECODE##", courseCode);
                    const int year = 2014;
                    var bulkOperationUrl = OwinUriHelper.BuildApiUri(year.ToString(CultureInfo.InvariantCulture), "BulkOperations");
                    var testData = OwinBulkHelper.BuildTestData(xmlData, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture));
                    var testXml = testData.Xml;

                    var request = new BulkOperationCreateRequest { UploadFiles = new[] { new UploadFileRequest { Format = "text/xml", InterchangeType = InterchangeType.EducationOrganization.Name, Size = testXml.Length } } };
                    var postResult = client.PostAsync(bulkOperationUrl, new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json")).Result;
                    postResult.StatusCode.ShouldEqual(HttpStatusCode.Created);
                    var postContent = postResult.Content.ReadAsAsync<BulkOperationResource>().Result;
                    var uploadedFileId = postContent.UploadFiles.First().Id;

                    var url = string.Format("http://owin/api/v2.0/2015/Uploads/{0}/chunk?offset={1}&size={2}", uploadedFileId, 0, testXml.Length);

                    var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(testXml));
                    fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                    var content = new MultipartFormDataContent { { fileContent, "File" } };
                    var uploadResult = client.PostAsync(url, content).Result;
                    uploadResult.StatusCode.ShouldEqual(HttpStatusCode.BadRequest);
                    uploadResult.Content.ReadAsStringAsync().Result.ShouldContain("School year of 2015 does not match bulk operation's school year of 2014.");

                }
            }
        }
    }
}
