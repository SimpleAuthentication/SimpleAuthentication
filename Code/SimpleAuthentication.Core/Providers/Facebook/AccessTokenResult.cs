namespace SimpleAuthentication.Core.Providers.Facebook
{
    public class AccessTokenResult
    {
        // ReSharper disable InconsistentNaming

        // REFERENCE: {"access_token":"CAALJTz2108cBADVTdzl6YUhORAKqJAOIogaa48AUM0yzkcqqKy9VRLbOQUelCYUOZCVT5eoONZBv9zCLyDfAhtMRwcLA6SP5Jd7RQDWj9NX4YsPQ8uMqT9Luq4qqQc8Sov9B1S9BklOYiz9NT1cGDrPzGfG0iZBckusYPYvGFetNgZAnYOjonZCYY8snixLIuM3Rl7HiTNgZDZD","token_type":"bearer","expires_in":5122056}

        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }

        // ReSharper restore InconsistentNaming
    }
}