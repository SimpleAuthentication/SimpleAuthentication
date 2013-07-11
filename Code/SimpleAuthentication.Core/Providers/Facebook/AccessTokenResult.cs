namespace SimpleAuthentication.Core.Providers.Facebook
{
    public class AccessTokenResult
    {
        // ReSharper disable InconsistentNaming

        // REFERENCE: access_token=CAABmZBTPQJVoBAEZBZBrDcoKBC2fH72VdiKZBKjw9rf4AFqtw8feKd2gzWaZBP0xUs58xPgyJ6mlWMpgJc2Y9Pb1vxiWPKKMZCQu4LSK1sMoPdhPxON8goK7oID2OcEspwdXJo8sQwq1yTqQlOQWm0&expires=5152280

        public string access_token { get; set; }
        public int expires { get; set; }

        // ReSharper restore InconsistentNaming
    }
}