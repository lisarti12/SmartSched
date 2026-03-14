using System.ComponentModel.DataAnnotations;

namespace SmartSched.Api.DTOs
{
    public class CreateHolidayBlockDto
    {
        [Required]
        [StringLength(120, MinimumLength = 3)]
        public string Title { get; set; } = string.Empty;

        [StringLength(300)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }
    }
}