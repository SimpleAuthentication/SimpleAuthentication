using Nancy.Bootstrapper;

namespace Nancy.Authentication.WorldDomination
{
    public class EnableSessions : IApplicationStartup
    {
        public void Initialize(IPipelines pipelines)
        {
            Session.CookieBasedSessions.Enable(pipelines);
        }
    }
}