﻿using System;
using WorldDomination.Web.Authentication;

namespace NancyFXWindowsLiveSample.Model
{
    public class AuthenticateCallbackViewModel
    {
        public IAuthenticatedClient AuthenticatedClient { get; set; }
        public Exception Exception { get; set; }
    }
}