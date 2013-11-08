namespace SimpleAuthentication.Core.Providers.Google
{
    public class AccessTokenResult
    {
        /* Sample result
        {
          "access_token" : "ya29.AHES6ZRkbCe14R8ZgnsKEgWxcntWLxuYZ7Uy6Q8jWEVgiCHPKu0CYpVY",
          "token_type" : "Bearer",
          "expires_in" : 3586,
          "id_token" : "eyJhbGciOiJSUzI1NiIsImtpZCI6IjY5Y2EyM2FmYjhlZWIyMTM0NTMwYjExNDc1MzFmMzc0MTg3YTBiOWYifQ.eyJpc3MiOiJhY2NvdW50cy5nb29nbGUuY29tIiwidG9rZW5faGFzaCI6ImRrZkVpT0NWSDgwVjRaZXVRc0tma2ciLCJhdF9oYXNoIjoiZGtmRWlPQ1ZIODBWNFpldVFzS2ZrZyIsImF1ZCI6IjU4NzE0MDA5OTE5NC5hcHBzLmdvb2dsZXVzZXJjb250ZW50LmNvbSIsImNpZCI6IjU4NzE0MDA5OTE5NC5hcHBzLmdvb2dsZXVzZXJjb250ZW50LmNvbSIsImF6cCI6IjU4NzE0MDA5OTE5NC5hcHBzLmdvb2dsZXVzZXJjb250ZW50LmNvbSIsImlkIjoiMTE2NzEyMzIwMDUxMzQwNjY5MjMzIiwic3ViIjoiMTE2NzEyMzIwMDUxMzQwNjY5MjMzIiwidmVyaWZpZWRfZW1haWwiOiJ0cnVlIiwiZW1haWxfdmVyaWZpZWQiOiJ0cnVlIiwiaGQiOiJhZGxlci5jb20uYXUiLCJlbWFpbCI6Imp1c3RpbkBhZGxlci5jb20uYXUiLCJpYXQiOjEzNzI5NDMzNTAsImV4cCI6MTM3Mjk0NzI1MH0.rOdHq1FBi4Sfi1WfOThmDLruC38fD8u5Vq8AtDoVZSF3j8z05zv6JueWJyF2by4NGFS-T8FroLJJCz2U_WS420crmZcDuEEZkbjzmrZYaerVwfkAtGvjQUykxI5Imv0Bgnwl9v_CtM5uoejH9e7bzEnfs17Gz3pKnPETaGIf4tc"
        }
        */

        public string AccessToken { get; set; }
        public string TokenType { get; set; }
        public long ExpiresIn { get; set; }
        public string IdToken { get; set; }
    }
}