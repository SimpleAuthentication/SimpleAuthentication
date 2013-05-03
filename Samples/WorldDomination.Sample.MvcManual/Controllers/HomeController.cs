using System.Web.Mvc;

namespace WorldDomination.Sample.MvcManual.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}