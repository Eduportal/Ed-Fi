using EdFi.Identity.Models;
using EdFi.Ods.WebService.Tests.Owin;
using Microsoft.Owin.Testing;
using Newtonsoft.Json;
using NUnit.Framework;
using Should;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace EdFi.Ods.WebService.Tests.UniqueIdentifierWebApi
{
    [TestFixture]
    public class UnimplementedUniqueIdentityTests : OwinTestBase, IDisposable
    {
        private OwinStartup _startup;
        private TestServer _server;
        private static readonly string _databaseName = string.Format("EdFi_Tests_UnimplementedUniqueIdentityTests_{0}", Guid.NewGuid().ToString("N"));
        private readonly List<int> _localEducationAgencyIds = new List<int> { 255901 };

        protected override bool CreateDatabase { get { return false; } }
        protected override string DatabaseName { get { return _databaseName; } }
        protected override string BaseDatabase { get { return "EdFi_Ods_Populated_Template"; } }

        [TestFixtureSetUp]
        public void CreateTestServer()
        {
            _startup = new OwinStartup(DatabaseName, _localEducationAgencyIds, useUniqueIdIntegration: true);
            _server = TestServer.Create(_startup.Configuration);
        }

        private HttpClient GetHttpClient()
        {
            var client = new HttpClient(_server.Handler)
            {
                Timeout = new TimeSpan(0, 0, 15, 0)
            };
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());
            return client;
        }

        [Test]
        public void When_getting_by_id_should_return_not_implemented()
        {
            using (var client = GetHttpClient())
            {
                var response =
                    client.GetAsync(
                    OwinUriHelper.BuildApiUri(null, "identities", string.Format("id={0}", Guid.NewGuid().ToString("N")))
                    ).Result;
                response.StatusCode.ShouldEqual(HttpStatusCode.NotImplemented);
            }
        }

        [Test]
        public void When_getting_by_example_should_return_not_implemented()
        {
            using (var client = GetHttpClient())
            {
                var response =
                    client.GetAsync(
                    OwinUriHelper.BuildApiUri(null, "identities", string.Format("familyNames={0}&givenNames={1}", "Smith", "John"))
                    ).Result;
                response.StatusCode.ShouldEqual(HttpStatusCode.NotImplemented);
            }
        }

        [Test]
        public void When_posting_invalid_Identity_should_return_bad_request()
        {
            using (var client = GetHttpClient())
            {
                var response =
                   client.PostAsync(OwinUriHelper.BuildApiUri(null, "identities"),
                       new StringContent(JsonConvert.SerializeObject(new IdentityResource()), Encoding.UTF8,
                           "application/json")).Result;
                response.StatusCode.ShouldEqual(HttpStatusCode.BadRequest);
            }
        }

        [Test]
        public void When_posting_valid_identity_should_return_not_implemented()
        {
            using (var client = GetHttpClient())
            {
                var response =
                   client.PostAsync(OwinUriHelper.BuildApiUri(null, "identities"),
                       new StringContent(JsonConvert.SerializeObject(
                           new IdentityResource()
                           {
                               GivenNames = "Joe",
                               FamilyNames = "Student",
                               BirthDate = new DateTime(2000, 1, 1),
                               BirthGender = "male"
                           }
                           ), Encoding.UTF8,
                           "application/json")).Result;
                response.StatusCode.ShouldEqual(HttpStatusCode.NotImplemented);
            }
        }

        public void Dispose()
        {
            (_server as IDisposable).Dispose();
            (_startup as IDisposable).Dispose();
        }
    }
}