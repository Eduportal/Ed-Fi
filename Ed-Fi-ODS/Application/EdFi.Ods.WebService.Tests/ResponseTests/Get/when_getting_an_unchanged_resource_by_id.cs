using EdFi.Ods.WebService.Tests._Helpers;
using EdFi.Ods.WebService.Tests.Extensions;
using EdFi.Ods.WebService.Tests.Owin;
using Microsoft.Owin.Testing;
using NUnit.Framework;
using Should;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace EdFi.Ods.WebService.Tests.ResponseTests.Get
{
    [TestFixture]
    class when_getting_an_unchanged_resource_by_id : OwinTestBase
    {
        private static readonly string _databaseName = string.Format("EdFi_Tests_when_getting_an_unchanged_resource_by_id_{0}", Guid.NewGuid().ToString("N"));
        private readonly List<int> LocalEducationAgencyIds = new List<int> { 255901 };

        protected override string DatabaseName { get { return _databaseName; } }
        protected override string BaseDatabase { get { return "EdFi_Ods_Populated_Template"; } }

        [Test]
        public void api_should_succeed_with_304_code()
        {
            using (var startup = new OwinStartup(DatabaseName, LocalEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = new TimeSpan(0, 0, 15, 0);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var studentCreationResponse = StudentHelper.CreateStudentAndAssociateToSchool(client, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), 255901001);
                        var studentId = studentCreationResponse.ResponseMessage.Headers.Location.AbsoluteUri.Split('/').Last();

                        var response = client.GetAsync(OwinUriHelper.BuildApiUri("2014", "students/" + studentId, string.Empty)).Result;

                        client.DefaultRequestHeaders.TryAddWithoutValidation("If-None-Match", response.Headers.ETag.Tag.Replace("\"", string.Empty));

                        response = client.GetAsync(OwinUriHelper.BuildApiUri("2014", "students/" + studentId, string.Empty)).Result;
                        response.StatusCode.ShouldEqual(HttpStatusCode.NotModified);
                    }
                }
            }
        }
    }
}
