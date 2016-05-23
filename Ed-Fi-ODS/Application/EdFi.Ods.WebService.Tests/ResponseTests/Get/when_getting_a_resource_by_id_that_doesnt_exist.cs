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
    public class when_getting_a_resource_by_id_that_doesnt_exist : OwinTestBase
    {
        private static readonly string _databaseName = string.Format("EdFi_Tests_when_getting_a_resource_by_id_that_doesnt_exist_{0}", Guid.NewGuid().ToString("N"));
        private readonly List<int> LocalEducationAgencyIds = new List<int> { 255901 };

        protected override string DatabaseName { get { return _databaseName; } }
        protected override string BaseDatabase { get { return "EdFi_Ods_Populated_Template"; } }
        
        [Test]
        public void api_should_fail_with_404_code()
        {
            using (var startup = new OwinStartup(DatabaseName, LocalEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = new TimeSpan(0, 0, 15, 0);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var response = client.GetAsync(OwinUriHelper.BuildApiUri("2014", "students/" + Guid.NewGuid().ToString(), string.Empty)).Result;
                        response.IsSuccessStatusCode.ShouldBeFalse();
                        response.StatusCode.ShouldEqual(HttpStatusCode.NotFound);
                    }
                }
            }
        }
    }
}
