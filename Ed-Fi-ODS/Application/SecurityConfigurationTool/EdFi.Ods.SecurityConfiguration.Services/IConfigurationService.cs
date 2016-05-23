using System;

namespace EdFi.Ods.SecurityConfiguration.Services
{
    public interface IConfigurationService
    {
        int GetActivationCodeLength();
        TimeSpan GetChallengeExpiryInterval();
        int GetMaxRetries();
        string GetKeyRetrievalSite();
    }
}