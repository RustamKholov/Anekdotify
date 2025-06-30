namespace Anekdotify.Models.DTOs.Accounts;

public class UserDto
{
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? Role { get; set; }
    public DateTime CreatedAt { get; set; }
}