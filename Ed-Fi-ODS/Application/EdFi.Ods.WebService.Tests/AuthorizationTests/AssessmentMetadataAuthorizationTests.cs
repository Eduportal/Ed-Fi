using EdFi.Ods.WebService.Tests._Helpers;
using EdFi.Ods.WebService.Tests.Extensions;
using EdFi.Ods.WebService.Tests.Owin;
using Microsoft.Owin.Testing;
using NUnit.Framework;
using Should;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace EdFi.Ods.WebService.Tests.AuthorizationTests
{
    [TestFixture]
    public class AssessmentMetadataAuthorizationTests : OwinTestBase
    {
        private static readonly string _databaseName = string.Format("EdFi_Tests_AssessmentMetadataAuthorizationTests{0}", Guid.NewGuid().ToString("N"));
        private readonly List<int> LocalEducationAgencyIds = new List<int> { 255901 };

        protected override string DatabaseName { get { return _databaseName; } }
        protected override string BaseDatabase { get { return "EdFi_Ods_Populated_Template"; } }

        private static readonly string AssessmentFamilyResourceUri = OwinUriHelper.BuildApiUri("2014", "AssessmentFamilies");
        private const string TestNamespace = "http://www.TEST.org/";
        
        [Test]
        public void When_creating_an_AssessmentFamily_with_an_invalid_namespace_should_fail()
        {
            using (var startup = new OwinStartup(DatabaseName, LocalEducationAgencyIds, TestNamespace))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = new TimeSpan(0, 0, 15, 0);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var assessmentFamily = ResourceHelper.CreateAssessmentFamily(string.Format("{0}_TEST_AssessmentFamily", DateTime.Now.Ticks), "http://www.FAIL.org/");

                        var result = client.PostAsync(AssessmentFamilyResourceUri, new StringContent(assessmentFamily, Encoding.UTF8, "application/json")).Result;

                        Assert.IsNotNull(result);
                        Assert.IsFalse(result.IsSuccessStatusCode);
                    }
                }
            }
        }

        [Test]
        public void When_creating_an_AssessmentFamily_with_a_valid_namespace_should_succeed()
        {
            using (var startup = new OwinStartup(DatabaseName, LocalEducationAgencyIds, TestNamespace))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = new TimeSpan(0, 0, 15, 0);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var assessmentFamily = ResourceHelper.CreateAssessmentFamily(string.Format("{0}_TEST_AssessmentFamily", DateTime.Now.Ticks), TestNamespace);
                        var result = client.PostAsync(AssessmentFamilyResourceUri, new StringContent(assessmentFamily, Encoding.UTF8, "application/json")).Result;

                        Assert.IsNotNull(result);
                        result.EnsureSuccessStatusCode();
                        result.StatusCode.ShouldEqual(HttpStatusCode.Created);
                    }
                }
            }
        }
    }
}
