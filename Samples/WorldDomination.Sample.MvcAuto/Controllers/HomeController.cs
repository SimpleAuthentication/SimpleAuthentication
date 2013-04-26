using System.Web.Mvc;
using WorldDomination.Sample.MvcAuto.Models;

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