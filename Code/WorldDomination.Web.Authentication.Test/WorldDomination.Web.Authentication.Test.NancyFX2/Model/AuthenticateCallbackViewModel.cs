﻿using System;

namespace WorldDomination.Web.Authentication.Test.NancyFX2.Model
{
    public class AuthenticateCallbackViewModel
    {
        public IAuthenticatedClient AuthenticatedClient { get; set; }
        public Exception Exception { get; set; }
    }
}