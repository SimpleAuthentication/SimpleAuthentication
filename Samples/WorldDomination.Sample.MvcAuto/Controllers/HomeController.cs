using System.Web.Mvc;
using WorldDomination.Sample.MvcAuto.Models;
using WorldDomination.Web.Authentication.Tracing;

namespace WorldDomination.Sample.MvcAuto.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var traceManager = new TraceManager();
            var traceSource = traceManager["WD.Sample.MvcAuto.Controllers.HomeController"];
            traceSource.TraceVerbose("Hi There! Lets test this out :)");

            return View("Index", new IndexViewModel());
        }
    }
}