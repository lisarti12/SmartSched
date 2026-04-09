using System.ComponentModel.DataAnnotations;

namespace SmartSched.Api.DTOs
{
    public class RegisterDto
    {
        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare("Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        [RegularExpression("Student|Professor", ErrorMessage = "Role must be Student or Professor.")]
        public string Role { get; set; } = string.Empty;
    }
}
