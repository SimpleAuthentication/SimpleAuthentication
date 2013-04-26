using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorldDomination.Web.Authentication.Exceptions
{
    [Serializable]
    public class WorldDominationConfigurationException : Exception
    {
        public WorldDominationConfigurationException()
        {
        }

        public WorldDominationConfigurationException(string message) : base(message)
        {
        }
    }
}