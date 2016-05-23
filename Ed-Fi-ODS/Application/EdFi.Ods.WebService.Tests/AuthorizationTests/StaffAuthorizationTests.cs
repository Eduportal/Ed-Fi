using EdFi.Ods.Api.Models.Resources.Staff;
using EdFi.Ods.WebService.Tests._Helpers;
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

namespace EdFi.Ods.WebService.Tests.AuthorizationTests
{
    [TestFixture]
    public class StaffAuthorizationTests : OwinTestBase
    {
        const int StateEducationAgencyId = 31;
        const int Lea1Id = 2001;
        const int Lea2Id = 2002;
        const int Lea3Id = 2003;
        const int School1Id = 200101;
        const int School2Id = 200201;
        const int School3Id = 200301;
        static readonly TimeSpan _15_Minutes = new TimeSpan(0, 0, 15, 0);

        private static readonly string _databaseName = string.Format("EdFi_Tests_StaffAuthorizationTests_{0}", Guid.NewGuid().ToString("N"));
        private readonly List<int> LocalEducationAgencyIds = new List<int> {Lea1Id};

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

            helper.CreateSchool(School1Id, Lea1Id);
            helper.CreateSchool(School2Id, Lea2Id);
            helper.CreateSchool(School3Id, Lea3Id);
        }

        [Test]
        public void When_creating_a_staff_should_return_201_created()
        {
            using (var startup = new OwinStartup(DatabaseName, LocalEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = _15_Minutes;
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var createResponse = StaffHelper.CreateStaff(client, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture));
                        createResponse.ResponseMessage.EnsureSuccessStatusCode();
                        createResponse.ResponseMessage.StatusCode.ShouldEqual(HttpStatusCode.Created);
                    }
                }
            }
        }

        [Test]
        public void When_updating_an_unassociated_staff_should_fail_with_403_forbidden()
        {
            var localEducationAgencyIds = new List<int> {Lea1Id};
            string staffUri;
            Staff staff;

            using (var startup = new OwinStartup(DatabaseName, localEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = _15_Minutes;
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var createResponse = StaffHelper.CreateStaff(client, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture));
                        createResponse.ResponseMessage.EnsureSuccessStatusCode();

                        var associationResponse = client.PostAsync(OwinUriHelper.BuildApiUri("2014", "StaffEducationOrganizationEmploymentAssociations"),
                            new StringContent(ResourceHelper.CreateStaffEducationOrganizationEmploymentAssociation(createResponse.UniqueId, Lea1Id), Encoding.UTF8, "application/json")).Result;

                        associationResponse.EnsureSuccessStatusCode();

                        staffUri = createResponse.ResponseMessage.Headers.Location.AbsoluteUri;

                        var getResult = client.GetStringAsync(staffUri).Result;
                        staff = JsonConvert.DeserializeObject<Staff>(getResult);
                    }
                }
            }

            localEducationAgencyIds = new List<int> {Lea3Id};
            using (var startup = new OwinStartup(DatabaseName, localEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = _15_Minutes;
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        staff.LastSurname = "GotMarried";

                        var putResponse = client.PutAsync(staffUri, new StringContent(JsonConvert.SerializeObject(staff), Encoding.UTF8, "application/json")).Result;
                        putResponse.StatusCode.ShouldEqual(HttpStatusCode.Forbidden);
                    }
                }
            }
        }

        [Test]
        public void When_deleting_an_unused_staff_should_return_204_nocontent()
        {
            using (var startup = new OwinStartup(DatabaseName, LocalEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = _15_Minutes;
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var createResponse = StaffHelper.CreateStaff(client, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture));
                        createResponse.ResponseMessage.EnsureSuccessStatusCode();

                        var deleteResponse = client.DeleteAsync(createResponse.ResponseMessage.Headers.Location.AbsoluteUri).Result;
                        deleteResponse.StatusCode.ShouldEqual(HttpStatusCode.NoContent);
                    }
                }
            }
        }

        [Test]
        public void When_deleting_a_staff_in_use_should_return_409_conflict()
        {
            using (var startup = new OwinStartup(DatabaseName, LocalEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = _15_Minutes;
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var createResponse = StaffHelper.CreateStaff(client, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture));
                        createResponse.ResponseMessage.EnsureSuccessStatusCode();

                        var associationResponse = client.PostAsync(OwinUriHelper.BuildApiUri("2014", "StaffEducationOrganizationEmploymentAssociations"),
                            new StringContent(ResourceHelper.CreateStaffEducationOrganizationEmploymentAssociation(createResponse.UniqueId, Lea1Id), Encoding.UTF8, "application/json")).Result;

                        associationResponse.EnsureSuccessStatusCode();

                        var deleteResponse = client.DeleteAsync(createResponse.ResponseMessage.Headers.Location.AbsoluteUri).Result;
                        deleteResponse.StatusCode.ShouldEqual(HttpStatusCode.Conflict);
                    }
                }
            }
        }

        [Test]
        public void When_associating_a_staff_not_related_to_vendors_leas_should_fail_with_403_forbidden()
        {
            string uniqueId;

            var localEducationAgencyIds = new List<int> {Lea1Id};
            using (var startup = new OwinStartup(DatabaseName, localEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = _15_Minutes;
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var createResponse = StaffHelper.CreateStaff(client, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture));
                        createResponse.ResponseMessage.EnsureSuccessStatusCode();
                        uniqueId = createResponse.UniqueId;
                    }
                }
            }

            localEducationAgencyIds = new List<int> {Lea3Id};
            using (var startup = new OwinStartup(DatabaseName, localEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = _15_Minutes;
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var associationResponse = client.PostAsync(OwinUriHelper.BuildApiUri("2014", "StaffEducationOrganizationEmploymentAssociations"),
                            new StringContent(ResourceHelper.CreateStaffEducationOrganizationEmploymentAssociation(uniqueId, Lea1Id), Encoding.UTF8, "application/json")).Result;

                        associationResponse.StatusCode.ShouldEqual(HttpStatusCode.Forbidden);
                    }
                }
            }

        }

        [Test]
        public void When_associating_a_staff_related_to_vendors_leas_should_return_201_created()
        {
            using (var startup = new OwinStartup(DatabaseName, LocalEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = _15_Minutes;
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var createResponse = StaffHelper.CreateStaff(client, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture));
                        createResponse.ResponseMessage.EnsureSuccessStatusCode();

                        var associationResponse = client.PostAsync(OwinUriHelper.BuildApiUri("2014", "StaffEducationOrganizationEmploymentAssociations"),
                            new StringContent(ResourceHelper.CreateStaffEducationOrganizationEmploymentAssociation(createResponse.UniqueId, Lea1Id), Encoding.UTF8, "application/json")).Result;

                        associationResponse.EnsureSuccessStatusCode();
                        associationResponse.StatusCode.ShouldEqual(HttpStatusCode.Created);
                    }
                }
            }
        }
    }

    [TestFixture]
    public class StaffAuthorizationGetTests : OwinTestBase
    {
        const int StateEducationAgencyId = 31;
        const int Lea1Id = 2001;
        const int Lea2Id = 2002;
        const int Lea3Id = 2003;
        const int School1Id = 200101;
        const int School2Id = 200201;
        const int School3Id = 200301;
        static readonly TimeSpan _15_Minutes = new TimeSpan(0, 0, 15, 0);

        private static readonly string _databaseName = string.Format("EdFi_Tests_StaffAuthorizationGetTests_{0}", Guid.NewGuid().ToString("N"));
        
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

            helper.CreateSchool(School1Id, Lea1Id);
            helper.CreateSchool(School2Id, Lea2Id);
            helper.CreateSchool(School3Id, Lea3Id);
        }

        [Test]
        public void When_getting_all_staffs_should_return_only_authorized_staffs()
        {
            var authorizedUniqueIds = new List<string>();

            var localEducationAgencyIds = new List<int> { Lea1Id, Lea2Id, Lea3Id };
            using (var startup = new OwinStartup(DatabaseName, localEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = _15_Minutes;
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        //1st Staff
                        var createResponse = StaffHelper.CreateStaff(client, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture));
                        createResponse.ResponseMessage.EnsureSuccessStatusCode();
                        authorizedUniqueIds.Add(createResponse.UniqueId);

                        var associationResponse = client.PostAsync(OwinUriHelper.BuildApiUri("2014", "StaffEducationOrganizationEmploymentAssociations"),
                            new StringContent(ResourceHelper.CreateStaffEducationOrganizationEmploymentAssociation(createResponse.UniqueId, Lea1Id), Encoding.UTF8, "application/json")).Result;

                        associationResponse.EnsureSuccessStatusCode();

                        //2nd Student && Parent
                        createResponse = StaffHelper.CreateStaff(client, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture));
                        createResponse.ResponseMessage.EnsureSuccessStatusCode();
                        authorizedUniqueIds.Add(createResponse.UniqueId);

                        associationResponse = client.PostAsync(OwinUriHelper.BuildApiUri("2014", "StaffEducationOrganizationEmploymentAssociations"),
                            new StringContent(ResourceHelper.CreateStaffEducationOrganizationEmploymentAssociation(createResponse.UniqueId, Lea2Id), Encoding.UTF8, "application/json")).Result;

                        associationResponse.EnsureSuccessStatusCode();

                        //3rd Student && Parent
                        createResponse = StaffHelper.CreateStaff(client, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture));
                        createResponse.ResponseMessage.EnsureSuccessStatusCode();
                        authorizedUniqueIds.Add(createResponse.UniqueId);

                        associationResponse = client.PostAsync(OwinUriHelper.BuildApiUri("2014", "StaffEducationOrganizationEmploymentAssociations"),
                            new StringContent(ResourceHelper.CreateStaffEducationOrganizationEmploymentAssociation(createResponse.UniqueId, Lea3Id), Encoding.UTF8, "application/json")).Result;

                        associationResponse.EnsureSuccessStatusCode();
                    }
                }
            }

            localEducationAgencyIds = new List<int> { Lea1Id, Lea2Id };
            using (var startup = new OwinStartup(DatabaseName, localEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = _15_Minutes;
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var getAll = client.GetAsync(OwinUriHelper.BuildApiUri("2014", "staffs")).Result;
                        var parents = (Staff[]) JsonConvert.DeserializeObject(getAll.Content.ReadAsStringAsync().Result, typeof (Staff[]));
                        parents.Length.ShouldEqual(2);
                        parents.Select(x => x.StaffUniqueId).Intersect(authorizedUniqueIds).Count().ShouldEqual(2);
                    }
                }
            }
        }

        [Test]
        public void When_getting_staff_by_id_should_return_200_when_authorized()
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

                        //1st
                        var createResponse = StaffHelper.CreateStaff(client, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture));
                        createResponse.ResponseMessage.EnsureSuccessStatusCode();

                        var associationResponse = client.PostAsync(OwinUriHelper.BuildApiUri("2014", "StaffEducationOrganizationEmploymentAssociations"),
                            new StringContent(ResourceHelper.CreateStaffEducationOrganizationEmploymentAssociation(createResponse.UniqueId, Lea1Id), Encoding.UTF8, "application/json")).Result;

                        associationResponse.EnsureSuccessStatusCode();

                        authorizedUri = createResponse.ResponseMessage.Headers.Location.AbsoluteUri;

                        //2nd
                        createResponse = StaffHelper.CreateStaff(client, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture));
                        createResponse.ResponseMessage.EnsureSuccessStatusCode();

                        associationResponse = client.PostAsync(OwinUriHelper.BuildApiUri("2014", "StaffEducationOrganizationEmploymentAssociations"),
                            new StringContent(ResourceHelper.CreateStaffEducationOrganizationEmploymentAssociation(createResponse.UniqueId, Lea2Id), Encoding.UTF8, "application/json")).Result;

                        associationResponse.EnsureSuccessStatusCode();

                        unauthorizedUri = createResponse.ResponseMessage.Headers.Location.AbsoluteUri;
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
        public void When_getting_staff_by_key_should_return_200_when_authorized()
        {
            string authorizedUniqueId;
            string unauthorizedUniqueId;

            var localEducationAgencyIds = new List<int> { Lea1Id, Lea2Id };
            using (var startup = new OwinStartup(DatabaseName, localEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = _15_Minutes;
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        //1st
                        var createResponse = StaffHelper.CreateStaff(client, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture));
                        createResponse.ResponseMessage.EnsureSuccessStatusCode();

                        var associationResponse = client.PostAsync(OwinUriHelper.BuildApiUri("2014", "StaffEducationOrganizationEmploymentAssociations"),
                            new StringContent(ResourceHelper.CreateStaffEducationOrganizationEmploymentAssociation(createResponse.UniqueId, Lea1Id), Encoding.UTF8, "application/json")).Result;

                        associationResponse.EnsureSuccessStatusCode();

                        authorizedUniqueId = createResponse.UniqueId;

                        //2nd
                        createResponse = StaffHelper.CreateStaff(client, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture));
                        createResponse.ResponseMessage.EnsureSuccessStatusCode();

                        associationResponse = client.PostAsync(OwinUriHelper.BuildApiUri("2014", "StaffEducationOrganizationEmploymentAssociations"),
                            new StringContent(ResourceHelper.CreateStaffEducationOrganizationEmploymentAssociation(createResponse.UniqueId, Lea2Id), Encoding.UTF8, "application/json")).Result;

                        associationResponse.EnsureSuccessStatusCode();

                        unauthorizedUniqueId = createResponse.UniqueId;
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

                        var authorizedResult = client.GetAsync(OwinUriHelper.BuildApiUri("2014", "staffs", string.Format("staffUniqueId={0}", authorizedUniqueId))).Result;
                        authorizedResult.EnsureSuccessStatusCode();
                        authorizedResult.StatusCode.ShouldEqual(HttpStatusCode.OK);

                        var unauthorizedResult = client.GetAsync(OwinUriHelper.BuildApiUri("2014", "staffs", string.Format("staffUniqueId={0}", unauthorizedUniqueId))).Result;
                        unauthorizedResult.StatusCode.ShouldEqual(HttpStatusCode.Forbidden);
                    }
                }
            }
        }

        [Test]
        public void When_getting_staff_by_example_should_return_200_and_parent_when_authorized()
        {
            var authorizedFirstName = "John";
            var authorizedLastName = string.Format("A{0}", DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture));

            var unauthorizedFirstName = "Other";
            var unauthorizedLastName = string.Format("U{0}", DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture));

            var localEducationAgencyIds = new List<int> { Lea1Id, Lea2Id };
            using (var startup = new OwinStartup(DatabaseName, localEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = _15_Minutes;
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        //1st
                        var createResponse = StaffHelper.CreateStaff(client, authorizedLastName, authorizedFirstName);
                        createResponse.ResponseMessage.EnsureSuccessStatusCode();

                        var associationResponse = client.PostAsync(OwinUriHelper.BuildApiUri("2014", "StaffEducationOrganizationEmploymentAssociations"),
                            new StringContent(ResourceHelper.CreateStaffEducationOrganizationEmploymentAssociation(createResponse.UniqueId, Lea1Id), Encoding.UTF8, "application/json")).Result;

                        associationResponse.EnsureSuccessStatusCode();

                        //2nd
                        createResponse = StaffHelper.CreateStaff(client, unauthorizedLastName, unauthorizedFirstName);
                        createResponse.ResponseMessage.EnsureSuccessStatusCode();

                        associationResponse = client.PostAsync(OwinUriHelper.BuildApiUri("2014", "StaffEducationOrganizationEmploymentAssociations"),
                            new StringContent(ResourceHelper.CreateStaffEducationOrganizationEmploymentAssociation(createResponse.UniqueId, Lea2Id), Encoding.UTF8, "application/json")).Result;

                        associationResponse.EnsureSuccessStatusCode();
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

                        var authorizedResult = client.GetAsync(OwinUriHelper.BuildApiUri("2014", "staffs", string.Format("LastSurname={0}", authorizedLastName))).Result;
                        authorizedResult.EnsureSuccessStatusCode();
                        authorizedResult.StatusCode.ShouldEqual(HttpStatusCode.OK);

                        var staffs = JsonConvert.DeserializeObject<Staff[]>(authorizedResult.Content.ReadAsStringAsync().Result);
                        staffs.Length.ShouldEqual(1);
                        staffs[0].FirstName.ShouldEqual(authorizedFirstName);

                        var unauthorizedResult = client.GetAsync(OwinUriHelper.BuildApiUri("2014", "staffs", string.Format("LastSurname={0}", unauthorizedLastName))).Result;
                        unauthorizedResult.StatusCode.ShouldEqual(HttpStatusCode.OK);
                        staffs = JsonConvert.DeserializeObject<Staff[]>(unauthorizedResult.Content.ReadAsStringAsync().Result);
                        staffs.Length.ShouldEqual(0);
                    }
                }
            }
        }
    }
}
