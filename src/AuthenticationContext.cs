﻿namespace TooGoodToGoNotifier
{
    public class AuthenticationContext
    {
        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public int UserId { get; set; }
    }
}