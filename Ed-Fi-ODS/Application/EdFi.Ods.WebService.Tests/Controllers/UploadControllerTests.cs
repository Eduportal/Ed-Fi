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
    public class UploadResponse
    {
        public string Message { get; set; }
    }
    
    [TestFixture]
    public class When_uploading_a_file_chunk : OwinTestBase
    {
        private static readonly string _databaseName = string.Format("EdFi_Tests_When_uploading_a_file_chunk_{0}", Guid.NewGuid().ToString("N"));
        private readonly List<int> LocalEducationAgencyIds = new List<int> { 255901 };

        protected override string DatabaseName { get { return _databaseName; } }
        protected override string BaseDatabase { get { return "EdFi_Ods_Populated_Template"; } }
        
        [Test]
        public void Should_fail_if_upload_file_guid_is_not_in_the_database()
        {
            //initialize
            const int suppliedOffset = 0;
            const string textContent = "This is supposed to be the file";
            var suppliedChunkSize = textContent.Length;
            
            using (var startup = new OwinStartup(DatabaseName, LocalEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = new TimeSpan(0, 0, 15, 0);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var request = new BulkOperationCreateRequest {UploadFiles = new[] {new UploadFileRequest {Format = "text/xml", InterchangeType = InterchangeType.EducationOrganization.Name, Size = 1060},}};
                        var postResponse = client.PostAsync(OwinUriHelper.BuildApiUri("2014", "BulkOperations"), new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json")).Result;
                        postResponse.EnsureSuccessStatusCode();
                        postResponse.StatusCode.ShouldEqual(HttpStatusCode.Created);

                        var bulkContent = postResponse.Content.ReadAsAsync<BulkOperationResource>().Result;
                        var suppliedUploadId = new Guid().ToString();

                        //upload Chunk
                        var url = OwinUriHelper.CreateChunksUri(suppliedUploadId, suppliedOffset, suppliedChunkSize);

                        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(textContent));
                        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

                        var content = new MultipartFormDataContent {{fileContent, "File"}};

                        var uploadResponse = client.PostAsync(url, content).Result;
                        uploadResponse.StatusCode.ShouldEqual(HttpStatusCode.BadRequest);

                        var chunkResponse = (UploadResponse) JsonConvert.DeserializeObject(uploadResponse.Content.ReadAsStringAsync().Result, typeof (UploadResponse));
                        chunkResponse.Message.ShouldEqual(String.Format("An upload file with id {0} could not be found and has either expired or has been processed.", suppliedUploadId));
                    }
                }
            }
        }
        
        [Test]
        public void Should_fail_if_size_sent_plus_offset_is_greater_than_full_file_size()
        {
            //initialize
            const int suppliedOffset = 2000;
            const string textContent = "This is supposed to be the file";
            var suppliedChunkSize = textContent.Length;
            
            using (var startup = new OwinStartup(DatabaseName, LocalEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = new TimeSpan(0, 0, 15, 0);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var request = new BulkOperationCreateRequest {UploadFiles = new[] {new UploadFileRequest {Format = "text/xml", InterchangeType = InterchangeType.EducationOrganization.Name, Size = 1060},}};
                        var postResponse = client.PostAsync(OwinUriHelper.BuildApiUri("2014", "BulkOperations"), new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json")).Result;
                        postResponse.EnsureSuccessStatusCode();
                        postResponse.StatusCode.ShouldEqual(HttpStatusCode.Created);

                        var bulkContent = postResponse.Content.ReadAsAsync<BulkOperationResource>().Result;
                        var suppliedUploadId = bulkContent.UploadFiles[0].Id;

                        //upload Chunk
                        var url = OwinUriHelper.CreateChunksUri(suppliedUploadId, suppliedOffset, suppliedChunkSize);

                        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(textContent));
                        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

                        var content = new MultipartFormDataContent {{fileContent, "File"}};

                        var uploadResponse = client.PostAsync(url, content).Result;
                        uploadResponse.StatusCode.ShouldEqual(HttpStatusCode.BadRequest);

                        var chunkResponse = (UploadResponse) JsonConvert.DeserializeObject(uploadResponse.Content.ReadAsStringAsync().Result, typeof (UploadResponse));
                        chunkResponse.Message.ShouldEqual(String.Format("The offset + size exceeds the expected total size of the file.  {0} + {1} > 1060", suppliedOffset, suppliedChunkSize));
                    }
                }
            }
        }

        [Test]
        public void Should_fail_when_size_sent_does_not_match_chunk_size()
        {
            //initialize
            const int suppliedOffset = 0;
            const string textContent = "This is supposed to be the file";
            var suppliedChunkSize = textContent.Length + 10;

            using (var startup = new OwinStartup(DatabaseName, LocalEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = new TimeSpan(0, 0, 15, 0);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var request = new BulkOperationCreateRequest {UploadFiles = new[] {new UploadFileRequest {Format = "text/xml", InterchangeType = InterchangeType.EducationOrganization.Name, Size = 1060},}};
                        var postResponse = client.PostAsync(OwinUriHelper.BuildApiUri("2014", "BulkOperations"), new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json")).Result;
                        postResponse.EnsureSuccessStatusCode();
                        postResponse.StatusCode.ShouldEqual(HttpStatusCode.Created);

                        var bulkContent = postResponse.Content.ReadAsAsync<BulkOperationResource>().Result;
                        var suppliedUploadId = bulkContent.UploadFiles[0].Id;

                        //upload Chunk
                        var url = OwinUriHelper.CreateChunksUri(suppliedUploadId, suppliedOffset, suppliedChunkSize);

                        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(textContent));
                        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

                        var content = new MultipartFormDataContent {{fileContent, "File"}};

                        var uploadResponse = client.PostAsync(url, content).Result;
                        uploadResponse.StatusCode.ShouldEqual(HttpStatusCode.BadRequest);

                        var chunkResponse = (UploadResponse) JsonConvert.DeserializeObject(uploadResponse.Content.ReadAsStringAsync().Result, typeof (UploadResponse));
                        chunkResponse.Message.ShouldEqual("Uploaded chunk does not match file size indicated.");
                    }
                }
            }
        }

        [Test]
        public void Should_return_201_when_file_size_matches_size_sent()
        {
            //initialize
            const int suppliedOffset = 0;
            const string textContent = "This is supposed to be the file";
            var suppliedChunkSize = textContent.Length;

            using (var startup = new OwinStartup(DatabaseName, LocalEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = new TimeSpan(0, 0, 15, 0);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var request = new BulkOperationCreateRequest {UploadFiles = new[] {new UploadFileRequest {Format = "text/xml", InterchangeType = InterchangeType.EducationOrganization.Name, Size = 1060},}};
                        var postResponse = client.PostAsync(OwinUriHelper.BuildApiUri("2014", "BulkOperations"), new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json")).Result;
                        postResponse.EnsureSuccessStatusCode();
                        postResponse.StatusCode.ShouldEqual(HttpStatusCode.Created);

                        var bulkContent = postResponse.Content.ReadAsAsync<BulkOperationResource>().Result;
                        var suppliedUploadId = bulkContent.UploadFiles[0].Id;

                        //upload Chunk
                        var url = OwinUriHelper.CreateChunksUri(suppliedUploadId, suppliedOffset, suppliedChunkSize);

                        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(textContent));
                        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

                        var content = new MultipartFormDataContent {{fileContent, "File"}};

                        var uploadResponse = client.PostAsync(url, content).Result;
                        uploadResponse.StatusCode.ShouldEqual(HttpStatusCode.Created);
                    }
                }
            }
        }

        [Test]
        public void Should_return_405_when_sending_delete_verb()
        {
            const int suppliedOffset = 256;
            const int suppliedChunkSize = 74;
            
            using (var startup = new OwinStartup(DatabaseName, LocalEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = new TimeSpan(0, 0, 15, 0);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var request = new BulkOperationCreateRequest {UploadFiles = new[] {new UploadFileRequest {Format = "text/xml", InterchangeType = InterchangeType.EducationOrganization.Name, Size = 1060},}};
                        var postResponse = client.PostAsync(OwinUriHelper.BuildApiUri("2014", "BulkOperations"), new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json")).Result;
                        postResponse.EnsureSuccessStatusCode();
                        postResponse.StatusCode.ShouldEqual(HttpStatusCode.Created);

                        var bulkContent = postResponse.Content.ReadAsAsync<BulkOperationResource>().Result;
                        var suppliedUploadId = bulkContent.UploadFiles[0].Id;

                        //upload Chunk
                        var url = OwinUriHelper.CreateChunksUri(suppliedUploadId, suppliedOffset, suppliedChunkSize);

                        var deleteResponse = client.DeleteAsync(url).Result;
                        deleteResponse.StatusCode.ShouldEqual(HttpStatusCode.MethodNotAllowed);
                    }
                }
            }
        }

        [Test]
        public void Should_return_405_when_sending_get_verb()
        {
            const int suppliedOffset = 256;
            const int suppliedChunkSize = 74;

            using (var startup = new OwinStartup(DatabaseName, LocalEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = new TimeSpan(0, 0, 15, 0);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var request = new BulkOperationCreateRequest {UploadFiles = new[] {new UploadFileRequest {Format = "text/xml", InterchangeType = InterchangeType.EducationOrganization.Name, Size = 1060},}};
                        var postResponse = client.PostAsync(OwinUriHelper.BuildApiUri("2014", "BulkOperations"), new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json")).Result;
                        postResponse.EnsureSuccessStatusCode();
                        postResponse.StatusCode.ShouldEqual(HttpStatusCode.Created);

                        var bulkContent = postResponse.Content.ReadAsAsync<BulkOperationResource>().Result;
                        var suppliedUploadId = bulkContent.UploadFiles[0].Id;

                        //upload Chunk
                        var url = OwinUriHelper.CreateChunksUri(suppliedUploadId, suppliedOffset, suppliedChunkSize);

                        var getResponse = client.GetAsync(url).Result;
                        getResponse.StatusCode.ShouldEqual(HttpStatusCode.MethodNotAllowed);
                    }
                }
            }
        }

        [Test]
        public void Should_return_405_when_sending_put_verb()
        {
            const int suppliedOffset = 256;
            const int suppliedChunkSize = 74;
            
            using (var startup = new OwinStartup(DatabaseName, LocalEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = new TimeSpan(0, 0, 15, 0);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var request = new BulkOperationCreateRequest {UploadFiles = new[] {new UploadFileRequest {Format = "text/xml", InterchangeType = InterchangeType.EducationOrganization.Name, Size = 1060},}};
                        var postResponse = client.PostAsync(OwinUriHelper.BuildApiUri("2014", "BulkOperations"), new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json")).Result;
                        postResponse.EnsureSuccessStatusCode();
                        postResponse.StatusCode.ShouldEqual(HttpStatusCode.Created);

                        var bulkContent = postResponse.Content.ReadAsAsync<BulkOperationResource>().Result;
                        var suppliedUploadId = bulkContent.UploadFiles[0].Id;

                        //upload Chunk
                        var url = OwinUriHelper.CreateChunksUri(suppliedUploadId, suppliedOffset, suppliedChunkSize);

                        var getResponse = client.PutAsync(url, new MultipartFormDataContent()).Result;
                        getResponse.StatusCode.ShouldEqual(HttpStatusCode.MethodNotAllowed);
                    }
                }
            }
        }
    }

    [TestFixture]
    public class When_posting_a_commit : OwinTestBase
    {
        private static readonly string _databaseName = string.Format("EdFi_Tests_When_posting_a_commit_{0}", Guid.NewGuid().ToString("N"));
        private readonly List<int> LocalEducationAgencyIds = new List<int> { 255901 };

        protected override string DatabaseName { get { return _databaseName; } }
        protected override string BaseDatabase { get { return "EdFi_Ods_Populated_Template"; } }
        
        [Test]
        public void Should_return_accepted_when_the_upload_id_is_in_the_database()
        {
            using (var startup = new OwinStartup(DatabaseName, LocalEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = new TimeSpan(0, 0, 15, 0);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var request = new BulkOperationCreateRequest {UploadFiles = new[] {new UploadFileRequest {Format = "text/xml", InterchangeType = InterchangeType.EducationOrganization.Name, Size = 1060},}};
                        var postResponse = client.PostAsync(OwinUriHelper.BuildApiUri("2014", "BulkOperations"), new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json")).Result;
                        postResponse.EnsureSuccessStatusCode();
                        postResponse.StatusCode.ShouldEqual(HttpStatusCode.Created);

                        var bulkContent = postResponse.Content.ReadAsAsync<BulkOperationResource>().Result;
                        var suppliedUploadId = bulkContent.UploadFiles[0].Id;

                        //upload Chunk
                        var url = OwinUriHelper.CreateCommitUri(suppliedUploadId);

                        var content = new MultipartFormDataContent();
                        content.Add(new StringContent(JsonConvert.SerializeObject(new {uploadId = suppliedUploadId}), Encoding.UTF8, "application/json"));

                        var commitResponse = client.PostAsync(url, content).Result;
                        commitResponse.StatusCode.ShouldEqual(HttpStatusCode.Accepted);
                    }
                }
            }
        }

        [Test]
        public void Should_return_bad_request_and_error_message_when_upload_id_is_not_in_the_database()
        {
            using (var startup = new OwinStartup(DatabaseName, LocalEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = new TimeSpan(0, 0, 15, 0);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var suppliedUploadId = new Guid().ToString();

                        //upload Chunk
                        var url = OwinUriHelper.CreateCommitUri(suppliedUploadId);

                        var content = new MultipartFormDataContent();
                        content.Add(new StringContent(JsonConvert.SerializeObject(new {uploadId = suppliedUploadId}), Encoding.UTF8, "application/json"));

                        var commitResponse = client.PostAsync(url, content).Result;
                        commitResponse.StatusCode.ShouldEqual(HttpStatusCode.BadRequest);

                        var result = (UploadResponse) JsonConvert.DeserializeObject(commitResponse.Content.ReadAsStringAsync().Result, typeof (UploadResponse));
                        result.Message.ShouldEqual(String.Format("Could not find an upload file with {0}.  The file has either expired or has completed processing.", suppliedUploadId));
                    }
                }
            }
        }

        [Test]
        public void Should_return_405_when_sending_delete_verb()
        {
            using (var startup = new OwinStartup(DatabaseName, LocalEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = new TimeSpan(0, 0, 15, 0);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var request = new BulkOperationCreateRequest {UploadFiles = new[] {new UploadFileRequest {Format = "text/xml", InterchangeType = InterchangeType.EducationOrganization.Name, Size = 1060},}};
                        var postResponse = client.PostAsync(OwinUriHelper.BuildApiUri("2014", "BulkOperations"), new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json")).Result;
                        postResponse.EnsureSuccessStatusCode();
                        postResponse.StatusCode.ShouldEqual(HttpStatusCode.Created);

                        var bulkContent = postResponse.Content.ReadAsAsync<BulkOperationResource>().Result;
                        var suppliedUploadId = bulkContent.UploadFiles[0].Id;

                        var url = OwinUriHelper.CreateCommitUri(suppliedUploadId);

                        var deleteResponse = client.DeleteAsync(url).Result;
                        deleteResponse.StatusCode.ShouldEqual(HttpStatusCode.MethodNotAllowed);
                    }
                }
            }
        }

        [Test]
        public void Should_return_405_when_sending_get_verb()
        {
            using (var startup = new OwinStartup(DatabaseName, LocalEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = new TimeSpan(0, 0, 15, 0);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var request = new BulkOperationCreateRequest {UploadFiles = new[] {new UploadFileRequest {Format = "text/xml", InterchangeType = InterchangeType.EducationOrganization.Name, Size = 1060},}};
                        var postResponse = client.PostAsync(OwinUriHelper.BuildApiUri("2014", "BulkOperations"), new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json")).Result;
                        postResponse.EnsureSuccessStatusCode();
                        postResponse.StatusCode.ShouldEqual(HttpStatusCode.Created);

                        var bulkContent = postResponse.Content.ReadAsAsync<BulkOperationResource>().Result;
                        var suppliedUploadId = bulkContent.UploadFiles[0].Id;

                        var url = OwinUriHelper.CreateCommitUri(suppliedUploadId);

                        var getResponse = client.GetAsync(url).Result;
                        getResponse.StatusCode.ShouldEqual(HttpStatusCode.MethodNotAllowed);
                    }
                }
            }
        }

        [Test]
        public void Should_return_405_when_sending_put_verb()
        {
            using (var startup = new OwinStartup(DatabaseName, LocalEducationAgencyIds))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = new TimeSpan(0, 0, 15, 0);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var request = new BulkOperationCreateRequest {UploadFiles = new[] {new UploadFileRequest {Format = "text/xml", InterchangeType = InterchangeType.EducationOrganization.Name, Size = 1060},}};
                        var postResponse = client.PostAsync(OwinUriHelper.BuildApiUri("2014", "BulkOperations"), new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json")).Result;
                        postResponse.EnsureSuccessStatusCode();
                        postResponse.StatusCode.ShouldEqual(HttpStatusCode.Created);

                        var bulkContent = postResponse.Content.ReadAsAsync<BulkOperationResource>().Result;
                        var suppliedUploadId = bulkContent.UploadFiles[0].Id;

                        //upload Chunk
                        var url = OwinUriHelper.CreateCommitUri(suppliedUploadId);

                        var content = new MultipartFormDataContent();
                        content.Add(new StringContent(JsonConvert.SerializeObject(new {uploadId = suppliedUploadId}), Encoding.UTF8, "application/json"));

                        var putResponse = client.PutAsync(url, content).Result;
                        putResponse.StatusCode.ShouldEqual(HttpStatusCode.MethodNotAllowed);
                    }
                }
            }
        }
    }
}
