using Nancy.Bootstrapper;

namespace Nancy.SimpleAuthentication
{
    public class EnableSessions : IApplicationStartup
    {
        public void Initialize(IPipelines pipelines)
        {
            Session.CookieBasedSessions.Enable(pipelines);
        }
    }
}