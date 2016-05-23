using EdFi.Ods.Api.Models.Resources.GradeLevelDescriptor;
using EdFi.Ods.WebService.Tests.Extensions;
using EdFi.Ods.WebService.Tests.Owin;
using Microsoft.Owin.Testing;
using Newtonsoft.Json;
using NUnit.Framework;
using Should;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;

namespace EdFi.Ods.WebService.Tests.Bugs_NotResolved
{
    public class GradeLevelDescriptors_GetAll_OnlyReturnsKindergarten 
    {
        [TestFixture]
        public class When_getting_all_gradeLevelDescriptors : OwinTestBase
        {
            private static readonly string _databaseName = string.Format("When_getting_all_gradeLevelDescriptors{0}", Guid.NewGuid().ToString("N"));
            private readonly List<int> LocalEducationAgencyIds = new List<int> { 255901 };

            protected override string DatabaseName { get { return _databaseName; } }
            protected override string BaseDatabase { get { return "EdFi_Ods_Populated_Template"; } }

            [Test]
            public void When_getting_all_gradeLevelDescriptors_should_include_all_results()
            {
                using (var startup = new OwinStartup(DatabaseName, LocalEducationAgencyIds))
                {
                    using (var server = TestServer.Create(startup.Configuration))
                    {
                        using (var client = new HttpClient(server.Handler))
                        {
                            client.Timeout = new TimeSpan(0, 0, 15, 0);
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                            var response = client.GetAsync(OwinUriHelper.BuildApiUri("2014", "GradeLevelDescriptors")).Result;
                            response.EnsureSuccessStatusCode();
                            var results = (GradeLevelDescriptor[]) JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result, typeof (GradeLevelDescriptor[]));
                            results.Length.ShouldEqual(21);
                        }
                    }
                }
            }
        }
    }
}