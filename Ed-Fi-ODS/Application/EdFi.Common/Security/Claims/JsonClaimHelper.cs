using System;
using System.Security.Claims;
using Newtonsoft.Json;

namespace EdFi.Common.Security.Claims
{
    public static class JsonClaimHelper
    {
        private static JsonSerializerSettings _serializerSettings;

        static JsonClaimHelper()
        {
            _serializerSettings = new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.Ignore
            };
        }

        public static Claim CreateClaim(string claimType, EdFiResourceClaimValue edFiResourceClaimValue)
        {
            var value = JsonConvert.SerializeObject(edFiResourceClaimValue, _serializerSettings);
            var claim = new Claim(claimType, value, "application/json");
            return claim;
        }

        public static EdFiResourceClaimValue ToEdFiResourceClaimValue(this Claim claim)
        {
            if (claim.ValueType != "application/json")
                throw new InvalidOperationException(string.Format("Can NOT deseralize non JSON claim ({0}) of value type ({1})",claim.Type,claim.ValueType));

            return JsonConvert.DeserializeObject<EdFiResourceClaimValue>(claim.Value);
        }
    }
}
