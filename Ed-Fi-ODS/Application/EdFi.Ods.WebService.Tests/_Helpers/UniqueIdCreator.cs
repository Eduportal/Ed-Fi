using Newtonsoft.Json;
using System;
using System.Net.Http;
using EdFi.Identity.Models;

namespace EdFi.Ods.WebService.Tests._Helpers
{
    public class UniqueIdCreator
    {
        public static IdentityResource InitializeAPersonWithUniqueData()
        {
            return new IdentityResource
            {
                BirthDate = new DateTime(1995, 2, 3),
                BirthGender = "Male",
                FamilyNames = "Smith" + new Random().Next(1, 1000),
                GivenNames = "John" + new Random().Next(1, 1000),
                UniqueId = Guid.NewGuid().ToString("N"),
            };
        }

        public static string ExtractIdFromHttpResponse(HttpResponseMessage responseMessage)
        {
            return responseMessage.IsSuccessStatusCode
                ? ((IdentityResource)JsonConvert.DeserializeObject(responseMessage.Content.ReadAsStringAsync().Result, typeof(IdentityResource))).UniqueId
                : Guid.NewGuid().ToString("N");
        }
    }
}
