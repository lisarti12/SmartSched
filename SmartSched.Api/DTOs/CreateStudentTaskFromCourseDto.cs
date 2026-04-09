using System.ComponentModel.DataAnnotations;

namespace SmartSched.Api.DTOs
{
    public class CreateStudentTaskFromCourseDto
    {
        [Range(1, 12)]
        public int EstimatedHours { get; set; }

        [Range(1, 5)]
        public int Priority { get; set; }

        [Range(1, 5)]
        public int Difficulty { get; set; }
    }
}