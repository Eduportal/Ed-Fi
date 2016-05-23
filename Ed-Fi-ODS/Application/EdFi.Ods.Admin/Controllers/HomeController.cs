using System.Web.Mvc;
using EdFi.Ods.Admin.Models.Home;

namespace EdFi.Ods.Admin.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var user = System.Web.HttpContext.Current.User;
            
            var isLoggedIn = user.Identity.IsAuthenticated;
            var shouldLogin = !isLoggedIn;
            var isAdmin = isLoggedIn && user.IsInRole("Administrator");
            
            var model = new IndexViewModel {UserShouldLogin = shouldLogin, UserIsAdmin = isAdmin};
            return View(model);
        }
    }
}