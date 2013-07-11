using System.Web.Mvc;
using SimpleAuthentication.Sample.MvcAuto.Models;
using SimpleAuthentication.Core.Tracing;

namespace SimpleAuthentication.Sample.MvcAuto.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var traceManager = new TraceManager();
            var traceSource = traceManager["SimpleAuthentication.Sample.MvcAuto.Controllers.HomeController"];
            traceSource.TraceVerbose("Hi There! Lets test this out :)");

            return View("Index", new IndexViewModel());
        }
    }
}