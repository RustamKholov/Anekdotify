﻿namespace Anekdotify.Models.Models
{
    public class RegisterResponseModel
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
        public long ExpiresIn { get; set; } // in seconds
    }
}
