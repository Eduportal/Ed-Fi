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

namespace EdFi.Ods.WebService.Tests.ResponseTests.Delete
{
    [TestFixture]
    public class when_deleting_a_resource_with_a_malformed_id : OwinTestBase
    {
        private static readonly string _databaseName = string.Format("EdFi_Tests_when_deleting_a_resource_with_a_malformed_id_{0}", Guid.NewGuid().ToString("N"));
        private readonly List<int> LocalEducationAgencyIds = new List<int> { 255901 };

        protected override string DatabaseName { get { return _databaseName; } }
        protected override string BaseDatabase { get { return "EdFi_Ods_Populated_Template"; } }

        [Test]
        public void api_should_fail_with_400_code()
        {
            using (var startup = new OwinStartup(DatabaseName, LocalEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = new TimeSpan(0, 0, 15, 0);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var builder = new StringBuilder(Guid.NewGuid().ToString());
                        builder[3] = '-';

                        var deleteResponse = client.DeleteAsync(OwinUriHelper.BuildApiUri("2014", "Programs/" + builder)).Result;
                        deleteResponse.IsSuccessStatusCode.ShouldBeFalse();
                        deleteResponse.StatusCode.ShouldEqual(HttpStatusCode.BadRequest);
                    }
                }
            }
        }
    }
}
