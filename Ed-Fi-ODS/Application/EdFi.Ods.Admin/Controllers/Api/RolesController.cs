using EdFi.Ods.Admin.Services;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Security;

namespace EdFi.Ods.Admin.Controllers.Api
{
    //[Authorize(Roles="Administrator")]
    public class RolesController : ApiController
    {

        private ISecurityService _securityService;

        public RolesController(ISecurityService securityService)
        {
            _securityService = securityService;
        }

        public IEnumerable<MembershipUser> Get()
        {
            return null;
        }

        public MembershipUser Get(string id)
        {
            var result = Membership.GetUser(id);
            return result;
        }

    }
}
