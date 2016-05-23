using EdFi.Ods.WebService.Tests._Helpers;
using EdFi.Ods.WebService.Tests.Extensions;
using EdFi.Ods.WebService.Tests.Owin;
using Microsoft.Owin.Testing;
using Newtonsoft.Json;
using NUnit.Framework;
using Should;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;

namespace EdFi.Ods.WebService.Tests.ResponseTests.Delete
{
    [TestFixture]
    public class when_deleting_a_resource_with_mismatched_etag : OwinTestBase
    {
        private static readonly string _databaseName = string.Format("EdFi_Tests_when_deleting_a_resource_with_mismatched_etag_{0}", Guid.NewGuid().ToString("N"));
        private readonly List<int> LocalEducationAgencyIds = new List<int> { 255901 };

        protected override string DatabaseName { get { return _databaseName; } }
        protected override string BaseDatabase { get { return "EdFi_Ods_Populated_Template"; } }

        [Test]
        public void api_should_fail_with_412_code()
        {
            using (var startup = new OwinStartup(DatabaseName, LocalEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = new TimeSpan(0, 0, 15, 0);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var createStudentResponse = StudentHelper.CreateStudentAndAssociateToSchool(client, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), 255901001);
                        createStudentResponse.ResponseMessage.EnsureSuccessStatusCode();

                        client.DefaultRequestHeaders.TryAddWithoutValidation("If-Match", "InvalidETAG");

                        var deleteResponse = client.DeleteAsync(createStudentResponse.ResponseMessage.Headers.Location.AbsoluteUri).Result;
                        deleteResponse.IsSuccessStatusCode.ShouldBeFalse();
                        deleteResponse.StatusCode.ShouldEqual(HttpStatusCode.PreconditionFailed);

                        var result = deleteResponse.Content.ReadAsStringAsync().Result;
                        var resource = JsonConvert.DeserializeObject<HttpError>(result);
                        resource.Message.ShouldContain("Resource was modified");
                    }
                }
            }
        }
    }
}
