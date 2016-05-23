using EdFi.Ods.Api.Models.Resources.Course;
using EdFi.Ods.Api.Models.Resources.EducationOrganizationNetwork;
using EdFi.Ods.WebService.Tests.Extensions;
using EdFi.Ods.WebService.Tests.Owin;
using Microsoft.Owin.Testing;
using Newtonsoft.Json;
using NUnit.Framework;
using Should;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

namespace EdFi.Ods.WebService.Tests.ResponseTests.Get
{
    [TestFixture]
    public class GetAll_200_Tests : OwinTestBase
    {
        private static readonly string _databaseName = string.Format("EdFi_Tests_GetAll_200_Tests_{0}", Guid.NewGuid().ToString("N"));
        private readonly List<int> LocalEducationAgencyIds = new List<int> { 255901 };

        protected override string DatabaseName { get { return _databaseName; } }
        protected override string BaseDatabase { get { return "EdFi_Ods_Populated_Template"; } }
        
        [Test]
        public void When_No_Parameters_are_supplied_should_return_25_results()
        {
            using (var startup = new OwinStartup(DatabaseName, LocalEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = new TimeSpan(0, 0, 15, 0);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var response = client.GetAsync(OwinUriHelper.BuildApiUri("2014", "courses", string.Empty)).Result;
                        var results = (Object[])JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result, typeof(Course[]));
                        results.Count().ShouldEqual(25);
                    }
                }
            }
        }
        
        [Test]
        public void When_No_Resources_Exist_Should_Return_Empty_Collection_With_200()
        {
            using (var startup = new OwinStartup(DatabaseName, LocalEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = new TimeSpan(0, 0, 15, 0);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var response = client.GetAsync(OwinUriHelper.BuildApiUri("2014", "educationorganizationnetworks", string.Empty)).Result;
                        var results = (Object[])JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result, typeof(EducationOrganizationNetwork[]));
                        results.Count().ShouldEqual(0);
                    }
                }
            }
        }

        [Test]
        public void When_Limit_Value_Supplied_Should_Not_Return_more_than_that_number()
        {
            using (var startup = new OwinStartup(DatabaseName, LocalEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = new TimeSpan(0, 0, 15, 0);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var response = client.GetAsync(OwinUriHelper.BuildApiUri("2014", "courses", "limit=10")).Result;
                        var results = (Object[]) JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result, typeof (Course[]));
                        results.Count().ShouldBeLessThanOrEqualTo(10);
                    }
                }
            }
        }

        [Test]
        public void When_Offset_Value_Supplied_Should_Return_Next_Set_of_Resources()
        {
            using (var startup = new OwinStartup(DatabaseName, LocalEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = new TimeSpan(0, 0, 15, 0);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var response = client.GetAsync(OwinUriHelper.BuildApiUri("2014", "courses", "limit=10")).Result;
                        var initialSet = (Object[])JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result, typeof(Course[]));
                        response = client.GetAsync(OwinUriHelper.BuildApiUri("2014", "courses", "Offset=10&limit=10")).Result;
                        var results = (Object[])JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result, typeof(Course[]));
                        results[0].ShouldNotBeSameAs(initialSet[0]);
                    }
                }
            }
        }
    }
}