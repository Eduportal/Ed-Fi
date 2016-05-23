using EdFi.Common.Database;
using EdFi.Ods.Api.Common.Authorization;
using EdFi.Ods.Api.Data.Repositories.BulkOperations;
using EdFi.Ods.Api.Models.Resources;
using EdFi.Ods.Api.Models.Resources.AcademicSubjectDescriptor;
using EdFi.Ods.Api.Models.Resources.Enums;
using EdFi.Ods.Common;
using EdFi.Ods.Security.Metadata.Contexts;
using EdFi.Ods.Security.Metadata.Models;
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

namespace EdFi.Ods.WebService.Tests.XmlShredding
{
    [TestFixture]
    public class XmlBulkDescriptorTests : OwinBulkTestBase
    {
        private static readonly string _databaseName = string.Format("EdFi_Tests_XmlBulkDescriptorTests_{0}", Guid.NewGuid().ToString("N"));
        private readonly List<int> EducationOrganizationIds = new List<int> {255901};

        protected override string DatabaseName { get { return _databaseName; } }
        protected override string BaseDatabase { get { return "EdFi_Ods_Populated_Template"; } }
        protected override Func<IDatabaseNameProvider> CreateDatabaseNameProvider { get { return CreateDatabaseNameProviderFunc; } }
        protected override Func<IOAuthTokenValidator> CreateOAuthTokenValidator { get { return CreateOAuthTokenValidatorFunc; } }
        protected override Func<OwinSecurityRepository> CreateSecurityRepository { get { return CreateSecurityRepositoryFunc; } }

        private IDatabaseNameProvider CreateDatabaseNameProviderFunc()
        {
            var databaseNameProvider = MockRepository.GenerateStub<IDatabaseNameProvider>();
            databaseNameProvider.Stub(d => d.GetDatabaseName()).Return(DatabaseName);
            return databaseNameProvider;
        }
        private IOAuthTokenValidator CreateOAuthTokenValidatorFunc()
        {
            var oAuthTokenValidator = MockRepository.GenerateStub<IOAuthTokenValidator>();
            oAuthTokenValidator.Stub(t => t.GetClientDetailsForToken(Arg<Guid>.Is.Anything)).Return(new ApiClientDetails
            {
                ApiKey = DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture),
                ApplicationId = DateTime.Now.Millisecond,
                ClaimSetName = "SIS Vendor",
                NamespacePrefix = "http://www.TEST.org/",
                EducationOrganizationIds = EducationOrganizationIds,
            });
            return oAuthTokenValidator;
        }
        private static OwinSecurityRepository CreateSecurityRepositoryFunc()
        {
            var securityRepo = new OwinSecurityRepository();

            var authorizationStrategy = securityRepo.GetAuthorizationStrategies().First(auth => auth.AuthorizationStrategyName.Equals("NamespaceBased"));
            var claimset = securityRepo.GetClaimSets().First(cs => cs.ClaimSetName.Equals("SIS Vendor"));

            var academicSubjectDescriptor = securityRepo.GetResourceClaims().First(rc => rc.ResourceName.Equals("academicSubjectDescriptor"));
            var assessmentPeriodDescriptor = securityRepo.GetResourceClaims().First(rc => rc.ResourceName.Equals("assessmentPeriodDescriptor"));

            var resourceClaimAuthorizationStrategies = securityRepo.GetResourceClaimAuthorizationStrategies();
            var claimSetResourceClaims = securityRepo.GetClaimSetResourceClaims();

            foreach (var action in securityRepo.GetActions())
            {
                resourceClaimAuthorizationStrategies.Add(new ResourceClaimAuthorizationStrategy
                {
                    Action = action,
                    AuthorizationStrategy = authorizationStrategy,
                    ResourceClaim = academicSubjectDescriptor
                });

                claimSetResourceClaims.Add(new ClaimSetResourceClaim
                {
                    Action = action,
                    ClaimSet = claimset,
                    ResourceClaim = academicSubjectDescriptor
                });

                resourceClaimAuthorizationStrategies.Add(new ResourceClaimAuthorizationStrategy
                {
                    Action = action,
                    AuthorizationStrategy = authorizationStrategy,
                    ResourceClaim = assessmentPeriodDescriptor
                });

                claimSetResourceClaims.Add(new ClaimSetResourceClaim
                {
                    Action = action,
                    ClaimSet = claimset,
                    ResourceClaim = assessmentPeriodDescriptor
                });
            }

            securityRepo.ReIntitalize(securityRepo.GetApplication(), securityRepo.GetActions(), securityRepo.GetClaimSets(), securityRepo.GetResourceClaims(), securityRepo.GetAuthorizationStrategies(), claimSetResourceClaims, resourceClaimAuthorizationStrategies);

            return securityRepo;
        }
                    
        [Test]
        public void When_submitting_course_with_vendor_descriptor_should_have_status_of_completed()
            {
                const string codeValue = "9876";
                const string description = "Philosophy";
                const string typeMap = "Mathematics";
                const string courseCode = "Phil-1";
                const string descriptorType = "AcademicSubject";
                const int educationOrganizationId = 255901;

                const string XmlTemplate = @"<?xml version='1.0' encoding='UTF-8'?>
<InterchangeEducationOrganization xmlns:ann='http://ed-fi.org/annotation' xsi:schemaLocation='http://ed-fi.org/Interchange-EducationOrganization.xsd' xmlns='http://ed-fi.org/0200' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>
    <Course>
        <CourseCode>{0}</CourseCode>
        <CourseTitle>##SomeUniqueInformation##</CourseTitle>
        <NumberOfParts>1</NumberOfParts>
        <{1}>
            <CodeValue>{2}</CodeValue>
            <Namespace>{3}</Namespace>
        </{1}>
        <EducationOrganizationReference>
            <EducationOrganizationIdentity>
                <EducationOrganizationId>{4}</EducationOrganizationId>
            </EducationOrganizationIdentity>
        </EducationOrganizationReference>
    </Course>
</InterchangeEducationOrganization>";

                using (var startup = new OwinStartup(CreateDatabaseNameProvider, CreateOAuthTokenValidator, CreateSecurityRepository))
                {
                    using (var server = TestServer.Create(startup.Configuration))
                    {
                        using (var client = new HttpClient(server.Handler))
                        {
                            client.Timeout = new TimeSpan(0, 0, 15, 0);
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                            var academicSubjectDescriptor = new AcademicSubjectDescriptor
                            {
                                AcademicSubjectType = typeMap,
                                CodeValue = codeValue,
                                Description = description,
                                EffectiveBeginDate = DateTime.UtcNow,
                                EffectiveEndDate = null,
                                Namespace = "http://www.TEST.org/",
                                ShortDescription = description,
                            };

                            var message = JsonConvert.SerializeObject(academicSubjectDescriptor);

                            var result = client.PostAsync(OwinUriHelper.BuildApiUri("2014", "AcademicSubjectDescriptors"), new StringContent(message, Encoding.UTF8, "application/json")).Result;
                            result.EnsureSuccessStatusCode();

                            var courseTemplate = string.Format(XmlTemplate, courseCode, descriptorType, codeValue, "http://www.TEST.org/", educationOrganizationId);
                            var testData = OwinBulkHelper.BuildTestData(courseTemplate, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture));
                            var testXml = testData.Xml;

                            var request = new BulkOperationCreateRequest {UploadFiles = new[] {new UploadFileRequest {Format = "text/xml", InterchangeType = InterchangeType.EducationOrganization.Name, Size = testXml.Length}}};
                            var postResult = client.PostAsync(OwinUriHelper.BuildApiUri("2014", "BulkOperations"), new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json")).Result;
                            postResult.StatusCode.ShouldEqual(HttpStatusCode.Created);

                            var postContent = postResult.Content.ReadAsAsync<BulkOperationResource>().Result;
                            var uploadedFileId = postContent.UploadFiles.First().Id;

                            var url = string.Format("http://owin/api/v2.0/2014/Uploads/{0}/chunk?offset={1}&size={2}", uploadedFileId, 0, testXml.Length);

                            var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(testXml));
                            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                            var content = new MultipartFormDataContent {{fileContent, "File"}};

                            var uploadResult = client.PostAsync(url, content).Result;
                            uploadResult.EnsureSuccessStatusCode();
                            uploadResult.StatusCode.ShouldEqual(HttpStatusCode.Created);

                            url = OwinUriHelper.CreateCommitUri(uploadedFileId);
                            content = new MultipartFormDataContent {new StringContent(JsonConvert.SerializeObject(new {uploadId = uploadedFileId}), Encoding.UTF8, "application/json")};

                            var commitResponse = client.PostAsync(url, content).Result;
                            commitResponse.StatusCode.ShouldEqual(HttpStatusCode.Accepted);

                            var status = OwinBulkHelper.WaitForBulkOperationToComplete(client, postContent.Id);

                            url = string.Format("{0}/{1}/Exceptions/{2}", OwinUriHelper.BuildApiUri("2014", "BulkOperations"), postContent.Id, uploadedFileId);
                            var getExceptions = client.GetAsync(url).Result;
                            getExceptions.EnsureSuccessStatusCode();

                            var exceptions = getExceptions.Content.ReadAsAsync<BulkOperationException[]>().Result;
                            if (exceptions.Length == 0)
                                status.ShouldEqual(BulkOperationStatus.Completed);
                            else
                            {
                                foreach (var exception in exceptions)
                                    Console.WriteLine(exception.Message);
                                Assert.Fail();
                            }

                        }
                    }
                }
            }

        [Test]
        public void When_committing_descriptor_with_vendors_test_namespace_bulk_operation_status_should_be_completed()
        {
            const string descriptor = "AssessmentPeriod";
            const string codeValue = "9876";
            const string xmlTemplate = @"<?xml version='1.0' encoding='utf-8' standalone='yes'?>
<InterchangeDescriptors xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:ann='http://www.ed-fi.org/annotation'>
    <{0}Descriptor>
    <CodeValue>{2}</CodeValue>
    <ShortDescription>JOY</ShortDescription>
    <Description>JOY</Description>
    <EffectiveBeginDate>2012-07-01</EffectiveBeginDate>
    <Namespace>{1}Descriptor/{0}Descriptor.xml</Namespace>
    </{0}Descriptor>
</InterchangeDescriptors>";
            using (var startup = new OwinStartup(CreateDatabaseNameProvider, CreateOAuthTokenValidator, CreateSecurityRepository))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = new TimeSpan(0, 0, 15, 0);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var assessmentPeriodTemplate = string.Format(xmlTemplate, descriptor, "http://www.TEST.org/", codeValue);
                        var testData = OwinBulkHelper.BuildTestData(assessmentPeriodTemplate, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture));
                        var testXml = testData.Xml;

                        var request = new BulkOperationCreateRequest { UploadFiles = new[] { new UploadFileRequest { Format = "text/xml", InterchangeType = InterchangeType.Descriptors.Name, Size = testXml.Length } } };
                        var postResult = client.PostAsync(OwinUriHelper.BuildApiUri("2014", "BulkOperations"), new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json")).Result;
                        postResult.StatusCode.ShouldEqual(HttpStatusCode.Created);

                        var postContent = postResult.Content.ReadAsAsync<BulkOperationResource>().Result;
                        var uploadedFileId = postContent.UploadFiles.First().Id;

                        var url = string.Format("http://owin/api/v2.0/2014/Uploads/{0}/chunk?offset={1}&size={2}", uploadedFileId, 0, testXml.Length);

                        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(testXml));
                        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                        var content = new MultipartFormDataContent { { fileContent, "File" } };

                        var uploadResult = client.PostAsync(url, content).Result;
                        uploadResult.EnsureSuccessStatusCode();
                        uploadResult.StatusCode.ShouldEqual(HttpStatusCode.Created);

                        url = OwinUriHelper.CreateCommitUri(uploadedFileId);
                        content = new MultipartFormDataContent { new StringContent(JsonConvert.SerializeObject(new { uploadId = uploadedFileId }), Encoding.UTF8, "application/json") };

                        var commitResponse = client.PostAsync(url, content).Result;
                        commitResponse.StatusCode.ShouldEqual(HttpStatusCode.Accepted);

                        var status = OwinBulkHelper.WaitForBulkOperationToComplete(client, postContent.Id);

                        url = string.Format("{0}/{1}/Exceptions/{2}", OwinUriHelper.BuildApiUri("2014", "BulkOperations"), postContent.Id, uploadedFileId);
                        var getExceptions = client.GetAsync(url).Result;
                        getExceptions.EnsureSuccessStatusCode();

                        var exceptions = getExceptions.Content.ReadAsAsync<BulkOperationException[]>().Result;
                        if (exceptions.Length == 0)
                            status.ShouldEqual(BulkOperationStatus.Completed);
                        else
                        {
                            foreach (var exception in exceptions)
                                Console.WriteLine(exception.Message);
                            Assert.Fail();
                        }

                    }
                }
            }
        }

        [Test]
        public void When_committing_descriptor_with_edfi_namespace_bulk_operation_status_should_be_completed()
        {
            const string descriptor = "AssessmentPeriod";
            const string codeValue = "9876";
            const string xmlTemplate = @"<?xml version='1.0' encoding='utf-8' standalone='yes'?>
<InterchangeDescriptors xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:ann='http://www.ed-fi.org/annotation'>
<{0}Descriptor>
<CodeValue>{2}</CodeValue>
<ShortDescription>JOY</ShortDescription>
<Description>JOY</Description>
<EffectiveBeginDate>2012-07-01</EffectiveBeginDate>
<Namespace>{1}Descriptor/{0}Descriptor.xml</Namespace>
</{0}Descriptor>
</InterchangeDescriptors>";
            using (var startup = new OwinStartup(CreateDatabaseNameProvider, CreateOAuthTokenValidator, CreateSecurityRepository))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = new TimeSpan(0, 0, 15, 0);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var assessmentPeriodTemplate = string.Format(xmlTemplate, descriptor, "http://www.ed-fi.org/", codeValue);
                        var testData = OwinBulkHelper.BuildTestData(assessmentPeriodTemplate, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture));
                        var testXml = testData.Xml;

                        var request = new BulkOperationCreateRequest {UploadFiles = new[] {new UploadFileRequest {Format = "text/xml", InterchangeType = InterchangeType.Descriptors.Name, Size = testXml.Length}}};
                        var postResult = client.PostAsync(OwinUriHelper.BuildApiUri("2014", "BulkOperations"), new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json")).Result;
                        postResult.StatusCode.ShouldEqual(HttpStatusCode.Created);

                        var postContent = postResult.Content.ReadAsAsync<BulkOperationResource>().Result;
                        var uploadedFileId = postContent.UploadFiles.First().Id;

                        var url = string.Format("http://owin/api/v2.0/2014/Uploads/{0}/chunk?offset={1}&size={2}", uploadedFileId, 0, testXml.Length);

                        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(testXml));
                        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                        var content = new MultipartFormDataContent {{fileContent, "File"}};

                        var uploadResult = client.PostAsync(url, content).Result;
                        uploadResult.EnsureSuccessStatusCode();
                        uploadResult.StatusCode.ShouldEqual(HttpStatusCode.Created);

                        url = OwinUriHelper.CreateCommitUri(uploadedFileId);
                        content = new MultipartFormDataContent {new StringContent(JsonConvert.SerializeObject(new {uploadId = uploadedFileId}), Encoding.UTF8, "application/json")};

                        var commitResponse = client.PostAsync(url, content).Result;
                        commitResponse.StatusCode.ShouldEqual(HttpStatusCode.Accepted);

                        var status = OwinBulkHelper.WaitForBulkOperationToComplete(client, postContent.Id);

                        status.ShouldEqual(BulkOperationStatus.Error);

                        url = string.Format("{0}/{1}/Exceptions/{2}", OwinUriHelper.BuildApiUri("2014", "BulkOperations"), postContent.Id, uploadedFileId);
                        var getExceptions = client.GetAsync(url).Result;
                        getExceptions.EnsureSuccessStatusCode();

                        var exceptions = getExceptions.Content.ReadAsAsync<BulkOperationException[]>().Result;
                        exceptions.Length.ShouldEqual(1);
                        var message = exceptions[0].Message;
                        message.ShouldContain("Access to the resource item with namespace");
                        message.ShouldContain("could not be authorized based on the caller's NamespacePrefix claim of");
                    }
                }
            }
        }
    }
}