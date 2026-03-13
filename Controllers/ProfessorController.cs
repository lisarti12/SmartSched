using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartSched.Api.Data;
using SmartSched.Api.DTOs;
using SmartSched.Api.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;



namespace SmartSched.Api.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Professor")]
    public class ProfessorController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public ProfessorController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
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

        [HttpDelete("classes/{classId}/content/{contentId}")]
        public async Task<IActionResult> DeleteClassContent(int classId, int contentId)
        {
            var professorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var courseClass = await _context.CourseClasses
                .FirstOrDefaultAsync(c => c.Id == classId && c.ProfessorId == professorId);

            if (courseClass == null)
                return NotFound(new { message = "Class not found." });

            var contentItem = await _context.CourseContentItems
                .FirstOrDefaultAsync(ci => ci.Id == contentId && ci.CourseClassId == classId);

            if (contentItem == null)
                return NotFound(new { message = "Content item not found." });

            var matchingTasks = await _context.TaskItems
                .Where(t =>
                    t.Course == courseClass.Title &&
                    t.Title == contentItem.Title &&
                    t.Description == contentItem.Description &&
                    t.IsProfessorAssigned)
                .ToListAsync();

            if (matchingTasks.Any())
            {
                _context.TaskItems.RemoveRange(matchingTasks);
            }

            _context.CourseContentItems.Remove(contentItem);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Content item deleted successfully." });
        }

        [HttpDelete("classes/{classId}/lectures/{lectureId}")]
        public async Task<IActionResult> DeleteLecture(int classId, int lectureId)
        {
            var professorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var courseClass = await _context.CourseClasses
                .FirstOrDefaultAsync(c => c.Id == classId && c.ProfessorId == professorId);

            if (courseClass == null)
                return NotFound(new { message = "Class not found." });

            var lecture = await _context.LectureItems
                .FirstOrDefaultAsync(l => l.Id == lectureId && l.CourseClassId == classId);

            if (lecture == null)
                return NotFound(new { message = "Lecture not found." });

            _context.LectureItems.Remove(lecture);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Lecture deleted successfully." });
        }

        [HttpPost("classes/{classId}/lectures")]
        [RequestSizeLimit(52428800)]
        public async Task<IActionResult> AddLecture(
            int classId,
            [FromForm] CreateLectureDto dto,
            IFormFile? file)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var professorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var courseClass = await _context.CourseClasses
                .FirstOrDefaultAsync(c => c.Id == classId && c.ProfessorId == professorId);

            if (courseClass == null)
                return NotFound(new { message = "Class not found." });

            string filePath = string.Empty;

            if (file != null && file.Length > 0)
            {
                var uploadsFolder = Path.Combine(_environment.ContentRootPath, "Uploads");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
                var fullPath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                filePath = $"/Uploads/{uniqueFileName}";
            }

            var lecture = new LectureItem
            {
                CourseClassId = classId,
                Title = dto.Title,
                FilePath = filePath
            };

            _context.LectureItems.Add(lecture);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Lecture created successfully." });
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
                    StudentId = g.Key.StudentId,
                    FullName = g.Key.FirstName + " " + g.Key.LastName,
                    g.Key.Email,
                    MaxWorkloadLevel = g.OrderByDescending(x => x.WorkloadScore).Select(x => x.WorkloadLevel).FirstOrDefault(),
                    MaxWorkloadScore = g.Max(x => x.WorkloadScore)
                })
                .ToListAsync();

            return Ok(data);
        }

        [HttpPost("classes")]
        public async Task<IActionResult> CreateClass(CreateCourseClassDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var professorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var courseClass = new CourseClass
            {
                Title = dto.Title,
                Description = dto.Description,
                Semester = dto.Semester,
                ProfessorId = professorId!
            };

            _context.CourseClasses.Add(courseClass);
            await _context.SaveChangesAsync();

            return Ok(courseClass);
        }

        [HttpGet("classes")]
        public async Task<IActionResult> GetMyClasses()
        {
            var professorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var classes = await _context.CourseClasses
                .Where(c => c.ProfessorId == professorId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return Ok(classes);
        }

        [HttpGet("classes/{classId}")]
        public async Task<IActionResult> GetClassDetails(int classId)
        {
            var professorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var courseClass = await _context.CourseClasses
                .FirstOrDefaultAsync(c => c.Id == classId && c.ProfessorId == professorId);

            if (courseClass == null)
                return NotFound(new { message = "Class not found." });

            var students = await _context.CourseEnrollments
                .Include(e => e.Student)
                .Where(e => e.CourseClassId == classId)
                .Select(e => new
                {
                    e.StudentId,
                    FullName = e.Student!.FirstName + " " + e.Student.LastName,
                    e.Student.Email
                })
                .ToListAsync();

            var now = DateTime.Now;

            var activeContent = await _context.CourseContentItems
                .Where(ci => ci.CourseClassId == classId && ci.DueDate >= now)
                .OrderBy(ci => ci.DueDate)
                .ToListAsync();

            var previousContent = await _context.CourseContentItems
                .Where(ci => ci.CourseClassId == classId && ci.DueDate < now)
                .OrderByDescending(ci => ci.DueDate)
                .ToListAsync();

            var lectures = await _context.LectureItems
                .Where(l => l.CourseClassId == classId)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();

            return Ok(new
            {
                Class = courseClass,
                Students = students,
                ActiveContent = activeContent,
                PreviousContent = previousContent,
                Lectures = lectures
            });
        }

        [HttpPost("classes/{classId}/students")]
        public async Task<IActionResult> AddStudentsToClass(int classId, [FromBody] AddStudentsToCourseDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var professorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var courseClass = await _context.CourseClasses
                .FirstOrDefaultAsync(c => c.Id == classId && c.ProfessorId == professorId);

            if (courseClass == null)
                return NotFound(new { message = "Class not found." });

            var students = await _userManager.GetUsersInRoleAsync("Student");
            var validStudentIds = students.Select(s => s.Id).ToHashSet();

            var existingEnrollments = await _context.CourseEnrollments
                .Where(e => e.CourseClassId == classId)
                .Select(e => e.StudentId)
                .ToListAsync();

            var toAdd = dto.StudentIds
                .Where(id => validStudentIds.Contains(id) && !existingEnrollments.Contains(id))
                .Distinct()
                .Select(id => new CourseEnrollment
                {
                    CourseClassId = classId,
                    StudentId = id
                })
                .ToList();

            if (!toAdd.Any())
                return BadRequest(new { message = "No valid new students selected." });

            _context.CourseEnrollments.AddRange(toAdd);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"{toAdd.Count} student(s) added successfully." });
        }

        [HttpDelete("classes/{classId}/students/{studentId}")]
        public async Task<IActionResult> RemoveStudentFromClass(int classId, string studentId)
        {
            var professorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var courseClass = await _context.CourseClasses
                .FirstOrDefaultAsync(c => c.Id == classId && c.ProfessorId == professorId);

            if (courseClass == null)
                return NotFound(new { message = "Class not found." });

            var enrollment = await _context.CourseEnrollments
                .FirstOrDefaultAsync(e => e.CourseClassId == classId && e.StudentId == studentId);

            if (enrollment == null)
                return NotFound(new { message = "Student is not enrolled in this class." });

            _context.CourseEnrollments.Remove(enrollment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Student removed from class successfully." });
        }

        [HttpPost("classes/{classId}/content")]
        [RequestSizeLimit(52428800)]
        public async Task<IActionResult> AddClassContent(
            int classId,
            [FromForm] CreateCourseContentItemDto dto,
            IFormFile? file)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.DueDate <= DateTime.Now)
                return BadRequest(new { message = "Deadline must be in the future." });

            var professorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var courseClass = await _context.CourseClasses
                .FirstOrDefaultAsync(c => c.Id == classId && c.ProfessorId == professorId);

            if (courseClass == null)
                return NotFound(new { message = "Class not found." });

            string filePath = string.Empty;

            if (file != null && file.Length > 0)
            {
                var uploadsFolder = Path.Combine(_environment.ContentRootPath, "Uploads");
                var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
                var fullPath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                filePath = $"/Uploads/{uniqueFileName}";
            }

            var item = new CourseContentItem
            {
                CourseClassId = classId,
                Type = dto.Type,
                Title = dto.Title,
                Description = dto.Description,
                DueDate = dto.DueDate,
                FilePath = filePath
            };

            _context.CourseContentItems.Add(item);
            await _context.SaveChangesAsync();

            var enrolledStudentIds = await _context.CourseEnrollments
                .Where(e => e.CourseClassId == classId)
                .Select(e => e.StudentId)
                .ToListAsync();

            var tasks = enrolledStudentIds.Select(studentId => new TaskItem
            {
                Title = dto.Title,
                Description = dto.Description,
                Course = courseClass.Title,
                Deadline = dto.DueDate,
                EstimatedHours = 1,
                Priority = 3,
                Difficulty = 3,
                Status = "Pending",
                IsProfessorAssigned = true,
                StudentId = studentId
            }).ToList();

            _context.TaskItems.AddRange(tasks);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Class content created successfully." });
        }

        [HttpGet("classes/{classId}/available-students")]
        public async Task<IActionResult> GetAvailableStudentsForClass(int classId)
        {
            var professorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var courseClass = await _context.CourseClasses
                .FirstOrDefaultAsync(c => c.Id == classId && c.ProfessorId == professorId);

            if (courseClass == null)
                return NotFound(new { message = "Class not found." });

            var enrolledStudentIds = await _context.CourseEnrollments
                .Where(e => e.CourseClassId == classId)
                .Select(e => e.StudentId)
                .ToListAsync();

            var students = await _userManager.GetUsersInRoleAsync("Student");

            var availableStudents = students
                .Where(s => !enrolledStudentIds.Contains(s.Id))
                .Select(s => new
                {
                    s.Id,
                    FullName = s.FirstName + " " + s.LastName,
                    s.Email
                });

            return Ok(availableStudents);
        }
    }
}