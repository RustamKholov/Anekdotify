using System.ComponentModel.DataAnnotations;

namespace Anekdotify.Models.DTOs.Accounts
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Username is required.")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Not a valid e-mail address.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [MinLength(12, ErrorMessage = "Password must be at least 12 characters long.")]
        [PasswordComplexity]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Confirmation Password is required.")]
        [Compare("Password", ErrorMessage = "Password and Confirmation Password must match.")]
        public string? ConfirmPassword { get; set; }
    }

    public class PasswordComplexityAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            var password = value as string;
            if (string.IsNullOrEmpty(password))
                return false;

            bool hasDigit = password.Any(char.IsDigit);
            bool hasLower = password.Any(char.IsLower);
            bool hasUpper = password.Any(char.IsUpper);
            bool hasNonAlpha = password.Any(ch => !char.IsLetterOrDigit(ch));

            return hasDigit && hasLower && hasUpper && hasNonAlpha;
        }

        public override string FormatErrorMessage(string name)
        {
            return "Password must contain at least one digit, one lowercase letter, one uppercase letter, and one non-alphanumeric character.";
        }
    }
}