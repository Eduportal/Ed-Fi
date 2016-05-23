using EdFi.Common.Database;
using EdFi.Ods.Api.Common.Authorization;
using EdFi.Ods.Api.Data.Repositories.BulkOperations;
using EdFi.Ods.Api.Models.Resources;
using EdFi.Ods.Api.Models.Resources.Enums;
using EdFi.Ods.Common;
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
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace EdFi.Ods.WebService.Tests.XmlShredding
{
    [TestFixture]
    public class XmlBulkProcessingTests : OwinBulkTestBase
    {
        private static readonly string _databaseName = string.Format("EdFi_Tests_XmlBulkProcessingTests_{0}", Guid.NewGuid().ToString("N"));
        private readonly List<int> LocalEducationAgencyIds = new List<int> {255901};

        protected override string DatabaseName { get { return _databaseName; } }
        protected override string BaseDatabase { get { return "EdFi_Ods_Populated_Template"; } }
        protected override Func<IDatabaseNameProvider> CreateDatabaseNameProvider { get { return CreateDatabaseNameProviderFunc; } }
        protected override Func<IOAuthTokenValidator> CreateOAuthTokenValidator { get { return CreateOAuthTokenValidatorFunc; } }
        protected override Func<OwinSecurityRepository> CreateSecurityRepository { get { return () => new OwinSecurityRepository(); } }

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
                EducationOrganizationIds = LocalEducationAgencyIds,
            });
            return oAuthTokenValidator;
        }

        [Test]
        public void When_submitting_bulk_operations_with_invalid_datatype_xml_element_bulk_operation_status_should_be_error()
        {
            const string XmlTemplate = @"<?xml version='1.0' encoding='UTF-8'?>
            <InterchangeEducationOrganization xmlns:ann='http://ed-fi.org/annotation' xsi:schemaLocation='http://ed-fi.org/Interchange-EducationOrganization.xsd' xmlns='http://ed-fi.org/0200-Draft' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>
                <Course>
                    <CourseCode>99999</CourseCode>
                    <CourseTitle>##SomeUniqueInformation##</CourseTitle>
                    <NumberOfParts>1</NumberOfParts>
                    <DateCourseAdopted>**INVALIDDATE**</DateCourseAdopted>
                    <AcademicSubject>
                        <CodeValue>01</CodeValue>
                    </AcademicSubject>
                    <EducationOrganizationReference>
                        <EducationOrganizationIdentity>
                            <EducationOrganizationId>10</EducationOrganizationId>
                        </EducationOrganizationIdentity>
                    </EducationOrganizationReference>
                </Course>
            </InterchangeEducationOrganization>";

            using (var startup = new OwinStartup(CreateDatabaseNameProvider, CreateOAuthTokenValidator))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = new TimeSpan(0, 0, 15, 0);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var testData = OwinBulkHelper.BuildTestData(XmlTemplate, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture));
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
                        status.ShouldEqual(BulkOperationStatus.Error);

                        url = string.Format("{0}/{1}/Exceptions/{2}", OwinUriHelper.BuildApiUri("2014", "BulkOperations"), postContent.Id, uploadedFileId);
                        var getExceptions = client.GetAsync(url).Result;
                        getExceptions.EnsureSuccessStatusCode();

                        var exceptions = getExceptions.Content.ReadAsAsync<BulkOperationException[]>().Result;
                        exceptions.Length.ShouldEqual(1);
                        exceptions[0].Message.ShouldContain("String was not recognized as a valid DateTime.");

                    }
                }
            }
        }

        [Test]
        public void When_submitting_bulk_operations_data_containing_broken_references_bulk_operation_status_should_be_error()
        {
            const string XmlTemplate = @"<?xml version='1.0' encoding='UTF-8'?>
                <InterchangeStandards xmlns='http://ed-fi.org/0200' xmlns:ann='http://ed-fi.org/annotation' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xsi:schemaLocation='http://ed-fi.org/Interchange-Standards.xsd'>
                    <LearningStandard>
                        <LearningStandardId>111.32.NA.A.1.A</LearningStandardId>
                        <Description>describe independent and dependent quantities in functional relationships;</Description>
                        <ContentStandard>
                            <Title>State Standard</Title>
                        </ContentStandard>
                        <GradeLevel>
                            <CodeValue>Ninth grade</CodeValue>
                            <Namespace>http://www.ed-fi.org/Descriptor/GradeLevelDescriptor.xml</Namespace>
                        </GradeLevel>
                        <AcademicSubject>
                            <CodeValue>**BADCODE**</CodeValue>
                            <Namespace>http://www.ed-fi.org/Descriptor/AcademicSubjectDescriptor.xml</Namespace>
                        </AcademicSubject>
                        <CourseTitle>Algebra I</CourseTitle>
                        <Namespace>111.32.NA.A.1.A</Namespace>
                    </LearningStandard>
                </InterchangeStandards>";

            Func<IOAuthTokenValidator> createOAuthTokenValidatorWithNeededNamespace = () =>
            {
                var oAuthTokenValidator = MockRepository.GenerateStub<IOAuthTokenValidator>();
                oAuthTokenValidator.Stub(t => t.GetClientDetailsForToken(Arg<Guid>.Is.Anything))
                                   .Return(
                                       new ApiClientDetails
                                       {
                                           ApiKey = DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture),
                                           ApplicationId = DateTime.Now.Millisecond,
                                           ClaimSetName = "SIS Vendor",
                                           NamespacePrefix = "111.32", // Namespace prefix claim must be consistent with targeted sample data's namespace
                                           EducationOrganizationIds = LocalEducationAgencyIds,
                                       });
                return oAuthTokenValidator;
            };

            using (var startup = new OwinStartup(CreateDatabaseNameProvider, createOAuthTokenValidatorWithNeededNamespace))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = new TimeSpan(0, 0, 15, 0);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var testData = OwinBulkHelper.BuildTestData(XmlTemplate, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture));
                        var testXml = testData.Xml;

                        var request = new BulkOperationCreateRequest {UploadFiles = new[] {new UploadFileRequest {Format = "text/xml", InterchangeType = InterchangeType.Standards.Name, Size = testXml.Length}}};
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
                        Assert.That(exceptions[0].Message, Is.StringContaining("Unable to resolve value 'http://www.ed-fi.org/Descriptor/AcademicSubjectDescriptor.xml/**BADCODE**' to an existing 'AcademicSubjectDescriptor' resource."));
                    }
                }
            }
        }

        [Test]
        public void When_committing_student_with_duplicate_email_types_should_not_have_internal_error_bulk_operation_status_should_be_error()
        {
            const string XmlTemplate = @"<?xml version='1.0' encoding='UTF-8'?>
<InterchangeStudent xmlns:ann='http://ed-fi.org/annotation' xsi:schemaLocation='http://ed-fi.org/Interchange-Student.xsd' xmlns='http://ed-fi.org/0200' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>
    <Student>
        <StudentUniqueId>{0}</StudentUniqueId>
        <Name>
            <FirstName>Dallas</FirstName>
            <LastSurname>Adams</LastSurname>
        </Name>
        <Sex>Male</Sex>
        <BirthData>
            <BirthDate>2006-05-28</BirthDate>
        </BirthData>
        <ElectronicMail>
            <ElectronicMailAddress>dadams@hotmail.com</ElectronicMailAddress>
            <ElectronicMailType>Other</ElectronicMailType>
            <PrimaryEmailAddressIndicator>true</PrimaryEmailAddressIndicator>
        </ElectronicMail>
            <ElectronicMail>
            <ElectronicMailAddress>dadams@topica.com</ElectronicMailAddress>
            <ElectronicMailType>Other</ElectronicMailType>
        <PrimaryEmailAddressIndicator>false</PrimaryEmailAddressIndicator>
        </ElectronicMail>
        <HispanicLatinoEthnicity>false</HispanicLatinoEthnicity>
        <FirstSchoolEnrollmentDate>2011-08-24</FirstSchoolEnrollmentDate>
    </Student>
</InterchangeStudent>";

            using (var startup = new OwinStartup(CreateDatabaseNameProvider, CreateOAuthTokenValidator))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = new TimeSpan(0, 0, 15, 0);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var identitiesResponse = client.PostAsync(OwinUriHelper.BuildApiUri(null, "identities"), new StringContent(JsonConvert.SerializeObject(UniqueIdCreator.InitializeAPersonWithUniqueData()), Encoding.UTF8, "application/json")).Result;
                        var studentUniqueId = UniqueIdCreator.ExtractIdFromHttpResponse(identitiesResponse);

                        var xml = string.Format(XmlTemplate, studentUniqueId);
                        var testData = OwinBulkHelper.BuildTestData(xml, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture));
                        var testXml = testData.Xml;

                        var request = new BulkOperationCreateRequest {UploadFiles = new[] {new UploadFileRequest {Format = "text/xml", InterchangeType = InterchangeType.Student.Name, Size = testXml.Length}}};
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
                        Assert.IsTrue(System.Text.RegularExpressions.Regex.IsMatch(exceptions[0].Message, @"^A duplicate ElectronicMailTypeId conflict occurred when attempting to create a new Student resource with StudentUSI of \d*.$"));
                    }
                }
            }
        }

        [Test]
        public void When_committing_student_with_duplicate_phone_types_should_not_have_internal_error_bulk_operation_status_should_be_error()
        {
            const string XmlTemplate = @"<?xml version='1.0' encoding='UTF-8'?>
<InterchangeStudentParent xmlns='http://ed-fi.org/0200-Draft' xmlns:ann='http://ed-fi.org/annotation' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>
    <Student>
        <StudentUniqueId>{0}</StudentUniqueId>
        <Name>
            <FirstName>Dallas</FirstName>
            <LastSurname>Adams</LastSurname>
        </Name>
        <Sex>Male</Sex>
        <BirthData>
            <BirthDate>2006-05-28</BirthDate>
        </BirthData>
        <Telephone>
            <TelephoneNumber>(781) 749-8142</TelephoneNumber>
            <TelephoneNumberType>Home</TelephoneNumberType>
        </Telephone>
        <Telephone>
            <TelephoneNumber>(512) 749-8142</TelephoneNumber>
            <TelephoneNumberType>Home</TelephoneNumberType>
        </Telephone>
        <HispanicLatinoEthnicity>false</HispanicLatinoEthnicity>
        <FirstSchoolEnrollmentDate>2011-08-24</FirstSchoolEnrollmentDate>
    </Student>
</InterchangeStudentParent>";

            using (var startup = new OwinStartup(CreateDatabaseNameProvider, CreateOAuthTokenValidator))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = new TimeSpan(0, 0, 15, 0);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var identitiesResponse = client.PostAsync(OwinUriHelper.BuildApiUri(null, "identities"), new StringContent(JsonConvert.SerializeObject(UniqueIdCreator.InitializeAPersonWithUniqueData()), Encoding.UTF8, "application/json")).Result;
                        var studentUniqueId = UniqueIdCreator.ExtractIdFromHttpResponse(identitiesResponse);

                        var xml = string.Format(XmlTemplate, studentUniqueId);
                        var testData = OwinBulkHelper.BuildTestData(xml, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture));
                        var testXml = testData.Xml;

                        var request = new BulkOperationCreateRequest {UploadFiles = new[] {new UploadFileRequest {Format = "text/xml", InterchangeType = InterchangeType.Student.Name, Size = testXml.Length}}};
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
                        Assert.IsTrue(System.Text.RegularExpressions.Regex.IsMatch(exceptions[0].Message, @"^A duplicate TelephoneNumberTypeId conflict occurred when attempting to create a new Student resource with StudentUSI of \d*.$"));
                    }
                }
            }
        }
    }
}