using System.Web.Http;
using EdFi.Ods.SecurityConfiguration.Core.Models;
using EdFi.Ods.SecurityConfiguration.Services;

namespace EdFi.Ods.SecurityConfiguration.Core.Controllers.KeyRetrieval
{
    public class ActivationController : ApiController
    {
        private readonly IKeyRetrievalChallengeService _keyRetrievalChallengeService;

        public ActivationController(IKeyRetrievalChallengeService keyRetrievalChallengeService)
        {
            _keyRetrievalChallengeService = keyRetrievalChallengeService;
        }

        [Route("activate")]
        public IHttpActionResult Post(Activation activation)
        {
            var activationResult = _keyRetrievalChallengeService
                .TryActivate(activation.ChallengeId, activation.ActivationCode);
            
            if (!activationResult.IsValid)
                return BadRequest("Invalid activation request");

            return Ok(activationResult);
        }

        [Route("validate/{challengeId}")]
        public IHttpActionResult Get(string challengeId)
        {
            var isValid = _keyRetrievalChallengeService.IsValid(challengeId);
            
            return Ok(isValid);
        }
    }
}
