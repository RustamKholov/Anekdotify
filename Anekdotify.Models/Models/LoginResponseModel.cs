using System;

namespace Anekdotify.Models.Models;

public class LoginResponseModel
{
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public long ExpiresIn { get; set; } // in seconds
}
