using EdFi.Ods.Api.Models.Resources.Parent;
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
    public class ParentAuthorizationTests : OwinTestBase
    {
        const int StateEducationAgencyId = 31;
        const int Lea1Id = 2001;
        const int Lea2Id = 2002;
        const int Lea3Id = 2003;
        const int School1Id = 200101;
        const int School2Id = 200201;
        const int School3Id = 200301;
        static readonly TimeSpan _15_Minutes = new TimeSpan(0, 0, 15, 0);

        private static readonly string _databaseName = string.Format("EdFi_Tests_ParentAuthorizationTests_{0}", Guid.NewGuid().ToString("N"));
        private readonly List<int> LocalEducationAgencyIds = new List<int> { Lea1Id };

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

            helper.CreateSchool (School1Id, Lea1Id);
            helper.CreateSchool (School2Id, Lea2Id);
            helper.CreateSchool (School3Id, Lea3Id);
        }

        [Test]
        public void When_creating_a_parent_Should_return_201_created()
        {
            using (var startup = new OwinStartup(DatabaseName, LocalEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = _15_Minutes;
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var createResponse = ParentHelper.CreateParent(client, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture));
                        createResponse.ResponseMessage.EnsureSuccessStatusCode();
                        createResponse.ResponseMessage.StatusCode.ShouldEqual(HttpStatusCode.Created);
                    }
                }
            }
        }

        [Test]
        public void When_updating_an_unassociated_parent_should_fail_with_403_forbidden()
        {
            var localEducationAgencyIds = new List<int> { Lea1Id };
            string parentUri;
            Parent parent;

            using (var startup = new OwinStartup(DatabaseName, localEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = _15_Minutes;
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var studentCreateResponse = StudentHelper.CreateStudentAndAssociateToSchool(client, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), School1Id);
                        studentCreateResponse.ResponseMessage.EnsureSuccessStatusCode();

                        var parentCreateResponse = ParentHelper.CreateParent(client, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture));
                        parentCreateResponse.ResponseMessage.EnsureSuccessStatusCode();

                        parentUri = parentCreateResponse.ResponseMessage.Headers.Location.AbsoluteUri;

                        var studentParentResponse = StudentHelper.CreateStudentParentAssociation(client, studentCreateResponse.UniqueId, parentCreateResponse.UniqueId);
                        studentParentResponse.EnsureSuccessStatusCode();

                        var getParentResult = client.GetStringAsync(parentUri).Result;
                        parent = JsonConvert.DeserializeObject<Parent>(getParentResult);
                    }
                }
            }

            localEducationAgencyIds = new List<int> { Lea3Id };
            using (var startup = new OwinStartup(DatabaseName, localEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = _15_Minutes;
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        parent.LastSurname = "GotMarried";

                        var putResponse = client.PutAsync(parentUri, new StringContent(JsonConvert.SerializeObject(parent), Encoding.UTF8, "application/json")).Result;
                        putResponse.StatusCode.ShouldEqual(HttpStatusCode.Forbidden);
                    }
                }
            }
        }

        [Test]
        public void When_deleting_an_unused_parent_should_return_204_nocontent()
        {
            using (var startup = new OwinStartup(DatabaseName, LocalEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = _15_Minutes;
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var createResponse = ParentHelper.CreateParent(client, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture));
                        createResponse.ResponseMessage.EnsureSuccessStatusCode();

                        var deleteResponse = client.DeleteAsync(createResponse.ResponseMessage.Headers.Location.AbsoluteUri).Result;
                        deleteResponse.StatusCode.ShouldEqual(HttpStatusCode.NoContent);
                    }
                }
            }
        }

        [Test]
        public void When_deleting_a_parent_in_use_should_return_409_conflict()
        {
            using (var startup = new OwinStartup(DatabaseName, LocalEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = _15_Minutes;
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var studentCreateResponse = StudentHelper.CreateStudentAndAssociateToSchool(client, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), School1Id);
                        studentCreateResponse.ResponseMessage.EnsureSuccessStatusCode();

                        var parentCreateResponse = ParentHelper.CreateParent(client, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture));
                        parentCreateResponse.ResponseMessage.EnsureSuccessStatusCode();

                        var studentParentResponse = StudentHelper.CreateStudentParentAssociation(client, studentCreateResponse.UniqueId, parentCreateResponse.UniqueId);
                        studentParentResponse.EnsureSuccessStatusCode();

                        var deleteResponse = client.DeleteAsync(parentCreateResponse.ResponseMessage.Headers.Location.AbsoluteUri).Result;
                        deleteResponse.StatusCode.ShouldEqual(HttpStatusCode.Conflict);
                    }
                }
            }
        }

        [Test]
        public void When_associating_a_parent_not_related_to_vendors_leas_should_fail_with_403_forbidden()
        {
            var localEducationAgencyIds = new List<int> { Lea1Id };
            string studentUniqueId;
            string parentUniqueId;

            using (var startup = new OwinStartup(DatabaseName, localEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = _15_Minutes;
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var studentCreateResponse = StudentHelper.CreateStudentAndAssociateToSchool(client, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), School1Id);
                        studentCreateResponse.ResponseMessage.EnsureSuccessStatusCode();
                        studentUniqueId = studentCreateResponse.UniqueId;

                        var parentCreateResponse = ParentHelper.CreateParent(client, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture));
                        parentCreateResponse.ResponseMessage.EnsureSuccessStatusCode();
                        parentUniqueId = parentCreateResponse.UniqueId;
                    }
                }
            }

            localEducationAgencyIds = new List<int> { Lea3Id };
            using (var startup = new OwinStartup(DatabaseName, localEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = _15_Minutes;
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var studentParentResponse = StudentHelper.CreateStudentParentAssociation(client, studentUniqueId, parentUniqueId);
                        studentParentResponse.StatusCode.ShouldEqual(HttpStatusCode.Forbidden);
                    }
                }
            }
        }

        [Test]
        public void When_associating_a_parent_related_to_vendors_leas_should_return_201_created()
        {
            using (var startup = new OwinStartup(DatabaseName, LocalEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = _15_Minutes;
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var studentCreateResponse = StudentHelper.CreateStudentAndAssociateToSchool(client, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), School1Id);
                        studentCreateResponse.ResponseMessage.EnsureSuccessStatusCode();

                        var parentCreateResponse = ParentHelper.CreateParent(client, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture));
                        parentCreateResponse.ResponseMessage.EnsureSuccessStatusCode();

                        var studentParentResponse = StudentHelper.CreateStudentParentAssociation(client, studentCreateResponse.UniqueId, parentCreateResponse.UniqueId);
                        studentParentResponse.EnsureSuccessStatusCode();
                        studentParentResponse.StatusCode.ShouldEqual(HttpStatusCode.Created);
                    }
                }
            }
        }

        [Test]
        public void When_deleting_data_for_unrelated_parent_should_fail_with_409_Conflict()
        {
            var localEducationAgencyIds = new List<int> { Lea1Id };
            string parentUri;

            using (var startup = new OwinStartup(DatabaseName, localEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = _15_Minutes;
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var studentCreateResponse = StudentHelper.CreateStudentAndAssociateToSchool(client, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), School1Id);
                        studentCreateResponse.ResponseMessage.EnsureSuccessStatusCode();

                        var parentCreateResponse = ParentHelper.CreateParent(client, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture));
                        parentCreateResponse.ResponseMessage.EnsureSuccessStatusCode();

                        parentUri = parentCreateResponse.ResponseMessage.Headers.Location.AbsoluteUri;

                        var studentParentResponse = StudentHelper.CreateStudentParentAssociation(client, studentCreateResponse.UniqueId, parentCreateResponse.UniqueId);
                        studentParentResponse.EnsureSuccessStatusCode();
                    }
                }
            }

            localEducationAgencyIds = new List<int> { 1 };
            using (var startup = new OwinStartup(DatabaseName, localEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = _15_Minutes;
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var deleteResponse = client.DeleteAsync(parentUri).Result;
                        deleteResponse.StatusCode.ShouldEqual(HttpStatusCode.Conflict);
                    }
                }
            }
        }
    }

    public class ParentAuthorizationGetTests : OwinTestBase
    {
        const int StateEducationAgencyId = 31;
        const int Lea1Id = 2001;
        const int Lea2Id = 2002;
        const int Lea3Id = 2003;
        const int School1Id = 200101;
        const int School2Id = 200201;
        const int School3Id = 200301;
        static readonly TimeSpan _15_Minutes = new TimeSpan(0, 0, 15, 0);

        private static readonly string _databaseName = string.Format("EdFi_Tests_ParentAuthorizationTests_{0}", Guid.NewGuid().ToString("N"));
        
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
        public void When_getting_all_parents_should_return_only_authorized_parents()
        {
            var authorizedParentUniqueIds = new List<string>();

            var localEducationAgencyIds = new List<int> { Lea1Id, Lea2Id, Lea3Id };
            using (var startup = new OwinStartup(DatabaseName, localEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = _15_Minutes;
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        //1st Student && Parent
                        var studentCreateResponse = StudentHelper.CreateStudentAndAssociateToSchool(client, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), School1Id);
                        studentCreateResponse.ResponseMessage.EnsureSuccessStatusCode();

                        var parentCreateResponse = ParentHelper.CreateParent(client, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture));
                        parentCreateResponse.ResponseMessage.EnsureSuccessStatusCode();
                        authorizedParentUniqueIds.Add(parentCreateResponse.UniqueId);

                        var studentParentResponse = StudentHelper.CreateStudentParentAssociation(client, studentCreateResponse.UniqueId, parentCreateResponse.UniqueId);
                        studentParentResponse.EnsureSuccessStatusCode();

                        //2nd Student && Parent
                        studentCreateResponse = StudentHelper.CreateStudentAndAssociateToSchool(client, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), School2Id);
                        studentCreateResponse.ResponseMessage.EnsureSuccessStatusCode();


                        parentCreateResponse = ParentHelper.CreateParent(client, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture));
                        parentCreateResponse.ResponseMessage.EnsureSuccessStatusCode();
                        authorizedParentUniqueIds.Add(parentCreateResponse.UniqueId);

                        studentParentResponse = StudentHelper.CreateStudentParentAssociation(client, studentCreateResponse.UniqueId, parentCreateResponse.UniqueId);
                        studentParentResponse.EnsureSuccessStatusCode();

                        //3rd Student && Parent
                        studentCreateResponse = StudentHelper.CreateStudentAndAssociateToSchool(client, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), School3Id);
                        studentCreateResponse.ResponseMessage.EnsureSuccessStatusCode();

                        parentCreateResponse = ParentHelper.CreateParent(client, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture));
                        parentCreateResponse.ResponseMessage.EnsureSuccessStatusCode();

                        studentParentResponse = StudentHelper.CreateStudentParentAssociation(client, studentCreateResponse.UniqueId, parentCreateResponse.UniqueId);
                        studentParentResponse.EnsureSuccessStatusCode();
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

                        var getAllParents = client.GetAsync(OwinUriHelper.BuildApiUri("2014", "parents")).Result;
                        var parents = (Parent[]) JsonConvert.DeserializeObject(getAllParents.Content.ReadAsStringAsync().Result, typeof (Parent[]));
                        parents.Length.ShouldEqual(2);
                        parents.Select(x => x.ParentUniqueId).Intersect(authorizedParentUniqueIds).Count().ShouldEqual(2);
                    }
                }
            }
        }

        [Test]
        public void When_getting_parent_by_id_should_return_200_when_authorized()
        {
            string unauthorizedParentUri;
            string authorizedParentUri;

            var localEducationAgencyIds = new List<int> { Lea1Id, Lea2Id };
            using (var startup = new OwinStartup(DatabaseName, localEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = _15_Minutes;
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        //1st Student && Parent
                        var studentCreateResponse = StudentHelper.CreateStudentAndAssociateToSchool(client, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), School1Id);
                        studentCreateResponse.ResponseMessage.EnsureSuccessStatusCode();

                        var parentCreateResponse = ParentHelper.CreateParent(client, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture));
                        parentCreateResponse.ResponseMessage.EnsureSuccessStatusCode();
                        authorizedParentUri = parentCreateResponse.ResponseMessage.Headers.Location.AbsoluteUri;

                        var studentParentResponse = StudentHelper.CreateStudentParentAssociation(client, studentCreateResponse.UniqueId, parentCreateResponse.UniqueId);
                        studentParentResponse.EnsureSuccessStatusCode();

                        //2nd Student && Parent
                        studentCreateResponse = StudentHelper.CreateStudentAndAssociateToSchool(client, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), School2Id);
                        studentCreateResponse.ResponseMessage.EnsureSuccessStatusCode();

                        parentCreateResponse = ParentHelper.CreateParent(client, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture));
                        parentCreateResponse.ResponseMessage.EnsureSuccessStatusCode();
                        unauthorizedParentUri = parentCreateResponse.ResponseMessage.Headers.Location.AbsoluteUri;

                        studentParentResponse = StudentHelper.CreateStudentParentAssociation(client, studentCreateResponse.UniqueId, parentCreateResponse.UniqueId);
                        studentParentResponse.EnsureSuccessStatusCode();
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

                        var authorizedParentResult = client.GetAsync(authorizedParentUri).Result;
                        authorizedParentResult.EnsureSuccessStatusCode();
                        authorizedParentResult.StatusCode.ShouldEqual(HttpStatusCode.OK);

                        var unauthorizedParentResult = client.GetAsync(unauthorizedParentUri).Result;
                        unauthorizedParentResult.StatusCode.ShouldEqual(HttpStatusCode.Forbidden);
                    }
                }
            }
        }

        [Test]
        public void When_getting_parent_by_key_should_return_200_when_authorized()
        {
            string authorizedParentUniqueId;
            string unauthorizedParentUniqueId;

            var localEducationAgencyIds = new List<int> { Lea1Id, Lea2Id };
            using (var startup = new OwinStartup(DatabaseName, localEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = _15_Minutes;
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        //1st Student && Parent
                        var studentCreateResponse = StudentHelper.CreateStudentAndAssociateToSchool(client, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), School1Id);
                        studentCreateResponse.ResponseMessage.EnsureSuccessStatusCode();

                        var parentCreateResponse = ParentHelper.CreateParent(client, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture));
                        parentCreateResponse.ResponseMessage.EnsureSuccessStatusCode();
                        authorizedParentUniqueId = parentCreateResponse.UniqueId;

                        var studentParentResponse = StudentHelper.CreateStudentParentAssociation(client, studentCreateResponse.UniqueId, parentCreateResponse.UniqueId);
                        studentParentResponse.EnsureSuccessStatusCode();

                        //2nd Student && Parent
                        studentCreateResponse = StudentHelper.CreateStudentAndAssociateToSchool(client, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), School2Id);
                        studentCreateResponse.ResponseMessage.EnsureSuccessStatusCode();

                        parentCreateResponse = ParentHelper.CreateParent(client, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture));
                        parentCreateResponse.ResponseMessage.EnsureSuccessStatusCode();
                        unauthorizedParentUniqueId = parentCreateResponse.UniqueId;

                        studentParentResponse = StudentHelper.CreateStudentParentAssociation(client, studentCreateResponse.UniqueId, parentCreateResponse.UniqueId);
                        studentParentResponse.EnsureSuccessStatusCode();
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

                        var authorizedParentResult = client.GetAsync(OwinUriHelper.BuildApiUri("2014", "parents", string.Format("parentUniqueId={0}", authorizedParentUniqueId))).Result;
                        authorizedParentResult.EnsureSuccessStatusCode();
                        authorizedParentResult.StatusCode.ShouldEqual(HttpStatusCode.OK);

                        var unauthorizedParentResult = client.GetAsync(OwinUriHelper.BuildApiUri("2014", "parents", string.Format("parentUniqueId={0}", unauthorizedParentUniqueId))).Result;
                        unauthorizedParentResult.StatusCode.ShouldEqual(HttpStatusCode.Forbidden);
                    }
                }
            }
        }

        [Test]
        public void When_getting_parent_by_example_should_return_200_and_parent_when_authorized()
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

                        //1st Student && Parent
                        var studentCreateResponse = StudentHelper.CreateStudentAndAssociateToSchool(client, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), School1Id);
                        studentCreateResponse.ResponseMessage.EnsureSuccessStatusCode();

                        var parentCreateResponse = ParentHelper.CreateParent(client, authorizedLastName, authorizedFirstName);
                        parentCreateResponse.ResponseMessage.EnsureSuccessStatusCode();

                        var studentParentResponse = StudentHelper.CreateStudentParentAssociation(client, studentCreateResponse.UniqueId, parentCreateResponse.UniqueId);
                        studentParentResponse.EnsureSuccessStatusCode();

                        //2nd Student && Parent
                        studentCreateResponse = StudentHelper.CreateStudentAndAssociateToSchool(client, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture), School2Id);
                        studentCreateResponse.ResponseMessage.EnsureSuccessStatusCode();

                        parentCreateResponse = ParentHelper.CreateParent(client, unauthorizedLastName, unauthorizedFirstName);
                        parentCreateResponse.ResponseMessage.EnsureSuccessStatusCode();

                        studentParentResponse = StudentHelper.CreateStudentParentAssociation(client, studentCreateResponse.UniqueId, parentCreateResponse.UniqueId);
                        studentParentResponse.EnsureSuccessStatusCode();
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

                        var authorizedParentResult = client.GetAsync(OwinUriHelper.BuildApiUri("2014", "parents", string.Format("LastSurname={0}", authorizedLastName))).Result;
                        authorizedParentResult.EnsureSuccessStatusCode();
                        authorizedParentResult.StatusCode.ShouldEqual(HttpStatusCode.OK);

                        var parents = JsonConvert.DeserializeObject<Parent[]>(authorizedParentResult.Content.ReadAsStringAsync().Result);
                        parents.Length.ShouldEqual(1);
                        parents[0].FirstName.ShouldEqual(authorizedFirstName);

                        var unauthorizedParentResult = client.GetAsync(OwinUriHelper.BuildApiUri("2014", "parents", string.Format("LastSurname={0}", unauthorizedLastName))).Result;
                        unauthorizedParentResult.StatusCode.ShouldEqual(HttpStatusCode.OK);
                        parents = JsonConvert.DeserializeObject<Parent[]>(unauthorizedParentResult.Content.ReadAsStringAsync().Result);
                        parents.Length.ShouldEqual(0);
                    }
                }
            }
        }
    }
}
