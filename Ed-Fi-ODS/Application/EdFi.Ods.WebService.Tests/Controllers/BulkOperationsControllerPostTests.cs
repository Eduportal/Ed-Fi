using EdFi.Ods.Api.Data.Repositories.BulkOperations;
using EdFi.Ods.Api.Models.Resources;
using EdFi.Ods.Common;
using EdFi.Ods.WebService.Tests.Extensions;
using EdFi.Ods.WebService.Tests.Owin;
using Microsoft.Owin.Testing;
using Newtonsoft.Json;
using NUnit.Framework;
using Should;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace EdFi.Ods.WebService.Tests.Controllers
{
    [TestFixture]
    public class BulkOperationsControllerPostTests : OwinTestBase
    {
        private static readonly string _databaseName = string.Format("BulkOperationsControllerPostTests_{0}", Guid.NewGuid().ToString("N"));
        private readonly List<int> LocalEducationAgencyIds = new List<int> { 255901 };

        protected override string DatabaseName { get { return _databaseName; } }
        protected override string BaseDatabase { get { return "EdFi_Ods_Populated_Template"; } }

        [Test]
        public void When_bulk_operation_api_recieves_a_post_request_should_return_status_code_201()
        {
            using (var startup = new OwinStartup(DatabaseName, LocalEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = new TimeSpan(0, 0, 15, 0);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var request = new BulkOperationCreateRequest {UploadFiles = new[] {new UploadFileRequest {Format = "text/xml", InterchangeType = InterchangeType.EducationOrganization.Name,},}};
                        var postResponse = client.PostAsync(OwinUriHelper.BuildApiUri("2014", "BulkOperations"), new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json")).Result;
                        postResponse.EnsureSuccessStatusCode();
                        postResponse.StatusCode.ShouldEqual(HttpStatusCode.Created);

                        var content = postResponse.Content.ReadAsAsync<BulkOperationResource>().Result;
                        content.ShouldNotBeNull();
                    }
                }
            }
        }

        [Test]
        public void When_bulk_operation_api_recieves_an_invalid_post_request_should_respond_with_400()
        {
            using (var startup = new OwinStartup(DatabaseName, LocalEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = new TimeSpan(0, 0, 15, 0);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var request = new BulkOperationCreateRequest();
                        var postResponse = client.PostAsync(OwinUriHelper.BuildApiUri("2014", "BulkOperations"), new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json")).Result;
                        postResponse.StatusCode.ShouldEqual(HttpStatusCode.BadRequest);
                    }
                }
            }
        }

        [Test]
        public void When_bulk_operation_receives_a_put_request_should_respond_with_method_not_allowed_405()
        {
            using (var startup = new OwinStartup(DatabaseName, LocalEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = new TimeSpan(0, 0, 15, 0);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var request = new BulkOperationCreateRequest();
                        var putResponse = client.PutAsync(OwinUriHelper.BuildApiUri("2014", "BulkOperations"), new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json")).Result;
                        putResponse.StatusCode.ShouldEqual(HttpStatusCode.MethodNotAllowed);
                    }
                }
            }
        }

        [Test]
        public void When_bulk_operation_receives_a_delete_request_should_respond_with_method_not_allowed_405()
        {
            using (var startup = new OwinStartup(DatabaseName, LocalEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = new TimeSpan(0, 0, 15, 0);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var deleteResponse = client.DeleteAsync(OwinUriHelper.BuildApiUri("2014", "BulkOperations")).Result;
                        deleteResponse.StatusCode.ShouldEqual(HttpStatusCode.MethodNotAllowed);
                    }
                }
            }
        }
    }
}