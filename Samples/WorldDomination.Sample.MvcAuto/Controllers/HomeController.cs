using System.Web.Mvc;

namespace WorldDomination.Sample.MvcAuto.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View("Index", new IndexViewModel());
        }
    }
}