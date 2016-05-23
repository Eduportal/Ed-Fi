using System.Web.Mvc;
using EdFi.Ods.Admin.Models;

namespace EdFi.Ods.Admin.Controllers
{
    public class TestUsersController : Controller
    {
        [HttpGet]
        public void Initialize()
        {
            new TestUserInitializer().Initialize(new UsersContext());
        }
    }
}
