using EdFi.Ods.Api.Models.Resources.Student;
using EdFi.Ods.WebService.Tests._Helpers;
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
    public class StudentAuthorizationTests : OwinTestBase
    {
        const int StateEducationAgencyId = 31;
        const int Lea1Id = 2001;
        const int Lea3Id = 2003;
        const int School1Id = 200101;
        const int School2Id = 200102;
        const int School3Id = 200103;
        const int GrandBendSchoolId = 255901001;

        static readonly TimeSpan _15_Minutes = new TimeSpan(0, 0, 15, 0);

        private static readonly string _databaseName = string.Format("EdFi_Tests_StudentAuthorizationTests_{0}", Guid.NewGuid().ToString("N"));
        private readonly List<int> LocalEducationAgencyIds = new List<int> {Lea1Id};
        private readonly List<int> GrandBendLeaIds = new List<int> { 255901 };

        protected override string DatabaseName { get { return _databaseName; } }
        protected override string BaseDatabase { get { return "EdFi_Ods_Populated_Template"; } }

        public override void SetUp()
        {
            base.SetUp();

            var helper = new DataSeedHelper(DatabaseName);
            helper.CreateStateEducationAgency(StateEducationAgencyId);

            helper.CreateLocalEducationAgency(Lea1Id, StateEducationAgencyId);
        }

        [Test]
        public void When_creating_a_student_Should_return_201_created()
        {
            using (var startup = new OwinStartup(DatabaseName, LocalEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = _15_Minutes;
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var createResponse = StudentHelper.CreateStudent(client, DataSeedHelper.RandomName, DataSeedHelper.RandomName);
                        createResponse.ResponseMessage.EnsureSuccessStatusCode();
                        createResponse.ResponseMessage.StatusCode.ShouldEqual(HttpStatusCode.Created);
                    }
                }
            }
        }

        [Test]
        public void When_updating_an_unassociated_student_should_fail_with_403_forbidden()
        {
            string studentUri;
            Student student;

            using (var startup = new OwinStartup(DatabaseName, GrandBendLeaIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = _15_Minutes;
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var studentCreateResponse = StudentHelper.CreateStudentAndAssociateToSchool(client, DataSeedHelper.RandomName, DataSeedHelper.RandomName, GrandBendSchoolId);
                        studentCreateResponse.ResponseMessage.EnsureSuccessStatusCode();
                        studentUri = studentCreateResponse.ResponseMessage.Headers.Location.AbsoluteUri;

                        var getStudentResult = client.GetStringAsync(studentUri).Result;
                        student = JsonConvert.DeserializeObject<Student>(getStudentResult);
                    }
                }
            }

            var localEducationAgencyIds = new List<int> {Lea3Id};
            using (var startup = new OwinStartup(DatabaseName, localEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = _15_Minutes;
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        student.LastSurname = "GotMarried";

                        var putResponse = client.PutAsync(studentUri, new StringContent(JsonConvert.SerializeObject(student), Encoding.UTF8, "application/json")).Result;
                        putResponse.StatusCode.ShouldEqual(HttpStatusCode.Forbidden);
                    }
                }
            }
        }

        [Test]
        public void When_deleting_an_unused_student_should_return_204_nocontent()
        {
            using (var startup = new OwinStartup(DatabaseName, LocalEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = _15_Minutes;
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var createResponse = StudentHelper.CreateStudent(client, DataSeedHelper.RandomName, DataSeedHelper.RandomName);
                        createResponse.ResponseMessage.EnsureSuccessStatusCode();

                        var deleteResponse = client.DeleteAsync(createResponse.ResponseMessage.Headers.Location.AbsoluteUri).Result;
                        deleteResponse.StatusCode.ShouldEqual(HttpStatusCode.NoContent);
                    }
                }
            }
        }

        [Test]
        public void When_deleting_a_student_in_use_should_fail_with_409_Conflict()
        {
            using (var startup = new OwinStartup(DatabaseName, GrandBendLeaIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = _15_Minutes;
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var createResponse = StudentHelper.CreateStudentAndAssociateToSchool(client, DataSeedHelper.RandomName, DataSeedHelper.RandomName, GrandBendSchoolId);
                        createResponse.ResponseMessage.EnsureSuccessStatusCode();

                        var deleteResponse = client.DeleteAsync(createResponse.ResponseMessage.Headers.Location.AbsoluteUri).Result;
                        deleteResponse.StatusCode.ShouldEqual(HttpStatusCode.Conflict);
                    }
                }
            }
        }

        [Test]
        public void When_associating_a_student_not_related_to_vendors_leas_should_fail_with_403_forbidden()
        {
            using (var startup = new OwinStartup(DatabaseName, LocalEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = _15_Minutes;
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var studentCreateResponse = StudentHelper.CreateStudent(client, DataSeedHelper.RandomName, DataSeedHelper.RandomName);
                        var associateResponse =
                            client.PostAsync(OwinUriHelper.BuildApiUri("2014", "StudentSchoolAssociations"), new StringContent(ResourceHelper.CreateStudentSchoolAssociation(studentCreateResponse.UniqueId, 1), Encoding.UTF8, "application/json")).Result;
                        associateResponse.StatusCode.ShouldEqual(HttpStatusCode.Forbidden);
                    }
                }
            }
        }

        [Test]
        public void When_associating_a_student_related_to_vendors_leas_should_return_201_created()
        {
            using (var startup = new OwinStartup(DatabaseName, GrandBendLeaIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = _15_Minutes;
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var studentCreateResponse = StudentHelper.CreateStudent(client, DataSeedHelper.RandomName, DataSeedHelper.RandomName);
                        var associateResponse =
                            client.PostAsync(OwinUriHelper.BuildApiUri("2014", "StudentSchoolAssociations"), new StringContent(ResourceHelper.CreateStudentSchoolAssociation(studentCreateResponse.UniqueId, GrandBendSchoolId), Encoding.UTF8, "application/json")).Result;
                        associateResponse.StatusCode.ShouldEqual(HttpStatusCode.Created);
                    }
                }
            }
        }

        [Test]
        public void When_deleting_data_for_unrelated_student_should_fail_with_409_Conflict()
        {
            string studentUri;

            using (var startup = new OwinStartup(DatabaseName, GrandBendLeaIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = _15_Minutes;
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var studentCreateResponse = StudentHelper.CreateStudentAndAssociateToSchool(client, DataSeedHelper.RandomName, DataSeedHelper.RandomName, GrandBendSchoolId);
                        studentCreateResponse.ResponseMessage.EnsureSuccessStatusCode();

                        studentUri = studentCreateResponse.ResponseMessage.Headers.Location.AbsoluteUri;
                    }
                }
            }

            var localEducationAgencyIds = new List<int> {Lea3Id};
            using (var startup = new OwinStartup(DatabaseName, localEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = _15_Minutes;
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var deleteResponse = client.DeleteAsync(studentUri).Result;
                        deleteResponse.StatusCode.ShouldEqual(HttpStatusCode.Conflict);
                    }
                }
            }
        }
    }
    
    [TestFixture]
    public class StudentAuthorizationGetTests : OwinTestBase
    {
        const int StateEducationAgencyId = 31;
        const int Lea1Id = 2001;
        const int Lea2Id = 2002;
        const int Lea3Id = 2003;
        const int School1Id = 200101;
        const int School2Id = 200201;
        const int School3Id = 200301;
        const int StudentId1 = 41;
        const int StudentId2 = 42;
        const int StudentId3 = 43;
        static readonly TimeSpan _15_Minutes = new TimeSpan(0, 0, 15, 0);

        private static readonly string _databaseName = string.Format("EdFi_Tests_StudentAuthorizationTests_{0}", Guid.NewGuid().ToString("N"));
        
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
        public void When_getting_student_by_id_should_return_200_when_authorized()
        {
            string authorizedStudentUri;
            string unauthorizedStudentUri;

            var localEducationAgencyIds = new List<int> { Lea1Id, Lea2Id };
            using (var startup = new OwinStartup(DatabaseName, localEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = _15_Minutes;
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        //1st Student
                        var studentCreateResponse = StudentHelper.CreateStudentAndAssociateToSchool(client, DataSeedHelper.RandomName, DataSeedHelper.RandomName, School1Id);
                        studentCreateResponse.ResponseMessage.EnsureSuccessStatusCode();
                        authorizedStudentUri = studentCreateResponse.ResponseMessage.Headers.Location.AbsoluteUri;

                        //2nd Student
                        studentCreateResponse = StudentHelper.CreateStudentAndAssociateToSchool(client, DataSeedHelper.RandomName, DataSeedHelper.RandomName, School2Id);
                        studentCreateResponse.ResponseMessage.EnsureSuccessStatusCode();
                        unauthorizedStudentUri = studentCreateResponse.ResponseMessage.Headers.Location.AbsoluteUri;
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

                        var authorizedResult = client.GetAsync(authorizedStudentUri).Result;
                        authorizedResult.EnsureSuccessStatusCode();
                        authorizedResult.StatusCode.ShouldEqual(HttpStatusCode.OK);

                        var unauthorizedResult = client.GetAsync(unauthorizedStudentUri).Result;
                        unauthorizedResult.StatusCode.ShouldEqual(HttpStatusCode.Forbidden);
                    }
                }
            }
        }

        [Test]
        public void When_getting_student_by_key_should_return_200_for_authorized_resource()
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

                        //1st Student
                        var studentCreateResponse = StudentHelper.CreateStudentAndAssociateToSchool(client, DataSeedHelper.RandomName, DataSeedHelper.RandomName, School1Id);
                        studentCreateResponse.ResponseMessage.EnsureSuccessStatusCode();
                        authorizedUniqueId = studentCreateResponse.UniqueId;

                        //2nd Student
                        studentCreateResponse = StudentHelper.CreateStudentAndAssociateToSchool(client, DataSeedHelper.RandomName, DataSeedHelper.RandomName, School2Id);
                        studentCreateResponse.ResponseMessage.EnsureSuccessStatusCode();
                        unauthorizedUniqueId = studentCreateResponse.UniqueId;
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

                        var authorizedParentResult = client.GetAsync(OwinUriHelper.BuildApiUri("2014", "students", string.Format("studentUniqueId={0}", authorizedUniqueId))).Result;
                        authorizedParentResult.EnsureSuccessStatusCode();
                        authorizedParentResult.StatusCode.ShouldEqual(HttpStatusCode.OK);

                        var unauthorizedParentResult = client.GetAsync(OwinUriHelper.BuildApiUri("2014", "students", string.Format("studentUniqueId={0}", unauthorizedUniqueId))).Result;
                        unauthorizedParentResult.StatusCode.ShouldEqual(HttpStatusCode.Forbidden);
                    }
                }
            }
        }

        [Test]
        public void When_getting_student_by_example_should_return_200_and_student_when_authorized()
        {
            var authorizedFirstName = "John";
            var authorizedLastName = string.Format("A{0}", DataSeedHelper.RandomName);

            var unauthorizedFirstName = "Other";
            var unauthorizedLastName = string.Format("U{0}", DataSeedHelper.RandomName);

            var localEducationAgencyIds = new List<int> {Lea1Id, Lea2Id};
            using (var startup = new OwinStartup(DatabaseName, localEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = _15_Minutes;
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        //1st Student
                        var studentCreateResponse = StudentHelper.CreateStudentAndAssociateToSchool(client, authorizedLastName, authorizedFirstName, School1Id);
                        studentCreateResponse.ResponseMessage.EnsureSuccessStatusCode();

                        //2nd Student
                        studentCreateResponse = StudentHelper.CreateStudentAndAssociateToSchool(client, unauthorizedLastName, unauthorizedFirstName, School2Id);
                        studentCreateResponse.ResponseMessage.EnsureSuccessStatusCode();
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

                        var authorizedResult = client.GetAsync(OwinUriHelper.BuildApiUri("2014", "students", string.Format("LastSurname={0}", authorizedLastName))).Result;
                        authorizedResult.EnsureSuccessStatusCode();
                        authorizedResult.StatusCode.ShouldEqual(HttpStatusCode.OK);

                        var students = JsonConvert.DeserializeObject<Student[]>(authorizedResult.Content.ReadAsStringAsync().Result);
                        students.Length.ShouldEqual(1);
                        students[0].FirstName.ShouldEqual(authorizedFirstName);

                        var unauthorizedResult = client.GetAsync(OwinUriHelper.BuildApiUri("2014", "students", string.Format("LastSurname={0}", unauthorizedLastName))).Result;
                        unauthorizedResult.StatusCode.ShouldEqual(HttpStatusCode.OK);
                        students = JsonConvert.DeserializeObject<Student[]>(unauthorizedResult.Content.ReadAsStringAsync().Result);
                        students.Length.ShouldEqual(0);
                    }
                }
            }
        }

        [Test]
        public void When_getting_all_students_should_return_only_authorized_students()
        {
            var authorizedStudentUniqueIds = new List<string>();

            var localEducationAgencyIds = new List<int> { Lea1Id, Lea2Id, Lea3Id };
            using (var startup = new OwinStartup(DatabaseName, localEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = _15_Minutes;
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        //1st Student
                        var studentCreateResponse = StudentHelper.CreateStudentAndAssociateToSchool(client, DataSeedHelper.RandomName, DataSeedHelper.RandomName, School1Id);
                        studentCreateResponse.ResponseMessage.EnsureSuccessStatusCode();
                        authorizedStudentUniqueIds.Add(studentCreateResponse.UniqueId);

                        //2nd Student
                        studentCreateResponse = StudentHelper.CreateStudentAndAssociateToSchool(client, DataSeedHelper.RandomName, DataSeedHelper.RandomName, School2Id);
                        studentCreateResponse.ResponseMessage.EnsureSuccessStatusCode();
                        authorizedStudentUniqueIds.Add(studentCreateResponse.UniqueId);

                        //3rd Student
                        studentCreateResponse = StudentHelper.CreateStudentAndAssociateToSchool(client, DataSeedHelper.RandomName, DataSeedHelper.RandomName, School3Id);
                        studentCreateResponse.ResponseMessage.EnsureSuccessStatusCode();
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

                        var getAllParents = client.GetAsync(OwinUriHelper.BuildApiUri("2014", "students")).Result;
                        var students = (Student[]) JsonConvert.DeserializeObject(getAllParents.Content.ReadAsStringAsync().Result, typeof (Student[]));
                        students.Length.ShouldEqual(2);
                        students.Select(x => x.StudentUniqueId).Intersect(authorizedStudentUniqueIds).Count().ShouldEqual(2);
                    }
                }
            }
        }
    }
}
