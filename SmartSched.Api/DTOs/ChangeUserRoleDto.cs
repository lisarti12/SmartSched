using System.ComponentModel.DataAnnotations;

namespace SmartSched.Api.DTOs
{
    public class ChangeUserRoleDto
    {
        [Required]
        [RegularExpression("Student|Professor|Admin")]
        public string NewRole { get; set; } = string.Empty;
    }
}