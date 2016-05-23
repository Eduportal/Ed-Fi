using EdFi.Ods.WebService.Tests.Extensions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System.Net.Http;
using System.Net.Http.Headers;

namespace EdFi.Ods.WebService.Tests._Helpers
{
    internal class OwinAuthorizationTokenHelper
    {
        /// <summary>
        /// Most tests simply Mock out the IOAuthTokenValidator as to remove any dependencies on the Admin database, however
        /// if a test in the future needs to to use a valid key & secret this function should be used to retrieve the toekn to set on the header
        /// </summary>
        internal static string GetAuthorizationToken(HttpClient client, string key, string secret)
        {
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var state = "";
            var postResult = client.PostAsJsonAsync("http://owin/oauth/authorize", new { Client_id = key, Response_type = "code", State = state }).Result;
            var jobj = postResult.Content.ReadAsAsync<JObject>().Result;
            var code = jobj.Value<string>("code");

            Assert.AreEqual(state, jobj.Value<string>("state"));

            postResult = client.PostAsJsonAsync("http://owin/oauth/token", new { Code = code, Client_id = key, Client_secret = secret, Grant_type = "authorization_code", }).Result;
            jobj = postResult.Content.ReadAsAsync<JObject>().Result;
            var token = jobj.Value<string>("access_token");

            return token;
        }
    }
}
