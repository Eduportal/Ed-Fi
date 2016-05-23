using EdFi.Ods.Api.Models.Resources.AcademicSubjectDescriptor;
using EdFi.Ods.Api.Models.Resources.Assessment;
using EdFi.Ods.Api.Models.Resources.AssessmentFamily;
using EdFi.Ods.Api.Models.Resources.AssessmentItem;
using EdFi.Ods.Api.Models.Resources.GradebookEntry;
using EdFi.Ods.Api.Models.Resources.ObjectiveAssessment;
using EdFi.Ods.Api.Models.Resources.StudentAssessment;
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

namespace EdFi.Ods.WebService.Tests.AuthorizationTests
{
    /// <summary>
    /// These tests are really just testing that we are able to successfully set up and add the nHibernate filters 
    /// to the below objects and that the sql syntax on the filters works, there may be a better way to test this in the future
    /// </summary>
    [TestFixture]
    public class GetAllAuthorizationTests : OwinTestBase
    {
        private static readonly string _databaseName = string.Format("EdFi_Tests_GetAllAuthorizationTests_{0}", Guid.NewGuid().ToString("N"));
        private readonly List<int> LocalEducationAgencyIds = new List<int> { 255901 };

        protected override string DatabaseName { get { return _databaseName; } }
        protected override string BaseDatabase { get { return "EdFi_Ods_Populated_Template"; } }

        private const string TestNamespace = "http://www.TEST.org/";
        
        [Test]
        public void GetAll_Descriptors_ShouldUseNHibernateFilters()
        {
            using (var startup = new OwinStartup(DatabaseName, LocalEducationAgencyIds, TestNamespace))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = new TimeSpan(0, 0, 15, 0);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var uri = OwinUriHelper.BuildApiUri("2014", "academicsubjectdescriptors");
                        var something = client.GetStringAsync(uri).Result;
                        var results = (Object[]) JsonConvert.DeserializeObject(something, typeof (AcademicSubjectDescriptor[]));
                        results.Count().ShouldBeInRange(0, 25);
                    }
                }
            }
        }

        [Test]
        public void GetAll_AssessmentFamily_ShouldUseNHibernateFilters()
        {
            using (var startup = new OwinStartup(DatabaseName, LocalEducationAgencyIds, TestNamespace))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = new TimeSpan(0, 0, 15, 0);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var uri = OwinUriHelper.BuildApiUri("2014", "assessmentfamilies");
                        var something = client.GetStringAsync(uri).Result;
                        var results = (Object[]) JsonConvert.DeserializeObject(something, typeof (AssessmentFamily[]));

                        results.Count().ShouldBeInRange(0, 25);
                    }
                }
            }
        }

        [Test]
        public void GetAll_Assessment_ShouldUseNHibernateFilters()
        {
            using (var startup = new OwinStartup(DatabaseName, LocalEducationAgencyIds, TestNamespace))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = new TimeSpan(0, 0, 15, 0);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var uri = OwinUriHelper.BuildApiUri("2014", "assessments");
                        var something = client.GetStringAsync(uri).Result;
                        var results = (Object[]) JsonConvert.DeserializeObject(something, typeof (Assessment[]));

                        results.Count().ShouldBeInRange(0, 25);
                    }
                }
            }
        }

        [Test]
        public void GetAll_AssessmentItem_ShouldUseNHibernateFilters()
        {
            using (var startup = new OwinStartup(DatabaseName, LocalEducationAgencyIds, TestNamespace))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = new TimeSpan(0, 0, 15, 0);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var uri = OwinUriHelper.BuildApiUri("2014", "assessmentitems");
                        var something = client.GetStringAsync(uri).Result;
                        var results = (Object[]) JsonConvert.DeserializeObject(something, typeof (AssessmentItem[]));

                        results.Count().ShouldBeInRange(0, 25);
                    }
                }
            }
        }

        [Test]
        public void GetAll_ObjectiveAssessment_ShouldUseNHibernateFilters()
        {
            using (var startup = new OwinStartup(DatabaseName, LocalEducationAgencyIds, TestNamespace))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = new TimeSpan(0, 0, 15, 0);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var uri = OwinUriHelper.BuildApiUri("2014", "objectiveassessments");
                        var something = client.GetStringAsync(uri).Result;
                        var results = (Object[]) JsonConvert.DeserializeObject(something, typeof (ObjectiveAssessment[]));

                        results.Count().ShouldBeInRange(0, 25);
                    }
                }
            }
        }

        [Test]
        public void GetAll_StudentAssessment_ShouldUseNHibernateFilters()
        {
            using (var startup = new OwinStartup(DatabaseName, LocalEducationAgencyIds, TestNamespace))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = new TimeSpan(0, 0, 15, 0);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var uri = OwinUriHelper.BuildApiUri("2014", "studentassessments");
                        var something = client.GetStringAsync(uri).Result;
                        var results = (Object[]) JsonConvert.DeserializeObject(something, typeof (StudentAssessment[]));

                        results.Count().ShouldBeInRange(0, 25);
                    }
                }
            }
        }

        [Test]
        public void GetAll_GradebookEntry_ShouldUseNHibernateFilters()
        {
            using (var startup = new OwinStartup(DatabaseName, LocalEducationAgencyIds, TestNamespace))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = new TimeSpan(0, 0, 15, 0);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var uri = OwinUriHelper.BuildApiUri("2014", "gradebookentries");
                        var something = client.GetStringAsync(uri).Result;
                        var results = (Object[])JsonConvert.DeserializeObject(something, typeof(GradebookEntry[]));

                        results.Count().ShouldBeInRange(0, 25);
                    }
                }
            }
        }

    }
}
