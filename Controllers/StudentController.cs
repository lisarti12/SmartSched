using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSched.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using SmartSched.Api.Data;
using SmartSched.Api.DTOs;
using SmartSched.Api.Services;

namespace SmartSched.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Student")]
    public class StudentController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly SchedulingService _schedulingService;

        public StudentController(AppDbContext context, SchedulingService schedulingService)
        {
            _context = context;
            _schedulingService = schedulingService;
        }

        [HttpPost("tasks")]
        public async Task<IActionResult> CreateTask(CreateTaskDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var task = new TaskItem
            {
                Title = dto.Title,
                Description = dto.Description,
                Course = dto.Course,
                Deadline = dto.Deadline,
                EstimatedHours = dto.EstimatedHours,
                Priority = dto.Priority,
                Difficulty = dto.Difficulty,
                Status = "Pending",
                StudentId = studentId!
            };

            _context.TaskItems.Add(task);
            await _context.SaveChangesAsync();

            return Ok(task);
        }

        [HttpGet("tasks")]
        public async Task<IActionResult> GetMyTasks()
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var tasks = await _context.TaskItems
                .Where(t => t.StudentId == studentId)
                .OrderBy(t => t.Deadline)
                .ToListAsync();

            return Ok(tasks);
        }

        [HttpPost("availability")]
        public async Task<IActionResult> SaveAvailability(CreateAvailabilityDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var existing = await _context.AvailabilityRules
                .FirstOrDefaultAsync(a => a.StudentId == studentId && a.DayOfWeek == dto.DayOfWeek);

            if (existing != null)
            {
                existing.StartTime = dto.StartTime;
                existing.EndTime = dto.EndTime;
                existing.MaxStudyHours = dto.MaxStudyHours;
                existing.IsAvailable = true;
            }
            else
            {
                _context.AvailabilityRules.Add(new AvailabilityRule
                {
                    StudentId = studentId!,
                    DayOfWeek = dto.DayOfWeek,
                    StartTime = dto.StartTime,
                    EndTime = dto.EndTime,
                    MaxStudyHours = dto.MaxStudyHours,
                    IsAvailable = true
                });
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Availability saved." });
        }

        [HttpGet("availability")]
        public async Task<IActionResult> GetAvailability()
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var rules = await _context.AvailabilityRules
                .Where(a => a.StudentId == studentId)
                .OrderBy(a => a.DayOfWeek)
                .ToListAsync();

            return Ok(rules);
        }

        [HttpPost("generate-schedule")]
        public async Task<IActionResult> GenerateSchedule()
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            await _schedulingService.GenerateScheduleForStudentAsync(studentId!);

            return Ok(new { message = "Schedule generated successfully." });
        }

        [HttpGet("schedule")]
        public async Task<IActionResult> GetSchedule()
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var suggestions = await _context.ScheduleSuggestions
                .Include(s => s.TaskItem)
                .Where(s => s.StudentId == studentId)
                .OrderBy(s => s.ScheduledDate)
                .ThenBy(s => s.StartTime)
                .Select(s => new ScheduleSuggestionDto
                {
                    ScheduleSuggestionId = s.Id,
                    TaskItemId = s.TaskItemId,
                    TaskTitle = s.TaskItem!.Title,
                    Course = s.TaskItem.Course,
                    ScheduledDate = s.ScheduledDate,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    AllocatedHours = s.AllocatedHours,
                    Status = s.Status
                })
                .ToListAsync();

            return Ok(suggestions);
        }

        [HttpGet("workload")]
        public async Task<IActionResult> GetWorkload()
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var metrics = await _context.WorkloadMetrics
                .Where(w => w.StudentId == studentId)
                .OrderBy(w => w.Date)
                .ToListAsync();

            return Ok(metrics);
        }

        [HttpPut("tasks/{id}/complete")]
        public async Task<IActionResult> CompleteTask(int id)
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var task = await _context.TaskItems
                .FirstOrDefaultAsync(t => t.Id == id && t.StudentId == studentId);

            if (task == null)
                return NotFound(new { message = "Task not found." });

            task.Status = "Completed";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Task marked as completed." });
        }
    }
}
