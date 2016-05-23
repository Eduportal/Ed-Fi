using EdFi.Ods.WebService.Tests._Helpers;
using EdFi.Ods.WebService.Tests.Extensions;
using EdFi.Ods.WebService.Tests.Owin;
using Microsoft.Owin.Testing;
using NUnit.Framework;
using Should;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace EdFi.Ods.WebService.Tests.ResponseTests.Delete
{
    [TestFixture]
    public class when_deleting_a_resource : OwinTestBase
    {
        private static readonly string _databaseName = string.Format("EdFi_Tests_when_deleting_a_resource_{0}", Guid.NewGuid().ToString("N"));
        private readonly List<int> LocalEducationAgencyIds = new List<int> { 255901 };

        protected override string DatabaseName { get { return _databaseName; } }
        protected override string BaseDatabase { get { return "EdFi_Ods_Populated_Template"; } }

        [Test]
        public void api_should_succeed_with_204_code()
        {
            using (var startup = new OwinStartup(DatabaseName, LocalEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = new TimeSpan(0, 0, 15, 0);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var createStudentResponse = StudentHelper.CreateStudent(client, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture));
                        createStudentResponse.ResponseMessage.EnsureSuccessStatusCode();

                        var deleteResponse = client.DeleteAsync(createStudentResponse.ResponseMessage.Headers.Location.AbsoluteUri).Result;
                        deleteResponse.IsSuccessStatusCode.ShouldBeTrue();
                        deleteResponse.StatusCode.ShouldEqual(HttpStatusCode.NoContent);
                    }
                }
            }
        }
    }
}