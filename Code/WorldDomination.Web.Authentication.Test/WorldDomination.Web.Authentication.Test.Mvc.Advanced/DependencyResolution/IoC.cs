// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IoC.cs" company="Web Advanced">
// Copyright 2012 Web Advanced (www.webadvanced.com)
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0

// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


using System;
using System.Configuration;
using StructureMap;
using WorldDomination.Web.Authentication.Facebook;
using WorldDomination.Web.Authentication.Google;
using WorldDomination.Web.Authentication.Twitter;

namespace WorldDomination.Web.Authentication.Test.Mvc.Advanced.DependencyResolution
{
    public static class IoC
    {
        public static IContainer Initialize()
        {
            ObjectFactory.Initialize(x =>
                                     {
                                         var authenticationRegistry = new AuthenticationRegistry(
                                             new FacebookProvider(ConfigurationManager.AppSettings["FacebookAppId"],
                                                                  ConfigurationManager.AppSettings["FacebookAppSecret"],
                                                                  new Uri(ConfigurationManager.AppSettings["FacebookRedirectUri"])),
                                             new GoogleProvider(ConfigurationManager.AppSettings["GoogleConsumerKey"],
                                                                ConfigurationManager.AppSettings["GoogleConsumerSecret"],
                                                                new Uri(ConfigurationManager.AppSettings["GoogleConsumerRedirectUri"])),
                                             new TwitterProvider(ConfigurationManager.AppSettings["TwitterConsumerKey"],
                                                                 ConfigurationManager.AppSettings["TwitterConsumerSecret"],
                                                                 new Uri(ConfigurationManager.AppSettings["TwitterConsumerRedirectUri"]))
                                             );
                                         x.AddRegistry(authenticationRegistry);
                                     });
            return ObjectFactory.Container;
        }
    }
}