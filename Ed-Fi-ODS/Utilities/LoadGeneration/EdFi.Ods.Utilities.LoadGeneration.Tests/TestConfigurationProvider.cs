using EdFi.Ods.Utilities.LoadGeneration.Security;

namespace EdFi.Ods.Utilities.LoadGeneration.Tests
{
    public class TestApiSecurityContextProvider : IApiSecurityContextProvider
    {
        private ApiSecurityContext _context = new ApiSecurityContext()
        {
            ApiKey = "3914569BBD",
            ApiSecret = "3F9A997F22",
            ApiUrl = "https://tn-ed-restapi.cloudapp.net/api/v1.0/2015",
            OAuthUrl = "https://tn-ed-restadm.cloudapp.net/",
        };

        public ApiSecurityContext GetSecurityContext()
        {
            return _context;
        }

        public void SetSecurityContext(ApiSecurityContext apiSecurityContext)
        {
            throw new System.NotImplementedException();
        }
    }
}