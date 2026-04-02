using System.ComponentModel.DataAnnotations;

namespace SmartSched.Api.DTOs
{
    public class CreateAvailabilityDto : IValidatableObject
    {
        [Required]
        public DateTime AvailableDate { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var selectedDate = AvailableDate.Date;
            var today = DateTime.Today;

            if (selectedDate < today)
            {
                yield return new ValidationResult(
                    "You cannot add availability for a past day.",
                    new[] { nameof(AvailableDate) });
            }

            if (EndTime <= StartTime)
            {
                yield return new ValidationResult(
                    "End time must be after start time.",
                    new[] { nameof(EndTime) });
            }

            // Optional: if date is today, do not allow a start time that already passed
            if (selectedDate == today && StartTime <= DateTime.Now.TimeOfDay)
            {
                yield return new ValidationResult(
                    "For today, the start time must be later than the current time.",
                    new[] { nameof(StartTime) });
            }
        }
    }
}