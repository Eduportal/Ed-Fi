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

namespace EdFi.Ods.WebService.Tests.ResponseTests.Get
{
    [TestFixture]
    public class GetAll_400_Tests : OwinTestBase
    {
        private static readonly string _databaseName = string.Format("GetAll_400_Tests_{0}", Guid.NewGuid().ToString("N"));
        private readonly List<int> LocalEducationAgencyIds = new List<int> { 255901 };

        protected override string DatabaseName { get { return _databaseName; } }
        protected override string BaseDatabase { get { return "EdFi_Ods_Populated_Template"; } }

        [Test]
        public void When_Given_a_limit_Negative1_or_less_Should_return_400()
        {
            using (var startup = new OwinStartup(DatabaseName, LocalEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = new TimeSpan(0, 0, 15, 0);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var response = client.GetAsync(OwinUriHelper.BuildApiUri("2014", "courses", "limit=-1")).Result;
                        response.StatusCode.ShouldEqual(HttpStatusCode.BadRequest);
                    }
                }
            }
        }

        [Test]
        public void When_Given_a_limit_0_Should_return_400()
        {
            using (var startup = new OwinStartup(DatabaseName, LocalEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = new TimeSpan(0, 0, 15, 0);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var response = client.GetAsync(OwinUriHelper.BuildApiUri("2014", "courses", "limit=0")).Result;
                        response.StatusCode.ShouldEqual(HttpStatusCode.BadRequest);
                    }
                }
            }
        }

        [Test]
        public void When_Given_a_limit_101_or_more_Should_return_400()
        {
            using (var startup = new OwinStartup(DatabaseName, LocalEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = new TimeSpan(0, 0, 15, 0);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var response = client.GetAsync(OwinUriHelper.BuildApiUri("2014", "courses", "limit=101")).Result;
                        response.StatusCode.ShouldEqual(HttpStatusCode.BadRequest);
                    }
                }
            }
        }
    }
}