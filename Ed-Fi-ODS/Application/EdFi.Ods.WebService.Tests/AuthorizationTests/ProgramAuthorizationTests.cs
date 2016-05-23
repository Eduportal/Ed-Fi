using EdFi.Ods.Api.Models.Resources.Program;
using EdFi.Ods.WebService.Tests._Helpers;
using EdFi.Ods.WebService.Tests.Extensions;
using EdFi.Ods.WebService.Tests.Owin;
using Microsoft.Owin.Testing;
using Newtonsoft.Json;
using NUnit.Framework;
using Should;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace EdFi.Ods.WebService.Tests.AuthorizationTests
{
    [TestFixture]
    public class ProgramAuthorizationTests : OwinTestBase
    {
        const int Lea1Id = 23001;
        const int Lea2Id = 23002;
        const int Lea3Id = 23003;
        const int StateEducationAgencyId = 31;
        static readonly TimeSpan _15_Minutes = new TimeSpan(0, 0, 15, 0);

        private static readonly string _databaseName = string.Format("EdFi_Tests_ProgramAuthorizationTests_{0}", Guid.NewGuid().ToString("N"));
        
        protected override string DatabaseName { get { return _databaseName; } }
        protected override string BaseDatabase { get { return "EdFi_Ods_Populated_Template"; } }

        public override void SetUp()
        {
            base.SetUp();

            var helper = new DataSeedHelper(DatabaseName);
            helper.CreateStateEducationAgency(StateEducationAgencyId);

            helper.CreateLocalEducationAgency(Lea1Id, StateEducationAgencyId);
            helper.CreateLocalEducationAgency(Lea2Id, StateEducationAgencyId);
            helper.CreateLocalEducationAgency(Lea3Id, StateEducationAgencyId);
        }

        [Test]
        public void When_getting_all_programs_should_return_only_authorized_programs()
        {
            var authorizedIds = new List<Guid>();

            var localEducationAgencyIds = new List<int> { Lea1Id, Lea2Id, Lea3Id };
            using (var startup = new OwinStartup(DatabaseName, localEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = _15_Minutes;
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        //1st program
                        var createResponse = client.PostAsync(OwinUriHelper.BuildApiUri("2014", "programs"), new StringContent(ResourceHelper.CreateProgram(Lea1Id), Encoding.UTF8, "application/json")).Result;
                        createResponse.EnsureSuccessStatusCode();
                        authorizedIds.Add(new Guid(createResponse.Headers.Location.AbsoluteUri.Split('/').Last()));

                        //2nd program
                        createResponse = client.PostAsync(OwinUriHelper.BuildApiUri("2014", "programs"), new StringContent(ResourceHelper.CreateProgram(Lea2Id), Encoding.UTF8, "application/json")).Result;
                        createResponse.EnsureSuccessStatusCode();
                        authorizedIds.Add(new Guid(createResponse.Headers.Location.AbsoluteUri.Split('/').Last()));

                        //3rd program
                        createResponse = client.PostAsync(OwinUriHelper.BuildApiUri("2014", "programs"), new StringContent(ResourceHelper.CreateProgram(Lea3Id), Encoding.UTF8, "application/json")).Result;
                        createResponse.EnsureSuccessStatusCode();
                    }
                }
            }

            localEducationAgencyIds = new List<int> { Lea1Id, Lea2Id, };
            using (var startup = new OwinStartup(DatabaseName, localEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = _15_Minutes;
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var getAll = client.GetAsync(OwinUriHelper.BuildApiUri("2014", "programs")).Result;
                        var programs = (Program[]) JsonConvert.DeserializeObject(getAll.Content.ReadAsStringAsync().Result, typeof (Program[]));
                        programs.Select(x => x.Id).Intersect(authorizedIds).Count().ShouldEqual(2);
                    }
                }
            }
        }

        [Test]
        public void When_getting_program_by_id_should_return_200_for_authorized_resource()
        {
            string authorizedUri;
            string unauthorizedUri;

            var localEducationAgencyIds = new List<int> { Lea1Id, Lea2Id };
            using (var startup = new OwinStartup(DatabaseName, localEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = _15_Minutes;
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        //1st program
                        var createResponse = client.PostAsync(OwinUriHelper.BuildApiUri("2014", "programs"), new StringContent(ResourceHelper.CreateProgram(Lea1Id), Encoding.UTF8, "application/json")).Result;
                        createResponse.EnsureSuccessStatusCode();
                        authorizedUri = createResponse.Headers.Location.AbsoluteUri;

                        //2nd program
                        createResponse = client.PostAsync(OwinUriHelper.BuildApiUri("2014", "programs"), new StringContent(ResourceHelper.CreateProgram(Lea2Id), Encoding.UTF8, "application/json")).Result;
                        createResponse.EnsureSuccessStatusCode();
                        unauthorizedUri = createResponse.Headers.Location.AbsoluteUri;
                    }
                }
            }

            localEducationAgencyIds = new List<int> { Lea1Id };
            using (var startup = new OwinStartup(DatabaseName, localEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = _15_Minutes;
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var authorizedResult = client.GetAsync(authorizedUri).Result;
                        authorizedResult.EnsureSuccessStatusCode();
                        authorizedResult.StatusCode.ShouldEqual(HttpStatusCode.OK);

                        var unauthorizedResult = client.GetAsync(unauthorizedUri).Result;
                        unauthorizedResult.StatusCode.ShouldEqual(HttpStatusCode.Forbidden);
                    }
                }
            }
        }

        [Test]
        public void When_getting_program_by_key_should_return_200_for_authorized_resource()
        {
            var program1 = JsonConvert.DeserializeObject<Program>(ResourceHelper.CreateProgram(Lea1Id));
            var program1QueryString = string.Format("educationOrganizationId={0}&type={1}&name={2}", program1.EducationOrganizationReference.EducationOrganizationId, program1.ProgramType, program1.ProgramName);
            var program2 = JsonConvert.DeserializeObject<Program>(ResourceHelper.CreateProgram(Lea2Id));
            var program2QueryString = string.Format("educationOrganizationId={0}&type={1}&name={2}", program2.EducationOrganizationReference.EducationOrganizationId, program2.ProgramType, program2.ProgramName);

            var localEducationAgencyIds = new List<int> { Lea1Id, Lea2Id };
            using (var startup = new OwinStartup(DatabaseName, localEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = _15_Minutes;
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        //1st program
                        var createResponse = client.PostAsync(OwinUriHelper.BuildApiUri("2014", "programs"), new StringContent(JsonConvert.SerializeObject(program1), Encoding.UTF8, "application/json")).Result;
                        createResponse.EnsureSuccessStatusCode();

                        //2nd program
                        createResponse = client.PostAsync(OwinUriHelper.BuildApiUri("2014", "programs"), new StringContent(JsonConvert.SerializeObject(program2), Encoding.UTF8, "application/json")).Result;
                        createResponse.EnsureSuccessStatusCode();
                    }
                }
            }

            localEducationAgencyIds = new List<int> { Lea1Id };
            using (var startup = new OwinStartup(DatabaseName, localEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = _15_Minutes;
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var authorizedParentResult = client.GetAsync(OwinUriHelper.BuildApiUri("2014", "programs", program1QueryString)).Result;
                        authorizedParentResult.EnsureSuccessStatusCode();
                        authorizedParentResult.StatusCode.ShouldEqual(HttpStatusCode.OK);

                        var unauthorizedParentResult = client.GetAsync(OwinUriHelper.BuildApiUri("2014", "programs", program2QueryString)).Result;
                        unauthorizedParentResult.StatusCode.ShouldEqual(HttpStatusCode.Forbidden);
                    }
                }
            }
        }
    }
}
