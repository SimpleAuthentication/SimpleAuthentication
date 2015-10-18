namespace SimpleAuthentication.ExtraProviders.ThirtySevenSignals
{
    /*
     
    {
        "access_token": "BAhbByIByHsiZXhwaXJlc19hdCI6IjIwMTMtMDctMjBUMDA6NDM6NTlaIiwidXNlcl9pZHMiOls0NjE2OTUxLDY1NTgwNjUsODYyNDEwNywxMTc2MzU5NV0sImNsaWVudF9pZCI6ImFkZDMyZjZhYTJkNjJmNjUwMzEyY2ExOGM5MDhhYWMyMWE0NzNmMGIiLCJ2ZXJzaW9uIjoxLCJhcGlfZGVhZGJvbHQiOiIwMzY0ZTFmYjk3ZjI3MDEzNThhYjIwYzg5OWJjMGY5MCJ9dToJVGltZQ2AWhzAq7+wrw==--193db4a6f3f4e2302c603bf1c189780fc6330bb7",
        "refresh_token": "BAhbByIByHsiZXhwaXJlc19hdCI6IjIwMjMtMDctMDZUMDA6NDM6NTlaIiwidXNlcl9pZHMiOls0NjE2OTUxLDY1NTgwNjUsODYyNDEwNywxMTc2MzU5NV0sImNsaWVudF9pZCI6ImFkZDMyZjZhYTJkNjJmNjUwMzEyY2ExOGM5MDhhYWMyMWE0NzNmMGIiLCJ2ZXJzaW9uIjoxLCJhcGlfZGVhZGJvbHQiOiIwMzY0ZTFmYjk3ZjI3MDEzNThhYjIwYzg5OWJjMGY5MCJ9dToJVGltZQ3A2B7AA8qwrw==--2fe2def8a7a741fc1a4504da5f71d14e3a1778de",
        "expires_in": 1209600
    }
     
     */
    public class AccessTokenResult
    {
        public string access_token { get; set; }
        public string refresh_token { get; set; }
        public int expires_in { get; set; }
    }
}