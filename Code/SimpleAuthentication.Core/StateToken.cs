using System;

namespace SimpleAuthentication.Core
{
    public class StateToken
    {
        public StateToken()
        {
            State = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// "State" for this authentication process.
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// Querystring parameter state key.
        /// </summary>
        public string StateKey { get; set; }
    }
}