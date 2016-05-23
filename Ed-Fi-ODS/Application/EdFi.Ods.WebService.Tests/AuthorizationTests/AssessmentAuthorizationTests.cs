using EdFi.Ods.Api.Models.Resources.Assessment;
using EdFi.Ods.WebService.Tests._Helpers;
using EdFi.Ods.WebService.Tests.Extensions;
using EdFi.Ods.WebService.Tests.Owin;
using Microsoft.Owin.Testing;
using Newtonsoft.Json;
using NUnit.Framework;
using Should;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace EdFi.Ods.WebService.Tests.AuthorizationTests
{
    [TestFixture]
    public class AssessmentAuthorizationTests : OwinTestBase
    {
        private static readonly string _databaseName = string.Format("EdFi_Tests_AssessmentMetadataAuthorizationTests{0}", Guid.NewGuid().ToString("N"));
        private readonly List<int> LocalEducationAgencyIds = new List<int> { 255901 };

        protected override string DatabaseName { get { return _databaseName; } }
        protected override string BaseDatabase { get { return "EdFi_Ods_Populated_Template"; } }

        private static readonly string AssessmentResourceUri = OwinUriHelper.BuildApiUri("2014", "assessments");
        private const string TestNamespace = "http://www.TEST.org/";
        
        [Test]
        public void Creating_An_Assessment_With_An_Invalid_Namespace_Should_Fail()
        {
            using (var startup = new OwinStartup(DatabaseName, LocalEducationAgencyIds, TestNamespace))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = new TimeSpan(0, 0, 15, 0);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var assessment = ResourceHelper.CreateAssessment(string.Format("{0}_AssessmentTitle", DateTime.Now.Ticks), null, "http://www.FAIL.org/");

                        var assessmentInsertFailureResult = client.PostAsync(AssessmentResourceUri, new StringContent(assessment, Encoding.UTF8, "application/json")).Result;

                        Assert.IsNotNull(assessmentInsertFailureResult);
                        Assert.IsFalse(assessmentInsertFailureResult.IsSuccessStatusCode);
                    }
                }
            }
        }

        [Test]
        public void Creating_an_Assessment_With_An_Invalid_AssessmentFamily_Namespace_Should_Fail()
        {
            using (var startup = new OwinStartup(DatabaseName, LocalEducationAgencyIds, TestNamespace))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = new TimeSpan(0, 0, 15, 0);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var assessment = ResourceHelper.CreateAssessment(string.Format("{0}_AssessmentTitle", DateTime.Now.Ticks), "FAIL_AssessmentFamily", null);

                        var assessmentInsertByAssessmentFamilyFailureResult = client.PostAsync(AssessmentResourceUri, new StringContent(assessment, Encoding.UTF8, "application/json")).Result;

                        Assert.IsNotNull(assessmentInsertByAssessmentFamilyFailureResult);
                        Assert.IsFalse(assessmentInsertByAssessmentFamilyFailureResult.IsSuccessStatusCode);
                    }
                }
            }
        }

        [Test]
        public void Creating_An_Assessment_With_A_Valid_Namespace_Should_Succeed()
        {
            using (var startup = new OwinStartup(DatabaseName, LocalEducationAgencyIds, TestNamespace))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = new TimeSpan(0, 0, 15, 0);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var assessment = ResourceHelper.CreateAssessment(string.Format("{0}_AssessmentTitle", DateTime.Now.Ticks), null, TestNamespace);

                        var assessmentInsertSuccessResult = client.PostAsync(AssessmentResourceUri, new StringContent(assessment, Encoding.UTF8, "application/json")).Result;

                        Assert.IsNotNull(assessmentInsertSuccessResult);
                        assessmentInsertSuccessResult.EnsureSuccessStatusCode();
                        assessmentInsertSuccessResult.StatusCode.ShouldEqual(HttpStatusCode.Created);
                    }
                }
            }
        }

        [Test]
        public void Creating_An_Assessment_With_A_Valid_AssessmentFamilty_Namespace_Should_Succeed()
        {
            using (var startup = new OwinStartup(DatabaseName, LocalEducationAgencyIds, TestNamespace))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = new TimeSpan(0, 0, 15, 0);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var assementFamilyName = string.Format("{0}_TEST_AssessmentFamily", DateTime.Now.Ticks);
                        var assessmentFamily = ResourceHelper.CreateAssessmentFamily(assementFamilyName, TestNamespace);
                        var assessmentFamilyResult = client.PostAsync(OwinUriHelper.BuildApiUri("2014", "AssessmentFamilies"), new StringContent(assessmentFamily, Encoding.UTF8, "application/json")).Result;

                        assessmentFamilyResult.EnsureSuccessStatusCode();

                        var assessment = ResourceHelper.CreateAssessment(string.Format("{0}_AssessmentTitle", DateTime.Now.Ticks), assementFamilyName, null);

                        var assessmentInsertByAssessmentFamilySuccessResult = client.PostAsync(AssessmentResourceUri, new StringContent(assessment, Encoding.UTF8, "application/json")).Result;

                        Assert.IsNotNull(assessmentInsertByAssessmentFamilySuccessResult);
                        assessmentInsertByAssessmentFamilySuccessResult.EnsureSuccessStatusCode();
                        assessmentInsertByAssessmentFamilySuccessResult.StatusCode.ShouldEqual(HttpStatusCode.Created);
                    }
                }
            }
        }

        [Test]
        public void GetAll_Assessments_ShouldReturn_OnlyVendorSpecific_Assessments()
        {
            using (var startup = new OwinStartup(DatabaseName, LocalEducationAgencyIds, TestNamespace))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = new TimeSpan(0, 0, 15, 0);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var assementFamilyName = string.Format("{0}_TEST_AssessmentFamily", DateTime.Now.Ticks);
                        var assessmentFamily = ResourceHelper.CreateAssessmentFamily(assementFamilyName, TestNamespace);
                        var assessmentFamilyResult = client.PostAsync(OwinUriHelper.BuildApiUri("2014", "AssessmentFamilies"), new StringContent(assessmentFamily, Encoding.UTF8, "application/json")).Result;

                        assessmentFamilyResult.EnsureSuccessStatusCode();

                        var assessment = ResourceHelper.CreateAssessment(string.Format("{0}_AssessmentTitle", DateTime.Now.Ticks), assementFamilyName, null);

                        var assessmentInsertByAssessmentFamilySuccessResult = client.PostAsync(AssessmentResourceUri, new StringContent(assessment, Encoding.UTF8, "application/json")).Result;

                        Assert.IsNotNull(assessmentInsertByAssessmentFamilySuccessResult);
                        assessmentInsertByAssessmentFamilySuccessResult.EnsureSuccessStatusCode();
                        assessmentInsertByAssessmentFamilySuccessResult.StatusCode.ShouldEqual(HttpStatusCode.Created);

                        var assessmentGetAllResult = client.GetStringAsync(AssessmentResourceUri).Result;
                        var results = (Object[]) JsonConvert.DeserializeObject(assessmentGetAllResult, typeof (Assessment[]));
                        results.Count().ShouldBeGreaterThanOrEqualTo(1);
                    }
                }
            }
        }    
    }
}
