using EdFi.Ods.Api.Models.Resources.Student;
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
using System.Net.Http;
using System.Net.Http.Headers;

namespace EdFi.Ods.WebService.Tests.ResponseTests.Get
{
    [TestFixture]
    public class GetByExample_200_Tests : OwinTestBase
    {
        private readonly List<int> GrandBendLeaIds = new List<int> { 255901 };
        const int GrandBendSchoolId = 255901001;

        private static readonly string _databaseName = string.Format("GetByExample_200_Tests_{0}", Guid.NewGuid().ToString("N"));

        protected override string DatabaseName { get { return _databaseName; } }
        protected override string BaseDatabase { get { return "EdFi_Ods_Populated_Template"; } }
        
        [Test]
        public void When_Given_A_Valid_Example_Should_Return_All_Resources_For_That_Example()
        {
            using (var startup = new OwinStartup(DatabaseName, GrandBendLeaIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = new TimeSpan(0, 0, 15, 0);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var lastName = DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture);

                        StudentHelper.CreateStudentAndAssociateToSchool(client, lastName, "John", GrandBendSchoolId);
                        StudentHelper.CreateStudentAndAssociateToSchool(client, lastName, "Jane", GrandBendSchoolId);
                        StudentHelper.CreateStudentAndAssociateToSchool(client, "OtherName", "Jefe", GrandBendSchoolId);

                        var getByLastName = client.GetAsync(OwinUriHelper.BuildApiUri("2014", "students", string.Format("{0}={1}", "LastSurname", lastName))).Result;
                        var lastNameSearchResult = JsonConvert.DeserializeObject<Student[]>(getByLastName.Content.ReadAsStringAsync().Result);

                        lastNameSearchResult.Length.ShouldEqual(2);
                        lastNameSearchResult.Select(x => x.FirstName).ShouldContain("John");
                        lastNameSearchResult.Select(x => x.FirstName).ShouldContain("Jane");
                        lastNameSearchResult.Select(x => x.FirstName).ShouldNotContain("Jefe");
                    }
                }
            }
        }

        [Test]
        public void When_Given_A_Valid_Example_With_Multiple_Properties_Should_Return_All_Resources_For_That_Example()
        {
            using (var startup = new OwinStartup(DatabaseName, GrandBendLeaIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = new TimeSpan(0, 0, 15, 0);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var lastName = DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture);

                        StudentHelper.CreateStudentAndAssociateToSchool(client, lastName, "John", GrandBendSchoolId);
                        StudentHelper.CreateStudentAndAssociateToSchool(client, lastName, "Jane", GrandBendSchoolId);
                        StudentHelper.CreateStudentAndAssociateToSchool(client, "OtherName", "Jefe", GrandBendSchoolId);

                        var getByFirstAndLastName = client.GetAsync(OwinUriHelper.BuildApiUri("2014", "students", string.Format("{0}={1}&{2}={3}", "LastSurname", lastName, "FirstName", "John"))).Result;
                        var firstAndLastNameSearchResult = JsonConvert.DeserializeObject<Student[]>(getByFirstAndLastName.Content.ReadAsStringAsync().Result);

                        firstAndLastNameSearchResult.Length.ShouldEqual(1);
                        firstAndLastNameSearchResult[0].FirstName.ShouldEqual("John");
                        firstAndLastNameSearchResult[0].LastSurname.ShouldEqual(lastName);
                    }
                }
            }
        }

        [Test]
        public void When_Given_A_Valid_Example_With_Non_Key_Properties_Should_Return_All_Resources_For_That_Example()
        {
            using (var startup = new OwinStartup(DatabaseName, GrandBendLeaIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = new TimeSpan(0, 0, 15, 0);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        const string sexTypeMale = "Male";

                        var getBySexTypeMale = client.GetAsync(OwinUriHelper.BuildApiUri("2014", "students", string.Format("limit=100&{0}={1}", "SexType", sexTypeMale))).Result;
                        var getBySexTypeMaleResult = JsonConvert.DeserializeObject<Student[]>(getBySexTypeMale.Content.ReadAsStringAsync().Result);

                        getBySexTypeMaleResult.All(gbstmr => gbstmr.SexType == sexTypeMale).ShouldBeTrue();
                    }
                }
            }
        }
        [Test]
        public void When_Given_An_Invalid_Example_Should_Return_No_Resources_For_That_Example()
        {
            using (var startup = new OwinStartup(DatabaseName, GrandBendLeaIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = new TimeSpan(0, 0, 15, 0);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var lastName = DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture);

                        StudentHelper.CreateStudentAndAssociateToSchool(client, lastName, "John", GrandBendSchoolId);
                        StudentHelper.CreateStudentAndAssociateToSchool(client, lastName, "Jane", GrandBendSchoolId);
                        StudentHelper.CreateStudentAndAssociateToSchool(client, "OtherName", "Jefe", GrandBendSchoolId);

                        var getByLastName = client.GetAsync(OwinUriHelper.BuildApiUri("2014", "students", string.Format("{0}={1}", "LastSurname", "kj34lk3j4lk3j4"))).Result;
                        var lastNameSearchResult = JsonConvert.DeserializeObject<Student[]>(getByLastName.Content.ReadAsStringAsync().Result);

                        lastNameSearchResult.Length.ShouldEqual(0);
                    }
                }
            }
        }

        [Test]
        public void When_Limit_Value_Supplied_Should_Not_Return_more_than_that_number()
        {
            using (var startup = new OwinStartup(DatabaseName, GrandBendLeaIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = new TimeSpan(0, 0, 15, 0);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var lastName = DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture);

                        StudentHelper.CreateStudentAndAssociateToSchool(client, lastName, "John", GrandBendSchoolId);
                        StudentHelper.CreateStudentAndAssociateToSchool(client, lastName, "Jane", GrandBendSchoolId);
                        StudentHelper.CreateStudentAndAssociateToSchool(client, "OtherName", "Jefe", GrandBendSchoolId);

                        var getByLastName = client.GetAsync(OwinUriHelper.BuildApiUri("2014", "students", string.Format("limit=1&{0}={1}", "LastSurname", lastName))).Result;
                        var results = JsonConvert.DeserializeObject<Student[]>(getByLastName.Content.ReadAsStringAsync().Result);
                        results.Count().ShouldEqual(1);
                    }
                }
            }
        }

        [Test]
        public void When_Offset_Value_Supplied_Should_Return_Next_Set_of_Resources()
        {
            using (var startup = new OwinStartup(DatabaseName, GrandBendLeaIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = new TimeSpan(0, 0, 15, 0);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var lastName = DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture);

                        StudentHelper.CreateStudentAndAssociateToSchool(client, lastName, "John", GrandBendSchoolId);
                        StudentHelper.CreateStudentAndAssociateToSchool(client, lastName, "Jane", GrandBendSchoolId);
                        StudentHelper.CreateStudentAndAssociateToSchool(client, "OtherName", "Jefe", GrandBendSchoolId);

                        var response = client.GetAsync(OwinUriHelper.BuildApiUri("2014", "students", string.Format("limit=1&{0}={1}", "LastSurname", lastName))).Result;
                        var initialSet = JsonConvert.DeserializeObject<Student[]>(response.Content.ReadAsStringAsync().Result);
                        response = client.GetAsync(OwinUriHelper.BuildApiUri("2014", "students", string.Format("Offset=1&limit=1&{0}={1}", "LastSurname", lastName))).Result;
                        var results = JsonConvert.DeserializeObject<Student[]>(response.Content.ReadAsStringAsync().Result);
                        results.First().ShouldNotBeSameAs(initialSet.First());
                    }
                }
            }
        }
    }
}
