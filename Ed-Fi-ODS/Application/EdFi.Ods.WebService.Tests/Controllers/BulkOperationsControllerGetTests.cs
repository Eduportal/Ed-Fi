using EdFi.Ods.Api.Data.Repositories.BulkOperations;
using EdFi.Ods.Api.Models.Resources;
using EdFi.Ods.Api.Models.Resources.Enums;
using EdFi.Ods.Common;
using EdFi.Ods.Common.Utils.Extensions;
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

namespace EdFi.Ods.WebService.Tests.Controllers
{
    [TestFixture]
    public class BulkOperationsControllerGetTests : OwinTestBase
    {
        private static readonly string _databaseName = string.Format("BulkOperationsControllerPostTests_{0}", Guid.NewGuid().ToString("N"));
        private readonly List<int> LocalEducationAgencyIds = new List<int> { 255901 };

        protected override string DatabaseName { get { return _databaseName; } }
        protected override string BaseDatabase { get { return "EdFi_Ods_Populated_Template"; } }

        [Test]
        public void When_calling_get_by_id_with_non_matching_year_Should_return_404_not_found()
        {
            using (var startup = new OwinStartup(DatabaseName, LocalEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = new TimeSpan(0, 0, 15, 0);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var request = new BulkOperationCreateRequest {ResetDistrictData = "Yes, please!", UploadFiles = new[] {new UploadFileRequest {Format = "text/xml", InterchangeType = InterchangeType.EducationOrganization.Name,},}};
                        var postResponse = client.PostAsync(OwinUriHelper.BuildApiUri("2014", "BulkOperations"), new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json")).Result;
                        postResponse.EnsureSuccessStatusCode();
                        postResponse.StatusCode.ShouldEqual(HttpStatusCode.Created);

                        var content = postResponse.Content.ReadAsAsync<BulkOperationResource>().Result;

                        var getResponse = client.GetAsync(OwinUriHelper.BuildApiUri("2015", "BulkOperations" + "/" + content.Id)).Result;
                        getResponse.StatusCode.ShouldEqual(HttpStatusCode.NotFound);
                    }
                }
            }
        }

        [Test]
        public void When_calling_get_by_id_on_bulk_operation_api_after_creating_resource_Should_return_a_bulk_operation_resource()
        {
            using (var startup = new OwinStartup(DatabaseName, LocalEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = new TimeSpan(0, 0, 15, 0);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var request = new BulkOperationCreateRequest {ResetDistrictData = "Yes, please!", UploadFiles = new[] {new UploadFileRequest {Format = "text/xml", InterchangeType = InterchangeType.EducationOrganization.Name,},}};
                        var postResponse = client.PostAsync(OwinUriHelper.BuildApiUri("2014", "BulkOperations"), new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json")).Result;
                        postResponse.EnsureSuccessStatusCode();
                        postResponse.StatusCode.ShouldEqual(HttpStatusCode.Created);

                        var getResponse = client.GetAsync(postResponse.Headers.Location.AbsoluteUri).Result;
                        getResponse.EnsureSuccessStatusCode();

                        var returnedResource = getResponse.Content.ReadAsAsync<BulkOperationResource>().Result;
                        returnedResource.ShouldNotBeNull();

                        returnedResource.ResetDistrictData.ShouldEqual(request.ResetDistrictData);
                        returnedResource.UploadFiles.Length.ShouldEqual(1);

                        var actualFile = returnedResource.UploadFiles.First();
                        var expectedFile = request.UploadFiles.First();

                        actualFile.Format.ShouldEqual(expectedFile.Format);
                        actualFile.InterchangeType.ShouldEqual(expectedFile.InterchangeType);

                        returnedResource.UploadFiles.First().Status.ShouldEqual(UploadFileStatus.Initialized.GetName());
                    }
                }
            }
        }

        [Test]
        public void When_getting_bulk_operations_exceptions_with_limit_greater_than_100_Should_return_a_400_bad_request()
        {
            using (var startup = new OwinStartup(DatabaseName, LocalEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = new TimeSpan(0, 0, 15, 0);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        const string bogusOpId = "4493bf89-77df-43e6-ac3b-f416c70a1427";
                        const string bogusUploadId = "0f893b90-ee94-4f07-b2f0-ebe20c20b94f";

                        var url = string.Format("{0}/{1}/Exceptions/{2}?offset=0&limit=2000", OwinUriHelper.BuildApiUri("2014", "BulkOperations"), bogusOpId, bogusUploadId);
                        var getResponse = client.GetAsync(url).Result;

                        getResponse.StatusCode.ShouldEqual(HttpStatusCode.BadRequest);
                    }
                }
            }
        }
    }
}