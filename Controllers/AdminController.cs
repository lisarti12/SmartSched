using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartSched.Api.Data;
using SmartSched.Api.DTOs;
using SmartSched.Api.Models;
using Microsoft.EntityFrameworkCore;

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
        public IActionResult GetUsers()
        {
            var users = _userManager.Users.Select(u => new
            {
                u.Id,
                FullName = u.FirstName + " " + u.LastName,
                u.Email
            }).ToList();

            return Ok(users);
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

            settings.DefaultMaxStudyHoursPerDay = dto.DefaultMaxStudyHoursPerDay;
            settings.DefaultStartHour = dto.DefaultStartHour;
            settings.DefaultEndHour = dto.DefaultEndHour;
            settings.DefaultBreakMinutes = dto.DefaultBreakMinutes;

            await _context.SaveChangesAsync();

            return Ok(new { message = "System settings updated." });
        }
    }
}
