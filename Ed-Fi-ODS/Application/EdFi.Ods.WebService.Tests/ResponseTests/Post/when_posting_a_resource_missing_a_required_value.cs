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

namespace EdFi.Ods.WebService.Tests.ResponseTests.Post
{
    [TestFixture]
    class when_posting_a_resource_missing_a_required_value : OwinTestBase
    {
        private static readonly string _databaseName = string.Format("EdFi_Tests_when_posting_a_resource_missing_a_required_value_{0}", Guid.NewGuid().ToString("N"));
        private readonly List<int> EducationOrganizationIds = new List<int> { 255901 };

        protected override string DatabaseName { get { return _databaseName; } }
        protected override string BaseDatabase { get { return "EdFi_Ods_Populated_Template"; } }
        
        [Test]
        public void api_should_fail_with_400_code()
        {
            using (var startup = new OwinStartup(DatabaseName, EducationOrganizationIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = new TimeSpan(0, 0, 15, 0);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var response = StudentHelper.CreateStudentAndAssociateToSchool(client, string.Empty, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), 255901001);
                        response.ResponseMessage.IsSuccessStatusCode.ShouldBeFalse();
                        response.ResponseMessage.StatusCode.ShouldEqual(HttpStatusCode.BadRequest);
                        Assert.That(response.ResponseMessage.Content.ReadAsStringAsync().Result, Is.StringContaining("LastSurname is required"));
                    }
                }
            }            
        }
    }
}
