using System;
using System.Configuration;

namespace EdFi.Ods.SecurityConfiguration.Services.Implementation
{
    public class ConfigurationService : IConfigurationService
    {
        private const string ActivationCodeLength = "ActivationCodeLength";
        private const string ChallengeExpiryHours = "ChallengeExpiryHours";
        private const string MaxChallengeRetries = "MaxChallengeRetries";
        private const string KeyRetrievalSite = "KeyRetrievalSite";

        public int GetActivationCodeLength()
        {
            var value = ConfigurationManager.AppSettings[ActivationCodeLength];

            ValidateApplicationSetting(ActivationCodeLength, value);

            return int.Parse(value);
        }

        public TimeSpan GetChallengeExpiryInterval()
        {
            var value = ConfigurationManager.AppSettings[ChallengeExpiryHours];
            ValidateApplicationSetting(ChallengeExpiryHours, value);

            var challengeExpiryHours = int.Parse(value);
            return new TimeSpan(challengeExpiryHours, 0, 0);
        }

        public int GetMaxRetries()
        {
            var value = ConfigurationManager.AppSettings[MaxChallengeRetries];

            ValidateApplicationSetting(MaxChallengeRetries, value);

            return int.Parse(value);
        }

        public string GetKeyRetrievalSite()
        {
            var value = ConfigurationManager.AppSettings[KeyRetrievalSite];

            ValidateApplicationSetting(KeyRetrievalSite, value);

            return value;
        }

        private static void ValidateApplicationSetting(string key, string value)
        {
            if (value == null)
                throw new ConfigurationErrorsException(string.Format("cannot find '{0}' in the configuration file", key));
        }
    }
}