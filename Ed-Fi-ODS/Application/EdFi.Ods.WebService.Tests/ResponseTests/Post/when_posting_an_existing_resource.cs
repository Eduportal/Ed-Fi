﻿using EdFi.Ods.Api.Models.Resources.Student;
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
using System.Text;

namespace EdFi.Ods.WebService.Tests.ResponseTests.Post
{
    [TestFixture]
    class when_posting_an_existing_resource : OwinTestBase
    {
        private static readonly string _databaseName = string.Format("EdFi_Tests_when_posting_an_existing_resource_{0}", Guid.NewGuid().ToString("N"));
        private readonly List<int> LocalEducationAgencyIds = new List<int> { 255901 };

        protected override string DatabaseName { get { return _databaseName; } }
        protected override string BaseDatabase { get { return "EdFi_Ods_Populated_Template"; } }

        [Test]
        public void api_should_succeed_with_200_code()
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
                        var resource = JsonConvert.DeserializeObject<Student>(getById.Content.ReadAsStringAsync().Result);

                        //update the student
                        resource.Id = Guid.Empty;
                        resource.LastSurname = "GotMarried";

                        //submit the updated student
                        var responseMessage = client.PostAsync(OwinUriHelper.BuildApiUri("2014", "students"), new StringContent(JsonConvert.SerializeObject(resource), Encoding.UTF8, "application/json")).Result;
                        responseMessage.StatusCode.ShouldEqual(HttpStatusCode.OK);
                    }
                }
            }
        }
    }
}
