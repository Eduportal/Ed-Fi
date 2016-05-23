using System.Web.Mvc;

namespace EdFi.Ods.Admin.Controllers
{
    [Authorize]
    public class ClientController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}