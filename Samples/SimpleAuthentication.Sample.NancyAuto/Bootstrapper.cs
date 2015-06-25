using System;
using System.Collections.Generic;
using Nancy;
using Nancy.TinyIoc;
using SimpleAuthentication.Core.Providers;
using SimpleAuthentication.ExtraProviders;

namespace SimpleAuthentication.Sample.NancyAuto
{
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        protected override void ConfigureRequestContainer(TinyIoCContainer container, NancyContext context)
        {
            base.ConfigureRequestContainer(container, context);

            var providerWhiteList = new ProviderWhiteList
            {
                ProvidersToAllow = new List<Type>
                {
                    typeof (FacebookProvider),
                    typeof (GoogleProvider),
                    typeof (TwitterProvider),
                    typeof (WindowsLiveProvider),
                    typeof (GitHubProvider)
                }
            };
            container.Register<IProviderWhiteList>(providerWhiteList);
        }
    }
}