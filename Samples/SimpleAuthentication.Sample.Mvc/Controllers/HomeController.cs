using System.Web.Mvc;
using SimpleAuthentication.Sample.Mvc.Models;

namespace SimpleAuthentication.Sample.Mvc.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View("Index", new IndexViewModel());
        }
    }
}