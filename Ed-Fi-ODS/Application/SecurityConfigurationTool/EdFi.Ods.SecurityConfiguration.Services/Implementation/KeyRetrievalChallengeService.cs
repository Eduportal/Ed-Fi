using System;
using System.Linq;
using EdFi.Ods.Admin.Models;
using EdFi.Ods.SecurityConfiguration.Services.Model;
using Context = EdFi.Ods.Admin.Models.UsersContext;

namespace EdFi.Ods.SecurityConfiguration.Services.Implementation
{
    public class KeyRetrievalChallengeService : IKeyRetrievalChallengeService
    {
        private readonly IAdminContextFactory _context;
        private readonly IConfigurationService _configurationService;
        private readonly ICredentialService _credentialService;

        public KeyRetrievalChallengeService(
            IAdminContextFactory context, 
            IConfigurationService configurationService, 
            ICredentialService credentialService)
        {
            _context = context;
            _configurationService = configurationService;
            _credentialService = credentialService;
        }

        public KeyGenResult GenerateKeySecretAndChallenge(int applicationId)
        {
            using (var context = _context.Create())
            {
                var application = context.Applications.SingleOrDefault(a => a.ApplicationId == applicationId);

                if (application == null)
                    throw new ArgumentException(string.Format("cannot find {{applicationId: {0} }}", applicationId));

                // at this point, application should have one and only one ApiClient
                var apiClient = application.ApiClients.Single();

                var newChallengeId = Guid.NewGuid().ToString().Replace("-", "");
                var newActivationCode = RandomString(_configurationService.GetActivationCodeLength());

                var credentials = _credentialService.GetNewCredentials();
                apiClient.Key = credentials.Key;
                apiClient.Secret = credentials.Secret;
                apiClient.KeyStatus = KeyStatus.Sent;
                apiClient.ChallengeId = newChallengeId;
                apiClient.ChallengeExpiry = DateTime.Now + _configurationService.GetChallengeExpiryInterval();
                apiClient.ActivationCode = newActivationCode;
                apiClient.ActivationRetried = 0;

                context.SaveChanges();

                return new KeyGenResult
                {
                    ChallengeId = newChallengeId,
                    ActivationCode = newActivationCode
                };
            }
        }

        private ApiClient ValidApiClient(IUsersContext context, string challengeId)
        {
            var maxNumberOfRetrial = _configurationService.GetMaxRetries();

            return context.Clients
                .FirstOrDefault(c => c.ChallengeId == challengeId
                                     && c.ChallengeExpiry > DateTime.Now
                                     && (c.ActivationRetried == null || c.ActivationRetried < maxNumberOfRetrial)
                                     && c.KeyStatus == KeyStatus.Sent
                                     && c.ActivationCode != null
                                     && c.ActivationCode.Trim() != "");
        }

        public bool IsValid(string challengeId)
        {
            using (var context = _context.Create())
            {
                return ValidApiClient(context, challengeId) != null;
            }
        }

        public ActivateResult TryActivate(string challengeId, string activationCode)
        {

            var maxNumberOfRetrial = _configurationService.GetMaxRetries();

            using (var context = _context.Create())
            {
                var result = new ActivateResult();

                var apiClient = ValidApiClient(context, challengeId);
                if (apiClient == null) throw new ArgumentException();
                
                apiClient.ActivationRetried++;

                if (AreEqual(apiClient.ActivationCode, activationCode))
                {
                    apiClient.KeyStatus = KeyStatus.Active;
                    apiClient.ChallengeExpiry = null;
                    apiClient.ActivationCode = null;

                    result.Activated = true;
                    result.ApiKey = apiClient.Key;
                    result.ApiSecret = apiClient.Secret;
                    result.IsValid = true;
                }
                else
                {
                    result.Activated = false;
                    result.LeftChances = maxNumberOfRetrial - apiClient.ActivationRetried;
                    result.IsValid = true;

                    if (apiClient.ActivationRetried >= maxNumberOfRetrial)
                    {
                        apiClient.KeyStatus = KeyStatus.Locked;
                        apiClient.ChallengeExpiry = null;
                        apiClient.ActivationCode = null;
                    
                        result.IsValid = false;
                    }
                }

                context.SaveChanges();
                return result;
            }
        }

        private static bool AreEqual(string str1, string str2)
        {
            return string.Equals(str1, str2, StringComparison.CurrentCultureIgnoreCase);
        }

        private static string RandomString(int size)
        {
            const string chars = "ABCDEFGHJKMNPQRSTUVWXYZ23456789"; // removed misleading letters and numbers (0, O, 1, L, ...)
            var random = new Random();
            return new string(
                Enumerable.Repeat(chars, size)
                    .Select(s => s[random.Next(s.Length)])
                    .ToArray());
        }
    }
}