using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartSched.Api.Data;
using SmartSched.Api.DTOs;
using SmartSched.Api.Models;

namespace SmartSched.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userManager.Users.ToListAsync();

            var result = new List<object>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                result.Add(new
                {
                    user.Id,
                    FullName = user.FirstName + " " + user.LastName,
                    user.Email,
                    Role = roles.FirstOrDefault() ?? "None",
                    user.IsApproved
                });
            }

            return Ok(result);
        }

        [HttpGet("kpis")]
        public async Task<IActionResult> GetKpis()
        {
            var users = await _userManager.Users.ToListAsync();

            int totalAdmins = 0;
            int totalStudents = 0;
            int totalProfessors = 0;
            int totalPendingProfessors = 0;

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var role = roles.FirstOrDefault();

                if (role == "Professor" && !user.IsApproved)
                {
                    totalPendingProfessors++;
                    continue;
                }

                if (role == "Admin") totalAdmins++;
                if (role == "Student") totalStudents++;
                if (role == "Professor") totalProfessors++;
            }

            return Ok(new
            {
                totalUsers = totalAdmins + totalStudents + totalProfessors,
                totalAdmins,
                totalStudents,
                totalProfessors,
                totalPendingProfessors
            });
        }

        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found." });

            if (user.Email == "lisart.mella@gmail.com")
            {
                return BadRequest(new { message = "The main admin account cannot be deleted." });
            }

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { message = "User deleted successfully." });
        }

        [HttpGet("pending-professors")]
        public async Task<IActionResult> GetPendingProfessors()
        {
            var users = await _userManager.Users
                .Where(u => !u.IsApproved)
                .ToListAsync();

            var pendingProfessors = new List<object>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                if (roles.Contains("Professor"))
                {
                    pendingProfessors.Add(new
                    {
                        user.Id,
                        Username = user.UserName,
                        FullName = user.FirstName + " " + user.LastName,
                        user.Email
                    });
                }
            }

            return Ok(pendingProfessors);
        }

        [HttpPut("approve-professor/{id}")]
        public async Task<IActionResult> ApproveProfessor(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found." });

            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains("Professor"))
                return BadRequest(new { message = "User is not a professor." });

            user.IsApproved = true;
            await _userManager.UpdateAsync(user);

            return Ok(new { message = "Professor approved successfully." });
        }

        [HttpDelete("decline-professor/{id}")]
        public async Task<IActionResult> DeclineProfessor(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found." });

            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains("Professor"))
                return BadRequest(new { message = "User is not a professor." });

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { message = "Professor registration declined and removed." });
        }

        [HttpPut("change-role/{id}")]
        public async Task<IActionResult> ChangeRole(string id, ChangeUserRoleDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found." });

            var currentRoles = await _userManager.GetRolesAsync(user);

            if (currentRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
            }

            await _userManager.AddToRoleAsync(user, dto.NewRole);

            if (dto.NewRole == "Professor")
            {
                user.IsApproved = true;
            }
            else
            {
                user.IsApproved = true;
            }

            await _userManager.UpdateAsync(user);

            return Ok(new { message = $"User role changed to {dto.NewRole}." });
        }

        [HttpGet("analytics")]
        public async Task<IActionResult> GetAnalytics()
        {
            var totalUsers = await _userManager.Users.CountAsync();
            var totalTasks = await _context.TaskItems.CountAsync();
            var totalSchedules = await _context.ScheduleSuggestions.CountAsync();
            var totalWarnings = await _context.WorkloadMetrics.CountAsync(w => w.WorkloadLevel == "High");

            return Ok(new
            {
                totalUsers,
                totalTasks,
                totalSchedules,
                totalWarnings
            });
        }

        [HttpGet("settings")]
        public async Task<IActionResult> GetSettings()
        {
            var settings = await _context.SystemSettings.FirstOrDefaultAsync();
            return Ok(settings);
        }

        [HttpPut("settings")]
        public async Task<IActionResult> UpdateSettings(UpdateSystemSettingDto dto)
        {
            var settings = await _context.SystemSettings.FirstOrDefaultAsync();

            if (settings == null)
            {
                settings = new SystemSetting();
                _context.SystemSettings.Add(settings);
            }

            settings.DefaultStartHour = dto.DefaultStartHour;
            settings.DefaultEndHour = dto.DefaultEndHour;
            settings.DefaultBreakMinutes = dto.DefaultBreakMinutes;

            await _context.SaveChangesAsync();

            return Ok(new { message = "System settings updated." });
        }
    }
}