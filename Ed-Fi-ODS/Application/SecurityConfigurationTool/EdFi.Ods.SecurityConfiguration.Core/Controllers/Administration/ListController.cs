using System.Linq;
using System.Web.Http;
using EdFi.Ods.SecurityConfiguration.Services;

namespace EdFi.Ods.SecurityConfiguration.Core.Controllers.Administration
{
    [Authorize]
    [RoutePrefix("api/list")]
    public class ListController : ApiController
    {
        private readonly IProfileListService _profileListService;
        private readonly IClaimSetListService _claimSetListService;

        public ListController(IProfileListService profileListService, IClaimSetListService claimSetListService)
        {
            _profileListService = profileListService;
            _claimSetListService = claimSetListService;
        }

        [Route("claimsets")]
        public IHttpActionResult GetClaimSet()
        {
            return Ok(_claimSetListService.GetAllClaimSets().ToList());
        }

        [Route("profiles")]
        public IHttpActionResult GetProfiles()
        {
            return Ok(_profileListService.GetAllProfiles().ToList());
        }
    }
}
