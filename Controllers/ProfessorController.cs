using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartSched.Api.Data;
using SmartSched.Api.DTOs;
using SmartSched.Api.Models;
using System.Security.Claims;

namespace SmartSched.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Professor")]
    public class ProfessorController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfessorController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost("announcements")]
        public async Task<IActionResult> CreateAnnouncement(CreateAnnouncementDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var professorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var announcement = new Announcement
            {
                Title = dto.Title,
                Message = dto.Message,
                Type = dto.Type,
                ProfessorId = professorId!
            };

            _context.Announcements.Add(announcement);
            await _context.SaveChangesAsync();

            var students = await _userManager.GetUsersInRoleAsync("Student");

            var notifications = students.Select(student => new StudentNotification
            {
                StudentId = student.Id,
                AnnouncementId = announcement.Id,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            _context.StudentNotifications.AddRange(notifications);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Announcement created and notifications sent." });
        }

        [HttpPost("assign-task")]
        public async Task<IActionResult> AssignTask(AssignTaskDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var student = await _userManager.FindByIdAsync(dto.StudentId);
            if (student == null)
                return NotFound(new { message = "Student not found." });

            var task = new TaskItem
            {
                Title = dto.Title,
                Description = dto.Description,
                Course = dto.Course,
                Deadline = dto.Deadline,
                EstimatedHours = dto.EstimatedHours,
                Priority = dto.Priority,
                Difficulty = dto.Difficulty,
                StudentId = dto.StudentId,
                IsProfessorAssigned = true,
                Status = "Pending"
            };

            _context.TaskItems.Add(task);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Task assigned successfully." });
        }

        [HttpGet("students")]
        public async Task<IActionResult> GetStudents()
        {
            var students = await _userManager.GetUsersInRoleAsync("Student");

            var result = students.Select(s => new
            {
                s.Id,
                FullName = s.FirstName + " " + s.LastName,
                s.Email
            });

            return Ok(result);
        }

        [HttpGet("student-workload")]
        public async Task<IActionResult> GetStudentWorkload()
        {
            var data = await _context.WorkloadMetrics
                .Include(w => w.Student)
                .GroupBy(w => new { w.StudentId, w.Student!.FirstName, w.Student.LastName, w.Student.Email })
                .Select(g => new
                {
                    g.Key.StudentId,
                    FullName = g.Key.FirstName + " " + g.Key.LastName,
                    g.Key.Email,
                    MaxWorkloadLevel = g.OrderByDescending(x => x.WorkloadScore).Select(x => x.WorkloadLevel).FirstOrDefault(),
                    MaxWorkloadScore = g.Max(x => x.WorkloadScore)
                })
                .ToListAsync();

            return Ok(data);
        }
    }
}
