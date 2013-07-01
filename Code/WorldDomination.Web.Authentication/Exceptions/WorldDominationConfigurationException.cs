using System;

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