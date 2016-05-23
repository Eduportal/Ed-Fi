using System;
using System.Web.Http;

namespace EdFi.Ods.SecurityConfiguration.Core.Controllers.Shared
{
    public class SystemDateController : ApiController
    {
        [Route("api/server-date")]
        public IHttpActionResult Get()
        {
            return Ok(DateTime.Now);
        }
    }
}
