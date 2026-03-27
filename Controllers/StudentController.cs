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
        public async Task<IActionResult> GetMySchedule()
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var schedule = await _context.ScheduleSuggestions
                .Include(s => s.TaskItem)
                .Where(s => s.StudentId == studentId)
                .OrderBy(s => s.ScheduledDate)
                .ThenBy(s => s.StartTime)
                .Select(s => new
                {
                    s.Id,
                    s.ScheduledDate,
                    s.StartTime,
                    s.EndTime,
                    s.AllocatedHours,
                    TaskTitle = s.TaskItem!.Title,
                    Course = s.TaskItem.Course
                })
                .ToListAsync();

            return Ok(schedule);
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
        [HttpGet("courses")]
        public async Task<IActionResult> GetMyCourses()
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var courses = await _context.CourseEnrollments
                .Include(e => e.CourseClass)
                .Where(e => e.StudentId == studentId)
                .Select(e => new
                {
                    e.CourseClassId,
                    Title = e.CourseClass!.Title,
                    Description = e.CourseClass.Description,
                    Semester = e.CourseClass.Semester
                })
                .OrderBy(x => x.Title)
                .ToListAsync();

            return Ok(courses);
        }

        [HttpGet("courses/{classId}")]
        public async Task<IActionResult> GetCourseDetails(int classId)
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var enrolled = await _context.CourseEnrollments
                .AnyAsync(e => e.CourseClassId == classId && e.StudentId == studentId);

            if (!enrolled)
                return NotFound(new { message = "Course not found." });

            var course = await _context.CourseClasses.FirstAsync(c => c.Id == classId);

            var now = DateTime.Now;

            var activeContent = await _context.CourseContentItems
                .Where(ci => ci.CourseClassId == classId && ci.DueDate >= now)
                .OrderBy(ci => ci.DueDate)
                .Select(ci => new
                {
                    ci.Id,
                    ci.Type,
                    ci.Title,
                    ci.Description,
                    ci.DueDate,
                    ci.FilePath,
                    AlreadyImported = _context.TaskItems.Any(t => t.StudentId == studentId && t.CourseContentItemId == ci.Id)
                })
                .ToListAsync();

            var previousContent = await _context.CourseContentItems
                .Where(ci => ci.CourseClassId == classId && ci.DueDate < now)
                .OrderByDescending(ci => ci.DueDate)
                .Select(ci => new
                {
                    ci.Id,
                    ci.Type,
                    ci.Title,
                    ci.Description,
                    ci.DueDate,
                    ci.FilePath,
                    AlreadyImported = _context.TaskItems.Any(t => t.StudentId == studentId && t.CourseContentItemId == ci.Id)
                })
                .ToListAsync();

            var lectures = await _context.LectureItems
                .Where(l => l.CourseClassId == classId)
                .OrderByDescending(l => l.CreatedAt)
                .Select(l => new
                {
                    l.Id,
                    l.Title,
                    l.FilePath
                })
                .ToListAsync();

            return Ok(new
            {
                Course = course,
                ActiveContent = activeContent,
                PreviousContent = previousContent,
                Lectures = lectures
            });
        }

        [HttpGet("task-notifications")]
        public async Task<IActionResult> GetTaskNotifications()
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var notifications = await _context.StudentTaskNotifications
                .Where(n => n.StudentId == studentId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return Ok(notifications);
        }

        [HttpPut("task-notifications/{id}/read")]
        public async Task<IActionResult> MarkTaskNotificationRead(int id)
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var notification = await _context.StudentTaskNotifications
                .FirstOrDefaultAsync(n => n.Id == id && n.StudentId == studentId);

            if (notification == null)
                return NotFound(new { message = "Notification not found." });

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Notification marked as read." });
        }

        [HttpPost("course-content/{contentId}/create-task")]
        public async Task<IActionResult> CreateTaskFromCourseContent(int contentId, CreateStudentTaskFromCourseDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var content = await _context.CourseContentItems
                .Include(ci => ci.CourseClass)
                .FirstOrDefaultAsync(ci => ci.Id == contentId);

            if (content == null)
                return NotFound(new { message = "Content item not found." });

            var enrolled = await _context.CourseEnrollments
                .AnyAsync(e => e.CourseClassId == content.CourseClassId && e.StudentId == studentId);

            if (!enrolled)
                return BadRequest(new { message = "You are not enrolled in this class." });

            var exists = await _context.TaskItems
                .AnyAsync(t => t.StudentId == studentId && t.CourseContentItemId == contentId);

            if (exists)
                return BadRequest(new { message = "This class item is already added to SmartSched." });

            bool hasAvailability = await _context.AvailabilityRules.AnyAsync(a => a.StudentId == studentId);

            var task = new TaskItem
            {
                Title = content.Title,
                Description = content.Description,
                Course = content.CourseClass!.Title,
                Deadline = content.DueDate,
                EstimatedHours = dto.EstimatedHours,
                Priority = dto.Priority,
                Difficulty = dto.Difficulty,
                Status = "Pending",
                IsProfessorAssigned = false,
                StudentId = studentId!,
                CourseContentItemId = contentId
            };

            _context.TaskItems.Add(task);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Task added to SmartSched successfully.",
                warning = hasAvailability
                    ? ""
                    : "You have not set availability yet. SmartSched will use default availability of 9:00 AM to 11:00 PM for all days until you configure it."
            });
        }

        [HttpDelete("tasks/{id}")]
        public async Task<IActionResult> DeleteMyTask(int id)
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var task = await _context.TaskItems
                .FirstOrDefaultAsync(t => t.Id == id && t.StudentId == studentId);

            if (task == null)
                return NotFound(new { message = "Task not found." });

            if (task.IsProfessorAssigned)
                return BadRequest(new { message = "Professor-assigned tasks cannot be deleted from here." });

            var suggestions = await _context.ScheduleSuggestions
                .Where(s => s.TaskItemId == task.Id)
                .ToListAsync();

            if (suggestions.Any())
                _context.ScheduleSuggestions.RemoveRange(suggestions);

            _context.TaskItems.Remove(task);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Task removed from SmartSched." });
        }

        [HttpPost("holidays")]
        public async Task<IActionResult> AddHoliday(CreateHolidayBlockDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.EndDate < dto.StartDate)
                return BadRequest(new { message = "Holiday end date must be after start date." });

            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var holiday = new HolidayBlock
            {
                StudentId = studentId!,
                Title = dto.Title,
                Description = dto.Description,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate
            };

            _context.HolidayBlocks.Add(holiday);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Holiday/unavailability added successfully." });
        }

        [HttpGet("holidays")]
        public async Task<IActionResult> GetHolidays()
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var holidays = await _context.HolidayBlocks
                .Where(h => h.StudentId == studentId)
                .OrderBy(h => h.StartDate)
                .ToListAsync();

            return Ok(holidays);
        }

        [HttpDelete("holidays/{id}")]
        public async Task<IActionResult> DeleteHoliday(int id)
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var holiday = await _context.HolidayBlocks
                .FirstOrDefaultAsync(h => h.Id == id && h.StudentId == studentId);

            if (holiday == null)
                return NotFound(new { message = "Holiday not found." });

            _context.HolidayBlocks.Remove(holiday);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Holiday removed." });
        }

        [HttpGet("calendar")]
        public async Task<IActionResult> GetCalendar([FromQuery] string view = "week", [FromQuery] DateTime? date = null)
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(studentId))
                return Unauthorized();

            var baseDate = (date ?? DateTime.Today).Date;

            DateTime start;
            DateTime end;

            switch (view.ToLower())
            {
                case "day":
                    start = baseDate;
                    end = baseDate.AddDays(1);
                    break;

                case "month":
                    start = new DateTime(baseDate.Year, baseDate.Month, 1);
                    end = start.AddMonths(1);
                    break;

                default:
                    int diff = (7 + (baseDate.DayOfWeek - DayOfWeek.Monday)) % 7;
                    start = baseDate.AddDays(-diff);
                    end = start.AddDays(7);
                    break;
            }

            // Personal/student tasks
            var personalDeadlines = await _context.TaskItems
                .Where(t => t.StudentId == studentId
                            && t.Deadline >= start
                            && t.Deadline < end
                            && t.Status != "Completed")
                .Select(t => new
                {
                    type = "Deadline",
                    title = t.Title,
                    course = t.Course,
                    date = t.Deadline,
                    startTime = (TimeSpan?)t.Deadline.TimeOfDay,
                    endTime = (TimeSpan?)null,
                    source = t.IsProfessorAssigned ? "ProfessorTask" : "PersonalTask"
                })
                .ToListAsync();

            // Professor-created course deadlines for courses where the student is enrolled
            var courseDeadlines = await _context.CourseEnrollments
                .Where(e => e.StudentId == studentId)
                .Join(
                    _context.CourseContentItems,
                    e => e.CourseClassId,
                    cci => cci.CourseClassId,
                    (e, cci) => new { e, cci }
                )
                .Where(x => x.cci.DueDate >= start && x.cci.DueDate < end)
                .Select(x => new
                {
                    type = "Deadline",
                    title = x.cci.Title,
                    course = _context.CourseClasses
                        .Where(c => c.Id == x.e.CourseClassId)
                        .Select(c => c.Title)
                        .FirstOrDefault(),
                    date = x.cci.DueDate,
                    startTime = (TimeSpan?)x.cci.DueDate.TimeOfDay,
                    endTime = (TimeSpan?)null,
                    source = "CourseContent"
                })
                .ToListAsync();

            var scheduledTasks = await _context.ScheduleSuggestions
                .Include(s => s.TaskItem)
                .Where(s => s.StudentId == studentId
                            && s.ScheduledDate >= start
                            && s.ScheduledDate < end)
                .Select(s => new
                {
                    type = "ScheduledTask",
                    title = s.TaskItem!.Title,
                    course = s.TaskItem.Course,
                    date = s.ScheduledDate,
                    startTime = (TimeSpan?)s.StartTime,
                    endTime = (TimeSpan?)s.EndTime,
                    source = "ScheduleSuggestion"
                })
                .ToListAsync();

            var holidays = await _context.HolidayBlocks
                .Where(h => h.StudentId == studentId
                            && h.StartDate < end
                            && h.EndDate >= start)
                .Select(h => new
                {
                    type = "Holiday",
                    title = h.Title,
                    course = "",
                    date = h.StartDate,
                    startTime = (TimeSpan?)null,
                    endTime = (TimeSpan?)null,
                    source = "Holiday"
                })
                .ToListAsync();

            var items = scheduledTasks.Cast<object>()
                .Concat(personalDeadlines)
                .Concat(courseDeadlines)
                .Concat(holidays)
                .OrderBy(x => ((dynamic)x).date)
                .ToList();

            return Ok(new
            {
                view,
                start,
                end,
                items
            });
        }
    }
}