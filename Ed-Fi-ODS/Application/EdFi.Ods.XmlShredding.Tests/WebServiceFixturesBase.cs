using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using EdFi.Ods.Admin;
using EdFi.Ods.WebTest.Common.Extensions;
using Newtonsoft.Json.Linq;
using Should;

namespace EdFi.Ods.XmlShredding.Tests
{
    public abstract class WebServiceFixturesBase : TestFixtureBase
    {

        protected HttpClient Client;
        protected string BaseServiceUrl = ConfigurationManager.AppSettings["WebApiEndPoint"];
        protected string AdminBaseServiceUrl = ConfigurationManager.AppSettings["AdminEndPoint"];

        private string GetAbsoluteUrl(string relativeUrl)
        {
            return string.Format("{0}/{1}", AdminBaseServiceUrl.TrimEnd('/'), relativeUrl.TrimStart('/'));
        }

 
        protected void InitClient()
        {
            var userDetails = DeployedUsers.GetNamedUser("XmlTest");
            // Ignore SSL errors on out-of-band HTTP-based calls
            ServicePointManager.ServerCertificateValidationCallback +=
                (sender, certificate, chain, sslPolicyErrors) => true;

            Console.WriteLine("Base Test Address at: {0}", BaseServiceUrl);

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var state = "";
            var postTask = httpClient.PostAsJsonAsync(GetAbsoluteUrl("/oauth/authorize"),
                                                     new
                                                     {
                                                         Client_id = userDetails.Applications[0].Key,
                                                         Response_type = "code",
                                                         State = state
                                                     });

            var message = postTask.WaitForResponse();
            var jobj = message.Content.ReadAsAsync<JObject>().Sync();
            var code = jobj.Value<string>("Code");
            jobj.Value<string>("State").ShouldEqual(state);

            postTask = httpClient.PostAsJsonAsync(GetAbsoluteUrl("/oauth/token"), new
            {
                Code = code,
                Client_id = userDetails.Applications[0].Key,
                Client_secret = userDetails.Applications[0].Secret,
                Grant_type = "authorization_code",
            });
            message = postTask.WaitForResponse();
            jobj = message.Content.ReadAsAsync<JObject>().Sync();
            var token = jobj.Value<string>("Access_token");
            Console.WriteLine(token);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Turn off certificate validation for integration testing
            ServicePointManager.ServerCertificateValidationCallback =
                (s, certificate, chain, sslPolicyErrors) => true;

            Client = httpClient;
        }
    }


}