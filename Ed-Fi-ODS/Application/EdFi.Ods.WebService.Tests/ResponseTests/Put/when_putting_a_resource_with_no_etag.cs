using Castle.MicroKernel.Registration;
using Castle.Windsor;
using EdFi.Common.Database;
using EdFi.Ods.Api.Common.Authorization;
using EdFi.Ods.Api.Models.Resources.Student;
using EdFi.Ods.Common._Installers.ComponentNaming;
using EdFi.Ods.WebService.Tests.Extensions;
using EdFi.Ods.WebService.Tests.Owin;
using EdFi.Ods.WebService.Tests._Helpers;
using Microsoft.Owin.Testing;
using Newtonsoft.Json;
using NUnit.Framework;
using Rhino.Mocks;
using Should;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace EdFi.Ods.WebService.Tests.ResponseTests.Put
{
    [TestFixture]
    public class when_putting_a_resource_with_no_etag : OwinTestBase
    {
        private static readonly string _databaseName = string.Format("EdFi_Tests_when_putting_a_resource_with_no_etag_{0}", Guid.NewGuid().ToString("N"));
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

                        //create student
                        var studentCreationResponse = StudentHelper.CreateStudentAndAssociateToSchool(client, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), 255901001);

                        //retrieve the created student
                        var getById = client.GetAsync(studentCreationResponse.ResponseMessage.Headers.Location.AbsoluteUri).Result;
                        var originalETag = getById.Headers.ETag.Tag;
                        var resource = JsonConvert.DeserializeObject<Student>(getById.Content.ReadAsStringAsync().Result);

                        //update the student
                        resource.MaidenName = resource.MaidenName == "MaidenName" ? "OtherMaiden" : "MaidenName";

                        //submit the updated student
                        var response = client.PutAsync(studentCreationResponse.ResponseMessage.Headers.Location.AbsoluteUri, new StringContent(JsonConvert.SerializeObject(resource), Encoding.UTF8, "application/json")).Result;

                        response.IsSuccessStatusCode.ShouldBeTrue();
                        response.StatusCode.ShouldEqual(HttpStatusCode.NoContent);
                        response.Headers.ETag.Tag.ShouldNotEqual(originalETag);
                    }
                }
            }   
        }
    }
}
