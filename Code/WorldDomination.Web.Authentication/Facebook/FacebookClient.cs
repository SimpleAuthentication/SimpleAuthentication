using System;

namespace WorldDomination.Web.Authentication.Facebook
{
    public class FacebookClient
    {
        private const string HashFragment = "#_=_";
        private string _state;
        public string Code { get; set; }
        public string AccessToken { get; set; }

        /// <summary>
        ///   This expires date/time is always represented as UTC.
        /// </summary>
        public DateTime ExpiresOn { get; set; }

        public UserInformation UserInformation { get; set; }

        public string State
        {
            get { return _state; }
            set
            {
                _state = value.EndsWith(HashFragment)
                             ? value.Remove(value.LastIndexOf(HashFragment, StringComparison.Ordinal))
                             : value;
            }
        }

        public string ErrorReason { get; set; }
        public string ErrorDescription { get; set; }
        public string Error { get; set; }
    }
}