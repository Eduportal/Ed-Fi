using System.Web.Http;
using EdFi.Ods.SecurityConfiguration.Services;

namespace EdFi.Ods.SecurityConfiguration.Core.Controllers.Administration
{
    [Authorize]
    public class EdOrgController : ApiController
    {
        private readonly IEducationOrganizationProvider _educationOrganizationProvider;

        public EdOrgController(IEducationOrganizationProvider educationOrganizationProvider)
        {
            _educationOrganizationProvider = educationOrganizationProvider;
        }

        [Route("api/leas")]
        public IHttpActionResult GetLeas()
        {
            return Ok(_educationOrganizationProvider.GetLocalEducationAgencies());
        }

        [Route("api/schools")]
        public IHttpActionResult GetSchools()
        {
            return Ok(_educationOrganizationProvider.GetSchools());
        }
    }
}
