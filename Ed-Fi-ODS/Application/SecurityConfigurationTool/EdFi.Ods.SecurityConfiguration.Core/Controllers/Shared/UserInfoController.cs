using System.Web.Http;

namespace EdFi.Ods.SecurityConfiguration.Core.Controllers.Shared
{
    public class UserInfoController : ApiController
    {
        [Route("api/user-info")]
        public IHttpActionResult Get()
        {
            return Ok(User.Identity.Name);
        }
    }
}