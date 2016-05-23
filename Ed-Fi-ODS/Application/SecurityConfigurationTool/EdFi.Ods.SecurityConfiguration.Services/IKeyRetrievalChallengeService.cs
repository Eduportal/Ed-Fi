using EdFi.Ods.SecurityConfiguration.Services.Model;

namespace EdFi.Ods.SecurityConfiguration.Services
{
    public interface IKeyRetrievalChallengeService
    {
        KeyGenResult GenerateKeySecretAndChallenge(int applicationId);
        bool IsValid(string challengeId);
        ActivateResult TryActivate(string challengeId, string activationCode);
    }
}