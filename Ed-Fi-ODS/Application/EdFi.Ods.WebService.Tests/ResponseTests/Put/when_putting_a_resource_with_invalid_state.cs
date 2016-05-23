using EdFi.Ods.Api.Models.Resources.Student;
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
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace EdFi.Ods.WebService.Tests.ResponseTests.Put
{
    [TestFixture]
    public class when_putting_a_resource_with_invalid_state : OwinTestBase
    {
        private static readonly string _databaseName = string.Format("EdFi_Tests_when_putting_a_resource_with_invalid_state_{0}", Guid.NewGuid().ToString("N"));
        private readonly List<int> LocalEducationAgencyIds = new List<int> { 255901 };

        protected override string DatabaseName { get { return _databaseName; } }
        protected override string BaseDatabase { get { return "EdFi_Ods_Populated_Template"; } }

        [Test]
        public void api_should_fail_with_400_bad_request()
        {
            using (var startup = new OwinStartup(DatabaseName, LocalEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = new TimeSpan(0, 0, 15, 0);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        //create student
                        var studentCreationResponse = StudentHelper.CreateStudentAndAssociateToSchool(client, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), 255901001);

                        //retrieve the created student
                        var studentId = studentCreationResponse.ResponseMessage.Headers.Location.AbsoluteUri.Split('/').Last();
                        var getById = client.GetAsync(studentCreationResponse.ResponseMessage.Headers.Location.AbsoluteUri).Result;
                        var resource = JsonConvert.DeserializeObject<Student>(getById.Content.ReadAsStringAsync().Result);

                        //update the student
                        resource.BirthCountryDescriptor = "00"; // intentionally invalid value

                        //submit the updated student
                        var responseMessage = client.PutAsync(studentCreationResponse.ResponseMessage.Headers.Location.AbsoluteUri, new StringContent(JsonConvert.SerializeObject(resource), Encoding.UTF8, "application/json")).Result;

                        responseMessage.IsSuccessStatusCode.ShouldBeFalse();
                        responseMessage.StatusCode.ShouldEqual(HttpStatusCode.BadRequest);
                        responseMessage.Content.ReadAsStringAsync().Result.ShouldContain("Unable to resolve value '00' to an existing 'CountryDescriptor' resource.");
                    }
                }
            }
        }
    }
}
