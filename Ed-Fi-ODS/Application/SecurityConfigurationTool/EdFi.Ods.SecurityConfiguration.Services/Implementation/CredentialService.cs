using System;
using EdFi.Ods.SecurityConfiguration.Services.Model;

namespace EdFi.Ods.SecurityConfiguration.Services.Implementation
{
    public class CredentialService : ICredentialService
    {
        public Credentials GetNewCredentials()
        {
            return new Credentials
            {
                Key = Guid.NewGuid().ToString().Substring(0, 20).Replace("-", ""),
                Secret = Guid.NewGuid().ToString().Substring(0, 15).Replace("-", "")
            };
        }    
    }
}