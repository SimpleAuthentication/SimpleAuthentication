using Nancy;

namespace WorldDomination.Sample.NancyAuto.Modules
{
    public class HomeModule : NancyModule
    {
        public HomeModule()
        {
            Get["/"] = _ => View["index"];
        }
    }
}