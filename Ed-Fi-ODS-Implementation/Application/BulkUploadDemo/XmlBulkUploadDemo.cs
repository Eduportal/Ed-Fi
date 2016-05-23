using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BulkUploadDemo
{
    [TestFixture]
    public class XmlBulkUploadDemo
    {

        //LOCAL ENVIRONMENT SETTINGS
        protected internal const string OauthAuthorizeUrl = @"http://localhost:54746/OAuth/Authorize";
        protected internal const string OauthTokenUrl = @"http://localhost:54746/OAuth/Token";
        private const string BulkOperationUrl = @"http://localhost:54746/api/v2.0/BulkOperations";
        private const string UploadsUrl = @"http://localhost:54746/api/v2.0/Uploads";

        //DEV ENVIRONMENT SETTINGS
        //protected internal const string OauthAuthorizeUrl = @"https://tn-rest-admin-dev.cloudapp.net/OAuth/Authorize";
        //protected internal const string OauthTokenUrl = @"https://tn-rest-admin-dev.cloudapp.net/OAuth/Token";
        //private const string BulkOperationUrl = @"https://tn-rest-dev.cloudapp.net/api/v2.0/BulkOperations";
        //private const string UploadsUrl = @"https://tn-rest-dev.cloudapp.net/api/v2.0/Uploads";

        //PRODUCTION SETTINGS
        //protected internal const string OauthAuthorizeUrl = @"https://tn-rest-admin-production.cloudapp.net/OAuth/Authorize";
        //protected internal const string OauthTokenUrl = @"https://tn-rest-admin-production.cloudapp.net/OAuth/Token";
        //private const string BulkOperationUrl = @"https://tn-rest-production.cloudapp.net/api/v2.0/BulkOperations";
        //private const string UploadsUrl = @"https://tn-rest-production.cloudapp.net/api/v2.0/Uploads";

        private const string Key = @"<Key Here>";
        private const string Secret = @"<Secret Here>";
        private const bool WaitForOperationToComplete = true;

        [Test, Ignore]
        public void PostDescriptorViaBulkOperation()
        {
            var xmlData = TestDataHelper.GetLocation();
            var interchangeType = "educationorganization";
//            var xmlData = ResourceReader.GetResourceString<TestDataMarker>("SkywardEducationOrganization.xml");
//            var interchangeType = "educationorganization";

            var xmlLength = xmlData.Length;
            var httpClient = HttpClientFactory.CreateHttpClient(Key, Secret);

            Console.WriteLine("Creating bulk operation...");
            var bulkOperation = CreateBulkLoadOperation(httpClient, xmlLength, interchangeType);
            var bulkOperationId = bulkOperation.Id;
            Console.WriteLine("Bulk operation {0} created", bulkOperationId);
            var uploadedFileId = bulkOperation.UploadFiles.First().Id;

            Console.WriteLine("Uploading chunks...");
            UploadChunks(uploadedFileId, httpClient, xmlData);

            Console.WriteLine("Committing upload...");
            CommitUpload(uploadedFileId, httpClient);

            if (WaitForOperationToComplete)
            {
                Console.WriteLine("Waiting for bulk operation to complete...");
                var finalStatus = WaitForBulkOperationToComplete(bulkOperationId, httpClient);
                Assert.AreEqual(BulkOperationStatus.Completed, finalStatus, "Failed while checking the final status of the operation");
            }
        }

        private static BulkOperation CreateBulkLoadOperation(HttpClient httpClient, int xmlLength, string interchangeType)
        {
            var uploadFileRequest = new UploadFileRequest
                                        {
                                            Format = "text/xml",
                                            InterchangeType = interchangeType,
                                            Size = xmlLength
                                        };
            var bulkOperationCreateRequest = new BulkOperationCreateRequest {UploadFiles = new[] {uploadFileRequest}};

            var requestJson = new StringContent(JsonConvert.SerializeObject(bulkOperationCreateRequest), Encoding.UTF8,
                                                "application/json");
            var postTask = httpClient.PostAsync(BulkOperationUrl, requestJson);
            postTask.WaitForResponse(HttpStatusCode.Created);
            var contentTask = postTask.Result.Content.ReadAsAsync<BulkOperation>();
            contentTask.Wait();
            Assert.AreEqual(HttpStatusCode.Created, postTask.Result.StatusCode);
            return contentTask.Result;
        }

        private void UploadChunks(string uploadedFileId, HttpClient client, string testXml)
        {
            const int offset = 0;
            var chunkSize = testXml.Length;
            var url = string.Format("{0}/{1}/chunk?offset={2}&size={3}", UploadsUrl, uploadedFileId, offset, chunkSize);

            var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(testXml));
            fileContent.Headers.ContentType =
                MediaTypeHeaderValue.Parse("application/json");

            var content = new MultipartFormDataContent {{fileContent, "File"}};


            var postTask = client.PostAsync(url, content);
            postTask.Wait();
            Assert.AreEqual(HttpStatusCode.Created, postTask.Result.StatusCode);
        }

        private static void CommitUpload(string uploadedFileId, HttpClient client)
        {
            var url = string.Format("{0}/{1}/commit", UploadsUrl, uploadedFileId);
            var request = new {uploadId = uploadedFileId};

            var content = new MultipartFormDataContent
                              {
                                  new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8,
                                                    "application/json")
                              };
            var postTask = client.PostAsync(url, content);
            postTask.Wait();
            Assert.AreEqual(HttpStatusCode.Accepted, postTask.Result.StatusCode);
        }

        private BulkOperationStatus WaitForBulkOperationToComplete(string bulkOperationId, HttpClient client)
        {
            var endStates = new[]
                                {BulkOperationStatus.Completed, BulkOperationStatus.Error, BulkOperationStatus.Expired};
            bool isDone;
            const int timeoutMilliseconds = 180*1000;
            var stopwatch = Stopwatch.StartNew();
            BulkOperationStatus status;
            do
            {
                Thread.Sleep(TimeSpan.FromSeconds(5));
                status = GetBulkOperationStatus(bulkOperationId, client);
                isDone = endStates.Contains(status);
                if (stopwatch.ElapsedMilliseconds > timeoutMilliseconds)
                    throw new TimeoutException();
            } while (!isDone);
            return status;
        }

        private BulkOperationStatus GetBulkOperationStatus(string bulkOperationId, HttpClient client)
        {
            var getTask = client.GetAsync(BulkOperationUrl + "/" + bulkOperationId);
            getTask.Wait();
            var contentTask = getTask.Result.Content.ReadAsAsync<BulkOperation>();
            contentTask.Wait();
            Assert.AreEqual(HttpStatusCode.OK, getTask.Result.StatusCode);
            return contentTask.Result.Status;
        }
    }

    public static class HttpClientFactory
    {
        private static bool _initialized;
        private static readonly object _lock = new object();

        public static HttpClient CreateHttpClient(string key, string secret)
        {
            InitializeGlobalHttpClientSettings();

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var state = "";
            var postTask = httpClient.PostAsJsonAsync(XmlBulkUploadDemo.OauthAuthorizeUrl,
                                                      new
                                                          {
                                                              Client_id = key,
                                                              Response_type = "code",
                                                              State = state
                                                          });

            var message = postTask.WaitForResponse();
            var jobj = message.Content.ReadAsAsync<JObject>().Sync();
            var code = jobj.Value<string>("Code");
            Assert.AreEqual(state, jobj.Value<string>("State"));

            postTask = httpClient.PostAsJsonAsync(XmlBulkUploadDemo.OauthTokenUrl, new
                                                                                       {
                                                                                           Code = code,
                                                                                           Client_id = key,
                                                                                           Client_secret = secret,
                                                                                           Grant_type = "authorization_code",
                                                                                       });
            message = postTask.WaitForResponse();
            jobj = message.Content.ReadAsAsync<JObject>().Sync();
            var token = jobj.Value<string>("Access_token");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return httpClient;
        }

        private static void InitializeGlobalHttpClientSettings()
        {
            lock (_lock)
            {
                if (!_initialized)
                {
                    DisableCertificateValidation();
                    _initialized = true;
                }
            }
        }

        private static void DisableCertificateValidation()
        {
            ServicePointManager.ServerCertificateValidationCallback =
                (s, certificate, chain, sslPolicyErrors) => true;
        }
    }

    public static class TestDataHelper
    {
        private const string DescriptorTemplate =
            @"<?xml version='1.0' encoding='UTF-8'?>
            <InterchangeDescriptors>
              <LevelDescriptor>
                <CodeValue>##CodeValue##</CodeValue>
                <ShortDescription>##SomeUniqueInformation##</ShortDescription>
                <Description>##SomeUniqueInformation##</Description>
                <EffectiveBeginDate>2012-07-01</EffectiveBeginDate>
                <Namespace>uri://ed-fi.org/Descriptors/Level/Adult-Education</Namespace>
                <LevelDescriptorId>##ID##</LevelDescriptorId>
              </LevelDescriptor>
            </InterchangeDescriptors>";

        public static string GetDescriptor(string description, int id)
        {
            ValidateDescriptorDescription(description);
            var codeValue = DateTime.Now.ToString();
            if (codeValue.Length > 50)
                throw new Exception("Max length for Code Value is 50");

            var xml = DescriptorTemplate
                .Replace("##SomeUniqueInformation##", description)
                .Replace("##ID##", id.ToString(CultureInfo.InvariantCulture))
                .Replace("##CodeValue##", codeValue);
            return xml;
        }

        private static void ValidateDescriptorDescription(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Description must be populated with a non-trivial value");
            if (description.Length > 75)
                throw new Exception("Max length for ShortDescription is 75 characters");
        }

        public static string GetLocation()
        {
            string classroomIdentificationCode = string.Format(@"Loc:{0:MM/dd/yy H:mm}", DateTime.Now);
            if (classroomIdentificationCode.Length > 20)
                throw new Exception(string.Format("Classroom ID Code too long.  {0} > 20", classroomIdentificationCode.Length));

            var XmlTemplate = @"<?xml version='1.0' encoding='UTF-8'?>
            <InterchangeEducationOrganization>
                <Location id=""Location_1"" xmlns=""http://ed-fi.org/0200"">
                    <SchoolReference>
                        <SchoolIdentity>
                            <SchoolId>900007</SchoolId>
                        </SchoolIdentity>
                    </SchoolReference>
                    <ClassroomIdentificationCode>##ClassroomIdCode##</ClassroomIdentificationCode>
                </Location>
            </InterchangeEducationOrganization>";

            return XmlTemplate
                .Replace("##ClassroomIdCode##", classroomIdentificationCode);
        }
    }

    public static class TaskExtensions
    {
        private const int DefaultHttpRequestTimeoutSeconds = 60;

        public static HttpResponseMessage WaitForResponse(this Task<HttpResponseMessage> task,
                                                          HttpStatusCode expectedStatus = HttpStatusCode.OK,
                                                          int timeoutSeconds = DefaultHttpRequestTimeoutSeconds)
        {
            var response = Sync(task, timeoutSeconds);

            ValidateResposeStatusCode(expectedStatus, timeoutSeconds, response);
            return response;
        }

        private static void ValidateResposeStatusCode(HttpStatusCode expectedStatus, int timeoutSeconds,
                                                      HttpResponseMessage response)
        {
            var actualStatus = response.StatusCode;
            if (actualStatus != expectedStatus)
            {
                var contentTask = response.Content.ReadAsStringAsync();
                var content = Sync(contentTask, timeoutSeconds);

                Console.WriteLine("********* FAILED REQUEST: (expected status: {0}, actual status: {1} *********",
                                  expectedStatus, actualStatus);
                Console.WriteLine("Response content for failed request:\n{0}\n", content);
                Console.WriteLine("******* END FAILED REQUEST *******");
            }
            Assert.AreEqual(expectedStatus, actualStatus);
        }

        public static T Sync<T>(this Task<T> task, int timeoutSeconds = DefaultHttpRequestTimeoutSeconds)
        {
            try
            {
                task.Wait(TimeSpan.FromSeconds(timeoutSeconds));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }

            if (task.IsCanceled)
                throw new Exception("Async test failure! Request was canceled");

            if (!task.IsCompleted)
                throw new Exception("Async test failure! Request timed out.");

            return task.Result;
        }
    }

    public class BulkOperationCreateRequest
    {
        public UploadFileRequest[] UploadFiles { get; set; }
        public string ResetDistrictData { get; set; }
    }

    public class UploadFileRequest
    {
        public string Format { get; set; }
        public string InterchangeType { get; set; }
        public long Size { get; set; }
    }

    public class BulkOperation
    {
        public string Id { get; set; }
        public UploadFile[] UploadFiles { get; set; }
        public string ResetDistrictData { get; set; }
        public BulkOperationStatus Status { get; set; }
    }

    public class UploadFile
    {
        public string Id { get; set; }
        public long Size { get; set; }
        public string Format { get; set; }
        public string InterchangeType { get; set; }
        public UploadFileStatus Status { get; set; }
        public UploadFileChunk[] UploadFileChunks { get; set; }
    }

    public enum BulkOperationStatus
    {
        Initialized,
        Incomplete,
        Ready,
        Started,
        Completed,
        Error,
        Expired
    }

    public enum UploadFileStatus
    {
        Initialized,
        Incomplete,
        Ready,
        Started,
        Completed,
        Error,
        Expired
    }

    public class UploadFileChunk
    {
        public string Id { get; set; }
        public long Offset { get; set; }
        public long Size { get; set; }
        public byte[] Chunk { get; set; }
    }

    public static class ResourceReader
    {
        public static Stream GetResourceStream<TMarkerType>(string resourceName)
        {
            var markerType = typeof (TMarkerType);
            var assembly = markerType.Assembly;
            return assembly.GetManifestResourceStream(markerType, resourceName);
        }

        public static string GetResourceString<TMarkerType>(string resourceName)
        {
            using (var stream = GetResourceStream<TMarkerType>(resourceName))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}