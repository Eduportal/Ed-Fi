using System.Linq;
using System.Net;
using System.Net.Http;
using EdFi.Ods.WebTest.Common.Extensions;
using NUnit.Framework;
using Should;

namespace EdFi.Ods.XmlShredding.Tests
{
    
    public class EdOrgFixture : WebServiceFixturesBase
    {
        private int _operationResult;

        protected override void ExecuteTest()
        {
            InitClient();

            var operationId = CreateOperation();

            UploadFile(operationId);

            StartOperation(operationId);

            _operationResult = PollStatusOfOperation(operationId);
        }
        
        [Test]
        public void Should_operation_result_be_success()
        {
            _operationResult.ShouldEqual(2/*Completed*/);
        }

        private int PollStatusOfOperation(string operationId)
        {
            int status;
            do
            {
                var response = Client.GetAsync(string.Format("{0}/BulkOperations/{1}", BaseServiceUrl, operationId)).WaitForResponse(timeoutSeconds:900);
                status = int.Parse(response.Content.ReadAsStringAsync().Sync(timeoutSeconds:900));

            } while (!new[] { 2, 3 }.Contains(status));

            return status;
        }

        private void StartOperation(string operationId)
        {
            var response = Client.PutAsJsonAsync(string.Format("{0}/BulkOperations/{1}", BaseServiceUrl, operationId),
                new object()).WaitForResponse(HttpStatusCode.Accepted);

            response.IsSuccessStatusCode.ShouldBeTrue(
                string.Format(
                    "The Begin Processing Put Failed with status code {0} and message {1}",
                        response.StatusCode, response.ReasonPhrase));
        }

        private string CreateOperation()
        {
            var response = Client
                .PostAsJsonAsync(string.Format("{0}/BulkOperations", BaseServiceUrl), new object())
                    .WaitForResponse(HttpStatusCode.Created);

            response.IsSuccessStatusCode.ShouldBeTrue(
                string.Format(
                    "The BulkOperations Post Failed with status code {0} and message {1}",
                        response.StatusCode, response.ReasonPhrase));

            return response.Content.ReadAsStringAsync().Sync().Replace("\"", "");
        }

        public byte[] ReadFile()
        {
            byte[] buffer;
            using (var stream =
                    GetType()
                        .Assembly.GetManifestResourceStream(
                            "EdFi.Ods.XmlShredding.Tests.Xml.Interchange-EducationOrganization.xml"))
            {
                stream.ShouldNotBeNull("Couldn't read file.");
// ReSharper disable once PossibleNullReferenceException - assertion above protects test
                var length = (int)stream.Length;  
                buffer = new byte[length];            
                int count; 
                var sum = 0;

                while ((count = stream.Read(buffer, sum, length - sum)) > 0)
                    sum += count;  
            }
            
            return buffer;
        }

       private void UploadFile(string operationId)
        {
            var bytes = ReadFile();

            var chunksize = bytes.Length/5;

            var start = 0;
            int chunkNo = 0;
            while (true)
            {
                var chunk = bytes.Skip(start).Take(chunksize).ToArray();

                if (chunk.Length == 0)
                    return;

                SendChunk(chunk, operationId, chunkNo);

                if (chunk.Length < chunksize)
                    break;

                chunkNo++;
                start += chunksize;
            }

            // upload is done. Commit uploaded chunks.
            var response = Client.PutAsJsonAsync(
                string.Format("{0}/Upload/EducationOrganization/{1}?chunkCount={2}",
                    BaseServiceUrl, operationId, (chunkNo + 1)), new object()).WaitForResponse();

            response.IsSuccessStatusCode.ShouldBeTrue(
                string.Format(
                    "The Complete Upload Put Failed with status code {0} and message {1}",
                        response.StatusCode, response.ReasonPhrase));
        }

        private void SendChunk(byte[] chunk, string operationId, int chunkNo)
        {
            var content = new MultipartFormDataContent();
            content.Add(new ByteArrayContent(chunk));

            var response =
                Client.PostAsync(
                    string.Format("{0}/Upload/EducationOrganization/{1}/{2}", BaseServiceUrl, operationId, chunkNo),
                    content).WaitForResponse();

            response.IsSuccessStatusCode.ShouldBeTrue(
                string.Format(
                    "The SendChunk {0} Post Failed with status code {1} and message {2}",
                        chunkNo, response.StatusCode, response.ReasonPhrase));
        }
   }

}
