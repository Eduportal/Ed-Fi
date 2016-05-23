using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using EdFi.Ods.Api.Models.Resources.Student;
using EdFi.Ods.WebService.Tests.Owin;
using EdFi.Ods.WebService.Tests._Helpers;
using Microsoft.Owin.Testing;
using Newtonsoft.Json;
using NUnit.Framework;
using Should;

namespace EdFi.Ods.WebService.Tests.UniqueIdentifierWebApi 
{
    [TestFixture, Ignore("Tests for an integrated Unique Id System")]
    public class IntegratedUniqueIdentityTests : OwinTestBase
    {
        private static readonly string _databaseName = string.Format("GetAll_200_Tests_{0}", Guid.NewGuid().ToString("N"));
        private readonly List<int> LocalEducationAgencyIds = new List<int> { 255901 };

        protected override string DatabaseName { get { return _databaseName; } }
        protected override string BaseDatabase { get { return "EdFi_Ods_Populated_Template"; } }

        [Test]
        public void When_putting_a_person_with_updated_unique_id_Should_return_a_204_code()
        {
            using (var startup = new OwinStartup(DatabaseName, LocalEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = new TimeSpan(0, 0, 15, 0);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        //Create Student
                        var createResponse = StudentHelper.CreateStudentAndAssociateToSchool(client, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), 255901001);
                        var studentUri = createResponse.ResponseMessage.Headers.Location.AbsoluteUri;

                        //Retrieve the Student
                        var getResponse = client.GetAsync(studentUri).Result;
                        getResponse.EnsureSuccessStatusCode();
                        var student = JsonConvert.DeserializeObject<Student>(getResponse.Content.ReadAsStringAsync().Result);

                        string newUniqueId = Guid.NewGuid().ToString("n");

                        //Update the student with a new uniqueId
                        student.StudentUniqueId = newUniqueId;

                        var putResponse = client.PutAsync(studentUri, new StringContent(JsonConvert.SerializeObject(student), Encoding.UTF8, "application/json")).Result;
                        putResponse.StatusCode.ShouldEqual(HttpStatusCode.NoContent);

                        //Retrieve the Student
                        var getResponse2 = client.GetAsync(studentUri).Result;
                        getResponse.EnsureSuccessStatusCode();
                        var studentAfterPut = JsonConvert.DeserializeObject<Student>(getResponse2.Content.ReadAsStringAsync().Result);

                        studentAfterPut.StudentUniqueId.ShouldEqual(newUniqueId);
                    }
                }
            }
        }

        [Test]
        public void When_configured_with_UniqueId_integration_putting_a_person_with_updated_unique_id_Should_return_a_400_code()
        {
            using (var startup = new OwinStartup(DatabaseName, LocalEducationAgencyIds, useUniqueIdIntegration: true))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = new TimeSpan(0, 0, 15, 0);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        //Create Student
                        var createResponse = StudentHelper.CreateStudentAndAssociateToSchool(client, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), 255901001);
                        var studentUri = createResponse.ResponseMessage.Headers.Location.AbsoluteUri;

                        //Retrieve the Student
                        var getResponse = client.GetAsync(studentUri).Result;
                        getResponse.EnsureSuccessStatusCode();
                        var student = JsonConvert.DeserializeObject<Student>(getResponse.Content.ReadAsStringAsync().Result);

                        //create new uniqueId
                        var response = client.PostAsync(OwinUriHelper.BuildApiUri(null, "identities"), new StringContent(JsonConvert.SerializeObject(UniqueIdCreator.InitializeAPersonWithUniqueData()), Encoding.UTF8, "application/json")).Result;
                        response.EnsureSuccessStatusCode();
                        var uniqueId = UniqueIdCreator.ExtractIdFromHttpResponse(response);

                        //Update the student with a new uniqueId
                        student.StudentUniqueId = uniqueId;

                        var putResponse = client.PutAsync(studentUri, new StringContent(JsonConvert.SerializeObject(student), Encoding.UTF8, "application/json")).Result;
                        putResponse.StatusCode.ShouldEqual(HttpStatusCode.BadRequest);
                        putResponse.Content.ReadAsStringAsync().Result.ShouldContain("UniqueId cannot be modified");
                    }
                }
            }
        }

        [Test]
        public void When_configured_with_UniqueId_integration_putting_a_person_with_unique_id_that_does_not_exist_Should_return_a_400_code()
        {
            using (var startup = new OwinStartup(DatabaseName, LocalEducationAgencyIds, useUniqueIdIntegration: true))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = new TimeSpan(0, 0, 15, 0);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        //Create Student
                        var createResponse = StudentHelper.CreateStudentAndAssociateToSchool(client, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), 255901001);
                        var studentUri = createResponse.ResponseMessage.Headers.Location.AbsoluteUri;

                        //Retrieve the Student
                        var getResponse = client.GetAsync(studentUri).Result;
                        getResponse.EnsureSuccessStatusCode();
                        var student = JsonConvert.DeserializeObject<Student>(getResponse.Content.ReadAsStringAsync().Result);

                        //Update the student with a new uniqueId
                        student.StudentUniqueId = Guid.NewGuid().ToString("N");

                        var putResponse = client.PutAsync(studentUri, new StringContent(JsonConvert.SerializeObject(student), Encoding.UTF8, "application/json")).Result;
                        putResponse.StatusCode.ShouldEqual(HttpStatusCode.BadRequest);
                        putResponse.Content.ReadAsStringAsync().Result.ShouldContain("UniqueId cannot be modified");
                    }
                }
            }
        }

        [Test, Ignore("UniqueId shim no longer requires identity controller")]
        public void When_posting_a_person_with_unique_id_that_does_not_exist_Should_return_a_400_code()
        {
            using (var startup = new OwinStartup(DatabaseName, LocalEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = new TimeSpan(0, 0, 15, 0);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        //Create Student
                        var uniqueId = Guid.NewGuid().ToString("N");
                        var student = ResourceHelper.CreateStudent(uniqueId, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture));

                        //Post the Student
                        var createResponse = client.PostAsync(OwinUriHelper.BuildApiUri("2014", "students"), new StringContent(student, Encoding.UTF8, "application/json")).Result;
                        createResponse.StatusCode.ShouldEqual(HttpStatusCode.BadRequest);
                        createResponse.Content.ReadAsStringAsync().Result.ShouldContain(string.Format("UniqueId: '{0}' not found.", uniqueId));
                    }
                }
            }
        }

        [Test]
        public void When_getting_by_unique_id_that_does_not_exist_Should_return_404()
        {
            using (var startup = new OwinStartup(DatabaseName, LocalEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = new TimeSpan(0, 0, 15, 0);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        //Create Student
                        var uniqueId = Guid.NewGuid().ToString("N");
                        var student = ResourceHelper.CreateStudent(uniqueId, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture));

                        //Post the Student
                        var createResponse = client.GetAsync(OwinUriHelper.BuildApiUri("2014", "students", string.Format("studentUniqueId={0}", Guid.NewGuid().ToString("N")))).Result;
                        createResponse.StatusCode.ShouldEqual(HttpStatusCode.NotFound);
                    }
                }
            }
        }
    }
}
